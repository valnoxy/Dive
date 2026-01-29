using Dive.UI.Common;
using Dive.UI.Common.UserInterface;
using System;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Windows;

namespace Dive.UI.Pages.TweaksPages
{
    /// <summary>
    /// Interaktionslogik für MigrateSettings.xaml
    /// </summary>
    public partial class MigrateSettings
    {
        private static readonly Tweaks TweaksInstance = Tweaks.Instance;

        public MigrateSettings()
        {
            InitializeComponent();

            if (TweaksContent.ContentWindow == null) return;
            TweaksContent.ContentWindow.NextBtn.IsEnabled = false;
            TweaksContent.ContentWindow.BackBtn.IsEnabled = true;
            LoadDisks();
        }

        private void LoadDisks()
        {
            DiskListView.Items.Clear();
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (var info in searcher.Get())
                {
                    var deviceId = "";
                    var model = "";
                    var interfaceType = "";
                    var serial = "";
                    var size = "";
                    var sizeInGb = "";

                    if (info["DeviceID"] != null) deviceId = info["DeviceID"].ToString();
                    if (info["Model"] != null) model = info["Model"].ToString();
                    if (info["InterfaceType"] != null) interfaceType = info["InterfaceType"].ToString();
                    if (info["SerialNumber"] != null) serial = info["SerialNumber"].ToString();
                    if (info["Size"] != null) size = info["Size"].ToString();
                    if (info["Size"] != null) sizeInGb = ByteToGb(Convert.ToDouble(info["Size"])).ToString();

                    Debug.WriteLine($"DeviceID: {deviceId}");
                    Debug.WriteLine($"Model: {model}");
                    Debug.WriteLine($"Interface: {interfaceType}");
                    Debug.WriteLine($"Serial: {serial}");
                    Debug.WriteLine($"Size: {size}");
                    Debug.WriteLine($"Size in GB: {sizeInGb}");

                    var ret = GetDiskNumber(Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 1));

                    if (deviceId != $"\\\\.\\PHYSICALDRIVE{ret}")
                    {
                        // Add to list
                        var driveId = deviceId;
                        if (driveId != null)
                        {
                            driveId = Regex.Match(driveId, @"\d+").Value;
                            var drive = $"{model} | {sizeInGb} GB | Disk {driveId}";
                            DiskListView.Items.Add(drive);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Skipping as this is the main disk ...", ConsoleColor.Yellow);
                    }

                    Debug.WriteLine("==========================================================", ConsoleColor.DarkGray);
                }
            }
            catch (Exception)
            {
                // throw;
            }
        }

        private static int ByteToGb(double bytes) // Convert Bytes to GB
        {
            if (bytes <= 0) throw new ArgumentOutOfRangeException(nameof(bytes));
            var gb = bytes / Math.Pow(10, 9);
            var i = (int)Math.Round(gb);
            return i;
        }

        private static string GetDiskNumber(string letter)
        {
            var ret = "";
            var scope = new ManagementScope("\\\\.\\ROOT\\cimv2");
            var query = new ObjectQuery("Associators of {Win32_LogicalDisk.DeviceID='" + letter + ":'} WHERE ResultRole=Antecedent");
            var searcher = new ManagementObjectSearcher(scope, query);
            var queryCollection = searcher.Get();
            foreach (var m in queryCollection)
            {
                var input = m["Name"].ToString()?.Replace("Disk #", "");
                if (input != null)
                    ret = new string(input.SkipWhile(c => !char.IsDigit(c))
                        .TakeWhile(char.IsDigit)
                        .ToArray());
            }
            return ret;
        }

        private void DiskListView_Selected(object sender, RoutedEventArgs e)
        {
            if (DiskListView.SelectedItem == null) return;
            var itemData = DiskListView.SelectedItem.ToString();
            const string toBeSearched = "| Disk ";
            if (itemData != null)
            {
                var ix = itemData.IndexOf(toBeSearched, StringComparison.Ordinal);

                if (ix == -1) return;
                var diskIndex = itemData[(ix + toBeSearched.Length)..];
                TweaksInstance.DiskIndex = Convert.ToInt32(diskIndex);
            }

            Debug.WriteLine($"Using disk {TweaksInstance.DiskIndex} for migration", ConsoleColor.White);

            if (TweaksContent.ContentWindow != null)
            {
                TweaksContent.ContentWindow.NextBtn.IsEnabled = true;
            }
        }
    }
}
