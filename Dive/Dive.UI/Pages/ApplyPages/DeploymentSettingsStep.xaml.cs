using Dive.UI.Common;
using Dive.UI.Common.Configuration;
using Dive.UI.Common.UserInterface;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Dive.UI.Pages.ApplyPages
{
    /// <summary>
    /// Step "Set up Windows": defines how Windows is configured on first start.
    /// </summary>
    public partial class DeploymentSettingsStep
    {
        /// <summary>Entry of a region / time zone list. An empty value means "unchanged".</summary>
        public sealed record Option(string Value, string Display);

        private readonly ObservableCollection<SummaryItem> _summary = new();
        private SetupConfiguration _configuration = null!;
        private bool _suppressProfileSelection;

        public DeploymentSettingsStep()
        {
            InitializeComponent();
            SummaryList.ItemsSource = _summary;

            FillLists();
            Attach(SetupConfiguration.Current);
            SetupConfiguration.CurrentChanged += Attach;
            LocalizationManager.LanguageChanged += _ => { FillLists(); UpdateSummary(); };

            Loaded += (_, _) => { RefreshProfiles(); UpdateSummary(); };
        }

        /// <summary>
        /// Called by the wizard before it moves on. Returns false (and shows the problems) if the
        /// configuration contains errors.
        /// </summary>
        public bool TryContinue()
        {
            UpdateSummary();
            var hasErrors = _summary.Any(i => i.Kind == SummaryKind.Error);
            BlockingHint.Visibility = hasErrors ? Visibility.Visible : Visibility.Collapsed;
            if (hasErrors)
                Debug.WriteLine("Setup configuration contains errors, cannot continue.", ConsoleColor.Yellow);
            else
                Debug.WriteLine($"Setup mode: {_configuration.Mode}");
            return !hasErrors;
        }

        #region Configuration binding

        private void Attach(SetupConfiguration configuration)
        {
            if (_configuration != null)
                _configuration.PropertyChanged -= Configuration_PropertyChanged;

            _configuration = configuration;
            _configuration.PropertyChanged += Configuration_PropertyChanged;
            DataContext = _configuration;
            UpdateSummary();
        }

        private void Configuration_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            BlockingHint.Visibility = Visibility.Collapsed;
            UpdateSummary();
        }

        private void UpdateSummary()
        {
            if (_configuration == null) return;

            var version = ApplyDetails.Instance.NTVersion ?? string.Empty;
            var isModern = version.StartsWith("10.", StringComparison.Ordinal);

            _summary.Clear();
            foreach (var item in _configuration.GetSummary(isModern))
                _summary.Add(item);
        }

        #endregion

        #region Lists

        private void FillLists()
        {
            var unchanged = LocalizationManager.LocalizeValue("SetupUnchanged");

            var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Where(c => !string.IsNullOrEmpty(c.Name))
                .OrderBy(c => c.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                .Select(c => new Option(c.Name, $"{c.DisplayName} ({c.Name})"))
                .ToList();
            var locales = new List<Option> { new(string.Empty, unchanged) };
            locales.AddRange(cultures);

            var keyboards = new List<Option> { new(string.Empty, LocalizationManager.LocalizeValue("SetupKeyboardSameAsRegion")) };
            keyboards.AddRange(cultures);

            var timeZones = new List<Option> { new(string.Empty, unchanged) };
            timeZones.AddRange(TimeZoneInfo.GetSystemTimeZones().Select(t => new Option(t.Id, t.DisplayName)));

            SetItems(ExpressLocaleBox, locales);
            SetItems(LocaleBox, locales);
            SetItems(KeyboardBox, keyboards);
            SetItems(ExpressTimeZoneBox, timeZones);
            SetItems(TimeZoneBox, timeZones);
        }

        private static void SetItems(ComboBox box, List<Option> items)
        {
            // Keep the selection when the list is rebuilt (e.g. after a language change)
            var selected = box.SelectedValue;
            box.ItemsSource = items;
            box.SelectedValue = selected ?? string.Empty;
        }

        #endregion

        #region Advanced sections

        private void SectionList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sections = new UIElement[]
            {
                SectionAccount, SectionDevice, SectionRegion, SectionSetup, SectionDomain, SectionOem, SectionOptions
            };
            for (var i = 0; i < sections.Length; i++)
            {
                if (sections[i] != null)
                    sections[i].Visibility = i == SectionList.SelectedIndex ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

        #region File selection

        private void BrowseLogo_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "Bitmap (*.bmp)|*.bmp" };
            if (dialog.ShowDialog() == true)
                _configuration.OemLogoPath = dialog.FileName;
        }

        private void BrowseCustomFile_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "Answer file (*.xml)|*.xml" };
            if (dialog.ShowDialog() == true)
                _configuration.CustomFilePath = dialog.FileName;
        }

        #endregion

        #region Profiles

        private void RefreshProfiles(string? selectFile = null)
        {
            _suppressProfileSelection = true;
            try
            {
                var items = new List<object> { LocalizationManager.LocalizeValue("SetupNoProfile") };
                items.AddRange(SetupProfiles.FindProfiles());
                ProfileBox.ItemsSource = items;
                ProfileBox.SelectedItem = items.OfType<SetupProfile>()
                    .FirstOrDefault(p => string.Equals(p.FilePath, selectFile, StringComparison.OrdinalIgnoreCase)) ?? items[0];
            }
            finally
            {
                _suppressProfileSelection = false;
            }
        }

        private void ProfileBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressProfileSelection || ProfileBox.SelectedItem is not SetupProfile profile) return;
            LoadProfile(profile.FilePath);
        }

        private void OpenProfile_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Dive Configuration (*.xml)|*.xml",
                InitialDirectory = SetupProfiles.DefaultFolder
            };
            if (dialog.ShowDialog() != true) return;

            LoadProfile(dialog.FileName);
            RefreshProfiles(dialog.FileName);
        }

        private void SaveProfile_OnClick(object sender, RoutedEventArgs e)
        {
            var folder = SetupProfiles.DefaultFolder;
            try { Directory.CreateDirectory(folder); } catch { /* the dialog falls back to another folder */ }

            var dialog = new SaveFileDialog
            {
                Filter = "Dive Configuration (*.xml)|*.xml",
                InitialDirectory = folder,
                FileName = (ProfileBox.SelectedItem as SetupProfile)?.Name ?? "Profile"
            };
            if (dialog.ShowDialog() != true) return;

            try
            {
                SetupProfiles.Save(_configuration, dialog.FileName);
                RefreshProfiles(dialog.FileName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not save profile: {ex}", ConsoleColor.Red);
                MessageBox.Show(LocalizationManager.LocalizeValue("SetupProfileSaveFailed") + "\n\n" + ex.Message, "Dive",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProfile(string file)
        {
            try
            {
                SetupConfiguration.Current = SetupProfiles.Load(file);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not load profile '{file}': {ex}", ConsoleColor.Red);
                MessageBox.Show(LocalizationManager.LocalizeValue("SetupProfileLoadFailed") + "\n\n" + ex.Message, "Dive",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                RefreshProfiles();
            }
        }

        #endregion
    }
}
