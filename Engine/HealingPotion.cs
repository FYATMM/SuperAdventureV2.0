using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    //使用继承避免重复，继承自item
    public class HealingPotion : Item
    {
        public int AmountToHeal { get; set; }

        //继承类需要给基类的构造函数传参
        public HealingPotion(int id, string name, string namePlural, int amountToHeal) : base(id, name, namePlural)
        {
               AmountToHeal = amountToHeal;
            //git test 1st times
        }
    }
}
