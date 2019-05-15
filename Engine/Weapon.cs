using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    //class Weapon
    //{
    //    public int ID { get; set; }
    //    public string Name { get; set; }
    //    public string NamePlural { get; set; }
    //    public int MinimumDamage { get; set; }
    //    public int MaximumDamage { get; set; }
    //}

    //使用继承避免重复，继承自item
   public  class Weapon : Item
    {
        public int MinimumDamage { get; set; }
        public int MaximumDamage { get; set; }

        public Weapon(int id, string name, string namePllural, int minimumDamage, int maximumDamage) : base(id, name, namePllural)
        {
            MinimumDamage = minimumDamage;
            MaximumDamage = maximumDamage;
        }
    }
}
