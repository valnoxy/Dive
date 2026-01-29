using Dive.Core.Common;
using Dive.UI.Common.UserInterface;
using Dive.UI.Common.USMT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Dive.UI.Pages.TweaksPages.USMT
{
    /// <summary>
    /// Interaktionslogik für UserList.xaml
    /// </summary>
    public partial class UserList
    {
        public List<RetrieveUsers.User?>? UsersList { get; private set; }
        internal string SelectedSid = string.Empty;

        public UserList()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            UsersList = RetrieveUsers.GetUsersFromHost(Environment.MachineName)!;
            this.DataContext = this;
            UserListView.ItemsSource = UsersList;
        }

        private void UserListView_Selected(object sender, RoutedEventArgs e)
        {
            if (UserListView.SelectedItem is not RetrieveUsers.User item) return;
            Debug.Write("Selected ");
            Debug.Write(item.Username, true, ConsoleColor.DarkYellow);
            Debug.Write(" with SID ", true);
            Debug.Write(item.SID, true, ConsoleColor.DarkYellow);
            Debug.Write(" for migration.\n", true);
            Logging.Log($"Selected {item.Username} with SID {item.SID} for migration.");
            TestMigrationBtn.IsEnabled = true;
            SelectedSid = item.SID;
            //ApplyContent.ContentWindow!.NextBtn.IsEnabled = true;
        }

        private void TestMigrationBtn_OnClick(object sender, RoutedEventArgs e)
        {
            // Perform USMT - ScanState
            // Extract and unzip the USMT resource
            var tempPath = System.IO.Path.GetTempPath();
            var usmtPath = Path.Combine(tempPath, "Dive", "USMT");
            var scanStatePath = Path.Combine(usmtPath, "USMT_x64", "scanstate.exe");
            const string repositoryPath = "C:\\Dive\\Store";
            Core.Action.USMT.USMTAction.PrepareEnvironment(usmtPath, repositoryPath);

            using var outputStream = Console.OpenStandardOutput();
            var result = Core.Action.USMT.USMTAction.ScanState(
                scanStatePath,
                repositoryPath,
                SelectedSid,
                msg => Debug.WriteLine(msg, ConsoleColor.Gray));
        }
    }
}
