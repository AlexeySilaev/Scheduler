using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Reminder.Models
{
    internal class ReminderModel
    {
        public static ReminderModel Instance { get; }
        internal static string AppDataPath 
        {
            get
            {
                string result =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AV Reminder");
                if (Directory.Exists(result) == false)
                    Directory.CreateDirectory(result);
                return result;
            }
        }

        private static string FilePath
        {
            get { return Path.Combine(AppDataPath, "data.xml"); }
        }

        static ReminderModel()
        {
            Instance = new ReminderModel();
        }

        public List<AlarmItem> Alarms { get; }
        public List<EventItem> Events { get; }
        public List<NoteItem> Notes { get; }
        public Settings Settings { get; private set; }
        public bool IsLoaded { get; private set; }

        private ReminderModel()
        {
            Alarms = new List<AlarmItem>();
            Events = new List<EventItem>();
            Notes = new List<NoteItem>();
            Settings = new Settings();
        }

        public void Load()
        {
            if (File.Exists(FilePath) == false)
                return;
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Data));
                using (FileStream fs = new FileStream(FilePath, FileMode.OpenOrCreate))
                {
                    Data data = xmlSerializer.Deserialize(fs) as Data;
                    Alarms.AddRange(data.Alarms);
                    Events.AddRange(data.Events);
                    Notes.AddRange(data.Notes);
                    Settings = data.Settings;
                }
            }
            catch (InvalidOperationException)
            {
            }
            
           /* Alarms.AddRange(new[]
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
               Events.ForEach(e => e.EventDate = DateTime.Now.AddSeconds(3));*/
               IsLoaded = true;
        }

        public void Save()
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Data));
                using (FileStream fs = new FileStream(FilePath, FileMode.Create))
                {
                    Data data = new Data
                    {
                        Alarms = Alarms.ToArray(),
                        Events = Events.ToArray(),
                        Notes = Notes.ToArray(),
                        Settings = Settings
                    };

                    xmlSerializer.Serialize(fs, data);

                }
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}
