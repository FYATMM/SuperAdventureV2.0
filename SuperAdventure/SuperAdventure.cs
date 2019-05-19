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
            ////////读取xml创建玩家，判断文件存在不存在
            if (File.Exists(PLAYER_DATA_FILE_NAME))
            {
                _player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
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

            _player.MoveHome();//MoveTo(_player.CurrentLocation);
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

        #region 下拉框手动选择改变后处理方法
        private void cboWeapons_SelectedIndexChanged(object sender, EventArgs e)
        {
            _player.CurrentWeapon = (Weapon)cboWeapons.SelectedItem;
        }
        #endregion

        #region 武器和解药和位置属性变化时的事件处理方法，并隐藏对应按键
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

                btnTrade.Visible =(_player.CurrentLocation.VendorWorkingHere != null);
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
            // Get the currently selected weapon from the cboWeapons ComboBox
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;
            _player.UseWeapon(currentWeapon);
        }
        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;
            _player.UsePotion(potion);
        }
        //自己增加按键
        private void btnTrade_Click(object sender, EventArgs e)
        {
            TradingScreen tradingScreen = new TradingScreen(_player);
            tradingScreen.StartPosition = FormStartPosition.CenterParent;
            tradingScreen.ShowDialog(this);
        }
        private void SuperAdventure_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, _player.ToXmlString());
        }
    }
}


