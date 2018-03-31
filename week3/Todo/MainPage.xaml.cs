using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Todo.Models;
using Todo.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Todo
{

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //public static BitmapImage bi = "Assets/optionHeader.jpg";
        public const string defaultImage = "/Assets/optionHeader.jpg";
        public string currentImage = defaultImage;

        public TodoItemViewModels ViewModels;
        public MainPage()
        {
            this.InitializeComponent();
            ViewModels = new TodoItemViewModels();
            //Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = false;
            var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = Colors.ForestGreen;
            titleBar.ForegroundColor = Colors.White;
            titleBar.ButtonBackgroundColor = Colors.ForestGreen;
            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonHoverBackgroundColor = Colors.White;
            titleBar.ButtonHoverForegroundColor = Colors.ForestGreen;

            //listView.ItemTemplate = new DataTemplate();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter.Equals("")) return;
            ViewModels = (TodoItemViewModels)e.Parameter;
        }

        private async void showMessageDialog(string warning)
        {
            var msgDialog = new Windows.UI.Popups.MessageDialog(warning) { };
            msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("OK"));
            await msgDialog.ShowAsync();
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            if (InlineTodoItemView.Visibility == Visibility.Collapsed)
            {
                Frame root = Window.Current.Content as Frame;
                root.Navigate(typeof(NewPage), ViewModels);
            }
            else
            {
                id.Text = "";
                title.Text = "";
                description.Text = "";
                datePicker.Date = DateTime.Now;
                submit.Content = "Create";
            }
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            if (id.Text.Equals("")) return;
            this.ViewModels.RemoveTodoItem(id.Text);
            id.Text = "";
            title.Text = "";
            description.Text = "";
            datePicker.Date = DateTime.Now;
            submit.Content = "Create";
            showMessageDialog("Delete successfully!");
        }

        private void submit_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage icon = (BitmapImage)image.Source;
            string warning = "";
            if (title.Text.Equals(""))
            {
                warning += "The title should not be blank!\n";
            }
            if (description.Text.Equals(""))
            {
                warning += "The description should not be blank!\n";
            }
            if (datePicker.Date.Year < DateTime.Now.Year ||
                datePicker.Date.Year.Equals(DateTime.Now.Year) && datePicker.Date.Month < DateTime.Now.Month ||
                datePicker.Date.Year.Equals(DateTime.Now.Year) && datePicker.Date.Month.Equals(DateTime.Now.Month)
                && datePicker.Date.Day < DateTime.Now.Day)
            {
                warning += "The date should not be earlier than today!\n";
            }
            if (!warning.Equals(""))
            {
                showMessageDialog(warning);
                return;
            }

            if (submit.Content.Equals("Create"))
            {
                ViewModels.AddTodoItem(title.Text, description.Text, datePicker.Date, icon);
                showMessageDialog("Create successfully!");
            }
            else if (submit.Content.Equals("Update"))
            {
                ViewModels.UpdateTodoItem(id.Text, title.Text, description.Text, datePicker.Date, icon);
                showMessageDialog("Update successfully!");
            }
            id.Text = "";
            title.Text = "";
            description.Text = "";
            datePicker.Date = DateTime.Now;
            submit.Content = "Create";
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            id.Text = "";
            title.Text = "";
            description.Text = "";
            datePicker.Date = DateTime.Now;
            submit.Content = "Create";
        }
        

        private void TodoItem_ItemClicked(object sender, ItemClickEventArgs e)
        {
            ViewModels.selectedItem = (TodoItem)e.ClickedItem;
            
            if (InlineTodoItemView.Visibility == Visibility.Collapsed)
            {
                Frame root = Window.Current.Content as Frame;
                root.Navigate(typeof(NewPage), ViewModels);
            }
            else
            {
                var item = ViewModels.selectedItem;
                id.Text = item.Id;
                title.Text = item.Title;
                description.Text = item.Description;
                datePicker.Date = item.Date;
                //imagePicker.Background.SetValue(ImageBrush.ImageSourceProperty, item.Icon);
                image.Source = item.Icon;
                submit.Content = "Update";
                ViewModels.selectedItem = null;
            }
            
        }

        async private void imagePicker_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker imgPicker = new FileOpenPicker();
            imgPicker.FileTypeFilter.Add(".jpg");
            imgPicker.FileTypeFilter.Add(".jpeg");
            imgPicker.FileTypeFilter.Add(".png");
            imgPicker.FileTypeFilter.Add(".bmp");

            imgPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            var file = await imgPicker.PickSingleFileAsync();
            if (file != null)
            {
                IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                BitmapImage bi = new BitmapImage();
                await bi.SetSourceAsync(ir);
                image.Source = bi;
                //imagePicker.Background.SetValue(ImageBrush.ImageSourceProperty, bi);
                //currentImage = bi.UriSource.OriginalString;

                //FileRandomAccessStream path = (FileRandomAccessStream)await file.OpenAsync(FileAccessMode.Read);
                //bi.SetSource(path);
            }
        }
    }
}
