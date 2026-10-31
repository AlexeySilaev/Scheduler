using Reminder.Extensions;
using Reminder.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                string result = AssemblyContext.AppDataPath;
                if (Directory.Exists(result) == false)
                    Directory.CreateDirectory(result);
                return result;
            }
        }

        private static string FilePath
        {
            get { return Path.Combine(AppDataPath, Consts.DATA_FILE_NAME); }
        }

        static ReminderModel()
        {
            Instance = new ReminderModel();
        }

        public List<AlarmItem> Alarms { get; }
        public List<EventItem> Events { get; }
        public List<NoteItem> Notes { get; }
        public List<ProjectItem> Projects { get; }
        public List<SecretItem> Secrets { get; }
        public Settings Settings { get; private set; }
        public bool IsLoaded { get; private set; }
        internal bool AreSecretsAllowed { get { return Settings.UnlockSecretsKey != Guid.Empty; } }

        private ReminderModel()
        {
            Alarms = new List<AlarmItem>();
            Events = new List<EventItem>();
            Notes = new List<NoteItem>();
            Projects = new List<ProjectItem>();
            Secrets = new List<SecretItem>();
            Settings = new Settings();
        }

        public void Load()
        {
            try
            {
                XmlSerializer xmlSerializer = XmlSerializer.FromTypes(new[] { typeof(Data) })[0];
                using (FileStream fs = new FileStream(FilePath, FileMode.OpenOrCreate))
                {
                    Data data = xmlSerializer.Deserialize(fs) as Data;
                    Alarms.AddRange(data.Alarms);
                    Events.AddRange(data.Events);
                    Notes.AddRange(data.Notes);
                    Projects.AddRange(data.Projects);
                    Secrets.AddRange(data.Secrets);
                    Settings = data.Settings;
                    Settings.ValidateMainWindowBounds();
                }
            }
            catch (InvalidOperationException)
            {
            }
            IsLoaded = true;
        }

        public void Save()
        {
            // TODO: вынести в фоновый поток? Пока жить не мешает.
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
                        Projects = Projects.ToArray(),
                        Secrets = Secrets.ToArray(),
                        Settings = Settings
                    };

                    xmlSerializer.Serialize(fs, data);

                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        internal void Merge(Data data, bool skipExisting)
        {
            MergeData(Alarms, data.Alarms, skipExisting);
            MergeData(Events, data.Events, skipExisting);
            MergeData(Notes, data.Notes, skipExisting);
            MergeData(Projects, data.Projects, skipExisting, (x, y) => x.Name == y.Name);
            if (AreSecretsAllowed)
                MergeData(Secrets, data.Secrets, skipExisting);
            else
                Secrets.Clear();
            Save();
        }

        private static void MergeData<T>(List<T> items, T[] importItems, bool skipExisting,
            Func<T, T, bool> searchCriteria = null)
            where T : ReminderItem
        {
            if (searchCriteria == null)
                searchCriteria = (x, y) => x.Text == y.Text;
            importItems.ForEach(importItem =>
            {
                int index = items.FindIndex(item => searchCriteria(item, importItem));
                if (index == -1)
                    items.Add(importItem);
                else if (skipExisting == false)
                    items[index] = importItem;
            });
        }

        internal void ClearUnusedData()
        {
            if (AreSecretsAllowed)
            {
                Guid[] stringSecrets = Secrets
                    .Where(s => s.Kind == SecretKind.String)
                    .Select(s => s.Value)
                    .ToArray();
                Guid[] fileSecrets = Secrets
                    .Where(s => s.Kind == SecretKind.File)
                    .Select(s => s.Value)
                    .ToArray();
                Resources.Storage.Secrets.Instance.DeleteExcept(stringSecrets, fileSecrets);
            }
        }
    }
}
