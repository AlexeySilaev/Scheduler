using Scheduler.Models;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using Size = System.Windows.Size;

namespace Scheduler.Extensions
{
    internal static class WindowPositioning
    {
        public static void SetPosition(this Window window, NotificationsWindowPosition position)
        {
            if(position == NotificationsWindowPosition.Center)
                SetCentered(window);
            else if (position == NotificationsWindowPosition.RightBottom)
                SetRightBottom(window);
        }

        private static void SetCentered(Window window)
        {
            Size winSize = GetElementPixelSize(window);
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            window.Left = workingArea.Left + (workingArea.Width - winSize.Width) / 2;
            window.Top = workingArea.Top + (workingArea.Height - winSize.Height) / 2;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        private static void SetRightBottom(Window window)
        {
            Size winSize = GetElementPixelSize(window);
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            window.Left = workingArea.Left + workingArea.Width - winSize.Width;
            window.Top = workingArea.Top + workingArea.Height - winSize.Height;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        public static Size GetElementPixelSize(FrameworkElement element)
        {
            Matrix matrix;
            var source = PresentationSource.FromVisual(element);
            if (source != null)
            {
                matrix = source.CompositionTarget.TransformToDevice;
            }
            else
            {
                using (var src = new HwndSource(new HwndSourceParameters()))
                {
                    matrix = src.CompositionTarget.TransformToDevice;
                }
            }

            return new Size
            {
                Width = (int)(matrix.M11 * element.Width),
                Height = (int)(matrix.M22 * element.Height)
            };
        }
    }
}
