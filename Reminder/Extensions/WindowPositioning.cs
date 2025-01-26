using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using Reminder.Models;
using Size = System.Windows.Size;

namespace Reminder.Extensions
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
            Size factor = GetScaleFactor(window);
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            window.Left = workingArea.Left + (workingArea.Width - window.Width * factor.Width) / factor.Width / 2;
            window.Top = workingArea.Top + (workingArea.Height - window.Height * factor.Height) / factor.Height / 2;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        private static void SetRightBottom(Window window)
        {
            Size factor = GetScaleFactor(window);
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            window.Left = workingArea.Left + (workingArea.Width  - window.Width * factor.Width) / factor.Width;
            window.Top = workingArea.Top + (workingArea.Height  - window.Height * factor.Height) / factor.Height;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        private static Size GetScaleFactor(FrameworkElement element)
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
                Width = matrix.M11, Height = matrix.M22
            };
        }

        public static Rect GetRightBottomBounds(Size elementSize)
        {
            using (var src = new HwndSource(new HwndSourceParameters()))
            {
                Matrix matrix = src.CompositionTarget.TransformToDevice;

                var workingArea = Screen.PrimaryScreen.WorkingArea;
                double left = workingArea.Left + (workingArea.Width - elementSize.Width * matrix.M11) / matrix.M11 - 5;
                double top = workingArea.Top + (workingArea.Height - elementSize.Height * matrix.M22) / matrix.M22 - 5;
                return new Rect(left, top, elementSize.Width * matrix.M11, elementSize.Height * matrix.M22);
            }
        }

    }
}
