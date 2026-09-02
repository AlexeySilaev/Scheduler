using System;
using System.Collections.Generic;
using System.Globalization;
using Reminder.Converters;
using Reminder.Models;

namespace Reminder.Extensions
{
    internal static class EnumExtensions
    {
        public static string GetDescription(this Enum enumMember)
        {
            EnumToStringConverter converter = new EnumToStringConverter();
            return converter.Convert(enumMember, enumMember.GetType(), null, CultureInfo.InvariantCulture)
                .ToString();
        }

        private static List<EnumInfo> _periodicityInfos;
        public static List<EnumInfo> PeriodicityInfos
        {
            get
            {
                if (_periodicityInfos == null)
                    _periodicityInfos = GetEnumInfos<Periodicity>();
                return _periodicityInfos;
            }
        }

        private static List<EnumInfo> _ringtoneInfos;
        public static List<EnumInfo> RingtoneInfos
        {
            get
            {
                if (_ringtoneInfos == null)
                {
                    _ringtoneInfos = new List<EnumInfo>
                    {
                        new EnumInfo(ReminderRingtone.Default, GetDescription(ReminderRingtone.Default)),
                        new EnumInfo(ReminderRingtone.SpeechSynthesizer, GetDescription(ReminderRingtone.SpeechSynthesizer)),
                        new EnumInfo(ReminderRingtone.NoSound, GetDescription(ReminderRingtone.NoSound)),
                    };
                }
                return _ringtoneInfos;
            }
        }

        private static List<EnumInfo> GetEnumInfos<TEnum>() where TEnum : Enum
        {
            TEnum[] array = (TEnum[])Enum.GetValues(typeof(TEnum));

            List<EnumInfo> result = new List<EnumInfo>();
            array.ForEach(entry => result.Add(new EnumInfo(entry, entry.GetDescription())));
            return result;
        }
    }

    public class EnumInfo
    {
        public Enum Member { get; }
        public string Description { get; }

        public EnumInfo(Enum member, string description)
        {
            Member = member;
            Description = description;
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
