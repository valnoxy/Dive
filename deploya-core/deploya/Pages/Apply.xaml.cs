using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace deploya.Pages
{
    /// <summary>
    /// Interaktionslogik für Apply.xaml
    /// </summary>
    public partial class Apply : Page
    {
        public Apply()
        {
            InitializeComponent();
        }

        private void DebugReload(object sender, RoutedEventArgs e)
        {
            hddBox.Items.Clear();
            try
            {
                ManagementObjectSearcher searcher =
                new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject info in searcher.Get())
                {
                    Debug.WriteLine("DeviceID: " + info["DeviceID"].ToString());
                    Debug.WriteLine("Model: " + info["Model"].ToString());
                    Debug.WriteLine("Interface: " + info["InterfaceType"].ToString());
                    Debug.WriteLine("Serial#: " + info["SerialNumber"].ToString());
                    Debug.WriteLine("Size: " + info["Size"].ToString());
                    Debug.WriteLine("Size in GB (Copilot): " + (Convert.ToInt64(info["Size"]) / 1024 / 1024 / 1024).ToString());
                    Debug.WriteLine("Size in GB: " + ByteToGB(Convert.ToDouble(info["Size"])).ToString());
                    Debug.WriteLine("==========================================================");

                    // Add to list
                    string driveid = info["DeviceID"].ToString();
                    driveid = Regex.Match(driveid, @"\d+").Value;
                    string drive = $"{info["Model"]} | Disk {driveid} | {info["InterfaceType"]} | {ByteToGB(Convert.ToDouble(info["Size"]))} GB";
                    hddBox.Items.Add(drive);
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
#warning Apply: Code not implemented yet
    }
}
