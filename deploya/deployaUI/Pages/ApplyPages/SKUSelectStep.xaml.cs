using deploya_core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace deploya.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für SKUSelectStep.xaml
    /// </summary>
    public partial class SKUSelectStep : UserControl
    {
        public class Image
        {
            public string Picture { get; set; }
            public string Name { get; set; }
            public string ImageFile { get; set; }
            public string Index { get; set;}
        }

        private List<Image> images;

        public List<Image> ImageList
        {
            get
            {
                return images;
            }
        }

        public SKUSelectStep()
        {
            InitializeComponent();
            LoadImages();
        }

        private void LoadImages()
        {
            images = new List<Image>();
            string[] dirs = Directory.GetFiles(@"G:\WIMs", "*.wim");

            foreach (string binary in dirs)
            {
                try
                {
                    string action = Actions.GetInfo(binary);

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(action);

                    XmlNodeList imageNames = doc.DocumentElement.SelectNodes("/WIM/IMAGE");

                    string product_id = "", product_name = "", product_size = "";

                    foreach (XmlNode node in imageNames)
                    {
                        product_id = node.Attributes?["INDEX"]?.Value;
                        product_name = node.SelectSingleNode("NAME").InnerText;
                        product_size = node.SelectSingleNode("TOTALBYTES").InnerText;
                        string sizeInGB = convertSize(Convert.ToDouble(product_size));

                        Console.WriteLine($"ID : {product_id}");
                        Console.WriteLine($"Name : {product_name}");
                        Console.WriteLine($"Size : {sizeInGB}\n");

                        string imageVersion = "";
                        if (product_name.Contains("Windows XP"))
                            imageVersion = "windows-xp";
                        if (product_name.Contains("Windows Vista"))
                            imageVersion = "windows-Vista";
                        if (product_name.Contains("Windows 7"))
                            imageVersion = "windows-7";
                        if (product_name.Contains("Windows 8"))
                            imageVersion = "windows-8";
                        if (product_name.Contains("Windows 10"))
                            imageVersion = "windows-10";
                        if (product_name.Contains("Windows 11"))
                            imageVersion = "windows-11";

                        images.Add(new Image { Picture = $"pack://application:,,,/assets/icon-{imageVersion}-40.png", ImageFile = binary, Name = product_name, Index = product_id });
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }

            //images.Add(new Image { Picture = "pack://application:,,,/assets/icon-windows-11-40.png", ImageFile = "win11.wim", Name = "Windows 11 Pro (22H2)" });
            //images.Add(new Image { Picture = "pack://application:,,,/assets/icon-windows-10-40.png", ImageFile = "win10.wim", Name = "Windows 10 Pro (21H2)" });
            //images.Add(new Image { Picture = "pack://application:,,,/assets/icon-windows-10-40.png", ImageFile = "win8.wim", Name = "Windows 8.1 Pro" });
            //images.Add(new Image { Picture = "pack://application:,,,/assets/icon-windows-7-40.png", ImageFile = "win7.wim", Name = "Windows 7 Professional" });
            //images.Add(new Image { Picture = "pack://application:,,,/assets/icon-windows-vista-40.png", ImageFile = "winvista.wim", Name = "Windows Vista" });
            //images.Add(new Image { Picture = "pack://application:,,,/assets/icon-windows-xp-40.png", ImageFile = "winxp.wim", Name = "Windows XP" });

            this.DataContext = this;
        }

        private String convertSize(double size)
        {
            String[] units = new String[] { "B", "KB", "MB", "GB", "TB", "PB" };

            double mod = 1024.0;

            int i = 0;

            while (size >= mod)
            {
                size /= mod;
                i++;
            }
            return Math.Round(size, 2) + units[i];//with 2 decimals
        }

        private void SKUListView_Selected(object sender, RoutedEventArgs e)
        {
            if (SKUListView.SelectedItem is Image item)
            {
                Common.ApplyDetails.Name = item.Name;
                Common.ApplyDetails.Index = Convert.ToInt32(item.Index);
                Common.ApplyDetails.FileName = item.ImageFile;

                Console.WriteLine(Common.ApplyDetails.Index);
                Console.WriteLine(Common.ApplyDetails.Name);
                Console.WriteLine(Common.ApplyDetails.FileName);

                ApplyContent.ContentWindow.NextBtn.IsEnabled = true;
            }
        }
    }
}
