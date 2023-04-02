using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace deployaUI.Pages.Extras
{
    /// <summary>
    /// Interaktionslogik für LoadDriversLiveSystem.xaml
    /// </summary>
    public partial class LoadDriversLiveSystem
    {
        private int _driverCount = 0;
        private List<string> _driverList = null;

        public LoadDriversLiveSystem(List<string> driverList)
        {
            InitializeComponent();

            _driverList = driverList;
            _driverCount = driverList.Count;
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += InjectDriver;
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += (sender, args) => Close();
            backgroundWorker.RunWorkerAsync();
        }

        private void backgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            MessageText.Text = $"Loading driver {e.ProgressPercentage} of {_driverCount} ...";
        }

        private void InjectDriver(object? sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "drvload.exe";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            var currentDriverCount = 1;
            foreach (var driver in _driverList)
            {
                worker?.ReportProgress(currentDriverCount);
                proc.StartInfo.Arguments = driver;
                proc.Start();
                proc.WaitForExit();

                if (proc.ExitCode != 0)
                {
                    MessageBox.Show($"Error ({proc.ExitCode}) loading driver: {driver}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                currentDriverCount++;
            }
        }
    }
}
