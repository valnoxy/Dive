using System;
using System.Windows;
using System.Windows.Threading;

namespace deployaUI.Pages.Extras
{
    /// <summary>
    /// Interaktionslogik für UnattendConfiguration.xaml
    /// </summary>
    public partial class UnattendConfiguration
    {
        public UnattendConfiguration()
        {
            InitializeComponent();
        }

        private void ToggleLog_OnChecked(object sender, RoutedEventArgs e)
        {
            Common.Debug.WriteLine("[UefiSeven] Enabled Boot logging");
            Common.WindowsModification.UsToggleLog = true;
        }

        private void ToggleLog_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Common.Debug.WriteLine("[UefiSeven] Disabled Boot logging");
            Common.WindowsModification.UsToggleLog = false;
        }

        private void ToggleVerbose_OnChecked_OnChecked(object sender, RoutedEventArgs e)
        {
            Common.Debug.WriteLine("[UefiSeven] Enabled Verbose boot");
            Common.WindowsModification.UsToggleVerbose = true;
        }

        private void ToggleVerbose_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Common.Debug.WriteLine("[UefiSeven] Disabled Verbose boot");
            Common.WindowsModification.UsToggleVerbose = false;
        }

        private void ToggleFakeVesa_OnChecked(object sender, RoutedEventArgs e)
        {
            Common.Debug.WriteLine("[UefiSeven] Enabled Fake Vesa Force");
            Common.WindowsModification.UsToggleFakeVesa = true;
        }

        private void ToggleFakeVesa_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Common.Debug.WriteLine("[UefiSeven] Disabled Fake Vesa Force");
            Common.WindowsModification.UsToggleFakeVesa = false;
        }

        private void ToggleSkipErrors_OnChecked(object sender, RoutedEventArgs e)
        {
            Common.Debug.WriteLine("[UefiSeven] Enabled Error skip");
            Common.WindowsModification.UsToggleSkipErros = true;
        }

        private void ToggleSkipErrors_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Common.Debug.WriteLine("[UefiSeven] Disabled Error skip");
            Common.WindowsModification.UsToggleSkipErros = false;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
