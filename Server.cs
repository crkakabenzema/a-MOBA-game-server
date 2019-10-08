using GameProtocol;
using LOLServer.logic;
using LOLServer.logic.fight;
using LOLServer.logic.login;
using LOLServer.logic.match;
using LOLServer.logic.select;
using LOLServer.logic.user;
using NetFrame;
using NetFrame.auto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOLServer
{
   public class HandlerCenter:AbsHandlerCenter
    {
       HandlerInterface login;
       HandlerInterface user;
       HandlerInterface match;
       HandlerInterface select;
       HandlerInterface fight;
       
       public HandlerCenter() {
           login = new LoginHandler();
           user = new UserHandler();
           match = new MatchHandler();
           select = new SelectHandler();
           fight = new FightHandler();
       }

        public override void ClientClose(UserToken token, string error)
        {
            Console.WriteLine("有客户端断开连接了");

            select.ClientClose(token, error);
            match.ClientClose(token, error);
            fight.ClientClose(token, error);
            //user的连接关闭方法 一定要放在逻辑处理单元后面
            //其他逻辑单元需要通过user绑定数据来进行内存清理 
            //如果先清除了绑定关系 其他模块无法获取角色数据会导致无法清理
            user.ClientClose(token, error);
            login.ClientClose(token, error);
        }

        public override void ClientConnect(UserToken token)
        {
            Console.WriteLine("有客户端连接了");
        }

        public override void MessageReceive(UserToken token, object message)
        {
            SocketModel model = message as SocketModel;
            switch (model.type) { 
                case Protocol.TYPE_LOGIN:
                    login.MessageReceive(token, model);
                    break;
                case Protocol.TYPE_USER:
                    user.MessageReceive(token, model);
                    break;
                case Protocol.TYPE_MATCH:
                    match.MessageReceive(token, model);
                    break;
                case Protocol.TYPE_SELECT:
                    select.MessageReceive(token, model);
                    break;
                case Protocol.TYPE_FIGHT:
                    fight.MessageReceive(token, model);
                    break;
                default:
                    //未知模块  可能是客户端作弊了 无视
                    break;
            }
        }
    }
}
