using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Todo.Models;
using Todo.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Todo
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ListItem : Page
    {
        public static ListItem latestInstance;  //最新的实例

        public TodoItemViewModels ViewModels;

        public ListItem()
        {
            this.InitializeComponent();
            ViewModels = TodoItemViewModels.Instance;
            latestInstance = this;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested -= OnShareDataRequested;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested += OnShareDataRequested;
        }

        //列表项被点击时触发事件
        private void TodoItem_ItemClicked(object sender, ItemClickEventArgs e)
        {
            ViewModels.SelectedItem = (TodoItem)e.ClickedItem;
            //Frame root = Window.Current.Content as Frame;
            //root.Navigate(typeof(MainPage));
        }

        //共享
        private void Share_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem se = sender as MenuFlyoutItem;
            var dc = se.DataContext as TodoItem;
            EditItem.latestInstance.ViewModels.SelectedItem = dc;
            DataTransferManager.ShowShareUI();
        }

        public void OnShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var dp = args.Request.Data;
            var deferral = args.Request.GetDeferral();
            dp.Properties.Title = EditItem.latestInstance.ViewModels.SelectedItem.Title;
            dp.Properties.Description = EditItem.latestInstance.ViewModels.SelectedItem.Description;
            dp.SetText(dp.Properties.Description);
            dp.SetBitmap(RandomAccessStreamReference.CreateFromUri(new Uri(MainPage.defaultImage)));
            deferral.Complete();
        }


        private void search_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = DataAccess.Instance.Search(word.Text);
            EditItem.latestInstance.showMessageDialog(sb.ToString());
        }
    }
}
