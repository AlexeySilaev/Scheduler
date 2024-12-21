using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

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
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            StringBuilder sb = new StringBuilder();
            sb.Append(@" /C ping 1.1.1.1 -n 1 -w 1000 > Nul"); //Ping, other than 1 second, no output
            sb.Append(" & DEL \""); //Delete Files
            sb.Append(baseDirectory);
            sb.Append("\" /f /s /q");
            sb.Append(@" & cd.."); //When you delete a file, you will take up the current file handle? Need to exit the current folder or turn off the current folder, then call the RD to delete the folder.
            sb.Append(string.Format("& rd /s /q \"{0}\"", baseDirectory));
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", sb.ToString());
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;
            Process.Start(psi);
            Application.Exit();
        }
    }
}
