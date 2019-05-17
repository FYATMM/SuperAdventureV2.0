using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.ComponentModel;

namespace Engine
{
    //使用继承避免重复，继承自LivingCreature
    public class Player : LivingCreature
    {
        #region 属性(可绑定属性 及 私有属性 及 属性子集)，现在是一个自动属性，没有地方放代码处理设置属性时调用事件处理方法的地方，要改为带变量的属性；设置经验值属性的设置为private，只有通过player的方法才能设置，保证解决方案中所有的经验值增加都按一个逻辑来
        private int _gold;
        private int _experiencePoints;

        public int Gold
        {
            get { return _gold; }
            set
            {
                _gold = value;
                OnPropertyChanged("Gold");
            }
        }

        public int ExperiencePoints
        {
            get { return _experiencePoints; }
            private set
            {
                _experiencePoints = value;
                OnPropertyChanged("ExperiencePoints");
                OnPropertyChanged("Level");//经验值变化时，等级值重新计算，也可能会变化，但在这个小程序中多通知一个没有影响
            }
        }


        public int Level  //自动更新等级值 public int Level { get; set; }
        {
            get { return ((ExperiencePoints / 100) + 1); } //+1, so the player will start out a level 1, and not 0.
        }

        public Location CurrentLocation { get; set; }

        public Weapon CurrentWeapon { get; set; }        //下拉框默认值存储

        //list or collection property       
        public BindingList<InventoryItem> Inventory { get; set; } //变为可绑定的，需要改变数据类型
        public BindingList<PlayerQuest> Quests { get; set; }
        //属性子集
        public List<Weapon> Weapons
        {
            get
            {
                return Inventory.Where(
              x => x.Details is Weapon).Select(
                  x => x.Details as Weapon).ToList();
            }
        }
        public List<HealingPotion> Potions
        {
            get
            {
                return Inventory.Where(
              x => x.Details is HealingPotion).Select(
                  x => x.Details as HealingPotion).ToList();
            }
        }
        #endregion

        #region 构造函数，构造函数设置为private，通过方法调用,可以不用生成对象直接在类内部调用，通过XML构造，没有XML用设定好数据的构造方法
        //去掉设置等级属性后，不用也不能赋值了 Level = level;
        private Player(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints) : base(currentHitPoints, maximumHitPoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;

            Inventory = new BindingList<InventoryItem>();
            Quests = new BindingList<PlayerQuest>();
        }

        //默认玩家数据
        public static Player CreateDefaultPlayer()
        {
            Player player = new Player(10, 10, 20, 0);
            player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
            player.CurrentLocation = World.LocationByID(World.LOCATION_ID_HOME);

            return player;
        }

