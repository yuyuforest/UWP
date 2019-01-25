using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Json;
using Windows.Storage;
using System.Xml;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Week7
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        //简体字转繁体字，使用Json格式
        private async void SimToTra(object sender, RoutedEventArgs e)
        {
            String sim = simple.Text;
            String appkey = "33227";
            String sign = "1aad55b9adfb581a932497a612860997";
            String format = "http://api.k780.com/?app=code.hanzi_fanjian&typeid=1&wd=" + sim + 
                "&appkey=" + appkey + "&sign=" + sign + "&format=json";

            //访问API
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(format);
            //读取数据
            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
            Stream stream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(stream);
            string strResult = streamReader.ReadToEnd();
            stream.Dispose();
            streamReader.Dispose();

            //解析Json
            JsonObject rootObject = JsonObject.Parse(strResult);
            if (rootObject["success"].GetString().Equals("1"))
                traditional.Text = rootObject["result"].GetObject()["text"].GetString();
            else traditional.Text = "出现异常，转换失败！";
        }

        //币种汇率查询，使用Xml格式
        private async void Query(object sender, RoutedEventArgs e)
        {
            ComboBoxItem srcItem = ((ComboBoxItem)srcCurrency.SelectedItem);
            String src = (String)srcItem.Content;
            ComboBoxItem dstItem = ((ComboBoxItem)dstCurrency.SelectedItem);
            String dst = (String)dstItem.Content;
            src = src.Substring(src.Length - 3);
            dst = dst.Substring(dst.Length - 3);

            String appkey = "33227";
            String sign = "1aad55b9adfb581a932497a612860997";
            String format = "http://api.k780.com/?app=finance.rate&scur=" + src + "&tcur=" + dst + 
                "&appkey=" + appkey + "&sign=" + sign + "&format=xml";

            //访问API
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(format);
            request.Method = "GET";
            request.ContentType = "application/xml";
            //读取数据
            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
            Stream stream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(stream);
            string xmlResult = streamReader.ReadToEnd();
            stream.Dispose();
            streamReader.Dispose();

            //解析Xml
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlResult);
            XmlNode root = xmlDocument.ChildNodes[1];
            if(root.ChildNodes[0].InnerText == "0")
            {
                rate.Text = "Error";
                return;
            }
            XmlNode result = root.ChildNodes[1];
            rate.Text = result.ChildNodes[4].InnerText;
        }
    }
}
