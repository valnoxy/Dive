using Dive.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Dive.UI.Common;
using Dive.UI.Common.UserInterface;

namespace Dive.UI.Pages.CapturePages
{
    /// <summary>
    /// Interaktionslogik für SaveToStep.xaml
    /// </summary>
    public partial class SaveToStep : UserControl
    {
        public class Disk
        {
            public string Picture { get; set; }
            public string Path { get; set; }
        }

        private List<Disk> disks;

        public List<Disk> DiskList
        {
            get
            {
                return disks;
            }
        }

        public SaveToStep()
        {
            InitializeComponent();
            LoadDisks();
        }

        private void LoadDisks()
        {
            disks = new List<Disk>();
            var counter = 0;

            // Find WIM USB device 
            var allDrives = DriveInfo.GetDrives();
            foreach (var d in allDrives)
            {
                if (File.Exists(Path.Combine(d.Name, ".diveusb")) && Directory.Exists(Path.Combine(d.Name, "WIMs")))
                {
                    Debug.WriteLine("Dive Disk detected: " + d.Name);
                    disks.Add(new Disk { Picture = "pack://application:,,,/assets/icon-hdd-40.png", Path = $"{d.Name}WIMs" });
                    counter++;
                }
            }

            var localizedImageCounter = (string)Application.Current.MainWindow!.FindResource("CaptureSaveToDisksCounter");
            if (string.IsNullOrEmpty(localizedImageCounter))
                localizedImageCounter = "Dive Disks loaded: {0}";

            ImageCounter.Text = string.Format(localizedImageCounter, counter);
            this.DataContext = this;
        }

        private void DiskListView_Selected(object sender, RoutedEventArgs e)
        {
            if (DiskListView.SelectedItem is Disk item)
            {
                Common.CaptureInfo.PathToImage = item.Path;

                Debug.WriteLine($"Selected Path: {Common.CaptureInfo.PathToImage}", ConsoleColor.White);
                Validate();
            }
        }

        private void Validate()
        {
            CaptureContent.ContentWindow.NextBtn.IsEnabled =
                !String.IsNullOrEmpty(Common.CaptureInfo.PathToImage) && !String.IsNullOrEmpty(TbFileName.Text);
        }

        private void TbFileName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Common.CaptureInfo.ImageFileName = TbFileName.Text + ".wim";
            Validate();
        }
    }
}
