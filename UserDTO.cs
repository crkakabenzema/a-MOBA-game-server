using System;
using System.Collections.Generic;
using System.Text;

namespace GameProtocol.dto
{
    [Serializable]
   public class UserDTO
   {
       public int id;//玩家ID 唯一主键
       public string name;//玩家昵称
       public int level;//玩家等级
       public int exp;//玩家经验
       public int winCount;//胜利场次
       public int loseCount;//失败场次
       public int ranCount;//逃跑场次
       public int[] heroList;//玩家拥有的英雄列表
       public UserDTO() { }
       public UserDTO(string name, int id, int level, int win, int lose, int ran,int[] heroList)
       {
           this.id = id;
           this.name = name;
           this.winCount = win;
           this.loseCount = lose;
           this.ranCount = ran;
           this.level = level;
           this.heroList = heroList;
       }
    }
}
