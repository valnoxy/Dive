﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Navigation;
using Wpf.Ui.Controls;

namespace deployaUI.Pages.Extras.UnattendConfigurationPages
{
    /// <summary>
    /// Interaktionslogik für DeploymentConfigurationPage.xaml
    /// </summary>
    public partial class DeploymentConfigurationPage : UserControl
    {
        public DeploymentConfigurationPage()
        {
            InitializeComponent();

            if (Common.ApplyDetails.NTVersion == "10.0" && Common.ApplyDetails.Build >= 17134)
                SModeSwitch.IsEnabled = true;
            else
                SModeSwitch.IsEnabled = false;

            DiveToRecovery.IsEnabled = Common.ApplyDetails.NTVersion is "10.0" or "6.3" or "6.2";

            SModeSwitch.IsChecked = Common.DeploymentOption.UseSMode;
            CopyProfileToggle.IsChecked = Common.DeploymentOption.UseCopyProfile;
            DiveToRecovery.IsChecked = Common.DeploymentOption.AddDiveToWinRE;
        }

        private void CodeIntegrityMode_Click(object sender, RoutedEventArgs e)
        {
            if (SModeSwitch.IsChecked == true)
            {
                Common.DeploymentOption.UseSMode = true;
                Common.Debug.Write("Using S Mode in Unattend: ");
                Common.Debug.Write("Enabled\n", true, ConsoleColor.DarkYellow);
            }
            else
            {
                Common.DeploymentOption.UseSMode = false;
                Common.Debug.Write("Using S Mode in Unattend: ");
                Common.Debug.Write("Disabled\n", true, ConsoleColor.DarkYellow);
            }
        }

        private void CopyProfileToggle_OnClick(object sender, RoutedEventArgs e)
        {
            if (CopyProfileToggle.IsChecked == true)
            {
                Common.DeploymentOption.UseCopyProfile = true;
                Common.Debug.Write("Using CopyProfile in Unattend: ");
                Common.Debug.Write("Enabled\n", true, ConsoleColor.DarkYellow);
            }
            else
            {
                Common.DeploymentOption.UseCopyProfile = false;
                Common.Debug.Write("Using CopyProfile in Unattend: ");
                Common.Debug.Write("Disabled\n", true, ConsoleColor.DarkYellow);
            }
        }

        private void DiveToRecovery_OnClick(object sender, RoutedEventArgs e)
        {
            if (DiveToRecovery.IsChecked == true)
            {
                Common.DeploymentOption.AddDiveToWinRE = true;
                Common.Debug.Write("Implement Dive into Windows RE image: ");
                Common.Debug.Write("Enabled\n", true, ConsoleColor.DarkYellow);
            }
            else
            {
                Common.DeploymentOption.AddDiveToWinRE = false;
                Common.Debug.Write("Implement Dive into Windows RE image: ");
                Common.Debug.Write("Disabled\n", true, ConsoleColor.DarkYellow);
            }
        }
    }
}
