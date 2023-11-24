using Dive.Core;
using Dive.UI.Common;
using Dive.UI.Common.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using Debug = Dive.UI.Common.Debug;

namespace Dive.UI.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für CloudSelectStep.xaml
    /// </summary>
    public partial class CloudSelectStep : UserControl
    {
        public class Image
        {
            public string Picture { get; set; }
            public string Name { get; set; }
            public string ImageFile { get; set; }
            public string Index { get; set; }
            public string Arch { get; set; }
            public int Build { get; set; }
            public string NTVersion { get; set; }
        }

        private List<Image> images;
        public List<Image> ImageList => images;

        public CloudSelectStep()
        {
            InitializeComponent();

            // Initialize network on Windows PE
            if (File.Exists("X:\\Windows\\System32\\wpeinit.exe"))
            {
                Process.Start("X:\\Windows\\System32\\wpeinit.exe");
            }

            // Assign network volume
            if (!Directory.Exists("P:\\"))
            {
                var allDrives = DriveInfo.GetDrives();
                foreach (var d in allDrives)
                {
                    if (File.Exists(Path.Combine(d.Name, "Dive.conf")))
                    {
                        try
                        {
                            var cloudConfigFile = File.ReadAllText(Path.Combine(d.Name, "Dive.conf"));
                            var cloudConfig = JsonConvert.DeserializeObject<CloudConfig>(cloudConfigFile);
                            if (string.IsNullOrEmpty(cloudConfig!.Username) || string.IsNullOrEmpty(cloudConfig.Password) || string.IsNullOrEmpty(cloudConfig.Hostname))
                                return; // Empty config
                            else
                            {
                                CloudDetails.Username = cloudConfig.Username;
                                CloudDetails.Password = cloudConfig.Password;
                                CloudDetails.Hostname = cloudConfig.Hostname;
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }

                var nv = new Process();
                nv.StartInfo.FileName = "cmd.exe";
                if (string.IsNullOrEmpty(CloudDetails.Username) || string.IsNullOrEmpty(CloudDetails.Password) || string.IsNullOrEmpty(CloudDetails.Hostname))
                    nv.StartInfo.Arguments = "/c \"net use P: \\\\dive.exploitox.de\\dive /user:diveuser D!veUs3r$\"";
                else
                    nv.StartInfo.Arguments = $"/c \"net use P: \\\\{CloudDetails.Hostname}\\dive /user:{CloudDetails.Username} {CloudDetails.Password}";
                nv.StartInfo.CreateNoWindow = true;
                nv.StartInfo.UseShellExecute = false;
                nv.Start();
                nv.WaitForExit();

                if (nv.ExitCode != 0)
                {
                    MessageBox.Show("Could not connect to network drive. Please check your network connection and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

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
                if (!File.Exists(Path.Combine(d.Name, ".divecloud")) ||
                    !Directory.Exists(Path.Combine(d.Name, "WIMs"))) continue;
                var dirs = Directory.GetFiles(Path.Combine(d.Name, "WIMs"), "*.wim");

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
            SkuListView.ItemsSource = images;
        }

        private void ReloadBtn_Clicked(object sender, RoutedEventArgs e) => LoadImages();

        private void SKUListView_Selected(object sender, RoutedEventArgs e)
        {
            if (SkuListView.SelectedItem is not Image item) return;
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

            CloudContent.ContentWindow!.NextBtn.IsEnabled = true;
        }
    }
}
