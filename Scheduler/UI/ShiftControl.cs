using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Scheduler.UI
{
    public class ShiftControl : Control
    {
        public static readonly DependencyProperty LeftContentProperty =
            DependencyProperty.Register("LeftContent", typeof(FrameworkElement), typeof(ShiftControl));
        public FrameworkElement LeftContent
        {
            get { return (FrameworkElement)GetValue(LeftContentProperty); }
            set { SetValue(LeftContentProperty, value); }
        }

        public static readonly DependencyProperty RightContentProperty =
            DependencyProperty.Register("RightContent", typeof(FrameworkElement), typeof(ShiftControl));
        public FrameworkElement RightContent
        { 
            get { return (FrameworkElement)GetValue(RightContentProperty); }
            set { SetValue(RightContentProperty, value); }
        }

        public static readonly DependencyProperty IsShiftedProperty =
            DependencyProperty.Register("IsShifted", typeof(bool), typeof(ShiftControl),
                new PropertyMetadata(OnIsShiftedChangedCallBack));

        private static void OnIsShiftedChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ShiftControl fc = sender as ShiftControl;
            if (fc != null)
                fc.OnIsShiftedChanged();
        }

        public bool IsShifted
        {
            get { return (bool)GetValue(IsShiftedProperty); }
            set { SetValue(IsShiftedProperty, value); }
        }

        public ShiftControl()
        {
            ClipToBounds = true;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var shiftPanel = (FrameworkElement)GetTemplateChild("shiftPanel");
            shiftPanel.Width = 2 * arrangeBounds.Width;
            shiftPanel.Arrange(new Rect(0, 0, arrangeBounds.Width, arrangeBounds.Height));
            if (IsShifted)
            {
                // просто установить отступ слева в ActualWidth эффекта не дает
               // PerformShift(shiftPanel, TimeSpan.Zero);
            }
            return arrangeBounds;
        }

        private void OnIsShiftedChanged()
        {
            var shiftPanel = (FrameworkElement)GetTemplateChild("shiftPanel");
            PerformShift(shiftPanel, TimeSpan.FromMilliseconds(250));
        }

        private void PerformShift(FrameworkElement shiftPanel, TimeSpan duration)
        {
            ThicknessAnimation animation = new ThicknessAnimation();
            if (!this.IsShifted)
            {
                animation.From = new Thickness(-ActualWidth, 0, 0, 0);
                animation.To = new Thickness(0);
            }
            else
            {
                animation.From = new Thickness(0);
                animation.To = new Thickness(-ActualWidth, 0, 0, 0);
            }

            animation.Duration = duration;
            animation.AccelerationRatio = 1.0;
            shiftPanel.BeginAnimation(MarginProperty, animation);
        }
    }
}
