using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Todo.Models;
using Todo.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;
using Todo.Services;
using Windows.Data.Xml.Dom;
using System.Text;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Todo
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public const String defaultImage = "ms-appx:///Assets/optionHeader.jpg";    //默认图像
        public static MainPage latestInstance;  //最新的实例
        public TodoItemViewModels ViewModels;
        private const String stateEdit = "Edit";
        private const String stateList = "List";
        private const String stateAllView = "All";
        private String state = stateAllView;

        public MainPage()
        {
            this.InitializeComponent();
            ViewModels = TodoItemViewModels.Instance;
            //ViewModels = ((App)App.Current).ViewModels;
            latestInstance = this;
            var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = Colors.ForestGreen;
            titleBar.ForegroundColor = Colors.White;
            titleBar.ButtonBackgroundColor = Colors.ForestGreen;
            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonHoverBackgroundColor = Colors.White;
            titleBar.ButtonHoverForegroundColor = Colors.ForestGreen;

            left.Navigate(typeof(ListItem));
            right.Navigate(typeof(EditItem));

            if (Window.Current.Bounds.Width < 800)
                VisualStateManager.GoToState(this, stateList, false);

            if (right.Visibility == Visibility.Collapsed) state = stateList;
            else if (left.Visibility == Visibility.Collapsed) state = stateEdit;

            this.SizeChanged += new SizeChangedEventHandler(Resize);
        }

        private void Resize(object sender, SizeChangedEventArgs e)
        {
            double neww = e.NewSize.Width;
            double oldw = e.PreviousSize.Width;
            if (neww >= 800) state = stateAllView;
            else
            {
                if (oldw >= 800) state = stateList;
            }
            VisualStateManager.GoToState(this, state, false);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            bool suspending = ((App)App.Current).isSuspending;
            if (suspending)
            {
                ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();

                //保存窗口大小和状态
                if (Window.Current.Bounds.Width >= 800) state = stateAllView;   
                composite["state"] = state; 

                //记录要序列化的数据
                int count = ViewModels.AllItems.Count;
                composite["count"] = count;
                composite["selected"] = -1;
                for(int i = 0; i < count; i++)
                {
                    if(ViewModels.SelectedItem != null && ViewModels.SelectedItem.Id == ViewModels.AllItems[i].Id)
                        composite["selected"] = i;
                    composite["id" + i] = ViewModels.AllItems[i].Id;
                    composite["title" + i] = ViewModels.AllItems[i].Title;
                    composite["description" + i] = ViewModels.AllItems[i].Description;
                    composite["completed" + i] = ViewModels.AllItems[i].Completed;
                    composite["date" + i] = ViewModels.AllItems[i].Date;
                    composite["imageType" + i] = ViewModels.AllItems[i].ImageType;
                }

                composite["editId"] = EditItem.latestInstance.Id;
                composite["editTitle"] = EditItem.latestInstance.Title;
                composite["editDescription"] = EditItem.latestInstance.Description;
                composite["editDate"] = EditItem.latestInstance.Date;
                
                ApplicationData.Current.LocalSettings.Values["newPage"] = composite;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.NavigationMode == NavigationMode.New)
            {
                ApplicationData.Current.LocalSettings.Values.Remove("newPage");
            }
            else
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("newPage"))
                {
                    var composite = ApplicationData.Current.LocalSettings.Values["newPage"] as ApplicationDataCompositeValue;
                    int count = (int)composite["count"];
                    int selected = (int)composite["selected"];
                    for(int i = 0; i < count; i++)
                    {
                        String id = (String)composite["id" + i];
                        String title = (String)composite["title" + i];
                        String description = (String)composite["description" + i];
                        bool completed = (bool)composite["completed" + i];
                        DateTimeOffset date = (DateTimeOffset)composite["date" + i];
                        String imageType = (String)composite["imageType" + i];
                        ViewModels.AddTodoItem(title, description, date, null, imageType, false, completed, id);
                    }
                    if (selected != -1) ViewModels.SelectedItem = ViewModels.AllItems[selected];
                    

                    String oldState = (String)composite["state"];
                    if (oldState == stateList) showItemList();
                    else if (oldState == stateEdit) showItemDetail();


                    EditItem.latestInstance.Id = (String)composite["editId"];
                    EditItem.latestInstance.Title = (String)composite["editTitle"];
                    EditItem.latestInstance.Description = (String)composite["editDescription"];
                    EditItem.latestInstance.Date = (DateTimeOffset)composite["editDate"];

                    //读取imagePicker应有的图片
                    if (ApplicationData.Current.LocalSettings.Values["imagepicker"] != null)
                    {
                        StorageFile file;
                        file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync((String)ApplicationData.Current.LocalSettings.Values["imagepicker"]);
                        IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                        BitmapImage bmp = new BitmapImage();
                        await bmp.SetSourceAsync(ir);
                        EditItem.latestInstance.setImage(bmp);
                        ApplicationData.Current.LocalSettings.Values["imagepicker"] = null;
                    }

                    ApplicationData.Current.LocalSettings.Values.Remove("newpage");
                }
            }
        }


        //点击底部的添加按钮触发
        private void add_Click(object sender, RoutedEventArgs e)
        {
            ViewModels.SelectedItem = null;
            showItemDetail();
        }

        //点击底部的删除按钮触发
        private void delete_Click(object sender, RoutedEventArgs e)
        {

            if(ViewModels.SelectedItem != null)
            {
                EditItem.latestInstance.delete();
                this.UpdatePrimaryTile();
            }
        }


        //显示详情/编辑界面
        public void showItemDetail()
        {
            EditItem.latestInstance.showItem();
            if (right.Visibility == Visibility.Collapsed)
            {
                VisualStateManager.GoToState(this, stateEdit, false);
                state = stateEdit;
            }
        }

        //显示列表界面
        public void showItemList()
        {
            if (left.Visibility == Visibility.Collapsed)
            {
                VisualStateManager.GoToState(this, stateList, false);
                state = stateList;
            }
        }

        public void UpdatePrimaryTile()
        {
            TileService.UpdatePrimaryTileByFormat();
        }

        private  void UpdatePrimaryTile(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TileService.UpdatePrimaryTileByFormat();
        }

        public List<TodoItem> TileList()
        {
            return ViewModels.AllItems.Reverse<TodoItem>().Take<TodoItem>(5).Reverse<TodoItem>().ToList<TodoItem>();
        }

    }
}
