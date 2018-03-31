using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Todo.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Todo
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NewPage : Page
    {
        private TodoItemViewModels ViewModels = null;
        public NewPage()
        {
            this.InitializeComponent();
            //OnNavigatedTo();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModels = (TodoItemViewModels)e.Parameter;
            if(ViewModels.selectedItem != null)
            {
                var item = ViewModels.selectedItem;
                id.Text = item.Id;
                title.Text = item.Title;
                description.Text = item.Description;
                datePicker.Date = item.Date;
                submit.Content = "Update";
                ViewModels.selectedItem = null;
            }
            else
            {
                delete.Visibility = Visibility.Collapsed;
            }
        }

        async void ToMainPage(IUICommand cmd)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    Frame root = Window.Current.Content as Frame;
                    root.Navigate(typeof(MainPage), ViewModels);
                });
        }

        private async void showMessageDialog(string warning, bool mainpage)
        {
            var msgDialog = new Windows.UI.Popups.MessageDialog(warning) { };
            if (mainpage)
                msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("OK", new Windows.UI.Popups.UICommandInvokedHandler(ToMainPage)));
            else
                msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("OK"));
            await msgDialog.ShowAsync();
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModels.RemoveTodoItem(id.Text);
            showMessageDialog("Delete successfully!", true);
        }

        private void submit_Click(object sender, RoutedEventArgs e)
        {

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
                showMessageDialog(warning, false);
                return;
            }

            var img = (ImageSource)imagePicker.Background.GetValue(ImageBrush.ImageSourceProperty);
            BitmapImage icon = (BitmapImage)img;
            if (ViewModels == null) return;

            if (submit.Content.Equals("Create"))
            {
                ViewModels.AddTodoItem(title.Text, description.Text, datePicker.Date, icon);
                showMessageDialog("Create successfully!", true);
            }
            else if (submit.Content.Equals("Update"))
            {
                ViewModels.UpdateTodoItem(id.Text, title.Text, description.Text, datePicker.Date, icon);
                showMessageDialog("Update successfully!", true);
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            if (submit.Content.Equals("Create"))
            {
                id.Text = "";
                title.Text = "";
                description.Text = "";
                datePicker.Date = DateTime.Now;
            }
            else
            {
                Frame root = Window.Current.Content as Frame;
                root.Navigate(typeof(MainPage), ViewModels);
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
                imagePicker.Background.SetValue(ImageBrush.ImageSourceProperty, bi);
            }
        }
    }
}
