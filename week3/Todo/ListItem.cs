using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo
{
    class ListItem : INotifyPropertyChanged
    {
        private string id;

        public event PropertyChangedEventHandler PropertyChanged;

        public string title
        {
            get; set;
        }
        public string description
        {
            get; set;
        }
        public bool completed
        {
            get; set;
        }
        public int year
        {
            get; set;
        }
        public int month
        {
            get; set;
        }
        public int day
        {
            get; set;
        }

        public ListItem(string title, string description)
        {
            this.id = Guid.NewGuid().ToString(); //生成id
            this.title = title;
            this.description = description;
            this.completed = false;  //默认为未完成

            //日期初始化？
        }
    }
}
