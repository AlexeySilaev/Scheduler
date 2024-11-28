using System;
using System.Collections.Generic;

namespace Scheduler.Models
{
    internal class SchedulerModel
    {
        public static SchedulerModel Instance { get; }

        static SchedulerModel()
        {
            Instance = new SchedulerModel();
        }

        public List<AlarmItem> Alarms { get; }
        public List<EventItem> Events { get; }
        public List<NoteItem> Notes { get; }
        public Settings Settings { get; }

        private SchedulerModel()
        {
            Alarms = new List<AlarmItem>();
            Events = new List<EventItem>();
            Notes = new List<NoteItem>();
            Settings = new Settings();
        }

        public void Load()
        {
            Alarms.AddRange(new[]
            {
                new AlarmItem { Text = "Через 20 минут", AlarmTime = TimeSpan.FromMinutes(20) },
                new AlarmItem { Text = "Через 15 минут" }
            });
            Events.AddRange(new[]
            {
                new EventItem
                {
                    Text = "Разово",
                },

                new EventItem
                {
                    Text = "Каждый день", EventDate = new DateTime(2024, 11, 22, 11, 00, 0),
                    Periodicity = Periodicity.Daily
                },
                new EventItem
                {
                    Text = "Каждую неделю", EventDate = new DateTime(2024, 11, 22, 16, 30, 0),
                    Periodicity = Periodicity.Weekly
                },
                new EventItem
                {
                    Text = "Каждый месяц", EventDate = new DateTime(2024, 11, 22, 16, 30, 0),
                    Periodicity = Periodicity.Monthly
                },
                new EventItem
                {
                    Text = "Каждый квартал", EventDate = new DateTime(2024, 11, 22, 16, 30, 0),
                    Periodicity = Periodicity.Quarterly
                },
                new EventItem
                {
                    Text = "Каждый год", EventDate = new DateTime(2024, 11, 22, 16, 30, 0),
                    Periodicity = Periodicity.Annually
                },

            });
            Notes.AddRange(new[]
            {
                new NoteItem { Text = "Сия гиперкапитальнейшая заметка не такая длинная как лорен ипсум" },
                new NoteItem
                {
                    Text =
                        "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo"
                }
            });
            Alarms[0].AlarmTime = TimeSpan.FromSeconds(2);
            Alarms[1].AlarmTime = TimeSpan.FromSeconds(3);
            Events.ForEach(e => e.EventDate = DateTime.Now.AddSeconds(3));
        }

        public void Save()
        {
        }
    }
}
