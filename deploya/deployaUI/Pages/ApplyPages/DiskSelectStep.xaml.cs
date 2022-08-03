using deploya_core;
using System;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace deployaUI.Pages.ApplyPages
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

            if (Common.ApplyDetails.Name.ToLower().Contains("windows xp"))
            {
                UseNTLDRBtn.IsChecked = true;
                BIOSRadio.IsChecked = true;
                UseRecoveryBtn.IsChecked = false;
            }
            else
            {
                UseNTLDRBtn.IsChecked = false;
                UseRecoveryBtn.IsChecked = true;
            }
        }

        private void CheckFirmware()
        {
            if (IsWindowsUEFI())
            {
                EFIRadio.IsChecked = true;
                Common.Debug.WriteLine("Firmware detected: EFI", ConsoleColor.White);
            }
            else
            {
                BIOSRadio.IsChecked = true;
                Common.Debug.WriteLine("Firmware detected: BIOS / CSM", ConsoleColor.White);
            }
        }

        private void LoadDisks()
        {
            DiskListView.Items.Clear();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject info in searcher.Get())
                {
                    string DeviceID = "";
                    string Model = "";
                    string Interface = "";
                    string Serial = "";
                    string Size = "";
                    string SizeInGB = "";

                    if (info["DeviceID"] != null) DeviceID = info["DeviceID"].ToString();
                    if (info["Model"] != null) Model = info["Model"].ToString();
                    if (info["InterfaceType"] != null) Interface = info["InterfaceType"].ToString();
                    if (info["SerialNumber"] != null) Serial = info["SerialNumber"].ToString();
                    if (info["Size"] != null) Size = info["Size"].ToString();
                    if (info["Size"] != null) SizeInGB = ByteToGB(Convert.ToDouble(info["Size"])).ToString();

                    Common.Debug.WriteLine($"DeviceID: {DeviceID}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Model: {Model}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Interface: {Interface}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Serial: {Serial}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Size: {Size}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Size in GB: {SizeInGB}", ConsoleColor.White);

                    if (Interface != "USB")
                    {
                        // Add to list
                        string driveid = DeviceID;
                        driveid = Regex.Match(driveid, @"\d+").Value;
                        string drive = $"{Model} | {Interface} | {SizeInGB} GB | Disk {driveid}";
                        DiskListView.Items.Add(drive);
                    }
                    else
                    {
                        Common.Debug.WriteLine("Skipping as this is a USB Device ...", ConsoleColor.Yellow);
                    }
                    Common.Debug.WriteLine("==========================================================", ConsoleColor.DarkGray);
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
                    Common.Debug.WriteLine($"Using disk {Common.ApplyDetails.DiskIndex} for deployment", ConsoleColor.White);

                    if (ApplyContent.ContentWindow != null)
                    {
                        ApplyContent.ContentWindow.NextBtn.IsEnabled = true;
                    }
                    if (CloudContent.ContentWindow != null)
                    {
                        CloudContent.ContentWindow.NextBtn.IsEnabled = true;
                    }
                }
            }
        }

        private void ReloadBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadDisks();
        }

        private void ConfigFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Deployment Information (*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == true)
            {
                TbCustomFile.Text = openFileDialog.FileName;
                TbUser.Clear();
                TbUser.IsEnabled = false;
            }
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
                Common.Debug.WriteLine("Using NTLDR", ConsoleColor.White);
                return true;
            }
            else
            {
                Common.Debug.WriteLine("Using BOOTMGR", ConsoleColor.White);
                return false;
            }
        }

        public bool IsRecoveryChecked()
        {
            if ((bool)UseRecoveryBtn.IsChecked)
            {
                Common.Debug.WriteLine("Using Recovery partition", ConsoleColor.White);
                return true;
            }
            else
            {
                Common.Debug.WriteLine("Using no Recovery partition", ConsoleColor.White);
                return false;
            }
        }

        private void TbUser_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TbUser.Text == "")
            {
                TbPassword.IsEnabled = false;
                TbPassword.Text = "";
            }
            else
            {
                TbPassword.IsEnabled = true;
                Common.DeploymentInfo.Username = TbUser.Text;
            }
        }

        private void TbPassword_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Common.DeploymentInfo.Password = TbPassword.Text;
        }

        private void ClearConfigTb_Click(object sender, RoutedEventArgs e)
        {
            TbCustomFile.Clear();
            TbUser.IsEnabled = true;
        }

        private void TbCustomFile_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Common.DeploymentInfo.CustomFilePath = TbCustomFile.Text;
        }
    }
}
