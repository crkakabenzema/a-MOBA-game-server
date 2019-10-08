using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace NetFrame
{
    public delegate byte[] LengthEncode(byte[] value);
    public delegate byte[] LengthDecode(ref List<byte> value);

    public delegate byte[] encode(object value);
    public delegate object decode(byte[] value);
   // 将数据结构转换成二进制串   
   public class SerializeUtil
    {
       /// <summary>
       /// 对象序列化
       /// </summary>
       /// <param name="value"></param>
       /// <returns></returns>
       public static byte[] encode(object value) {
           MemoryStream ms = new MemoryStream();//创建编码解码的内存流对象
           BinaryFormatter bw = new BinaryFormatter();//二进制流序列化对象
           //将obj对象序列化成二进制数据 写入到 内存流
           bw.Serialize(ms, value);
           byte[] result=new byte[ms.Length];
           //将流数据 拷贝到结果数组
           Buffer.BlockCopy(ms.GetBuffer(), 0, result, 0, (int)ms.Length);
           ms.Close();
           return result;
       }
       /// <summary>
       /// 反序列化为对象
       /// </summary>
       /// <param name="value"></param>
       /// <returns></returns>
       public static object decode(byte[] value) {
           MemoryStream ms = new MemoryStream(value);//创建编码解码的内存流对象 并将需要反序列化的数据写入其中
           BinaryFormatter bw = new BinaryFormatter();//二进制流序列化对象
           //将流数据反序列化为obj对象
           object result= bw.Deserialize(ms);
           ms.Close();
           return result;
       }
    }
    

    //将数据写入成二进制
    public class ByteArray
    {
       MemoryStream ms = new MemoryStream();

       BinaryWriter bw;
       BinaryReader br;
       public void Close() {
           bw.Close();
           br.Close();
           ms.Close();
       }

       /// <summary>
       /// 支持传入初始数据的构造
       /// </summary>
       /// <param name="buff"></param>
       public ByteArray(byte[] buff) {
           ms = new MemoryStream(buff);
           bw = new BinaryWriter(ms);
           br = new BinaryReader(ms);
       }

       /// <summary>
       /// 获取当前数据 读取到的下标位置
       /// </summary>
       public int Position {
           get { return (int)ms.Position; }
       }

       /// <summary>
       /// 获取当前数据长度
       /// </summary>
       public int Length
       {
           get { return (int)ms.Length; }
       }
       /// <summary>
       /// 当前是否还有数据可以读取
       /// </summary>
       public bool Readnable{
           get { return ms.Length > ms.Position; }
       }

       /// <summary>
       /// 默认构造
       /// </summary>
      public ByteArray() {
           bw = new BinaryWriter(ms);
           br = new BinaryReader(ms);
       }

      public void write(int value) {
          bw.Write(value);
      }
      public void write(byte value)
      {
          bw.Write(value);
      }
      public void write(bool value)
      {
          bw.Write(value);
      }
      public void write(string value)
      {
          bw.Write(value);
      }
      public void write(byte[] value)
      {
          bw.Write(value);
      }

      public void write(double value)
      {
          bw.Write(value);
      }
      public void write(float value)
      {
          bw.Write(value);
      }
      public void write(long value)
      {
          bw.Write(value);
      }


      public void read(out int value)
      {
          value= br.ReadInt32();
      }
      public void read(out byte value)
      {
          value = br.ReadByte();
      }
      public void read(out bool value)
      {
          value = br.ReadBoolean();
      }
      public void read(out string value)
      {
          value = br.ReadString();
      }
      public void read(out byte[] value,int length)
      {
          value = br.ReadBytes(length);
      }

      public void read(out double value)
      {
          value = br.ReadDouble();
      }
      public void read(out float value)
      {
          value = br.ReadSingle();
      }
      public void read(out long value)
      {
          value = br.ReadInt64();
      }

      public void reposition() {
          ms.Position = 0;
      }

       /// <summary>
       /// 获取数据
       /// </summary>
       /// <returns></returns>
      public byte[] getBuff()
      {
          byte[] result = new byte[ms.Length];
          Buffer.BlockCopy(ms.GetBuffer(), 0, result, 0, (int)ms.Length);
          return result;
      }
      public class ServerStart
    {
       Socket server;//服务器socket监听对象
       int maxClient;//最大客户端连接数
       Semaphore acceptClients;
       UserTokenPool pool;
       public LengthEncode LE;
       public LengthDecode LD;
       public encode encode;
       public decode decode;

       /// <summary>
       /// 消息处理中心，由外部应用传入
       /// </summary>
       public AbsHandlerCenter center;
       /// <summary>
       /// 初始化通信监听
       /// </summary>
       /// <param name="port">监听端口</param>
       public ServerStart(int max) {
           //实例化监听对象
           server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
           //设定服务器最大连接人数
           maxClient = max;
           
       }

       public void Start(int port) {
           //创建连接池
           pool = new UserTokenPool(maxClient);
           //连接信号量
           acceptClients = new Semaphore(maxClient, maxClient);
           for (int i = 0; i < maxClient; i++)
           {
               UserToken token = new UserToken();
               //初始化token信息               
               token.receiveSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Comleted);
               token.sendSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Comleted);
               token.LD = LD;
               token.LE = LE;
               token.encode = encode;
               token.decode = decode;
               token.sendProcess = ProcessSend;
               token.closeProcess = ClientClose;
               token.center = center;
               pool.push(token);
           }
           //监听当前服务器网卡所有可用IP地址的port端口
           // 外网IP  内网IP192.168.x.x 本机IP一个127.0.0.1
           try
           {
               server.Bind(new IPEndPoint(IPAddress.Any, port));
               //置于监听状态
               server.Listen(10);
               StartAccept(null);
           }
           catch (Exception e)
           {
               Console.WriteLine(e.Message);
           }
       }
       /// <summary>
       /// 开始客户端连接监听
       /// </summary>
       public void StartAccept(SocketAsyncEventArgs e) {
           //如果当前传入为空  说明调用新的客户端连接监听事件 否则的话 移除当前客户端连接
           if (e == null)
           {
               e = new SocketAsyncEventArgs();
               e.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Comleted);
           }
           else {
               e.AcceptSocket = null;
           }
           //信号量-1
           acceptClients.WaitOne();
           bool result= server.AcceptAsync(e);
           //判断异步事件是否挂起  没挂起说明立刻执行完成  直接处理事件 否则会在处理完成后触发Accept_Comleted事件
           if (!result) {
               ProcessAccept(e);
           }
       }

       public void ProcessAccept(SocketAsyncEventArgs e) {
           //从连接对象池取出连接对象 供新用户使用
           UserToken token = pool.pop();
           token.conn = e.AcceptSocket;
           //TODO 通知应用层 有客户端连接
           center.ClientConnect(token);
           //开启消息到达监听
           StartReceive(token);
           //释放当前异步对象
           StartAccept(e);
       }

       public void Accept_Comleted(object sender, SocketAsyncEventArgs e) {
           ProcessAccept(e);
       }

       public void StartReceive(UserToken token) {
           try
           {
               //用户连接对象 开启异步数据接收
               bool result = token.conn.ReceiveAsync(token.receiveSAEA);
               //异步事件是否挂起
               if (!result)
               {
                   ProcessReceive(token.receiveSAEA);
               }
           }
           catch (Exception e) {
               Console.WriteLine(e.Message);
           }
       }

       public void IO_Comleted(object sender, SocketAsyncEventArgs e)
       {
           if (e.LastOperation == SocketAsyncOperation.Receive)
           {
               ProcessReceive(e);
           }
           else {
               ProcessSend(e);
           }
       }

       public void ProcessReceive(SocketAsyncEventArgs e) {
           UserToken token= e.UserToken as UserToken;
           //判断网络消息接收是否成功
           if (token.receiveSAEA.BytesTransferred > 0 && token.receiveSAEA.SocketError == SocketError.Success)
           {
               byte[] message = new byte[token.receiveSAEA.BytesTransferred];
               //将网络消息拷贝到自定义数组
               Buffer.BlockCopy(token.receiveSAEA.Buffer, 0, message, 0, token.receiveSAEA.BytesTransferred);
               //处理接收到的消息
               token.receive(message);
               StartReceive(token);
           }
           else {
               if (token.receiveSAEA.SocketError != SocketError.Success)
               {
                   ClientClose(token, token.receiveSAEA.SocketError.ToString());
               }
               else {
                   ClientClose(token, "客户端主动断开连接");
               }
           }
       }
       public void ProcessSend(SocketAsyncEventArgs e) {
           UserToken token = e.UserToken as UserToken;
           if (e.SocketError != SocketError.Success)
           {
               ClientClose(token, e.SocketError.ToString());
           }
           else { 
            //消息发送成功，回调成功
               token.writed();
           }
       }

       /// <summary>
       /// 客户端断开连接
       /// </summary>
       /// <param name="token"> 断开连接的用户对象</param>
       /// <param name="error">断开连接的错误编码</param>
       public void ClientClose(UserToken token,string error) {
           if (token.conn != null) {
               lock (token) { 
                //通知应用层面 客户端断开连接了
                   center.ClientClose(token, error);
                   token.Close();
                   //加回一个信号量，供其它用户使用
                   pool.push(token);
                   acceptClients.Release();                   
               }
           }
        }
     }
   }


    /// <summary>
    /// 用户连接信息对象
    /// </summary>
   public class UserToken
    {
       /// <summary>
       /// 用户连接
       /// </summary>
       public Socket conn;
       //用户异步接收网络数据对象
       public SocketAsyncEventArgs receiveSAEA;
       //用户异步发送网络数据对象
       public SocketAsyncEventArgs sendSAEA;

       public LengthEncode LE;
       public LengthDecode LD;
       public encode encode;
       public decode decode;


       public delegate void SendProcess(SocketAsyncEventArgs e);

       public SendProcess sendProcess;

       public delegate void CloseProcess(UserToken token, string error);

       public CloseProcess closeProcess;

       public AbsHandlerCenter center;

       List<byte> cache = new List<byte>();

       private bool isReading = false;
       private bool isWriting = false;
       Queue<byte[]> writeQueue = new Queue<byte[]>();

       public UserToken() {
           receiveSAEA = new SocketAsyncEventArgs();
           sendSAEA = new SocketAsyncEventArgs();
           receiveSAEA.UserToken = this;
           sendSAEA.UserToken = this;
           //设置接收对象的缓冲区大小
           receiveSAEA.SetBuffer(new byte[1024], 0, 1024);
       }
       //网络消息到达
       public void receive(byte[] buff) {
           //将消息写入缓存
           cache.AddRange(buff);
           if (!isReading)
           {
               isReading = true;
               onData();
           }
       }
       //缓存中有数据处理
       void onData() {
           //解码消息存储对象
           byte[] buff = null;
           //当粘包解码器存在的时候 进行粘包处理
           if (LD != null)
           {
               buff = LD(ref cache);
               //消息未接收全 退出数据处理 等待下次消息到达
               if (buff == null) { isReading = false; return; }
           }
           else {
               //缓存区中没有数据 直接跳出数据处理 等待下次消息到达
               if (cache.Count == 0) { isReading = false; return; }
               buff = cache.ToArray();
               cache.Clear();
           }
           //反序列化方法是否存在
           if (decode == null) { throw new Exception("message decode process is null"); }
           //进行消息反序列化
           object message = decode(buff);
           //TODO 通知应用层 有消息到达
           center.MessageReceive(this, message);
           //尾递归 防止在消息处理过程中 有其他消息到达而没有经过处理
           onData();
       }

       public void write(byte[] value) {
           if (conn == null) {
               //此连接已经断开了
               closeProcess(this, "调用已经断开的连接");
               return;
           }
           writeQueue.Enqueue(value);
           if (!isWriting) {
               isWriting = true;
               onWrite();
           }
       }

       public void onWrite() {
           //判断发送消息队列是否有消息
           if (writeQueue.Count == 0) { isWriting = false; return; }
           //取出第一条待发消息
           byte[] buff = writeQueue.Dequeue();
           //设置消息发送异步对象的发送数据缓冲区数据
           sendSAEA.SetBuffer(buff, 0, buff.Length);
           //开启异步发送
           bool result = conn.SendAsync(sendSAEA);
           //是否挂起
           if (!result) {
               sendProcess(sendSAEA);
           }
       }

       public void writed() {
           //与onData尾递归同理
           onWrite();
       }
       public void Close() {
           try
           {
               writeQueue.Clear();
               cache.Clear();
               isReading = false;
               isWriting = false;
               conn.Shutdown(SocketShutdown.Both);
               conn.Close();
               conn = null;
           }
           catch (Exception e) {
               Console.WriteLine(e.Message);
           }
       }
    }
    public class UserTokenPool
    {
       private Stack<UserToken> pool;

       public UserTokenPool(int max) {
           pool = new Stack<UserToken>(max);
       }
       /// <summary>
       /// 取出一个连接对象 --创建连接
       /// </summary>
       public UserToken pop() {

           return pool.Pop();
       }
       //插入一个连接对象---释放连接
       public void push(UserToken token) {
           if (token != null)
               pool.Push(token);
       }
       public int Size {
           get { return pool.Count; } 
       }
    }
    定义了客户端连接、收到客户端消息和客户端断开连接的抽象类：
    public abstract class AbsHandlerCenter
    {
       /// <summary>
       /// 客户端连接
       /// </summary>
       /// <param name="token">连接的客户端对象</param>
       public abstract void ClientConnect(UserToken token);
       /// <summary>
       /// 收到客户端消息
       /// </summary>
       /// <param name="token">发送消息的客户端对象</param>
       /// <param name="message">消息内容</param>
       public abstract void MessageReceive(UserToken token, object message);
       /// <summary>
       /// 客户端断开连接
       /// </summary>
       /// <param name="token">断开的客户端对象</param>
       /// <param name="error">断开的错误信息</param>
       public abstract void ClientClose(UserToken token, string error);
    }
}
