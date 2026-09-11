namespace Reminder.ViewModels
{
    public interface ISupportHostContent
    {
        bool IsHostContentAllowed { get; }
        HostContent GetHostContent();
        void SetHostContentAllowed(bool value);
    }
}
