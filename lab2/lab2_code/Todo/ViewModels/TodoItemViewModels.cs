using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Models;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Todo.ViewModels
{
    public class TodoItemViewModels
    {
        private ObservableCollection<Models.TodoItem> allItems = new ObservableCollection<Models.TodoItem>();
        private TodoItem selectedItem;
        public TodoItem SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                selectedItem = value;
                if (selectedItem != null) MainPage.latestInstance.showItemDetail();

            }
        }
        public ObservableCollection<Models.TodoItem> AllItems
        {
            get
            {
                return allItems;
            }
        }
        private static TodoItemViewModels instance;
        public static TodoItemViewModels Instance
        {
            get
            {
                if (instance == null) instance = new TodoItemViewModels();
                return instance;
            }
        }

        public TodoItemViewModels() {
            selectedItem = null;
        }

        //添加项
        public String AddTodoItem(String title, String description, DateTimeOffset date, BitmapImage icon, String imageType = "default", bool saveImage = false, bool completed = false, String id = "")
        {
            TodoItem item = new TodoItem(title, description, date, icon, imageType, completed, id);
            allItems.Add(item);
            if (saveImage) saveIcon(item);
            return item.Id;
        }

        //删除项
        public void RemoveTodoItem(String id)
        {
            for(int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i].Id == id)
                {
                    selectedItem = allItems[i];
                    break;
                }
            }
            

            allItems.Remove(selectedItem);

            selectedItem = null;

        }


        //更新项
        public void UpdateTodoItem(String id, String title, String description, DateTimeOffset date, BitmapImage icon, String imageType, bool saveImage = false)
        {
            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i].Id == id)
                {
                    selectedItem = allItems[i];
                    break;
                }
            }
            if(selectedItem != null)
            {
                selectedItem.Id = id;
                selectedItem.Title = title;
                selectedItem.Description = description;
                selectedItem.Date = date;
                selectedItem.ImageType = imageType;
                selectedItem.Icon = icon;
                if(saveImage) saveIcon(selectedItem);
            }


            selectedItem = null;
        }

        public async void saveIcon(TodoItem item)
        {
            if (item.ImageType.Equals("default")) return;
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await storageFolder.GetFileAsync(EditItem.latestInstance.tempImage);

            try
            {
                await file.CopyAsync(storageFolder, item.Id + item.ImageType, NameCollisionOption.ReplaceExisting);
            }
            catch(Exception ee)
            {

            }
        }
    }
}
