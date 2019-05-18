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
        private Monster _currentMonster;
        private Location _currentLocation;

        public event EventHandler<MessageEventArgs> OnMessage;

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

        public Location CurrentLocation
        {
            get{ return _currentLocation; }
            set
            {
                _currentLocation = value;
                OnPropertyChanged("CurrentLocation");
            }
        }

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
        #region 移动后
        private void MoveTo(Location newLocation)
        {
            //修复类级变量，之前在界面代码中，现在移动到Player类里，关于Player类的变量都改为内部用
            #region  重构后的是否有进入当前位置的物品，的调用
            // Does the location have any required items
            ////if (!_player.HasRequiredItemToEnterThisLocation(newLocation))// We didn't find the required item in their inventory, so display a message and stop trying to move
            if (!HasRequiredItemToEnterThisLocation(newLocation))
            {
                // Environment.NewLine摘要: 获取为此环境定义的换行字符串。对于非 Unix 平台为包含“\r\n”的字符串，对于 Unix 平台则为包含“\n”的字符串。     
                ////rtbMessages.Text +="You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location." + Environment.NewLine;
                ////ScrollToBottomOfMessages();
                RaiseMessage("You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location."); //Here, we take the text in the rtbMessages RichTextBox, and add our new message to the end of it. That way, the player can still see the old messages.If we used the = sign instead, it would replace the existing Text value with our new message.

                return;// we don't want to do the rest of the function, which would actually move them to the location.
            }
            #endregion

            CurrentLocation = newLocation;////_player.CurrentLocation = newLocation;// Update the player's current location
            //????// Show/hide available movement buttons
            ////btnNorth.Visible = (newLocation.LocationToNorth != null);
            ////btnEast.Visible = (newLocation.LocationToEast != null);
            ////btnSouth.Visible = (newLocation.LocationToSouth != null);
            ////btnWest.Visible = (newLocation.LocationToWest != null);

            // Display current location name and description
            ////rtbLocation.Text = newLocation.Name + Environment.NewLine;
            ////rtbLocation.Text += newLocation.Description + Environment.NewLine;
            // Completely heal the player
            CurrentHitPoints = MaximumHitPoints;////_player.CurrentHitPoints = _player.MaximumHitPoints;
            // Update Hit Points in UI
            //// lblHitPoints.Text = _player.CurrentHitPoints.ToString();

            // Does the location have a quest?
            if (newLocation.QuestAvailableHere != null)
            {
                #region 重构后的，是否有这个关卡，这个关卡是否完成，的调用
                // See if the player already has the quest, and if they've completed it
                ////bool playerAlreadyHasQuest = _player.HasThisQuest(newLocation.QuestAvailableHere);
                bool playerAlreadyHasQuest = HasThisQuest(newLocation.QuestAvailableHere);
                ////bool playerAlreadyCompletedQuest = _player.CompletedThisQuest(newLocation.QuestAvailableHere);
                bool playerAlreadyCompletedQuest = CompletedThisQuest(newLocation.QuestAvailableHere);
                #endregion
                // See if the player already has the quest
                if (playerAlreadyHasQuest)
                {
                    // If the player has not completed the quest yet
                    if (!playerAlreadyCompletedQuest)
                    {
                        ////bool playerHasAllItemsToCompleteQuest = _player.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);// See if the player has all the items needed to complete the quest重构后的，判断玩家是否有完成相应关卡的所有物品
                        bool playerHasAllItemsToCompleteQuest = HasAllQuestCompletionItems(newLocation.QuestAvailableHere);
                        // The player has all items required to complete the quest
                        if (playerHasAllItemsToCompleteQuest)
                        {
                            // Display message
                            ////rtbMessages.Text += Environment.NewLine;
                            ////rtbMessages.Text += "You complete the " + newLocation.QuestAvailableHere.Name + " quest." + Environment.NewLine;
                            ////ScrollToBottomOfMessages();
                            RaiseMessage("");
                            RaiseMessage("You complete the " + newLocation.QuestAvailableHere.Name + " quest." );

                            ////_player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);// Remove quest items from inventory 重构后的，移除完成关卡用掉的物品
                            RemoveQuestCompletionItems(newLocation.QuestAvailableHere);

                            // Give quest rewards
                            ////rtbMessages.Text += "You receive: " + Environment.NewLine;
                            ////rtbMessages.Text += newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() + " experience points" + Environment.NewLine;
                            ////rtbMessages.Text += newLocation.QuestAvailableHere.RewardGold.ToString() + " gold" + Environment.NewLine;
                            ////rtbMessages.Text += newLocation.QuestAvailableHere.RewardItem.Name + Environment.NewLine;
                            ////rtbMessages.Text += Environment.NewLine;
                            ////ScrollToBottomOfMessages();
                            RaiseMessage("You receive: ");
                            RaiseMessage(newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() + " experience points");
                            RaiseMessage(newLocation.QuestAvailableHere.RewardGold.ToString() + " gold");
                            RaiseMessage(newLocation.QuestAvailableHere.RewardItem.Name);
                            RaiseMessage("");

                            // 重构后的，完成关卡的奖励
                            AddExperiencePoints(newLocation.QuestAvailableHere.RewardExperiencePoints);////_player.AddExperiencePoints(newLocation.QuestAvailableHere.RewardExperiencePoints);
                            Gold += newLocation.QuestAvailableHere.RewardGold; ////_player.Gold += newLocation.QuestAvailableHere.RewardGold;
                            AddItemToInventory(newLocation.QuestAvailableHere.RewardItem); ////_player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);// Add the reward item to the player's inventory

                            MarkQuestCompleted(newLocation.QuestAvailableHere); ////_player.MarkQuestCompleted(newLocation.QuestAvailableHere);// Mark the quest as completed重构后的，标记完成的关卡
                        }
                    }
                }
                else// The player does not already have the quest
                {
                    ////rtbMessages.Text += "You receive the " + newLocation.QuestAvailableHere.Name + " quest." + Environment.NewLine;// Display the messages
                    ////ScrollToBottomOfMessages();
                    ////rtbMessages.Text += newLocation.QuestAvailableHere.Description + Environment.NewLine;
                    ////ScrollToBottomOfMessages();
                    ////rtbMessages.Text += "To complete it, return with:" + Environment.NewLine;
                    ////ScrollToBottomOfMessages();
                    RaiseMessage("You receive the " + newLocation.QuestAvailableHere.Name + " quest.");
                    RaiseMessage(newLocation.QuestAvailableHere.Description);
                    RaiseMessage("To complete it, return with:");

                    foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if (qci.Quantity == 1)
                        {
                            ////rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.Name + Environment.NewLine;
                            ////ScrollToBottomOfMessages();
                            RaiseMessage(qci.Quantity.ToString() + " " + qci.Details.Name);
                        }
                        else
                        {
                            ////rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.NamePlural + Environment.NewLine;
                            ////ScrollToBottomOfMessages();
                            RaiseMessage(qci.Quantity.ToString() + " " + qci.Details.NamePlural);
                        }
                    }
                    ////rtbMessages.Text += Environment.NewLine;
                    ////ScrollToBottomOfMessages();
                    RaiseMessage("");

                    Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere)); ////_player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere)); // Add the quest to the player's quest list
                }
            }
            // Does the location have a monster?
            if (newLocation.MonsterLivingHere != null)
            {
                ////rtbMessages.Text += "You see a " + newLocation.MonsterLivingHere.Name + Environment.NewLine;
                ////ScrollToBottomOfMessages();
                RaiseMessage("You see a " + newLocation.MonsterLivingHere.Name);
                // Make a new monster, using the values from the standard monster in the World.Monster list
                Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);
                _currentMonster = new Monster(standardMonster.ID, standardMonster.Name,
                    standardMonster.MaximumDamage, standardMonster.RewardExperiencePoints,
                        standardMonster.RewardGold, standardMonster.CurrentHitPoints,
                            standardMonster.MaximumHitPoints);

                foreach (LootItem lootItem in standardMonster.LootTable)
                {
                    _currentMonster.LootTable.Add(lootItem);
                }
                ////cboWeapons.Visible = true;
                ////cboPotions.Visible = true;
                ////btnUseWeapon.Visible = true;
                ////btnUsePotion.Visible = true;
                ////cboWeapons.Visible = _player.Weapons.Any();
                ////cboPotions.Visible = _player.Potions.Any();
                ////btnUseWeapon.Visible = _player.Weapons.Any();
                ////btnUsePotion.Visible = _player.Potions.Any();
            }
            else
            {
                _currentMonster = null;
                ////cboWeapons.Visible = false;
                ////cboPotions.Visible = false;
                ////btnUseWeapon.Visible = false;
                ////btnUsePotion.Visible = false;
            }

            //// 重构后的跟新界面信息
            //// UpdateInventoryListInUI();
            ////UpdateQuestListInUI();
            ////UpdateWeaponListInUI();
            ////UpdatePotionListInUI();
            ////UpdatePlayerStats();
        }

        public void MoveHome()
        {
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
        }

        public void MoveNorth()
        {
            if (CurrentLocation.LocationToNorth != null)
            {
                MoveTo(CurrentLocation.LocationToNorth);
            }
        }

        public void MoveEast()
        {
            if (CurrentLocation.LocationToEast != null)
            {
                MoveTo(CurrentLocation.LocationToEast);
            }
        }

        public void MoveSouth()
        {
            if (CurrentLocation.LocationToSouth != null)
            {
                MoveTo(CurrentLocation.LocationToSouth);
            }
        }

        public void MoveWest()
        {
            if (CurrentLocation.LocationToWest != null)
            {
                MoveTo(CurrentLocation.LocationToWest);
            }
        }
        #endregion

        public void UseWeapon(Weapon weapon)
        {
            // Determine the amount of damage to do to the monster
            int damageToMonster = RandomNumberGenerator.NumberBetween(
                weapon.MinimumDamage, weapon.MaximumDamage);
            // Apply the damage to the monster's CurrentHitPoints
            _currentMonster.CurrentHitPoints -= damageToMonster;
            // Display message
            RaiseMessage("You hit the " + _currentMonster.Name +
                " for " + damageToMonster + " points.");
            // Check if the monster is dead
            if (_currentMonster.CurrentHitPoints <= 0)
            {
                // Monster is dead
                RaiseMessage("");
                RaiseMessage("You defeated the " + _currentMonster.Name);
                // Give player experience points for killing the monster
                AddExperiencePoints(_currentMonster.RewardExperiencePoints);
                RaiseMessage("You receive " + _currentMonster.RewardExperiencePoints +
                    " experience points");
                // Give player gold for killing the monster 
                Gold += _currentMonster.RewardGold;
                RaiseMessage("You receive " + _currentMonster.RewardGold + " gold");
                // Get random loot items from the monster
                List<InventoryItem> lootedItems = new List<InventoryItem>();
                // Add items to the lootedItems list, comparing a random number  to the drop percentage
                foreach (LootItem lootItem in _currentMonster.LootTable)
                {
                    if (RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage)
                    {
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }
                // If no items were randomly selected, then add the default loot item(s).
                if (lootedItems.Count == 0)
                {
                    foreach (LootItem lootItem in _currentMonster.LootTable)
                    {
                        if (lootItem.IsDefaultItem)
                        {
                            lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                        }
                    }
                }
                // Add the looted items to the player's inventory
                foreach (InventoryItem inventoryItem in lootedItems)
                {
                    AddItemToInventory(inventoryItem.Details);
                    if (inventoryItem.Quantity == 1)
                    {
                        RaiseMessage("You loot " +
                            inventoryItem.Quantity + " " + inventoryItem.Details.Name);
                    }
                    else
                    {
                        RaiseMessage("You loot " + inventoryItem.Quantity +
                            " " + inventoryItem.Details.NamePlural);
                    }
                }
                // Add a blank line to the messages box, just for appearance.
                RaiseMessage("");
                // Move player to current location (to heal player and create a new monster to fight)
      MoveTo(CurrentLocation);
            }
            else
            {
                // Monster is still alive
                // Determine the amount of damage the monster does to the player
                int damageToPlayer = RandomNumberGenerator.NumberBetween(
                    0, _currentMonster.MaximumDamage);
                // Display message
                RaiseMessage("The " + _currentMonster.Name + " did " +
                    damageToPlayer + " points of damage.");
                // Subtract damage from player
                CurrentHitPoints -= damageToPlayer;
                if (CurrentHitPoints <= 0)
                {
                    // Display message
                    RaiseMessage("The " + _currentMonster.Name + " killed you.");
                    // Move player to "Home"
                    MoveHome();
                }
            }
        }

        public void UsePotion(HealingPotion potion)
        {
            // Add healing amount to the player's current hit points
            CurrentHitPoints = (CurrentHitPoints + potion.AmountToHeal);
            // CurrentHitPoints cannot exceed player's MaximumHitPoints
            if (CurrentHitPoints > MaximumHitPoints)
            {
                CurrentHitPoints = MaximumHitPoints;
            }
            // Remove the potion from the player's inventory
            RemoveItemFromInventory(potion, 1);
            // Display message
            RaiseMessage("You drink a " + potion.Name);
            // Monster gets their turn to attack
            // Determine the amount of damage the monster does to the player
            int damageToPlayer = RandomNumberGenerator.NumberBetween(
               0, _currentMonster.MaximumDamage);
            // Display message
            RaiseMessage("The " + _currentMonster.Name +
               " did " + damageToPlayer + " points of damage.");
            // Subtract damage from player
            CurrentHitPoints -= damageToPlayer;
            if (CurrentHitPoints <= 0)
            {
                // Display message
                RaiseMessage("The " + _currentMonster.Name + " killed you.");
                // Move player to "Home"
                MoveHome();
            }
        }



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

        #region 事件
        ////public event EventHandler<MessageEventArgs> OnMessage;

        private void RaiseMessage(string message, bool addExtraNewLine = false)
        {
            if (OnMessage != null)
            {
                OnMessage(this, new MessageEventArgs(message, addExtraNewLine));
            }
        }
        #endregion

    }
}

