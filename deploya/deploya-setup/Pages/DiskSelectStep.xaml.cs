using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace deploya_setup.Pages
{
    /// <summary>
    /// Interaktionslogik für SKUSelectStep.xaml
    /// </summary>
    public partial class DiskSelectStep : UserControl
    {
        public DiskSelectStep()
        {
            InitializeComponent();
        }

        private void SKUListView_Selected(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    (window as MainWindow).NextBtn.IsEnabled = true;
                }
            }

            Debug.WriteLine($"Selected Item: {DiskListView.SelectedItem}");
        }
    }
}
