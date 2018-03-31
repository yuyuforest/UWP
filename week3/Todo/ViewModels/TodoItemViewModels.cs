using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Models;
using Windows.UI.Xaml.Media.Imaging;

namespace Todo.ViewModels
{
    public class TodoItemViewModels
    {
        private ObservableCollection<Models.TodoItem> allItems = new ObservableCollection<Models.TodoItem>();
        public TodoItem selectedItem = null;
        public ObservableCollection<Models.TodoItem> AllItems
        {
            get
            {
                return this.allItems;
            }
        }

        public void AddTodoItem(string title, string description, DateTimeOffset date, BitmapImage icon)
        {
            this.allItems.Add(new Models.TodoItem(title, description, date, icon));
        }

        public void RemoveTodoItem(string id)
        {
            for(int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i].Id == id)
                {
                    selectedItem = allItems[i];
                    break;
                }
            }
            this.allItems.Remove(selectedItem);

            this.selectedItem = null;
        }

        public void UpdateTodoItem(string id, string title, string description, DateTimeOffset date, BitmapImage icon)
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
                selectedItem.Icon = icon;
            }

            this.selectedItem = null;
        }
    }
}
