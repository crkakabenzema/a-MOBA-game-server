using LOLServer.biz;
using LOLServer.dao.model;
using NetFrame;
using NetFrame.auto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOLServer.logic
{
   public class AbsOnceHandler
    {
      public IUserBiz userBiz = BizFactory.userBiz;

       private byte type;
       private int area;

       public void SetArea(int area) {
           this.area = area;
       }

       public virtual int GetArea() {
           return area;
       }

       public void SetType(byte type)
       {
           this.type = type;
       }

       public new virtual byte GetType()
       {
           return type;
       }

       /// <summary>
       /// 通过连接对象获取用户
       /// </summary>
       /// <param name="token"></param>
       /// <returns></returns>
       public USER getUser(UserToken token)
       {
           return userBiz.get(token);
       }

       /// <summary>
       /// 通过ID获取用户
       /// </summary>
       /// <param name="token"></param>
       /// <returns></returns>
       public USER getUser(int id)
       {
           return userBiz.get(id);
       }

       /// <summary>
       /// 通过连接对象 获取用户ID
       /// </summary>
       /// <param name="token"></param>
       /// <returns></returns>
       public int getUserId(UserToken token){
           USER user = getUser(token);
           if(user==null)return -1;
           return user.id;
       }
       /// <summary>
       /// 通过用户ID获取连接
       /// </summary>
       /// <param name="id"></param>
       /// <returns></returns>
       public UserToken getToken(int id) {
           return userBiz.getToken(id);
       }


       #region 通过连接对象发送
       public void write(UserToken token,int command) {
           write(token, command, null);
       }
       public void write(UserToken token, int command,object message)
       {
           write(token,GetArea(), command, message);
       }
       public void write(UserToken token,int area, int command, object message)
       {
           write(token,GetType(), GetArea(), command, message);
       }
       public void write(UserToken token,byte type, int area, int command, object message)
       {
           byte[] value = MessageEncoding.encode(CreateSocketModel(type,area,command,message));
           value = LengthEncoding.encode(value);
           token.write(value);
       }
       #endregion

       #region 通过ID发送
       public void write(int id, int command)
       {
           write(id, command, null);
       }
       public void write(int id, int command, object message)
       {
           write(id, GetArea(), command, message);
       }
       public void write(int id, int area, int command, object message)
       {
           write(id, GetType(), area, command, message);
       }
       public void write(int id, byte type, int area, int command, object message)
       {
           UserToken token= getToken(id);
           if(token==null)return;
           write(token, type, area, command, message);
       }

       public void writeToUsers(int[] users, byte type, int area, int command, object message) {
           byte[] value = MessageEncoding.encode(CreateSocketModel(type, area, command, message));
           value = LengthEncoding.encode(value);
           foreach (int item in users)
           {
               UserToken token = userBiz.getToken(item);
               if (token == null) continue;
                   byte[] bs = new byte[value.Length];
                   Array.Copy(value, 0, bs, 0, value.Length);
                   token.write(bs);
               
           }
       }
       #endregion
       public SocketModel CreateSocketModel(byte type, int area, int command, object message)
       {
           return new SocketModel(type, area, command, message);
       }
    }

    public class AbsMulitHandler:AbsOnceHandler
    {
       public List<UserToken> list = new List<UserToken>();
       /// <summary>
       /// 用户进入当前子模块
       /// </summary>
       /// <param name="token"></param>
       /// <returns></returns>
       public bool enter(UserToken token) {
           if (list.Contains(token)) {
               return false;
           }
           list.Add(token);
           return true;
       }
       /// <summary>
       /// 用户是否在此子模块
       /// </summary>
       /// <param name="token"></param>
       /// <returns></returns>
       public bool isEntered(UserToken token) {
           return list.Contains(token);
       }
       /// <summary>
       /// 用户离开当前子模块
       /// </summary>
       /// <param name="token"></param>
       /// <returns></returns>
       public bool leave(UserToken token) {
           if (list.Contains(token)) {
               list.Remove(token);
               return true;
           }
           return false;
       }
       #region 消息群发API

       public void brocast(int command, object message,UserToken exToken=null) {
           brocast(GetArea(), command, message, exToken);
       }
       public void brocast(int area, int command, object message, UserToken exToken = null)
       {
           brocast(GetType(), area, command, message, exToken);
       }
       public void brocast(byte type, int area, int command, object message, UserToken exToken = null)
       {
           byte[] value = MessageEncoding.encode(CreateSocketModel(type, area, command, message));
           value = LengthEncoding.encode(value);
           foreach (UserToken item in list)
           {
               if (item != exToken)
               {
                   byte[] bs = new byte[value.Length];
                   Array.Copy(value, 0, bs, 0, value.Length);
                   item.write(bs);
               }
           }
       }
       #endregion
    }
}
