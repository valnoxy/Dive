using deployaCore;
using System;
using System.Collections.Generic;
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
            public string Arch { get; set; }
        }

        private List<Image> images;
        public List<Image> ImageList => images;

        public SKUSelectStep()
        {
            InitializeComponent();
            LoadImages();
        }

        private void LoadImages()
        {
            images = new List<Image>();
            var counter = 0;

            // Find WIM USB device 
            var allDrives = DriveInfo.GetDrives();
            foreach (var d in allDrives)
            {
                if (!File.Exists(Path.Combine(d.Name, ".diveusb")) ||
                    !Directory.Exists(Path.Combine(d.Name, "WIMs"))) continue;
                var dirs = Directory.GetFiles(Path.Combine(d.Name, "WIMs"), "*.wim");

                foreach (var binary in dirs)
                {
                    try
                    {
                        var action = Actions.GetInfo(binary);
                        var doc = new XmlDocument();
                        doc.LoadXml(action);

                        var imageNames = doc.DocumentElement.SelectNodes("/WIM/IMAGE");

                        if (imageNames == null) continue;
                        foreach (XmlNode node in imageNames)
                        {
                            var productId = node.Attributes?["INDEX"]?.Value;
                            var productName = node.SelectSingleNode("NAME")?.InnerText;
                            var productArch = node.SelectSingleNode("WINDOWS/ARCH")?.InnerText;

                            switch (productArch)
                            {
                                case "0":
                                    productArch = "x86";
                                    break;
                                case "9":
                                    productArch = "x64";
                                    break;
                                default:
                                    productArch = "none";
                                    break;
                            }

                            Common.Debug.Write("Found ");
                            Common.Debug.Write(productName + " (" + productArch + ") ", true, ConsoleColor.DarkYellow);
                            Common.Debug.Write(" with Index ", true);
                            Common.Debug.Write(productId, true, ConsoleColor.DarkYellow);
                            Common.Debug.Write(" in Image ", true);
                            Common.Debug.Write(binary + "\n", true, ConsoleColor.DarkYellow);

                            // Windows Client
                            var imageVersion = "windows";
                            if (productName.ToLower().Contains("windows 2000"))
                                imageVersion = "windows-2000";
                            if (productName.ToLower().Contains("windows xp"))
                                imageVersion = "windows-xp";
                            if (productName.ToLower().Contains("windows vista"))
                                imageVersion = "windows-vista";
                            if (productName.ToLower().Contains("windows 7"))
                                imageVersion = "windows-7";
                            if (productName.ToLower().Contains("windows 8") ||
                                productName.ToLower().Contains("windows 8.1"))
                                imageVersion = "windows-8";
                            if (productName.ToLower().Contains("windows 10"))
                                imageVersion = "windows-10";
                            if (productName.ToLower().Contains("windows 11"))
                                imageVersion = "windows-11";

                            // Windows Server
                            if (productName.ToLower().Contains("windows server 2003"))
                                imageVersion = "windows-xp";
                            if (productName.ToLower().Contains("windows server 2008"))
                                imageVersion = "windows-7";
                            if (productName.ToLower().Contains("windows server 2012"))
                                imageVersion = "windows-server-2012";
                            if (productName.ToLower().Contains("windows server 2016"))
                                imageVersion = "windows-server-2012";
                            if (productName.ToLower().Contains("windows server 2019"))
                                imageVersion = "windows-server-2012";
                            if (productName.ToLower().Contains("windows server 2022"))
                                imageVersion = "windows-server-2012";

                            // Exploitox Internal
                            if (productName.ToLower().Contains("ai operating system") ||
                                productName.ToLower().Contains("aios"))
                                imageVersion = "aios";

                            // Microsoft Internal / Other OS
                            if (productName.ToLower().Contains("windows core os") ||
                                productName.ToLower().Contains("wcos"))
                                imageVersion = "windows-10";
                            if (productName.ToLower().Contains("phone"))
                                imageVersion = "windows-10";

                            // Beta
                            if (productName.ToLower().Contains("whistler"))
                                imageVersion = "windows-xp";
                            if (productName.ToLower().Contains("longhorn"))
                                imageVersion = "windows-vista";
                            if (productName.ToLower().Contains("blue"))
                                imageVersion = "windows-10";

                            images.Add(new Image
                            {
                                Picture = $"pack://application:,,,/assets/icon-{imageVersion}-40.png",
                                ImageFile = binary, Name = productName, Index = productId, Arch = productArch
                            });
                            counter++;
                        }
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                }
            }
            
            ImageCounter.Text = $"Images loaded: {counter}";
            this.DataContext = this;
        }

        private void SKUListView_Selected(object sender, RoutedEventArgs e)
        {
            if (SKUListView.SelectedItem is not Image item) return;
            Common.ApplyDetails.Name = item.Name;
            Common.ApplyDetails.Index = Convert.ToInt32(item.Index);
            Common.ApplyDetails.FileName = item.ImageFile;
            Common.ApplyDetails.IconPath = item.Picture;

            Common.Debug.Write("Selected ");
            Common.Debug.Write(Common.ApplyDetails.Name, true, ConsoleColor.DarkYellow);
            Common.Debug.Write(" with Index ", true);
            Common.Debug.Write(Common.ApplyDetails.Index.ToString(), true, ConsoleColor.DarkYellow);
            Common.Debug.Write(" in Image ", true);
            Common.Debug.Write(Common.ApplyDetails.FileName, true, ConsoleColor.DarkYellow);
            Common.Debug.Write(" for deployment.\n", true);

            ApplyContent.ContentWindow!.NextBtn.IsEnabled = true;
        }
    }
}
