﻿using Dive.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using Dive.UI.Common;
using System.Threading.Tasks;
using Dive.Core.Common;
using Dive.UI.Common.UserInterface;

namespace Dive.UI.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für SKUSelectStep.xaml
    /// </summary>
    public partial class SKUSelectStep
    {
        private static ApplyDetails _applyDetailsInstance = ApplyDetails.Instance;

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
            Loaded += SKUSelectStep_OnLoaded;
            this.DataContext = this;
        }

        private void SKUSelectStep_OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadImages();
        }

        private async void LoadImages()
        {
            images = [];
            var counter = 0;

            LoadingScreen.Visibility = Visibility.Visible;
            SkuListView.Visibility = Visibility.Hidden;

            await Task.Run(() =>
            {
                // Find WIM USB device
                var allDrives = DriveInfo.GetDrives();
                foreach (var d in allDrives)
                {
                    string[] dirs = { };
                    if (Directory.Exists(Path.Combine(d.Name, "WIMs")))
                        dirs = Directory.GetFiles(Path.Combine(d.Name, "WIMs"), "*.wim")
                            .Concat(Directory.GetFiles(Path.Combine(d.Name, "WIMs"), "*.esd"))
                            .ToArray();

                    // Check if Windows Setup Media is presence
                    if (File.Exists(Path.Combine(d.Name, "setup.exe")) &&
                        Directory.Exists(Path.Combine(d.Name, "sources")))
                    {
                        var setupFiles = Directory.GetFiles(Path.Combine(d.Name, "sources"), "*.wim")
                            .Concat(Directory.GetFiles(Path.Combine(d.Name, "sources"), "*.esd"))
                            .ToArray();
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
                                    _ => "Unknown architecture"
                                };

                                if (!string.IsNullOrEmpty(productBuild))
                                    productArch = $"NT {productMajor}.{productMinor}.{productBuild} - {productArch}";

                                // Skip if image is Windows PE
                                if (productName != null && productName.ToLower().Contains("pe"))
                                {
                                    Debug.Write("Image ");
                                    Debug.Write(productName, true, ConsoleColor.DarkYellow);
                                    Debug.Write(" in file ", true);
                                    Debug.Write(binary, true, ConsoleColor.DarkYellow);
                                    Debug.Write(" is a Windows PE image and will be skipped\n", true);
                                    Logging.Log($"Image {productName} in file {binary} is a Windows PE image and will be skipped.", Logging.LogLevel.WARNING);
                                    break;
                                }

                                Debug.Write("Found ");
                                Debug.Write(productName + " (" + productArch + ")", true,
                                    ConsoleColor.DarkYellow);
                                Debug.Write(" with Index ", true);
                                Debug.Write(productId, true, ConsoleColor.DarkYellow);
                                Debug.Write(" in Image ", true);
                                Debug.Write(binary + "\n", true, ConsoleColor.DarkYellow);
                                Logging.Log($"Found {productName} ({productArch}) with Index {productId} in Image {binary}");

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
                                if (productName!.ToLower().Contains("ai operating system") ||
                                    productName.ToLower().Contains("aios") ||
                                    productName.ToLower().Contains("xorie"))
                                    imageVersion = "xorie";

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
                            Logging.Log(ex.Message, Logging.LogLevel.ERROR);
                        }
                    }
                }
            });
            Application.Current.Dispatcher.Invoke(() =>
            {
                var localizedImageCounter = (string)Application.Current.MainWindow!.FindResource("ImagesCounter");
                if (string.IsNullOrEmpty(localizedImageCounter))
                    localizedImageCounter = "Images loaded: {0}";

                ImageCounter.Text = string.Format(localizedImageCounter, counter);
                SkuListView.ItemsSource = images;
                LoadingScreen.Visibility = Visibility.Hidden;
                SkuListView.Visibility = Visibility.Visible;
            });
        }

        private void SKUListView_Selected(object sender, RoutedEventArgs e)
        {
            if (SkuListView.SelectedItem is not Image item) return;
            _applyDetailsInstance.Name = item.Name;
            _applyDetailsInstance.Index = Convert.ToInt32(item.Index);
            _applyDetailsInstance.FileName = item.ImageFile;
            _applyDetailsInstance.IconPath = item.Picture;
            _applyDetailsInstance.Build = item.Build;
            _applyDetailsInstance.NTVersion = item.NTVersion;

            Debug.Write("Selected ");
            Debug.Write(_applyDetailsInstance.Name, true, ConsoleColor.DarkYellow);
            Debug.Write(" with Index ", true);
            Debug.Write(_applyDetailsInstance.Index.ToString(), true, ConsoleColor.DarkYellow);
            Debug.Write(" in Image ", true);
            Debug.Write(_applyDetailsInstance.FileName, true, ConsoleColor.DarkYellow);
            Debug.Write(" for deployment.\n", true);
            Logging.Log($"Selected {_applyDetailsInstance.Name} with Index {_applyDetailsInstance.Index} in Image {_applyDetailsInstance.FileName} for deployment.");

            ApplyContent.ContentWindow!.NextBtn.IsEnabled = true;
        }

        private void ReloadBtn_Clicked(object sender, RoutedEventArgs e) => LoadImages();
    }
}
