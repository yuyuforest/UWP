using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Todo.Models
{
    public class TodoItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public string Id
        {
            get; set;
        }
        public string Title
        {
            get; set;
        }
        public string Description
        {
            get; set;
        }
        public bool Completed
        {
            get; set;
        }
        public DateTimeOffset Date
        {
            get; set;
        }
        public Nullable<bool> NullableCompleted
        {
            get { return Completed; }
            set { Completed = (bool)value; }
        }
        public BitmapImage Icon
        {
            get; set;
        }

        public TodoItem(string title, string description, DateTimeOffset date, BitmapImage icon)
        {
            this.Id = Guid.NewGuid().ToString(); //生成id
            this.Title = title;
            this.Description = description;
            this.Completed = false;  //默认为未完成
            this.Date = date;
            this.Icon = icon;
        }

        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
