using deployaCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using deployaUI.Common;

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
            public int Build { get; set; }
            public string NTVersion { get; set; }
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
                string[] dirs = { };
                if (Directory.Exists(Path.Combine(d.Name, "WIMs")))
                    dirs = Directory.GetFiles(Path.Combine(d.Name, "WIMs"), "*.wim");

                // Check if Windows Setup Media is presence
                if (File.Exists(Path.Combine(d.Name, "setup.exe")) && Directory.Exists(Path.Combine(d.Name, "sources")))
                {
                    var setupFiles = Directory.GetFiles(Path.Combine(d.Name, "sources"), "*.wim");
                    dirs = setupFiles.Aggregate(dirs, (current, setupFile) => current.Append(setupFile).ToArray());
                }

                foreach (var binary in dirs)
                {
                    try
                    {
                        var action = Actions.GetInfo(binary);
                        var doc = new XmlDocument();
                        doc.LoadXml(action);

                        var imageNames = doc.DocumentElement!.SelectNodes("/WIM/IMAGE");

                        if (imageNames == null) continue;
                        foreach (XmlNode node in imageNames)
                        {
                            var productId = node.Attributes?["INDEX"]?.Value;
                            var productName = node.SelectSingleNode("NAME")?.InnerText;
                            var productArch = node.SelectSingleNode("WINDOWS/ARCH")?.InnerText;
                            var productBuild = node.SelectSingleNode("WINDOWS/VERSION/BUILD")?.InnerText;
                            var productMajor = node.SelectSingleNode("WINDOWS/VERSION/MAJOR")?.InnerText;
                            var productMinor = node.SelectSingleNode("WINDOWS/VERSION/MINOR")?.InnerText;
                            var productType = node.SelectSingleNode("WINDOWS/PRODUCTTYPE")?.InnerText;

                            productArch = productArch switch
                            {
                                "0" => "x86",
                                "5" => "arm",
                                "6" => "ia64",
                                "9" => "x64",
                                "12" => "arm64",
                                _ => $"Unknown architecture ({productArch})"
                            };

                            if (!string.IsNullOrEmpty(productBuild))
                                productArch = $"Build {productBuild} - {productArch}";

                            // Skip if image is Windows PE
                            if (productName?.ToLower().Contains("pe") == true)
                            {
                                Common.Debug.Write("Image ");
                                Common.Debug.Write(productName, true, ConsoleColor.DarkYellow);
                                Common.Debug.Write(" in file ", true);
                                Common.Debug.Write(binary, true, ConsoleColor.DarkYellow);
                                Common.Debug.Write(" is a Windows PE image and will be skipped\n", true);
                                break;
                            }

                            Common.Debug.Write("Found ");
                            Common.Debug.Write(productName + " (" + productArch + ")", true, ConsoleColor.DarkYellow);
                            Common.Debug.Write(" with Index ", true);
                            Common.Debug.Write(productId, true, ConsoleColor.DarkYellow);
                            Common.Debug.Write(" in Image ", true);
                            Common.Debug.Write(binary + "\n", true, ConsoleColor.DarkYellow);

                            // Determine Windows Version
                            var imageVersion = "windows";
                            var osVersion = $"{productMajor}.{productMinor}";
                            if (productBuild != null)
                            {
                                var osBuild = int.Parse(productBuild);

                                imageVersion = osVersion switch
                                {
                                    "5.0" => "windows-2000",
                                    "5.1" => "windows-xp",
                                    "5.2" => "windows-xp", // Windows Server 2003
                                    "6.0" => "windows-vista",
                                    "6.1" => "windows-7",
                                    "6.2" => "windows-8",
                                    "6.3" => "windows-8", // Windows 8.1
                                    _ => "windows"
                                };

                                switch (osVersion)
                                {
                                    case "10.0" when productType == "WinNT":
                                        imageVersion = osBuild >= 22000 ? "windows-11" : "windows-10";
                                        break;

                                    // Windows Home Server
                                    case "5.2" when osBuild == 4500:
                                        imageVersion = "windows-vista";
                                        break;

                                    // Windows Home Server 2011
                                    case "6.1" when osBuild == 8400:
                                        imageVersion = "windows-7";
                                        break;

                                    // Windows Server 2012 (R2)
                                    case "6.2" when productType == "ServerNT":
                                    case "6.3" when productType == "ServerNT":
                                        imageVersion = "windows-server-2012";
                                        break;

                                    // Windows Server 2016 - 2022
                                    case "10.0" when productType == "ServerNT":
                                        imageVersion = "windows-server-2012";
                                        break;
                                }
                            }

                            // Exploitox Internal
                            if (productName.ToLower().Contains("ai operating system") ||
                                productName.ToLower().Contains("aios"))
                                imageVersion = "aios";

                            images.Add(new Image
                            {
                                Picture = $"pack://application:,,,/assets/icon-{imageVersion}-40.png",
                                ImageFile = binary, 
                                Name = productName, 
                                Index = productId, 
                                Arch = productArch, 
                                NTVersion = osVersion, 
                                Build = int.Parse(productBuild ?? "0")
                            });
                            counter++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, ConsoleColor.Red);
                    }
                }
            }

            var localizedImageCounter = (string)Application.Current.MainWindow!.FindResource("ImagesCounter");
            if (string.IsNullOrEmpty(localizedImageCounter))
                localizedImageCounter = "Images loaded: {0}";

            ImageCounter.Text = string.Format(localizedImageCounter, counter);
            this.DataContext = this;
            SKUListView.ItemsSource = images;
        }

        private void SKUListView_Selected(object sender, RoutedEventArgs e)
        {
            if (SKUListView.SelectedItem is not Image item) return;
            Common.ApplyDetails.Name = item.Name;
            Common.ApplyDetails.Index = Convert.ToInt32(item.Index);
            Common.ApplyDetails.FileName = item.ImageFile;
            Common.ApplyDetails.IconPath = item.Picture;
            Common.ApplyDetails.Build = item.Build;
            Common.ApplyDetails.NTVersion = item.NTVersion;

            Common.Debug.Write("Selected ");
            Common.Debug.Write(Common.ApplyDetails.Name, true, ConsoleColor.DarkYellow);
            Common.Debug.Write(" with Index ", true);
            Common.Debug.Write(Common.ApplyDetails.Index.ToString(), true, ConsoleColor.DarkYellow);
            Common.Debug.Write(" in Image ", true);
            Common.Debug.Write(Common.ApplyDetails.FileName, true, ConsoleColor.DarkYellow);
            Common.Debug.Write(" for deployment.\n", true);

            ApplyContent.ContentWindow!.NextBtn.IsEnabled = true;
        }

        private void ReloadBtn_Clicked(object sender, RoutedEventArgs e) => LoadImages();
    }
}
