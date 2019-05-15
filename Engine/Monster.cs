using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    //class Monster
    //{
    //    public int ID { get; set; }
    //    public string Name { get; set; }
    //    public int MaximumHitPoint { get; set; }
    //    public int CurrentHitPoint { get; set; }
    //    public int MaximumDamage { get; set; }
    //    public int RewordExperiencePoints { get; set; }
    //    public int RewordGold { get; set; }
    //}

    //使用继承避免重复，继承自LivingCreature
    public class Monster : LivingCreature
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int MaximumDamage { get; set; }
        public int RewardExperiencePoints { get; set; }
        public int RewardGold { get; set; }

        public List<LootItem> LootTable { get; set; }

        public Monster(int id, string name, int maximumDamage,
            int rewardExperiencePoints, int rewardGold, 
                int currentHitPoints, int maximumHitPoints) : 
                    base(currentHitPoints, maximumHitPoints)
        {
            ID = id;
            Name = name;
            MaximumDamage = maximumDamage;
            RewardExperiencePoints = rewardExperiencePoints;
            RewardGold = rewardGold;

            LootTable = new List<LootItem>();
        }
    }
}
