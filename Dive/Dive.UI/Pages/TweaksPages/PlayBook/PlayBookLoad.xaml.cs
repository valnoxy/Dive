using System;
using System.Windows;
using Dive.UI.Common;

namespace Dive.UI.Pages.TweaksPages.PlayBook
{
    /// <summary>
    /// Interaktionslogik für PlayBookLoad.xaml
    /// </summary>
    public partial class PlayBookLoad
    {
        public event DragEventHandler DragEnter;
        public event DragEventHandler Drop;

        public PlayBookLoad()
        {
            InitializeComponent();

            if (TweaksContent.ContentWindow == null) return;
            TweaksContent.ContentWindow.NextBtn.IsEnabled = false;
            TweaksContent.ContentWindow.BackBtn.IsEnabled = true;
        }

        private void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files)
                {
                    Debug.WriteLine(file);
                }
            }
        }

        private void UIElement_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }
    }
}