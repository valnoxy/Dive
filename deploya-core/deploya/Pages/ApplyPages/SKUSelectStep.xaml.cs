using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;


namespace deploya.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für SKUSelectStep.xaml
    /// </summary>
    public partial class SKUSelectStep : UserControl
    {
        public class Image
        {
            public string Picture { get; set; }
            public string Name { get; set; }
            public string ImageFile { get; set; }
        }

        private List<Image> images;

        public List<Image> ImageList
        {
            get
            {
                return images;
            }
        }

        public SKUSelectStep()
        {
            InitializeComponent();
            LoadImages();
        }

        private void LoadImages()
        {
            images = new List<Image>();
            images.Add(new Image { Picture = "pack://application:,,,/assets/icon-windows-11-40.png", ImageFile = "win11.wim", Name = "Windows 11 Pro (22H2)" });
            images.Add(new Image { Picture = "pack://application:,,,/assets/icon-windows-10-40.png", ImageFile = "win10.wim", Name = "Windows 10 Pro (21H2)" });
            images.Add(new Image { Picture = "pack://application:,,,/assets/icon-windows-10-40.png", ImageFile = "win8.wim", Name = "Windows 8.1 Pro" });
            images.Add(new Image { Picture = "pack://application:,,,/assets/icon-windows-7-40.png", ImageFile = "win7.wim", Name = "Windows 7 Professional" });
            images.Add(new Image { Picture = "pack://application:,,,/assets/icon-windows-vista-40.png", ImageFile = "winvista.wim", Name = "Windows Vista" });
            images.Add(new Image { Picture = "pack://application:,,,/assets/icon-windows-xp-40.png", ImageFile = "winxp.wim", Name = "Windows XP" });

            this.DataContext = this;
        }

        private void SKUListView_Selected(object sender, RoutedEventArgs e)
        {
            ApplyContent.ContentWindow.NextBtn.IsEnabled = true;

            // ApplyContent.NextButton.IsEnabled = true;
            Debug.WriteLine($"Selected Item: {SKUListView.SelectedItem}");
        }
    }
}
