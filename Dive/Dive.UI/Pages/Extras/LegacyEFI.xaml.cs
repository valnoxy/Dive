using System.Windows;
using Dive.UI.Common.UserInterface;

namespace Dive.UI.Pages.Extras
{
    /// <summary>
    /// Interaktionslogik für LegacyEFI.xaml
    /// </summary>
    public partial class LegacyEFI
    {
        public LegacyEFI()
        {
            InitializeComponent();
        }

        private void ToggleUefiSeven_OnChecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[UefiSeven] Selected for Installation.");
            Common.WindowsModification.InstallUefiSeven = true;

            ToggleCsmWrap.IsChecked = false;
            Common.WindowsModification.InstallCsmWrap = false;
        }

        private void ToggleUefiSeven_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[UefiSeven] Deselected for Installation.");
            Common.WindowsModification.InstallUefiSeven = false;
        }
        private void ToggleCsmWrap_OnChecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[CsmWrap] Selected for Installation.");
            Common.WindowsModification.InstallCsmWrap = true;

            ToggleUefiSeven.IsChecked = false;
            Common.WindowsModification.InstallUefiSeven = false;
        }

        private void ToggleCsmWrap_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[CsmWrap] Deselected for Installation.");
            Common.WindowsModification.InstallCsmWrap = false;
        }

        private void ToggleLog_OnChecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[UefiSeven] Enabled Boot logging");
            Common.WindowsModification.UsToggleLog = true;
        }

        private void ToggleLog_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[UefiSeven] Disabled Boot logging");
            Common.WindowsModification.UsToggleLog = false;
        }

        private void ToggleVerbose_OnChecked_OnChecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[UefiSeven] Enabled Verbose boot");
            Common.WindowsModification.UsToggleVerbose = true;
        }

        private void ToggleVerbose_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[UefiSeven] Disabled Verbose boot");
            Common.WindowsModification.UsToggleVerbose = false;
        }

        private void ToggleFakeVesa_OnChecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[UefiSeven] Enabled Fake Vesa Force");
            Common.WindowsModification.UsToggleFakeVesa = true;
        }

        private void ToggleFakeVesa_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[UefiSeven] Disabled Fake Vesa Force");
            Common.WindowsModification.UsToggleFakeVesa = false;
        }

        private void ToggleSkipErrors_OnChecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[UefiSeven] Enabled Error skip");
            Common.WindowsModification.UsToggleSkipErros = true;
        }

        private void ToggleSkipErrors_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[UefiSeven] Disabled Error skip");
            Common.WindowsModification.UsToggleSkipErros = false;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
