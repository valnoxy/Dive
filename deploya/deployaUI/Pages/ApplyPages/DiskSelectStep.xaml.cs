using deploya_core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace deploya.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für DiskSelectStep.xaml
    /// </summary>
    public partial class DiskSelectStep : UserControl
    {
        public const int ERROR_INVALID_FUNCTION = 1;
        public static DiskSelectStep? ContentWindow;

        [DllImport("kernel32.dll",
            EntryPoint = "GetFirmwareEnvironmentVariableW",
            SetLastError = true,
            CharSet = CharSet.Unicode,
            ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int GetFirmwareType(string lpName, string lpGUID, IntPtr pBuffer, uint size);

        public static bool IsWindowsUEFI()
        {
            // Call the function with a dummy variable name and a dummy variable namespace (function will fail because these don't exist.)
            GetFirmwareType("", "{00000000-0000-0000-0000-000000000000}", IntPtr.Zero, 0);

            if (Marshal.GetLastWin32Error() == ERROR_INVALID_FUNCTION)
            {
                // Calling the function threw an ERROR_INVALID_FUNCTION win32 error, which gets thrown if either
                // - The mainboard doesn't support UEFI and/or
                // - Windows is installed in legacy BIOS mode
                return false;
            }
            else
            {
                // If the system supports UEFI and Windows is installed in UEFI mode it doesn't throw the above error, but a more specific UEFI error
                return true;
            }
        }

        public DiskSelectStep()
        {
            InitializeComponent();

            ApplyContent.ContentWindow.NextBtn.IsEnabled = false;
            ApplyContent.ContentWindow.BackBtn.IsEnabled = true;

            ContentWindow = this;

            LoadDisks();
            CheckFirmware();

            if (Common.ApplyDetails.Name.Contains("Windows XP"))
            {
                UseNTLDRBtn.IsChecked = true;
                BIOSRadio.IsChecked = true;
            }
        }

        private void CheckFirmware()
        {
            if (IsWindowsUEFI())
                EFIRadio.IsChecked = true;
            else
                BIOSRadio.IsChecked = true;
        }

        private void LoadDisks()
        {
            DiskListView.Items.Clear();
            try
            {
                ManagementObjectSearcher searcher =
                new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject info in searcher.Get())
                {
                    Console.WriteLine("DeviceID: " + info["DeviceID"].ToString());
                    Console.WriteLine("Model: " + info["Model"].ToString());
                    Console.WriteLine("Interface: " + info["InterfaceType"].ToString());
                    Console.WriteLine("Serial#: " + info["SerialNumber"].ToString());
                    Console.WriteLine("Size: " + info["Size"].ToString());
                    Console.WriteLine("Size in GB: " + ByteToGB(Convert.ToDouble(info["Size"])).ToString());
                    Console.WriteLine("==========================================================");

                    if (info["InterfaceType"].ToString() == "USB")
                    {
                        Console.WriteLine("Skipping as this is a USB Device ...");
                        //return;
                    }

                    // Add to list
                    string driveid = info["DeviceID"].ToString();
                    driveid = Regex.Match(driveid, @"\d+").Value;
                    string drive = $"{info["Model"]} | {info["InterfaceType"]} | {ByteToGB(Convert.ToDouble(info["Size"]))} GB | Disk {driveid}";
                    DiskListView.Items.Add(drive);
                }
            }
            catch (Exception)
            {
#warning TODO: Better error handling
                throw;
            }
        }

        private int ByteToGB(double bytes) // Convert Bytes to GB
        {
            double gb = bytes / Math.Pow(10, 9);
            int i = (int)Math.Round(gb);
            return i;
        }

        private void DiskListView_Selected(object sender, RoutedEventArgs e)
        {
            if (DiskListView.SelectedItem != null)
            {
                string itemData = DiskListView.SelectedItem.ToString();
                string toBeSearched = "| Disk ";
                int ix = itemData.IndexOf(toBeSearched);

                if (ix != -1)
                {
                    string diskindex = itemData.Substring(ix + toBeSearched.Length);
                    Common.ApplyDetails.DiskIndex = Convert.ToInt32(diskindex);
                    Console.WriteLine(Common.ApplyDetails.DiskIndex);

                    ApplyContent.ContentWindow.NextBtn.IsEnabled = true;
                }
            }
        }

        private void ReloadBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadDisks();
        }

        private void EFIRadio_Checked(object sender, RoutedEventArgs e)
        {
            Common.ApplyDetails.UseEFI = true;
        }

        private void BIOSRadio_Checked(object sender, RoutedEventArgs e)
        {
            Common.ApplyDetails.UseEFI = false;
        }

        public bool IsNTLDRChecked()
        {
            if ((bool)UseNTLDRBtn.IsChecked)
            {
                Console.WriteLine("Using NTLDR");
                return true;
            }
            else
            {
                Console.WriteLine("Using BOOTMGR");
                return false;
            }
        }
    }
}
