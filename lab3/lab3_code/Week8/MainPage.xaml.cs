using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Week8
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer timer = null;   //计时器，控制播放进度条
        private bool has_loaded = false;        //记录是否已经加载了某个媒体文件，如果没有加载完，则点击工具栏不会有反应
        private bool has_begun = false;         //记录是否已经播放了某个媒体文件，用以判断动画运行是调用Begin()还是Resume()

        public MainPage()
        {
            this.InitializeComponent();
        }

        //播放/暂停按钮的操作
        private void playControl_Click(object sender, RoutedEventArgs e)
        {
            //没有加载文件，则该操作不执行
            if (!has_loaded)
            {
                return;
            }

            SymbolIcon icon = (SymbolIcon)playControl.Icon;
            if (icon.Symbol == Symbol.Play) //如果按钮是播放，则将媒体文件切换到播放状态
            {
                mediaPlayer.Play();

                icon.Symbol = Symbol.Pause;     //修改按钮样式
                playControl.Label = "暂停";

                if (ellipse.Visibility == Visibility.Visible)   //如果ellipse显示，说明当前是音频文件，应开始封面旋转动画
                {
                    if (!has_begun)
                    {
                        storyboard.Begin();
                        has_begun = true;
                    }
                    else
                    {
                        storyboard.Resume();
                    }
                }
                
            }
            else
            {   //如果按钮是暂停，则将媒体文件切换到暂停状态
                mediaPlayer.Pause();
                
                icon.Symbol = Symbol.Play;
                playControl.Label = "播放";

                if (ellipse.Visibility == Visibility.Visible)   //如果ellipse显示，说明当前是音频文件，应暂停封面旋转动画
                    storyboard.Pause();
            }
        }

        //停止按钮的操作
        private void stop_Click(object sender, RoutedEventArgs e)
        {
            if (!has_loaded)
            {
                return;
            }

            //停止媒体播放和动画
            mediaPlayer.Stop();
            if (ellipse.Visibility == Visibility.Visible)
                storyboard.Stop();

            SymbolIcon icon = (SymbolIcon)playControl.Icon;
            if (icon.Symbol == Symbol.Pause)
            {
                icon.Symbol = Symbol.Play;
                playControl.Label = "播放";
            }

            has_begun = false;
        }

        //打开媒体文件
        private async void openMedia_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker mediaPicker = new FileOpenPicker();
            mediaPicker.FileTypeFilter.Add(".avi");
            mediaPicker.FileTypeFilter.Add(".mp4");
            mediaPicker.FileTypeFilter.Add(".rmvb");
            mediaPicker.FileTypeFilter.Add(".flv");
            mediaPicker.FileTypeFilter.Add(".mkv");
            
            mediaPicker.FileTypeFilter.Add(".mp3");

            mediaPicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;

            var file = await mediaPicker.PickSingleFileAsync();
            
            if (file != null)
            {
                has_begun = false;
                has_loaded = false;

                //设置媒体播放源
                IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                MediaElement mediaElement = new MediaElement();
                MediaSource mediaSource = MediaSource.CreateFromStorageFile(file);
                mediaPlayer.SetSource(ir, " ");

                //修改播放/暂停按钮的样式
                ((SymbolIcon)playControl.Icon).Symbol = Symbol.Play;
                playControl.Label = "播放";

                //如果这是第一次打开媒体文件，则隐藏tip，显示播放器和进度条
                if (tip.Visibility == Visibility.Visible)
                {
                    mediaSlider.Visibility = Visibility.Visible;
                    tip.Visibility = Visibility.Collapsed;
                }


                if (file.FileType == ".mp3")
                {
                    storyboard.Stop();

                    uint size = 1000;

                    //读取专辑封面图，将其设为ellipse的图片源
                    StorageItemThumbnail thumbnail = await file.GetScaledImageAsThumbnailAsync(ThumbnailMode.MusicView, size);
                    BitmapImage bi = new BitmapImage();
                    bi.SetSource(thumbnail);
                    imageBrush.ImageSource = bi;

                    mediaPlayer.Visibility = Visibility.Collapsed;
                    ellipse.Visibility = Visibility.Visible;
                }
                else
                {
                    mediaPlayer.Visibility = Visibility.Visible;
                    ellipse.Visibility = Visibility.Collapsed;
                }
            }

        }

        //mediaPlayer完成加载时的操作
        private void mediaPlayer_Opened(object sender, RoutedEventArgs e)
        {
            //读取媒体时间长度
            mediaSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;

            //新的计时器
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += new EventHandler<object>(timer_tick);
            timer.Start();

            //指示加载完毕
            has_loaded = true;
        }

        //计时事件，控制进度条滑块的移动
        private void timer_tick(object sender, object e)
        {
            mediaSlider.Value = mediaPlayer.Position.TotalSeconds;
        }

        //修改音量
        private void volumn_Changed(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                mediaPlayer.Volume = slider.Value / 100;
            }
        }

        //全屏/退出全屏操作
        private void scale_Click(object sender, RoutedEventArgs e)
        {
            SymbolIcon icon = (SymbolIcon)scale.Icon;
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (icon.Symbol == Symbol.FullScreen)
            {
                view.TryEnterFullScreenMode();
                icon.Symbol = Symbol.BackToWindow;
                scale.Label = "退出全屏";
                mediaSlider.Visibility = Visibility.Collapsed;  //进入全屏时，隐藏进度条
                showSlider.Label = "显示进度条";
            }
            else
            {
                view.ExitFullScreenMode();
                icon.Symbol = Symbol.FullScreen;
                scale.Label = "全屏";
                mediaSlider.Visibility = Visibility.Visible;    //退出全屏时，显示进度条
                showSlider.Label = "隐藏进度条";
            }
        }

        //播放器进度条改变时的操作
        private void mediaSlider_Changed(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!has_loaded)
            {
                return;
            }
            mediaPlayer.Position = TimeSpan.FromSeconds(mediaSlider.Value);
        }

        //点击显示/隐藏进度条的操作
        private void showSlider_Click(object sender, RoutedEventArgs e)
        {
            if (!has_loaded)
            {
                return;
            }
            if (mediaSlider.Visibility == Visibility.Visible)
            {
                mediaSlider.Visibility = Visibility.Collapsed;
                showSlider.Label = "显示进度条";
            }
            else
            {
                mediaSlider.Visibility = Visibility.Visible;
                showSlider.Label = "隐藏进度条";
            }
        }

        //媒体结束播放时的操作
        private void mediaPlayer_Ended(object sender, RoutedEventArgs e)
        {
            ((SymbolIcon)playControl.Icon).Symbol = Symbol.Play;
            playControl.Label = "播放";
            mediaSlider.Value = 0;
            if(ellipse.Visibility == Visibility.Visible)
            {
                storyboard.Stop();
            }
            has_begun = false;
        }
    }
}
