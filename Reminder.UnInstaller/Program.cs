using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Reminder.UnInstaller
{
    internal class Program
    {
        private const string APP_NAME = "AV Reminder";

        static void Main(string[] args)
        {
            StopAvReminder();

            try
            {
                using (RegistryKey uninst =
                       Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\",
                           true))
                {
                    uninst.DeleteSubKeyTree(APP_NAME);
                }

                RegistryKey rk =
                    Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (rk != null)
                {
                    rk.DeleteValue("AvReminder", false);
                    rk.Close();
                }
            }
            catch (ArgumentException)
            {
                // страховка на редкий случай, если кто-то баловался и руками удалил
            }

            DeleteAppData();
            DeleteShortcut();
            SelfDelete();
        }

        private static void StopAvReminder()
        {
            var reminders = Process.GetProcessesByName("AvReminder");
            if(reminders.Any())
                reminders[0].Kill(); // достаточно первого, он один
        }

        private static void DeleteAppData()
        {
            string appDataFolder =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APP_NAME);
            if(Directory.Exists(appDataFolder))
                Directory.Delete(appDataFolder, true);
        }

        private static void DeleteShortcut()
        {
            string shortcutPath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "AvReminder.lnk");
            if(File.Exists(shortcutPath))
                File.Delete(shortcutPath);
        }

        private static void SelfDelete()
        {
            using (Stream stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Reminder.UnInstaller.batchCommand.bin"))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // некоторым антивирусам не нравится, когда файл удаляет сам себя
                    // поднимем текст команды из ресурсов
                    stream.CopyTo(memoryStream);
                    byte[] bytes = memoryStream.ToArray();
                    string commandFormat = Encoding.UTF8.GetString(bytes);

                    ProcessStartInfo psi = new ProcessStartInfo("cmd.exe",
                        String.Format(commandFormat, AppDomain.CurrentDomain.BaseDirectory));
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    Process.Start(psi);
                    Application.Exit();
                }
            }
        }
    }
}
