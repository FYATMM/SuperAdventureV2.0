using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

//增加一个类，提取player和monster的共同点，生物的共同属性
namespace Engine
{
    public class LivingCreature : INotifyPropertyChanged
    {
        //现在是一个自动属性，没有地方放代码处理设置属性时调用事件处理方法的地方，要改为带变量的属性
        ////public int CurrentHitPoints { get; set; }
        private int _currentHitPoints;
        public int CurrentHitPoints
        {
            get { return _currentHitPoints; }
            set
            {
                _currentHitPoints = value;
                OnPropertyChanged("CurrentHitPoints");
            }
        }
        public int MaximumHitPoints { get; set; }

        public LivingCreature(int currentHitPoints, int maximumHitPoints)
        {
            CurrentHitPoints = currentHitPoints;
            MaximumHitPoints = maximumHitPoints;
        }

        #region 事件
        //UI订阅这个事件,接口的实现
        public event PropertyChangedEventHandler PropertyChanged;
        //检查是否有订阅，没有订阅为null，不是null就是订阅了，当属性值变化时，抛出事件
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
