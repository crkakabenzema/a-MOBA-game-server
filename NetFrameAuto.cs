using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetFrame.auto
{
   public class SocketModel
    {
       /// <summary>
       /// 一级协议 用于区分所属模块
       /// </summary>
       public byte type {get;set;}
       /// <summary>
       /// 二级协议 用于区分 模块下所属子模块
       /// </summary>
       public int area { get; set; }
       /// <summary>
       /// 三级协议  用于区分当前处理逻辑功能
       /// </summary>
       public int command { get; set; }
       /// <summary>
       /// 消息体 当前需要处理的主体数据
       /// </summary>
       public object message { get; set; }

       public SocketModel() { }
       public SocketModel(byte t,int a,int c,object o) {
           this.type = t;
           this.area = a;
           this.command = c;
           this.message = o;
       }

       public T GetMessage<T>() {
           return (T)message;
       }
    }
     
    粘包出现原因：在流传输中出现（UDP不会出现粘包，因为它有消息边界）
　　　1 发送端需要等缓冲区满才发送出去，造成粘包
　　　2 接收方不及时接收缓冲区的包，造成多个包接收
    所以这里我们需要对粘包长度进行编码与解码 
   消息序列化：
   public class MessageEncoding
    {
       /// <summary>
       /// 消息体序列化
       /// </summary>
       /// <param name="value"></param>
       /// <returns></returns>
       public static byte[] encode(object value) {
           SocketModel model = value as SocketModel;
           ByteArray ba = new ByteArray();
           ba.write(model.type);
           ba.write(model.area);
           ba.write(model.command);
           //判断消息体是否为空  不为空则序列化后写入
           if (model.message != null)
           {
               ba.write(SerializeUtil.encode(model.message));
           }
           byte[] result = ba.getBuff();
           ba.Close();
           return result;
       }
       /// <summary>
       /// 消息体反序列化
       /// </summary>
       /// <param name="value"></param>
       /// <returns></returns>
       public static object decode(byte[] value)
       {
           ByteArray ba = new ByteArray(value);
           SocketModel model = new SocketModel();
           byte type;
           int area;
           int command;
           //从数据中读取 三层协议  读取数据顺序必须和写入顺序保持一致
           ba.read(out type);
           ba.read(out area);
           ba.read(out command);
           model.type = type;
           model.area = area;
           model.command = command;
           //判断读取完协议后 是否还有数据需要读取 是则说明有消息体 进行消息体读取
           if (ba.Readnable) {
               byte[] message;
               //将剩余数据全部读取出来
               ba.read(out message, ba.Length - ba.Position);
               //反序列化剩余数据为消息体
               model.message = SerializeUtil.decode(message);
           }
           ba.Close();
           return model;
       }
    }
 }
}
