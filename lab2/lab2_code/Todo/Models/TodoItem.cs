using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Todo.Models
{
    public class TodoItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public String Id
        {
            get; set;
        }

        private String title;
        public String Title
        {
            get { return title; }
            set {
                title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Title"));
            }
        }

        public String Description
        {
            get; set;
        }

        public bool Completed { get; set; }
        public DateTimeOffset Date
        {
            get; set;
        }

        public Nullable<bool> NullableCompleted
        {
            get { return Completed; }
            set {
                Completed = (bool)value;
                DataAccess.Instance.UpdateCompleted(Id, Completed);
            }
        }

        private BitmapImage icon;
        public BitmapImage Icon
        {
            get { return icon; }
            set
            {
                icon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Icon"));
            }
        }

        //获取绝对路径
        public String ImageUri
        {
            get
            {
                if (ImageType != "default") return "ms-appdata:///local/" + Id + ImageType;
                else return MainPage.defaultImage;
            }
        }
        public String ImageType { get; set; }

        /*
        private async void setIcon()
        {
            if (ImageType.Equals("default")) return;
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await storageFolder.GetFileAsync(EditItem.latestInstance.tempImage);

            await file.CopyAsync(storageFolder, Id + ImageType, NameCollisionOption.ReplaceExisting);
            icon = new BitmapImage(new Uri(ImageUri));
        }
        */

        public TodoItem(String title, String description, DateTimeOffset date, BitmapImage icon, String imageType = "default", bool completed = false, String id = "")
        {
            if (id == "") this.Id = Guid.NewGuid().ToString(); //生成id
            else this.Id = id;
            this.Title = title;
            this.Description = description;
            this.Completed = completed;
            this.Date = date;
            this.ImageType = imageType;
            if (icon == null) this.icon = new BitmapImage(new Uri(ImageUri));    //传过来的icon为空，说明是从数据库建立的，直接利用ImageUri读取图片即可
            else this.icon = icon;
        }

        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
