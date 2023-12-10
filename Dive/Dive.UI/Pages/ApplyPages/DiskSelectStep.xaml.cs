using Dive.UI.Common;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Dive.UI.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für DiskSelectStep.xaml
    /// </summary>
    public partial class DiskSelectStep : UserControl
    {
        public const int ErrorInvalidFunction = 1;
        public static DiskSelectStep? ContentWindow;
        private static ApplyDetails _applyDetailsInstance = ApplyDetails.Instance;

        [DllImport("kernel32.dll",
            EntryPoint = "GetFirmwareEnvironmentVariableW",
            SetLastError = true,
            CharSet = CharSet.Unicode,
            ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int GetFirmwareType(string lpName, string lpGUID, IntPtr pBuffer, uint size);

        public static bool IsWindowsUefi()
        {
            // Call the function with a dummy variable name and a dummy variable namespace (function will fail because these don't exist.)
            GetFirmwareType("", "{00000000-0000-0000-0000-000000000000}", IntPtr.Zero, 0);

            return Marshal.GetLastWin32Error() != ErrorInvalidFunction;
            // Calling the function threw an ERROR_INVALID_FUNCTION win32 error, which gets thrown if either
            // - The mainboard doesn't support UEFI and/or
            // - Windows is installed in legacy BIOS mode
            // If the system supports UEFI and Windows is installed in UEFI mode it doesn't throw the above error, but a more specific UEFI error
        }

        public class Disk
        {
            public string Picture { get; set; }
            public string Model { get; set; }
            public string Size { get; set; }
            public string DiskId { get; set; }
            public bool IsRemovableMedia { get; set; }
        }

        private List<Disk> disks;
        public List<Disk> DiskList => disks;


        public DiskSelectStep()
        {
            InitializeComponent();
            
            if (ApplyContent.ContentWindow != null)
            {
                ApplyContent.ContentWindow.NextBtn.IsEnabled = false;
                ApplyContent.ContentWindow.BackBtn.IsEnabled = true;
            }

            if (CloudContent.ContentWindow != null)
            {
                CloudContent.ContentWindow.NextBtn.IsEnabled = false;
                CloudContent.ContentWindow.BackBtn.IsEnabled = true;
            }

            ContentWindow = this;
            LoadDisks();
            CheckFirmware();

            if (_applyDetailsInstance.Name.ToLower().Contains("windows xp") || _applyDetailsInstance.Name.ToLower().Contains("windows 2000"))
            {
                UseNTLDRBtn.IsChecked = true;
                BIOSRadio.IsChecked = true;
                UseRecoveryBtn.IsChecked = false;
            }
            else if (_applyDetailsInstance.Name.ToLower().Contains("windows vista"))
            {
                UseRecoveryBtn.IsChecked = false;
                UseNTLDRBtn.IsChecked = false;
            }
            else if (_applyDetailsInstance.Name.ToLower().Contains("windows 7"))
            {
                UseRecoveryBtn.IsChecked = true;
            }
            else if (_applyDetailsInstance.Name.Contains("(") || _applyDetailsInstance.Name.Contains(")"))
            {
                Common.DeploymentOption.UseCopyProfile = true;
            }
            else
            {
                UseNTLDRBtn.IsChecked = false;
                UseRecoveryBtn.IsChecked = true;
            }
        }

        private void CheckFirmware()
        {
            if (IsWindowsUefi())
            {
                EFIRadio.IsChecked = true;

                Common.Debug.Write("Firmware detected: ");
                Common.Debug.Write("EFI / CSM\n", true, ConsoleColor.DarkYellow);
            }
            else
            {
                BIOSRadio.IsChecked = true;
                Common.Debug.Write("Firmware detected: ");
                Common.Debug.Write("BIOS (Legacy)\n", true, ConsoleColor.DarkYellow);
            }
        }

        private void LoadDisks()
        {
            disks = new List<Disk>();
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (var info in searcher.Get())
                {
                    var deviceId = info["DeviceID"].ToString();
                    var model = info["Model"].ToString();
                    var sizeInGb = ByteToGb(Convert.ToDouble(info["Size"])).ToString();
                    var ret = GetDiskNumber(Environment.GetFolderPath(Environment.SpecialFolder.System)[..1]);
                    var deviceType = info["MediaType"].ToString();

                    // Check for Dive Medium
                    var allDrives = DriveInfo.GetDrives();
                    var blackListedDisks = 
                        (from d in allDrives where File.Exists(Path.Combine(d.Name, ".diveusb")) 
                            select GetDiskNumber(d.Name[..1])).ToList();

                    Common.Debug.Write("Disk ");
                    Common.Debug.Write(model!, true, ConsoleColor.DarkYellow);
                    Common.Debug.Write(" with ID ", true);
                    Common.Debug.Write(deviceId!, true, ConsoleColor.DarkYellow);
                    Common.Debug.Write(" and a size of ", true);
                    Common.Debug.Write(sizeInGb + "GB", true, ConsoleColor.DarkYellow);

                    if (deviceId != $"\\\\.\\PHYSICALDRIVE{ret}")
                    {
                        // Add to list
                        var driveId = deviceId;
                        var isRemovable = false;
                        driveId = Regex.Match(driveId!, @"\d+").Value;

                        if (blackListedDisks.Contains(driveId))
                        {
                            Common.Debug.Write(" will be skipped (", true);
                            Common.Debug.Write("Dive Medium", true, ConsoleColor.DarkYellow);
                            Common.Debug.Write(").\n", true);
                            continue;
                        }
                        if (deviceType == "Removable Media")
                        {
                            isRemovable = true;
                        }

                        disks.Add(new Disk
                        {
                            Picture = "pack://application:,,,/assets/icon-hdd-40.png",
                            Model = model!,
                            Size = $"{sizeInGb} GB",
                            DiskId = $"Disk {driveId}",
                            IsRemovableMedia = isRemovable
                        });

                        Common.Debug.Write(" is available", true);
                        if (isRemovable)
                        {
                            Common.Debug.Write(" (", true);
                            Common.Debug.Write("removable", true, ConsoleColor.DarkYellow);
                            Common.Debug.Write(").\n", true);
                        }
                        else
                            Common.Debug.Write(".\n", true);
                    }
                    else
                    {
                        Common.Debug.Write(" will be skipped (", true);
                        Common.Debug.Write("Current Windows Disk", true, ConsoleColor.DarkYellow);
                        Common.Debug.Write(").\n", true);
                    }
                }
                this.DataContext = this;
                DiskListView.ItemsSource = disks;
            }
            catch
            {
                // throw;
            }
        }

        private static int ByteToGb(double bytes) // Convert Bytes to GB
        {
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
                var input = m["Name"].ToString()!.Replace("Disk #", "");
                ret = new string(input.SkipWhile(c => !char.IsDigit(c))
                    .TakeWhile(char.IsDigit)
                    .ToArray());
            }
            return ret;
        }

        private void DiskListView_Selected(object sender, RoutedEventArgs e)
        {
            if (DiskListView.SelectedItem is not Disk item) return;

            const string toBeSearched = "Disk ";
            var ix = item.DiskId!.IndexOf(toBeSearched, StringComparison.Ordinal);
            if (ix == -1) return;
            var diskIndex = item.DiskId[(ix + toBeSearched.Length)..];
            _applyDetailsInstance.DiskIndex = Convert.ToInt32(diskIndex);
            _applyDetailsInstance.IsDriveRemovable = item.IsRemovableMedia;

            Common.Debug.Write("The disk with ID ");
            Common.Debug.Write(_applyDetailsInstance.DiskIndex.ToString(), true, ConsoleColor.DarkYellow);
            Common.Debug.Write(" will be used for deployment.\n", true);
            
            if (ApplyContent.ContentWindow != null)
            {
                ApplyContent.ContentWindow.NextBtn.IsEnabled = true;
            }
            if (CloudContent.ContentWindow != null)
            {
                CloudContent.ContentWindow.NextBtn.IsEnabled = true;
            }
        }

        private void ReloadBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadDisks();
        }

        private void EFIRadio_Checked(object sender, RoutedEventArgs e)
        {
            _applyDetailsInstance.UseEFI = true;
            Common.Debug.Write("Firmware has been changed to: ");
            Common.Debug.Write("EFI\n", true, ConsoleColor.DarkYellow);
        }

        private void BIOSRadio_Checked(object sender, RoutedEventArgs e)
        {
            _applyDetailsInstance.UseEFI = false;
            Common.Debug.Write("Firmware has been changed to: ");
            Common.Debug.Write("BIOS\n", true, ConsoleColor.DarkYellow);
        }

        public bool IsNTLDRChecked()
        {
            if ((bool)UseNTLDRBtn.IsChecked)
            {
                Common.Debug.Write("Bootloader has been changed to: ");
                Common.Debug.Write("NTLDR\n", true, ConsoleColor.DarkYellow);
                return true;
            }
            else
            {
                Common.Debug.Write("Bootloader has been changed to: ");
                Common.Debug.Write("BOOTMGR\n", true, ConsoleColor.DarkYellow);
                return false;
            }
        }

        public bool IsRecoveryChecked()
        {
            if ((bool)UseRecoveryBtn.IsChecked)
            {
                Common.Debug.Write("Create separate recovery partition: ");
                Common.Debug.Write("Enabled\n", true, ConsoleColor.DarkYellow);
                return true;
            }
            else
            {
                Common.Debug.Write("Create separate recovery partition: ");
                Common.Debug.Write("Disabled\n", true, ConsoleColor.DarkYellow);
                return false;
            }
        }
    }
}
