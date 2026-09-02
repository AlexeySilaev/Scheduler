using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using Reminder.Models;
using Size = System.Windows.Size;
using Point = System.Windows.Point;

namespace Reminder.Extensions
{
    internal static class WindowPositioning
    {
        private const double SCREEN_EDGE_INDENT = 5.0;
        
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
            Point location = GetCenteredLocation(new Size(window.Width, window.Height), factor);
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            window.Left = location.X;
            window.Top = location.Y;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        private static Point GetCenteredLocation(Size windowSize, Size factor)
        {
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            return new Point
            {
                X = workingArea.Left + (workingArea.Width - windowSize.Width * factor.Width) / factor.Width / 2,
                Y = workingArea.Top + (workingArea.Height - windowSize.Height * factor.Height) / factor.Height / 2
            };
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
            var source = PresentationSource.FromVisual(element);
            if (source != null)
            {
                Matrix matrix = source.CompositionTarget.TransformToDevice;
                return new Size
                {
                    Width = matrix.M11,
                    Height = matrix.M22
                };
            }

            return ExecuteWithScaleFactor (scaleFactor => scaleFactor);
        }

        public static Rect GetRightBottomBounds(Size elementSize)
        {
            return ExecuteWithScaleFactor((sf) =>
            {
                var workingArea = Screen.PrimaryScreen.WorkingArea;
                double left = workingArea.Left +
                    (workingArea.Width - elementSize.Width * sf.Width) / sf.Width - SCREEN_EDGE_INDENT;
                double top = workingArea.Top +
                    (workingArea.Height - elementSize.Height * sf.Height) / sf.Height - SCREEN_EDGE_INDENT;
                return new Rect(left, top, elementSize.Width * sf.Width, elementSize.Height * sf.Height);
            });
        }

        public static Point GetValidLocation(Rect windowBounds, bool setCentered)
        {
            return ExecuteWithScaleFactor((scaleFactor) =>
            {
                if (setCentered)
                {
                    Point centeredLocation = GetCenteredLocation(windowBounds.Size, scaleFactor);
                    windowBounds.Location = centeredLocation;
                }
                // проверим, что границы окна попадают полностью в какой-либо экран
                Screen windowScreen = Screen.FromRectangle(ToRectangle(windowBounds)) ?? Screen.PrimaryScreen;
                var workingArea = windowScreen.WorkingArea;
                if (windowBounds.X < workingArea.Left) windowBounds.X = workingArea.Left + SCREEN_EDGE_INDENT;
                if (windowBounds.Y < workingArea.Top) windowBounds.Y = workingArea.Top + SCREEN_EDGE_INDENT;
                if (windowBounds.Right > workingArea.Right)
                    windowBounds.X -= windowBounds.Right - workingArea.Right + SCREEN_EDGE_INDENT;
                if (windowBounds.Bottom > workingArea.Bottom)
                    windowBounds.Y -= windowBounds.Bottom - workingArea.Bottom + SCREEN_EDGE_INDENT;
                return windowBounds.Location;
            });
        }

        private static T ExecuteWithScaleFactor<T>(Func<Size, T> func)
        {
            using (var src = new HwndSource(new HwndSourceParameters()))
            {
                Matrix matrix = src.CompositionTarget.TransformToDevice;
                Size scaleFactor = new Size
                {
                    Width = matrix.M11,
                    Height = matrix.M22
                };
                return func(scaleFactor);
            }
        }

        private static Rectangle ToRectangle(Rect rect)
        {
            return new Rectangle(Convert.ToInt32(rect.Left), Convert.ToInt32(rect.Top),
                Convert.ToInt32(rect.Width), Convert.ToInt32(rect.Height));
        }
    }
}
