using System.ComponentModel;

namespace Reminder.Models
{
    /// <summary>
    /// Периодичность.
    /// </summary>
    public enum Periodicity
    {
        [Description("Один раз")] Once,
        [Description("Ежедневно")] Daily,
        [Description("Еженедельно")] Weekly,
        [Description("Ежемесячно")] Monthly,
        [Description("Ежеквартально")] Quarterly,
        [Description("Ежегодно")] Annually
    }

    /// <summary>
    /// Общий рингтон в настройках.
    /// </summary>
    public enum RingtoneMode
    {
        NoSound,
        Default,
        Custom,
        SpeechSynthesizer
    }

    /// <summary>
    /// Рингтон индивидуально для уведомления.
    /// </summary>
    public enum ReminderRingtone
    {
        [Description("Из настроек")] AsIs = -1,
        [Description("Без звука")] NoSound = RingtoneMode.NoSound,
        [Description("По умолчанию")] Default = RingtoneMode.Default,
        [Description("Озвучить текст")] SpeechSynthesizer = RingtoneMode.SpeechSynthesizer
    }

    public enum DeleteReminderItemsMode
    {
        Confirm,
        Immediately
    }

    public enum NotificationsWindowPosition
    {
        Center,
        RightBottom
    }

}
