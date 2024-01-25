using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Dive.UI.Common.USMT;

namespace Dive.UI.Pages.TweaksPages.USMT
{
    /// <summary>
    /// Interaktionslogik für UserList.xaml
    /// </summary>
    public partial class UserList
    {
        private List<RetrieveUsers.User> users;
        public List<RetrieveUsers.User> UsersList => users;

        public UserList()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            users = RetrieveUsers.GetUsersFromHost(Environment.MachineName);
            this.DataContext = this;
            UserListView.ItemsSource = UsersList;
        }

        private void UserListView_Selected(object sender, SelectionChangedEventArgs e)
        {
            TestMigrationBtn.IsEnabled = true;
        }

        private void TestMigrationBtn_OnClick(object sender, RoutedEventArgs e)
        {
            // Perform USMT - ScanState

        }
    }
}