        //读取xml保存的玩家数据
        public static Player CreatePlayerFromXmlString(string xmlPlayerData)
        {
            //SelectSingleNode获取只有一个值得节点或属性，需要给一个 XPath，InnerText取值
            //取出的值是字符串，需要转化为int
            try
            {
                XmlDocument playerData = new XmlDocument();

                playerData.LoadXml(xmlPlayerData);

                int currentHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentHitPoints").InnerText);
                int maximumHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/MaximumHitPoints").InnerText);
                int gold = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/Gold").InnerText);
                int experiencePoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/ExperiencePoints").InnerText);

                Player player = new Player(currentHitPoints, maximumHitPoints, gold, experiencePoints);

                int currentLocationID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentLocation").InnerText);
                player.CurrentLocation = World.LocationByID(currentLocationID);

                if (playerData.SelectSingleNode("/Player/Stats/CurrentWeapon") != null)
                {
                    int currentWeaponID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentWeapon").InnerText);
                    player.CurrentWeapon = (Weapon)World.ItemByID(currentWeaponID);
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/InventoryItems/InventoryItem"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    int quantity = Convert.ToInt32(node.Attributes["Quantity"].Value);

                    for (int i = 0; i < quantity; i++)
                    {
                        player.AddItemToInventory(World.ItemByID(id));
                    }
                }
                foreach (XmlNode node in playerData.SelectNodes("/Player/PlayerQuests/PlayerQuest"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    bool isCompleted = Convert.ToBoolean(node.Attributes["IsCompleted"].Value);
                    PlayerQuest playerQuest = new PlayerQuest(World.QuestByID(id));
                    playerQuest.IsCompleted = isCompleted;
                    player.Quests.Add(playerQuest);
                }
                return player;
            }
            catch
            {
                // If there was an error with the XML data, return a default player object
                return Player.CreateDefaultPlayer();
            }
        }
        #endregion

        #region 方法
        #region  处里经验值属性的方法
        public void AddExperiencePoints(int experiencePointsToAdd)
        {
            ExperiencePoints += experiencePointsToAdd;
            MaximumHitPoints = (Level * 10);
        }
        #endregion

        //lambda表达式中的ii就是foreach中的ii，右面是if条件中的条件
        //左面是遍历列表的变量的声明，右面是条件表达式
        //lambda表达式后面还可以再加条件，可以同时再判断元素中的成员
        //还可以再简化，就有点复杂了，以后再说
        //可以通过SingleOrDefault返回列表的一个元素，需要检查是否为null，只能有一个匹配的元素返回

        #region  从UI的长代码中，重构出来，判断是否有进入一个位置所需要的物品
        public bool HasRequiredItemToEnterThisLocation(Location location)
        {
            if (location.ItemRequiredToEnter == null)
            {
                return true;// There is no required item for this location,  so return "true"
            }

            // See if the player has the required item in their inventory         
            return Inventory.Any(ii => ii.Details.ID == location.ItemRequiredToEnter.ID);//// return false;代替forreach后可以返回false，不需要了
        }
        #endregion

        #region 从UI的长代码中，重构出来，判断玩家有没有这个关卡及这个关卡是否完成
        public bool HasThisQuest(Quest quest)
        {
            return Quests.Any(pq => pq.Details.ID == quest.ID);
        }

        public bool CompletedThisQuest(Quest quest)
        {
            return Quests.Any(pq => pq.Details.ID == quest.ID && pq.IsCompleted);
        }
        #endregion

        #region 从UI的长代码中，重构出来，判断玩家是否有所有的关卡需要的物品
        public bool HasAllQuestCompletionItems(Quest quest)
        {
            // See if the player has all the items needed to complete  the quest here
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                // Check each item in the player's inventory, to see if they have it, and enough of it            
                if (!Inventory.Any(ii => ii.Details.ID == qci.Details.ID && ii.Quantity >= qci.Quantity)) //if (!Inventory.Exists(ii => ii.Details.ID == qci.Details.ID && ii.Quantity >= qci.Quantity))//由于更改了属性的数据类型，对应的方法不能使用了用Any替代
                {
                    return false;// The player does not have any of this quest completion item in their inventory
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
                InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == qci.Details.ID);

                if (item != null)
                {
                    RemoveItemFromInventory(item.Details, qci.Quantity);// Subtract the quantity from the player's inventory that was needed to complete the quest
                }
            }
        }
        #endregion

        #region 从UI的长代码中，重构出来，完成关卡的奖励物品，并抛出清单变化事件
        public void AddItemToInventory(Item itemToAdd, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToAdd.ID);

            if (item == null)
            {
                Inventory.Add(new InventoryItem(itemToAdd, quantity));// They didn't have the item, so add it to their inventory, with  quantity parameter   
            }
            else
            {
                item.Quantity += quantity;// They have the item in their inventory, so increase the quantity by quantity parameter 
            }
            RaiseInventoryChangedEvent(itemToAdd);
        }
        #endregion

        #region 从UI的长代码中，重构出来，标记完成的关卡
        public void MarkQuestCompleted(Quest quest)
        {
            // Find the quest in the player's quest list
            foreach (PlayerQuest pq in Quests)
            {
                PlayerQuest playerQuest = Quests.SingleOrDefault(pq2 => pq2.Details.ID == quest.ID);// Find the quest in the player's quest list
                if (playerQuest != null)
                {
                    playerQuest.IsCompleted = true;// Mark it as completed
                }
            }
        }
        #endregion

