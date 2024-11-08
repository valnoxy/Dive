using Dive.Core;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using Dive.UI.Common;
using System.Threading.Tasks;

namespace Dive.UI.Pages
{
    /// <summary>
    /// Interaktionslogik für Dashboard.xaml
    /// </summary>
    public partial class Dashboard
    {
        private bool _firstLoad;
        public Dashboard()
        {
            InitializeComponent();
        }

        private static int CountAvailableImages()
        {
            var counter = 0;

            // Find WIM USB device
            var allDrives = DriveInfo.GetDrives();
            foreach (var d in allDrives)
            {
                string[] dirs = [];
                if (Directory.Exists(Path.Combine(d.Name, "WIMs")))
                    dirs = Directory.GetFiles(Path.Combine(d.Name, "WIMs"), "*.wim")
                        .Concat(Directory.GetFiles(Path.Combine(d.Name, "WIMs"), "*.esd"))
                        .ToArray();

                // Check if Windows Setup Media is presence
                if (File.Exists(Path.Combine(d.Name, "setup.exe")) && Directory.Exists(Path.Combine(d.Name, "sources")))
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
                        counter += imageNames.Cast<XmlNode>().Count();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, ConsoleColor.Red);
                    }
                }
            }
            return counter;
        }

        private void SwitchToApplyPage(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.RootNavigation.Navigate(typeof(ApplyContent));
        }

        private void SwitchToCapturePage(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.RootNavigation.Navigate(typeof(CaptureContent));
        }

        private void SwitchToCloudPage(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.RootNavigation.Navigate(typeof(CloudContent));
        }

        private async void Dashboard_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var availableImages = await Task.Run(CountAvailableImages);
                ImagesSkeleton.Value = Convert.ToString(availableImages);

                var availableConfigs = 0;
                await Task.Run(() =>
                {
                    var allDrives = DriveInfo.GetDrives();
                    foreach (var d in allDrives)
                    {
                        string[] dirs = [];
                        if (Directory.Exists(Path.Combine(d.Name, "Configs")))
                            dirs = Directory.GetFiles(Path.Combine(d.Name, "Configs"), "*.xml");
                        if (dirs.Length > 0)
                            availableConfigs = dirs.Length;
                    }
                });

                ConfigSkeleton.Value = Convert.ToString(availableConfigs);

                if (!_firstLoad)
                {
                    ImagesSkeleton.Switch();
                    ConfigSkeleton.Switch();
                }
                _firstLoad = true;
            }
            catch
            {
                // ignored
            }
        }

    }
}
