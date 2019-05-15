using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
   public  class Quest
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RewardExperiencePoints { get; set; }//RewardExperiencePoints
        public int RewardGold { get; set; }

        public Item RewardItem { get; set; }

        public List<QuestCompletionItem> QuestCompletionItems { get; set; }

        //通过构造函数初始化，传入参数，保留相应的属性到特定对象
        public Quest(int id, string name, string description, int rewardExperiencePoints, int rewardGold,
            Item rewardItem = null)
        {
            ID = id;
            Name = name;
            Description = description;
            RewardExperiencePoints = rewardExperiencePoints;
            RewardGold = RewardGold;

            RewardItem = rewardItem;

            QuestCompletionItems= new List<QuestCompletionItem>();
        }
    }
}
