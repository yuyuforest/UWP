using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Todo.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
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
    public sealed partial class EditItem : Page
    {
        public static EditItem latestInstance;  //最新的实例

        public String tempImage;
        private String imageType = "default";

        public TodoItemViewModels ViewModels;
        public EditItem()
        {
            this.InitializeComponent();
            ViewModels = TodoItemViewModels.Instance;
            latestInstance = this;
        }

        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (ViewModels.SelectedItem != null)
            {
                showItem();
            }
        }

        //显示消息提示框
        public async void showMessageDialog(string warning)
        {
            var msgDialog = new Windows.UI.Popups.MessageDialog(warning) { };
            msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("OK"));
            await msgDialog.ShowAsync();
        }

        //清空编辑项目的界面
        private void clearForm()
        {
            id.Text = "";
            title.Text = "";
            description.Text = "";
            datePicker.Date = DateTime.Now;
            submit.Content = "Create";
            Uri uri = new Uri(MainPage.defaultImage);
            BitmapImage bi = new BitmapImage(uri);
            image.Source = bi;
            imageType = "default";
            ViewModels.SelectedItem = null;
        }

        //显示当前选中的项的详情
        public void showItem()
        {
            if (ViewModels.SelectedItem == null)
            {
                this.clearForm();
                return;
            }

            var item = ViewModels.SelectedItem;
            id.Text = item.Id;
            title.Text = item.Title;
            description.Text = item.Description;
            datePicker.Date = item.Date;
            image.Source = item.Icon;
            imageType = item.ImageType;
            submit.Content = "Update";
        }

        //点击底部的添加按钮时触发
        public void add() {
            clearForm();
        }

        //点击底部的删除按钮时触发
        public void delete()
        {
            if (id.Text.Equals("")) return;
            ViewModels.RemoveTodoItem(id.Text);
            DataAccess.Instance.DeleteData(id.Text);
            clearForm();
            showMessageDialog("Delete successfully!");
            
            MainPage.latestInstance.showItemList();
        }

        //提交表单时的处理
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
                String id = ViewModels.AddTodoItem(title.Text, description.Text, datePicker.Date, (BitmapImage)image.Source, imageType, imageType == "default" ? false : true);
                DataAccess.Instance.AddData(id, title.Text, description.Text, datePicker.Date, imageType);
                showMessageDialog("Create successfully!");
            }
            else if (submit.Content.Equals("Update"))
            {
                ViewModels.UpdateTodoItem(id.Text, title.Text, description.Text, datePicker.Date, (BitmapImage)image.Source, imageType, imageType == "default" ? false : true);
                DataAccess.Instance.UpdateData(id.Text, title.Text, description.Text, datePicker.Date, imageType);
                showMessageDialog("Update successfully!");
            }
            clearForm();

            MainPage.latestInstance.showItemList();
            MainPage.latestInstance.UpdatePrimaryTile();
            
        }

        //点击cancel按钮时触发
        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            clearForm();
            
            MainPage.latestInstance.showItemList();
        }

        //选择图像
        async private void imagePicker_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker imgPicker = new FileOpenPicker();
            imgPicker.FileTypeFilter.Add(".jpg");
            imgPicker.FileTypeFilter.Add(".jpeg");
            imgPicker.FileTypeFilter.Add(".png");
            imgPicker.FileTypeFilter.Add(".bmp");

            imgPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            var file = await imgPicker.PickSingleFileAsync();

            StorageApplicationPermissions.FutureAccessList.Clear();
            if (file != null)
            {
                imageType = file.FileType;

                //挂起时保存图片
                ApplicationData.Current.LocalSettings.Values["imagepicker"] = StorageApplicationPermissions.FutureAccessList.Add(file);
                IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                
                //把图片存为LocalFolder/tempImage
                tempImage = "tempImage" + imageType;
                try
                {
                    //由该图片创建tempImage
                    await file.CopyAsync(ApplicationData.Current.LocalFolder, tempImage);
                }
                catch(Exception ee)
                {
                    //已经有tempImage存在，则替换
                    await file.CopyAndReplaceAsync(await ApplicationData.Current.LocalFolder.GetFileAsync(tempImage));
                }

                BitmapImage bi = new BitmapImage();
                await bi.SetSourceAsync(ir);
                image.Source = bi;
            }
            
        }

        public void setImage(BitmapImage bmp)
        {
            image.Source = bmp;
        }

        public DateTimeOffset Date
        {
            get
            {
                return datePicker.Date;
            }
            set
            {
                datePicker.Date = value;
            }
        }

        public string Title
        {
            get
            {
                return title.Text;
            }
            set
            {
                title.Text = value;
            }
        }

        public string Description
        {
            get
            {
                return description.Text;
            }
            set
            {
                description.Text = value;
            }
        }

        public string Id
        {
            get
            {
                return id.Text;
            }
            set
            {
                id.Text = value;
            }
        }
    }
}