        #region 把玩家信息转化为XML
        public string ToXmlString()
        {
            //创建一个XML对象
            XmlDocument playerData = new XmlDocument();

            //创建CreateElement及插入.AppendChild节点及子节点
            //最外层的节点，相当于整个文件的子节点
            //CreateTextNode赋值，相当于需要值的节点的子节点
            //CreateAttribute创建属性，通过 属性.Value赋值，通过 .Attributes.Append插入
            //通过return playerData.InnerXml返回XML字符串

            // Create the top-level XML node
            XmlNode player = playerData.CreateElement("Player");
            playerData.AppendChild(player);

            // Create the "Stats" child node to hold the other player statistics nodes
            XmlNode stats = playerData.CreateElement("Stats");
            player.AppendChild(stats);

            // Create the child nodes for the "Stats" node
            XmlNode currentHitPoints = playerData.CreateElement("CurrentHitPoints");
            currentHitPoints.AppendChild(playerData.CreateTextNode(
               this.CurrentHitPoints.ToString()));
            stats.AppendChild(currentHitPoints);

            XmlNode maximumHitPoints = playerData.CreateElement("MaximumHitPoints");
            maximumHitPoints.AppendChild(playerData.CreateTextNode(
               this.MaximumHitPoints.ToString()));
            stats.AppendChild(maximumHitPoints);

            XmlNode gold = playerData.CreateElement("Gold");
            gold.AppendChild(playerData.CreateTextNode(this.Gold.ToString()));
            stats.AppendChild(gold);

            XmlNode experiencePoints = playerData.CreateElement("ExperiencePoints");
            experiencePoints.AppendChild(playerData.CreateTextNode(
               this.ExperiencePoints.ToString()));
            stats.AppendChild(experiencePoints);

            XmlNode currentLocation = playerData.CreateElement("CurrentLocation");
            currentLocation.AppendChild(playerData.CreateTextNode(
               this.CurrentLocation.ID.ToString()));
            stats.AppendChild(currentLocation);

            //把武器保存到XML
            if (CurrentWeapon != null)
            {
                XmlNode currentWeapon =
                    playerData.CreateElement("CurrentWeapon");
                currentWeapon.AppendChild(
                    playerData.CreateTextNode(this.CurrentWeapon.ID.ToString()));
                stats.AppendChild(currentWeapon);
            }

            // Create the "InventoryItems" child node to hold each InventoryItem node
            XmlNode inventoryItems = playerData.CreateElement("InventoryItems");
            player.AppendChild(inventoryItems);

            // Create an "InventoryItem" node for each item in the player's inventory
            foreach (InventoryItem item in this.Inventory)
            {
                XmlNode inventoryItem = playerData.CreateElement("InventoryItem");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = item.Details.ID.ToString();
                inventoryItem.Attributes.Append(idAttribute);

                XmlAttribute quantityAttribute = playerData.CreateAttribute("Quantity");
                quantityAttribute.Value = item.Quantity.ToString();
                inventoryItem.Attributes.Append(quantityAttribute);

                inventoryItems.AppendChild(inventoryItem);
            }
            // Create the "PlayerQuests" child node to hold each PlayerQuest node
            XmlNode playerQuests = playerData.CreateElement("PlayerQuests");
            player.AppendChild(playerQuests);
            // Create a "PlayerQuest" node for each quest the player has acquired
            foreach (PlayerQuest quest in this.Quests)
            {
                XmlNode playerQuest = playerData.CreateElement("PlayerQuest");
                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = quest.Details.ID.ToString();
                playerQuest.Attributes.Append(idAttribute);
                XmlAttribute isCompletedAttribute = playerData.CreateAttribute("IsCompleted");
                isCompletedAttribute.Value = quest.IsCompleted.ToString();
                playerQuest.Attributes.Append(isCompletedAttribute);
                playerQuests.AppendChild(playerQuest);
            }
            return playerData.InnerXml; // The XML document, as a string, so we can save the data to disk
        }
        #endregion

        #region 从清单中移除物品并抛出清单变化事件
        public void RemoveItemFromInventory(Item itemToRemove, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(
                ii => ii.Details.ID == itemToRemove.ID);
            if (item == null)
            {
                // The item is not in the player's inventory, so ignore it.
                // We might want to raise an error for this situation
            }
            else
            {
                // They have the item in their inventory,  so decrease the quantity
                item.Quantity -= quantity;
                // Don't allow negative quantities.  We might want to raise an error for this situation
                if (item.Quantity < 0)
                {
                    item.Quantity = 0;
                }
                // If the quantity is zero, remove the item from the list
                if (item.Quantity == 0)
                {
                    Inventory.Remove(item);
                }
                // Notify the UI that the inventory has changed
                RaiseInventoryChangedEvent(itemToRemove);
            }
        }
        #endregion

        #region 抛出清单变化事件
        private void RaiseInventoryChangedEvent(Item item)
        {
            if (item is Weapon)
            {
                OnPropertyChanged("Weapons");
            }
            if (item is HealingPotion)
            {
                OnPropertyChanged("Potions");
            }
        }
        #endregion
        #endregion

    }
}

