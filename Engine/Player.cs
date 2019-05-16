﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    //public class Player
    //{
    //    public int CurrentHitPoints { get; set; }
    //    public int MaxinumHitPoints { get; set; }
    //    public int Gold { get; set; }
    //    public int ExperiencePoints { get; set; }
    //    public int Level { get; set; }
    //}

    //使用继承避免重复，继承自LivingCreature
    public class Player : LivingCreature
    {
        public int Gold { get; set; }
        public int ExperiencePoints { get; set; }
        //自动更新等级值 public int Level { get; set; }
        public int Level
        {
            //+1, so the player will start out a level 1, and not 0.
            get { return ((ExperiencePoints / 100) + 1); }
        }

        public Location CurrentLocation { get; set; }

        //list or collection property
        public List<InventoryItem> Inventory { get; set; }
        public List<PlayerQuest> Quests { get; set; }

        #region 构造函数
        public Player(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints//, int level
            ) :
            base(currentHitPoints, maximumHitPoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;
            //去掉设置等级属性后，不用也不能赋值了 Level = level;

            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
        }
        #endregion

        #region  从UI的长代码中，重构出来，判断是否有进入一个位置所需要的物品
        public bool HasRequiredItemToEnterThisLocation(Location location)
        {
            if (location.ItemRequiredToEnter == null)
            {
                // There is no required item for this location,  so return "true"
                return true;
            }

            // See if the player has the required item in their inventory
            //用LINQ代替foreach
            //foreach (InventoryItem ii in Inventory)
            //{
            //    if (ii.Details.ID == location.ItemRequiredToEnter.ID)
            //    {
            //        // We found the required item, so return "true"
            //        return true;
            //    }
            //}

            // We didn't find the required item in their inventory, so return "false"
            //// return false;代替佛reach后可以返回false，不需要了

            //lambda表达式中的ii就是foreach中的ii，右面是if条件中的条件
            //左面是遍历列表的变量的声明，右面是条件表达式
            return Inventory.Exists(ii => ii.Details.ID == location.ItemRequiredToEnter.ID);
        }
        #endregion

        #region 从UI的长代码中，重构出来，判断玩家有没有这个关卡及这个关卡是否完成
        public bool HasThisQuest(Quest quest)
        {
            //用LINQ代替foreach
            //foreach (PlayerQuest playerQuest in Quests)
            //{
            //    if (playerQuest.Details.ID == quest.ID)
            //    {
            //        return true;
            //    }
            //}
            //return false;

            //lambda表达式中的ii就是foreach中的ii，右面是if条件中的条件
            //左面是遍历列表的变量的声明，右面是条件表达式

            return Quests.Exists(pq => pq.Details.ID == quest.ID);
        }

        //怎么用LINQ代替？取列表中元素的成员
        public bool CompletedThisQuest(Quest quest)
        {

            //用LINQ代替foreach
            //foreach (PlayerQuest playerQuest in Quests)
            //{
            //    if (playerQuest.Details.ID == quest.ID)
            //    {
            //        return playerQuest.IsCompleted;
            //    }
            //}
            //return false;

            //lambda表达式中的ii就是foreach中的ii，右面是if条件中的条件
            //左面是遍历列表的变量的声明，右面是条件表达式
            //lambda表达式后面还可以再加条件，可以同时再判断元素中的成员
            return Quests.Exists(pq => pq.Details.ID == quest.ID && pq.IsCompleted);
        }
        #endregion

        #region 从UI的长代码中，重构出来，判断玩家是否有所有的关卡需要的物品
        public bool HasAllQuestCompletionItems(Quest quest)
        {
            //用LINQ代替foreach
            //// See if the player has all the items needed to complete the quest here
            //foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            //{
            //    bool foundItemInPlayersInventory = false;

            //    // Check each item in the player's inventory, to see if they have it, and enough of it
            //    foreach (InventoryItem ii in Inventory)
            //    {
            //        // The player has the item in their inventory
            //        if (ii.Details.ID == qci.Details.ID)
            //        {
            //            foundItemInPlayersInventory = true;
            //            // The player does not have enough of this item to complete the quest
            //            if (ii.Quantity < qci.Quantity)
            //            {
            //                return false;
            //            }
            //        }
            //    }
            //    // The player does not have any of this quest completion item in their inventory
            //    if (!foundItemInPlayersInventory)
            //    {
            //        return false;
            //    }
            //}
            //// If we got here, then the player must have all the required items, and enough of them, to complete the quest.
            //return true;

            //lambda表达式中的ii就是foreach中的ii，右面是if条件中的条件
            //左面是遍历列表的变量的声明，右面是条件表达式
            //lambda表达式后面还可以再加条件，可以同时再判断元素中的成员
            //还可以再简化，就有点复杂了，以后再说

            // See if the player has all the items needed to complete  the quest here
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                // Check each item in the player's inventory, to see if they have it, and enough of it
                if (!Inventory.Exists(ii => ii.Details.ID == qci.Details.ID && ii.Quantity >= qci.Quantity))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region 从UI的长代码中，重构出来，移除完成关卡用掉的物品
        public void RemoveQuestCompletionItems(Quest quest)
        {
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                //用LINQ代替foreach
                //foreach (InventoryItem ii in Inventory)
                //{
                //    if (ii.Details.ID == qci.Details.ID)
                //    {
                //        // Subtract the quantity from the player's inventory that was needed to complete the quest
                //        ii.Quantity -= qci.Quantity;
                //        break;
                //    }
                //}

                //lambda表达式中的ii就是foreach中的ii，右面是if条件中的条件
                //左面是遍历列表的变量的声明，右面是条件表达式
                //lambda表达式后面还可以再加条件，可以同时再判断元素中的成员
                //还可以再简化，就有点复杂了，以后再说
                //可以通过SingleOrDefault返回列表的一个元素，需要检查是否为null，只能有一个匹配的元素返回
                InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == qci.Details.ID);

                if (item != null)
                {
                    // Subtract the quantity from the player's inventory that was needed to complete the quest
                    item.Quantity -= qci.Quantity;
                }
            }
        }
        #endregion

        #region 从UI的长代码中，重构出来，完成关卡的奖励
        public void AddItemToInventory(Item itemToAdd)
        {
            //用LINQ代替foreach
            //foreach (InventoryItem ii in Inventory)
            //{
            //    if (ii.Details.ID == itemToAdd.ID)
            //    {
            //        // They have the item in their inventory, so increase the quantity by one
            //        ii.Quantity++;

            //        return; // We added the item, and are done, so get out of this function
            //    }
            //}
            //// They didn't have the item, so add it to their inventory, with a quantity of 1
            //Inventory.Add(new InventoryItem(itemToAdd, 1));

            //lambda表达式中的ii就是foreach中的ii，右面是if条件中的条件
            //左面是遍历列表的变量的声明，右面是条件表达式
            //lambda表达式后面还可以再加条件，可以同时再判断元素中的成员
            //还可以再简化，就有点复杂了，以后再说
            //可以通过SingleOrDefault返回列表的一个元素，需要检查是否为null，只能有一个匹配的元素返回

            InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToAdd.ID);

            if (item == null)
            {
                // They didn't have the item, so add it to their inventory, with a quantity of 1
                Inventory.Add(new InventoryItem(itemToAdd, 1));
            }
            else
            {
                // They have the item in their inventory, so increase the quantity by one
                item.Quantity++;
            }
        }
        #endregion

        #region 从UI的长代码中，重构出来，标记完成的关卡
        public void MarkQuestCompleted(Quest quest)
        {
            // Find the quest in the player's quest list
            foreach (PlayerQuest pq in Quests)
            {
                //用LINQ代替foreach
                //if (pq.Details.ID == quest.ID)
                //{
                //    // Mark it as completed
                //    pq.IsCompleted = true;

                //    // We found the quest, and marked it complete, so get  out of this function
                //    return;
                //}

                //lambda表达式中的ii就是foreach中的ii，右面是if条件中的条件
                //左面是遍历列表的变量的声明，右面是条件表达式
                //lambda表达式后面还可以再加条件，可以同时再判断元素中的成员
                //还可以再简化，就有点复杂了，以后再说
                //可以通过SingleOrDefault返回列表的一个元素，需要检查是否为null，只能有一个匹配的元素返回
                // Find the quest in the player's quest list
                PlayerQuest playerQuest = Quests.SingleOrDefault(
                    pq => pq.Details.ID == quest.ID);
                if (playerQuest != null)
                {
                    playerQuest.IsCompleted = true;
                }
            }
        }
        #endregion


    }
}
