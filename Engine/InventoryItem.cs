using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Engine
{
    //继承要实现INotifyPropertyChanged接口
    public class InventoryItem : INotifyPropertyChanged
    {
        #region 属性
        //变为带变量的属性，设置变量时触发抛出事件的方法
        private Item _details;
        private int _quantity;

        public Item Details
        {
            get { return _details; }
            set
            {
                _details = value;
                OnPropertyChanged("Details");
            }
        }

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChanged("Quantity");
                OnPropertyChanged("Description");
            }
        }
        //只读属性，描述物品，注意当物品数量大于1时取名称复数
        public string Description
        {
            get
            {
                return Quantity > 1 ? Details.NamePlural : Details.Name;
            }
        }
        #endregion
        #region 构造函数
        public InventoryItem(Item details, int quantity)
        {
            Details = details;
            Quantity = quantity;
        }
        #endregion
        #region 事件及事件处理方法，INotifyPropertyChanged接口的实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
