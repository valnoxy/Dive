using Dive.Core.Common;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace Dive.UI.Pages.TweaksPages.PlayBook
{
    /// <summary>
    /// Interaktionslogik für PlayBookSettings.xaml
    /// </summary>
    public partial class PlayBookSettings
    {
        public PlayBookSettings()
        {
            InitializeComponent();

            if (TweaksContent.ContentWindow == null) return;
            TweaksContent.ContentWindow.NextBtn.IsEnabled = false;
            TweaksContent.ContentWindow.BackBtn.IsEnabled = true;
        }

        private void ShowFlyout(object sender, RoutedEventArgs e)
        {
            UiFlyout.IsOpen = true;
        }

        private void TestDeploy(object sender, RoutedEventArgs e)
        {
            Output.WriteLine("Cloning to System disk ...", ConsoleColor.DarkYellow);
            try
            {
                var clonePath = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory),
                    "DiveBoot");
                var thisPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

                if (Directory.Exists(clonePath))
                {
                    Directory.Delete(clonePath, true);
                }

                Output.WriteLine("clonePath value is: " + clonePath);
                Output.WriteLine("thisPath value is: " + thisPath);

                Directory.CreateDirectory(clonePath);
                CopyFilesRecursively(thisPath, clonePath);

                // Set Pwn-Boot keys
                Registry.SetKey(@"SYSTEM\Setup", "SetupType", RegistryValueKind.DWord, 1);
                Registry.SetKey(@"SYSTEM\Setup", "CmdLine", RegistryValueKind.String, "%SYSTEMDRIVE%\\Dive\\Dive.exe --autoinit-boot");

                Output.WriteLine("System prepared. Restart if you're ready!", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Failed to prepare post-configuration: {ex.Message}", ConsoleColor.Red);
            }
        }

        public static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        internal class Registry
        {
            internal static void SetKey(string keyPath, string keyName, RegistryValueKind valueKind, object valueData)
            {
                var regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, true);

                if (regKey != null)
                {
                    try
                    {
                        Output.WriteLine("Setting value ...", ConsoleColor.Gray);
                        regKey.SetValue(keyName, valueData, valueKind);
                    }
                    catch (Exception ex)
                    {
                        Output.WriteLine("Error setting the registry key: " + ex.Message, ConsoleColor.Red);
                    }
                    finally
                    {
                        regKey.Close();
                    }
                }
                else
                    Output.WriteLine("Error setting the registry key: regKey is null", ConsoleColor.Red);
            }
        }
    }
}
