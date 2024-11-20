using Debug = Dive.UI.Common.Debug;
using Dive.UI.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Threading.Tasks;

namespace Dive.UI.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für CloudSelectStep.xaml
    /// </summary>
    public partial class CloudSelectStep
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
        private const string DefaultServer = "https://dl.exploitox.de/dive/clouddeployment.json";

        public CloudSelectStep()
        {
            InitializeComponent();
        }

        private void CloudSelectStep_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Initialize network on Windows PE
            if (File.Exists("X:\\Windows\\System32\\wpeinit.exe"))
            {
                Process.Start("X:\\Windows\\System32\\wpeinit.exe");
            }

            LoadImages(DefaultServer);
        }

        private async void LoadImages(string defaultServer)
        {
            images = [];
            var counter = 0;

            LoadingScreen.Visibility = Visibility.Visible;
            SkuListView.Visibility = Visibility.Hidden;

            await Task.Run(() =>
            {
                // Fetch Server
                var cloudInfo = CloudInfo.FromUrlAsync(defaultServer).Result;
                if (cloudInfo == null)
                {
                    Debug.WriteLine("Failed to fetch cloud deployment information", ConsoleColor.Red);
                    return;
                }

                try
                {
                    // Load Images
                    foreach (var image in cloudInfo.Images!)
                    {
                        // Begin

                        var productArch = image.Arch switch
                        {
                            "0" => "x86",
                            "5" => "arm",
                            "6" => "ia64",
                            "9" => "x64",
                            "12" => "arm64",
                            _ => $"Unknown architecture ({image.Arch})"
                        };

                        if (!string.IsNullOrEmpty(image.Version?.Build))
                            productArch = $"NT {image.Version?.Major}.{image.Version?.Minor}.{image.Version?.Build} - {productArch}";

                        Debug.Write("Found ");
                        Debug.Write(image.Name + " (" + productArch + ")", true, ConsoleColor.DarkYellow);
                        Debug.Write(" on Cloud Deployment Server.\n", true);

                        // Determine Windows Version
                        var imageVersion = "windows";
                        var osVersion = $"{image.Version?.Major}.{image.Version?.Minor}";
                        if (!string.IsNullOrEmpty(image.Version?.Build))
                        {
                            var osBuild = int.Parse(image.Version?.Build!);

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
                                case "10.0" when image.InstallationType == "Client":
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
                                case "6.2" when image.InstallationType == "Server":
                                case "6.3" when image.InstallationType == "Server":
                                    imageVersion = "windows-server-2012";
                                    break;

                                // Windows Server 2016 - 2022
                                case "10.0" when image.InstallationType == "Server":
                                    imageVersion = "windows-server-2012";
                                    break;
                            }
                        }

                        // Exploitox Internal
                        if (image.Name!.ToLower().Contains("ai operating system") ||
                            image.Name.ToLower().Contains("aios") ||
                            image.Name.ToLower().Contains("xorie"))
                            imageVersion = "xorie";

                        ImageList.Add(new Image
                        {
                            Picture = $"pack://application:,,,/assets/icon-{imageVersion}-40.png",
                            ImageFile = image.FilePath,
                            Name = image.Name,
                            Index = image.Index,
                            Arch = productArch,
                            NTVersion = osVersion,
                            Build = int.Parse(image.Version?.Build ?? "0")
                        });
                        counter++;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message, ConsoleColor.Red);
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

        private void ReloadBtn_Clicked(object sender, RoutedEventArgs e) => LoadImages(DefaultServer);

        private void SKUListView_Selected(object sender, RoutedEventArgs e)
        {
            if (SkuListView.SelectedItem is not Image item) return;
            var applyDetailInstance = Common.ApplyDetails.Instance;
            applyDetailInstance.Name = item.Name;
            applyDetailInstance.Index = Convert.ToInt32(item.Index);
            applyDetailInstance.FileName = item.ImageFile;
            applyDetailInstance.IconPath = item.Picture;
            applyDetailInstance.Build = item.Build;
            applyDetailInstance.NTVersion = item.NTVersion;

            Common.Debug.Write("Selected ");
            Common.Debug.Write(applyDetailInstance.Name, true, ConsoleColor.DarkYellow);
            Common.Debug.Write(" with Index ", true);
            Common.Debug.Write(applyDetailInstance.Index.ToString(), true, ConsoleColor.DarkYellow);
            Common.Debug.Write(" in Image ", true);
            Common.Debug.Write(applyDetailInstance.FileName, true, ConsoleColor.DarkYellow);
            Common.Debug.Write(" for deployment.\n", true);

            CloudContent.ContentWindow!.NextBtn.IsEnabled = true;
        }
    }
}
