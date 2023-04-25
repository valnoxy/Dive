using Dive.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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
            int counter = 0;

            // Find WIM USB device 
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives)
            {
                if (File.Exists(Path.Combine(d.Name, ".diveusb")) && Directory.Exists(Path.Combine(d.Name, "WIMs")))
                {
                    Output.WriteLine($"Dive Disk detected: " + d.Name);
                    disks.Add(new Disk { Picture = "pack://application:,,,/assets/icon-hdd-40.png", Path = $"{d.Name}WIMs" });
                    counter++;
                }
            }

            ImageCounter.Text = $"Dive Disks loaded: {counter}";
            this.DataContext = this;
        }

        private void DiskListView_Selected(object sender, RoutedEventArgs e)
        {
            if (DiskListView.SelectedItem is Disk item)
            {
                Common.CaptureInfo.PathToImage = item.Path;

                Common.Debug.WriteLine($"Selected Path: {Common.CaptureInfo.PathToImage}", ConsoleColor.White);
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
