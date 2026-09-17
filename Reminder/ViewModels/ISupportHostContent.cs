using System;

namespace Reminder.ViewModels
{
    public interface ISupportHostContent
    {
        HostContent GetHostContent();
        /// <summary>
        /// Отображать ли previeм, а не содержимое.
        /// </summary>
        bool IsHostContentAllowed { get; set; }
        event EventHandler IsHostContentAllowedChanged;
    }
}
