using deploya_core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace deployaUI.Pages.ApplyPages
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
            int counter = 0;

            // Find WIM USB device 
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives)
            {
                if (File.Exists(Path.Combine(d.Name, ".diveusb")) && Directory.Exists(Path.Combine(d.Name, "WIMs")))
                {
                    string[] dirs = Directory.GetFiles(Path.Combine(d.Name, "WIMs"), "*.wim");

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

                                Common.Debug.WriteLine("--- Image ---", ConsoleColor.White);
                                Common.Debug.WriteLine($"ID : {product_id}", ConsoleColor.White);
                                Common.Debug.WriteLine($"Name : {product_name}", ConsoleColor.White);
                                Common.Debug.WriteLine($"Size : {sizeInGB}", ConsoleColor.White);
                                Common.Debug.WriteLine("--- Image ---\n", ConsoleColor.White);

                                string imageVersion = "";

                                // Windows Client
                                if (product_name.ToLower().Contains("windows 2000"))
                                    imageVersion = "windows-2000";
                                if (product_name.ToLower().Contains("windows xp"))
                                    imageVersion = "windows-xp";
                                if (product_name.ToLower().Contains("windows vista"))
                                    imageVersion = "windows-vista";
                                if (product_name.ToLower().Contains("windows 7"))
                                    imageVersion = "windows-7";
                                if (product_name.ToLower().Contains("windows 8") || product_name.ToLower().Contains("windows 8.1"))
                                    imageVersion = "windows-10";
                                if (product_name.ToLower().Contains("windows 10"))
                                    imageVersion = "windows-10";
                                if (product_name.ToLower().Contains("windows 11"))
                                    imageVersion = "windows-11";

                                // Windows Server
                                if (product_name.ToLower().Contains("windows server 2003"))
                                    imageVersion = "windows-xp";
                                if (product_name.ToLower().Contains("windows server 2008"))
                                    imageVersion = "windows-7";
                                if (product_name.ToLower().Contains("windows server 2012"))
                                    imageVersion = "windows-server-2012";
                                if (product_name.ToLower().Contains("windows server 2016"))
                                    imageVersion = "windows-server-2012";
                                if (product_name.ToLower().Contains("windows server 2019"))
                                    imageVersion = "windows-server-2012";
                                if (product_name.ToLower().Contains("windows server 2022"))
                                    imageVersion = "windows-server-2012";

                                // Exploitox Internal
                                if (product_name.ToLower().Contains("ai operating system") || product_name.ToLower().Contains("aios"))
                                    imageVersion = "aios";

                                // Microsoft Internal / Other OS
                                if (product_name.ToLower().Contains("windows core os") || product_name.ToLower().Contains("wcos"))
                                    imageVersion = "windows-10";
                                if (product_name.ToLower().Contains("phone"))
                                    imageVersion = "windows-10";

                                // Beta
                                if (product_name.ToLower().Contains("whistler"))
                                    imageVersion = "windows-xp";
                                if (product_name.ToLower().Contains("longhorn"))
                                    imageVersion = "windows-vista";
                                if (product_name.ToLower().Contains("blue"))
                                    imageVersion = "windows-10";

                                images.Add(new Image { Picture = $"pack://application:,,,/assets/icon-{imageVersion}-40.png", ImageFile = binary, Name = product_name, Index = product_id });
                                counter++;
                            }
                        }
                        catch (Exception ex) { Console.WriteLine(ex.Message); }
                    }
                }
            }

            //images.Add(new Image { Picture = "pack://application:,,,/assets/icon-windows-11-40.png", ImageFile = "win11.wim", Name = "Windows 11 Pro (22H2)" });
            //images.Add(new Image { Picture = "pack://application:,,,/assets/icon-windows-10-40.png", ImageFile = "win10.wim", Name = "Windows 10 Pro (21H2)" });
            //images.Add(new Image { Picture = "pack://application:,,,/assets/icon-windows-10-40.png", ImageFile = "win8.wim", Name = "Windows 8.1 Pro" });
            //images.Add(new Image { Picture = "pack://application:,,,/assets/icon-windows-7-40.png", ImageFile = "win7.wim", Name = "Windows 7 Professional" });
            //images.Add(new Image { Picture = "pack://application:,,,/assets/icon-windows-vista-40.png", ImageFile = "winvista.wim", Name = "Windows Vista" });
            //images.Add(new Image { Picture = "pack://application:,,,/assets/icon-windows-xp-40.png", ImageFile = "winxp.wim", Name = "Windows XP" });

            ImageCounter.Text = $"Images loaded: {counter}";
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
                Common.ApplyDetails.IconPath = item.Picture;

                Common.Debug.WriteLine($"Selected Index: {Common.ApplyDetails.Index}", ConsoleColor.White);
                Common.Debug.WriteLine($"Selected Name: {Common.ApplyDetails.Name}", ConsoleColor.White);
                Common.Debug.WriteLine($"Selected Image File Path: {Common.ApplyDetails.FileName}\n", ConsoleColor.White);

                ApplyContent.ContentWindow.NextBtn.IsEnabled = true;
            }
        }
    }
}
