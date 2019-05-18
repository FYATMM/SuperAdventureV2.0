using Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        #region 属性
        private Player _player;

        ////private Monster _currentMonster;

        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";
        #endregion

        #region 构造函数
        public SuperAdventure()
        {
            InitializeComponent();

               ////////读取xml创建玩家
            //通过构造函数初始化属性，不用一个一个手写赋值了
            //_player = new Player(10, 10, 20, 0, 1);更改构造函数后也不需要实例化时传参了
            //////// _player = new Player(10, 10, 20, 0);

            /*
                move the player to their home. Since the MoveTo() function expects a location as the parameter, 
                we need to use the World.GetLocationByID() function to get the correct location. 
             */
            ////////MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            /*
               we add an item to the player's inventory – a rusty sword. They'll need something to fight with when they encounter their first monster.
             */
            ////////_player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));

            ////////读取xml创建玩家，判断文件存在不存在
            if (File.Exists(PLAYER_DATA_FILE_NAME))
            {
                _player = Player.CreatePlayerFromXmlString(
                    File.ReadAllText(PLAYER_DATA_FILE_NAME));
            }
            else
            {
                _player = Player.CreateDefaultPlayer();
            }
            _player.OnMessage += DisplayMessage;//监视玩家的message事件，并调用对用的显示消息方法
            //bind the labels to the properties
            lblHitPoints.DataBindings.Add("Text", _player, "CurrentHitPoints");
            lblGold.DataBindings.Add("Text", _player, "Gold");
            lblExperience.DataBindings.Add("Text", _player, "ExperiencePoints");
            lblLevel.DataBindings.Add("Text", _player, "Level");

            dgvInventory.RowHeadersVisible = false;
            dgvInventory.AutoGenerateColumns = false;
            dgvInventory.DataSource = _player.Inventory;
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 197,
                DataPropertyName = "Description"
            });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Quantity",
                DataPropertyName = "Quantity"
            });

            dgvQuests.RowHeadersVisible = false;
            dgvQuests.AutoGenerateColumns = false;

            dgvQuests.DataSource = _player.Quests;

            dgvQuests.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 197,
                DataPropertyName = "Name"
            });

            dgvQuests.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Done?",
                DataPropertyName = "IsCompleted"
            });

            // set up the  comboboxes to bind to the new Player properties.
            cboWeapons.DataSource = _player.Weapons;
            cboWeapons.DisplayMember = "Name";
            cboWeapons.ValueMember = "Id";
            if (_player.CurrentWeapon != null)
            {
                cboWeapons.SelectedItem = _player.CurrentWeapon;
            }
            cboWeapons.SelectedIndexChanged += cboWeapons_SelectedIndexChanged;
            cboPotions.DataSource = _player.Potions;
            cboPotions.DisplayMember = "Name";
            cboPotions.ValueMember = "Id";
            _player.PropertyChanged += PlayerOnPropertyChanged;

            //????//MoveTo(_player.CurrentLocation);??
            _player.MoveHome();
            //_player.CurrentHitPoints = 10;
            //_player.MaximumHitPoints = 10;
            //_player.Gold = 20;
            //_player.ExperiencePoints = 0;
            //_player.Level = 1;

            //lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            //lblGold.Text = _player.Gold.ToString();
            //lblExperience.Text = _player.ExperiencePoints.ToString();
            //lblLevel.Text = _player.Level.ToString();
            //通过方法更新所有状态，同时也保证了属性调用的时候，根据计算自动更新
            ////////UpdatePlayerStats(); // We don't need to call that method anymore. The databinding  will automatically do that for us.
        }
        #endregion

        #region 方法
        #region 移动
        private void btnNorth_Click(object sender, EventArgs e)
        {
            _player.MoveNorth();
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            _player.MoveEast();
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            _player.MoveSouth();
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            _player.MoveWest();
        }
        #endregion

        #region  Update inventory list in UI 更新界面的冒险列表
        private void UpdateInventoryListInUI()
        {
            dgvInventory.RowHeadersVisible = false;

            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";

            dgvInventory.Rows.Clear();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Quantity > 0)
                {
                    dgvInventory.Rows.Add(new[] {
                inventoryItem.Details.Name,
                inventoryItem.Quantity.ToString() });
                }
            }
        }
        #endregion

        #region Update quest list in UI 更新界面的关卡列表
        private void UpdateQuestListInUI()
        {
            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";

            dgvQuests.Rows.Clear();

            foreach (PlayerQuest playerQuest in _player.Quests)
            {
                dgvQuests.Rows.Add(new[] { playerQuest.Details.Name, playerQuest.IsCompleted.ToString() });
            }
        }
        #endregion

        #region Update weapon list in UI 更新界面的武器列表
        private void UpdateWeaponListInUI()
        {
            List<Weapon> weapons = new List<Weapon>();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is Weapon)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        weapons.Add((Weapon)inventoryItem.Details);
                    }
                }
            }

            if (weapons.Count == 0)
            {
                // The player doesn't have any weapons, so hide the weapon combobox and "Use" button
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            }
            else
            //{
            //    cboWeapons.DataSource = weapons;
            //    cboWeapons.DisplayMember = "Name";
            //    cboWeapons.ValueMember = "ID";

            //    cboWeapons.SelectedIndex = 0;
            //}
            {
                //手动断开/连接一个事件到对应的事件处理方法，事件通过-=、+=来断开或连接
                //只希望手动更改后，才处理，而不希望第一次设置的时候
                //After the DataSource is set, we add the event handler function back to the SelectedIndex-Changed event (with the  +=  operator). This way, the function will run when the player changes the value.
                cboWeapons.SelectedIndexChanged -= cboWeapons_SelectedIndexChanged;
                cboWeapons.DataSource = weapons;
                cboWeapons.SelectedIndexChanged += cboWeapons_SelectedIndexChanged;

                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";

                if (_player.CurrentWeapon != null)
                {
                    cboWeapons.SelectedItem = _player.CurrentWeapon;
                }
                else
                {
                    cboWeapons.SelectedIndex = 0;
                }
            }
        }
        #endregion

        #region 下拉框手动选择改变后处理方法
        private void cboWeapons_SelectedIndexChanged(object sender, EventArgs e)
        {
            _player.CurrentWeapon = (Weapon)cboWeapons.SelectedItem;
        }
        #endregion

        #region Update potion list in UI 更新界面的解药列表
        private void UpdatePotionListInUI()
        {
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is HealingPotion)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        healingPotions.Add(
                            (HealingPotion)inventoryItem.Details);
                    }
                }
            }

            if (healingPotions.Count == 0)
            {
                // The player doesn't have any potions, so hide the potion combobox and "Use" button
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }
            else
            {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";

                cboPotions.SelectedIndex = 0;
            }
        }
        #endregion

        #region 更新玩家所有状态
        private void UpdatePlayerStats()
        {
            // Refresh player information and inventory controls
            ////lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
        }
        #endregion

        #region ////自动滚动到Message最底部，查看最新消息
        ////private void ScrollToBottomOfMessages()
        ////{
        ////    rtbMessages.SelectionStart = rtbMessages.Text.Length;
        ////    rtbMessages.ScrollToCaret();
        ////}
        #endregion

        #region 武器和解药属性变化时的事件处理方法，并隐藏对应按键
        /*
             The propertyChangedEventArgs.PropertyName tells us which property was changed on the Player object. This value comes from the Player.RaiseInventoryChangedEvent function, 
             where it says OnPropertyChanged("Weapons"), or OnPropertyChanged("Potions"). We rebind the combobox to the Weapons (or Potions) DataSource property, to refresh it 
            with the current items. Then, we see if the lists are empty, by using !_player.Weapons.Any(). Remember that Any() tells us if there are any items in the list: true if there are, false if there 
            are not. So, we are saying, "if there are not any items in the list, set the visibility of the combobox  and 'Use' button to false (not visible)". This is in case we use our last potion in the middle of a fight. 
            Since the player's Potions property will be an empty list, it will hide the potions combobox and Use button.
             */

        private void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Weapons")
            {
                cboWeapons.DataSource = _player.Weapons;
                if (!_player.Weapons.Any())
                {
                    cboWeapons.Visible = false;
                    btnUseWeapon.Visible = false;
                }
            }
            if (propertyChangedEventArgs.PropertyName == "Potions")
            {
                cboPotions.DataSource = _player.Potions;
                if (!_player.Potions.Any())
                {
                    cboPotions.Visible = false;
                    btnUsePotion.Visible = false;
                }
            }
            if (propertyChangedEventArgs.PropertyName == "CurrentLocation")
            {
                // Show/hide available movement buttons
                btnNorth.Visible = (_player.CurrentLocation.LocationToNorth != null);
                btnEast.Visible = ( _player.CurrentLocation.LocationToEast != null);
                btnSouth.Visible = ( _player.CurrentLocation.LocationToSouth != null);
                btnWest.Visible = ( _player.CurrentLocation.LocationToWest != null);
                // Display current location name and description
                rtbLocation.Text = _player.CurrentLocation.Name +  Environment.NewLine;
                rtbLocation.Text += _player.CurrentLocation.Description + Environment.NewLine;
                if (_player.CurrentLocation.MonsterLivingHere == null)
                {
                    cboWeapons.Visible = false;
                    cboPotions.Visible = false;
                    btnUseWeapon.Visible = false;
                    btnUsePotion.Visible = false;
                }
                else
                {
                    cboWeapons.Visible = _player.Weapons.Any();
                    cboPotions.Visible = _player.Potions.Any();
                    btnUseWeapon.Visible = _player.Weapons.Any();
                    btnUsePotion.Visible = _player.Potions.Any();
                }
            }
        }
        #endregion

        #region 显示消息方法，对应玩家的消息事件；同时把之前的ScrollToBottom方法放进来了
        private void DisplayMessage(object sender, MessageEventArgs messageEventArgs)
        {
            rtbMessages.Text +=
                messageEventArgs.Message + Environment.NewLine;
            if (messageEventArgs.AddExtraNewLine)
            {
                rtbMessages.Text += Environment.NewLine;
            }
            //scroll to bottom of message 自动滚动到Message最底部，查看最新消息
            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }
        #endregion

        #endregion

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            /*
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;// Get the currently selected weapon from the cboWeapons ComboBox
            int damageToMonster = RandomNumberGenerator.NumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);// Determine the amount of damage to do to the monster
            _currentMonster.CurrentHitPoints -= damageToMonster;// Apply the damage to the monster's CurrentHitPoints
            rtbMessages.Text += "You hit the " + _currentMonster.Name + " for " + damageToMonster.ToString() + " points." + Environment.NewLine;// Display message
            ScrollToBottomOfMessages();

            // Check if the monster is dead
            if (_currentMonster.CurrentHitPoints <= 0) // Monster is dead
            {
                rtbMessages.Text += Environment.NewLine;
                ScrollToBottomOfMessages();
                rtbMessages.Text += "You defeated the " + _currentMonster.Name + Environment.NewLine;
                ScrollToBottomOfMessages();
                _player.AddExperiencePoints(_currentMonster.RewardExperiencePoints); ////////_player.ExperiencePoints += _currentMonster.RewardExperiencePoints;// Give player experience points for killing the monster
                rtbMessages.Text += "You receive " + _currentMonster.RewardExperiencePoints.ToString() + " experience points" + Environment.NewLine;
                ScrollToBottomOfMessages();
                // Give player gold for killing the monster 
                _player.Gold += _currentMonster.RewardGold;
                rtbMessages.Text += "You receive " + _currentMonster.RewardGold.ToString() + " gold" + Environment.NewLine;
                ScrollToBottomOfMessages();
                // Get random loot items from the monster
                List<InventoryItem> lootedItems = new List<InventoryItem>();
                // Add items to the lootedItems list, comparing a random number to the drop percentage
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
                    _player.AddItemToInventory(inventoryItem.Details);
                    if (inventoryItem.Quantity == 1)
                    {
                        rtbMessages.Text += "You loot " + inventoryItem.Quantity.ToString() + " " + inventoryItem.Details.Name + Environment.NewLine;
                        ScrollToBottomOfMessages();
                    }
                    else
                    {
                        rtbMessages.Text += "You loot " + inventoryItem.Quantity.ToString() + " " + inventoryItem.Details.NamePlural + Environment.NewLine;
                        ScrollToBottomOfMessages();
                    }
                }

                #region 之前需要的更新方法的调用 及 标签text属性的手动更新，数据绑定后不需要了
                // Refresh player information and inventory controls
                //lblHitPoints.Text = _player.CurrentHitPoints.ToString();
                //lblGold.Text = _player.Gold.ToString();
                //lblExperience.Text = _player.ExperiencePoints.ToString();
                //lblLevel.Text = _player.Level.ToString();

                ////UpdatePlayerStats();
                ////UpdateInventoryListInUI();
                ////UpdateWeaponListInUI();
                ////UpdatePotionListInUI();
                #endregion

                rtbMessages.Text += Environment.NewLine;// Add a blank line to the messages box, just for appearance.
                ScrollToBottomOfMessages();
                MoveTo(_player.CurrentLocation);// Move player to current location (to heal player and create a new monster to fight)
            }
            else // Monster is still alive
            {
                int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);// Determine the amount of damage the monster does to the player                
                rtbMessages.Text += "The " + _currentMonster.Name + " did " + damageToPlayer.ToString() + " points of damage." + Environment.NewLine;// Display message
                ScrollToBottomOfMessages();
                _player.CurrentHitPoints -= damageToPlayer;// Subtract damage from player

                ////lblHitPoints.Text = _player.CurrentHitPoints.ToString();// Refresh player data in UI
                if (_player.CurrentHitPoints <= 0)
                {
                    rtbMessages.Text += "The " + _currentMonster.Name + " killed you." + Environment.NewLine;// Display message
                    ScrollToBottomOfMessages();
                    MoveTo(World.LocationByID(World.LOCATION_ID_HOME));// Move player to "Home"
                }
            }
            ScrollToBottomOfMessages();
            */
            // Get the currently selected weapon from the cboWeapons ComboBox
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;
            _player.UseWeapon(currentWeapon);
        }
        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            /*
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;// Get the currently selected potion from the combobox
            _player.CurrentHitPoints = (_player.CurrentHitPoints + potion.AmountToHeal);// Add healing amount to the player's current hit points

            // CurrentHitPoints cannot exceed player's MaximumHitPoints
            if (_player.CurrentHitPoints > _player.MaximumHitPoints)
            {
                _player.CurrentHitPoints = _player.MaximumHitPoints;
            }

            _player.RemoveItemFromInventory(potion, 1);// Remove the potion from the player's inventory           
            rtbMessages.Text += "You drink a " + potion.Name + Environment.NewLine; // Display message
            // Monster gets their turn to attack            
            int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);// Determine the amount of damage the monster does to the player            
            rtbMessages.Text += "The " + _currentMonster.Name + " did " + damageToPlayer.ToString() + " points of damage." + Environment.NewLine;// Display message           
            _player.CurrentHitPoints -= damageToPlayer; // Subtract damage from player
            if (_player.CurrentHitPoints <= 0)
            {
                rtbMessages.Text += "The " + _currentMonster.Name + " killed you." + Environment.NewLine;// Display message                
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));// Move player to "Home"
            }
            /*
            //Refresh player data in UI
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            UpdateInventoryListInUI();
            UpdatePotionListInUI();
             */
            // Get the currently selected potion from the combobox
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;
            _player.UsePotion(potion);


        }
        private void SuperAdventure_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, _player.ToXmlString());
        }
    }
}


