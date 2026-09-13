using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Reminder.Resources.Storage
{
    public class StringSecret
    {
        public StringSecret()
        { 
        }

        public Guid Key { get; set; }
        public string Value { get; set; }
    }

    [Serializable]
    public class StringSecrets
    {
        public static string FilePath => Path.Combine(AssemblyContext.AppDataPath, "strings.xml");

        private readonly List<StringSecret> _items;

        public StringSecret[] Items
        {
            get { return _items.ToArray(); }
            set
            {
                _items.Clear();
                _items.AddRange(value);
            }
        }

        public StringSecrets()
        { 
            _items = new List<StringSecret>();
        }

        public string Get(Guid key)
        {
            StringSecret existing = _items.SingleOrDefault(x => x.Key == key);
            return existing?.Value;
        }

        public void Add(StringSecret secret)
        {
            _items.Add(secret);
            Save();
        }

        public void Update(Guid key, StringSecret secret)
        {
            StringSecret existing = _items.SingleOrDefault(x => x.Key == key);
            if (existing != null)
            {
                existing.Key = secret.Key;
                existing.Value = secret.Value;
                Save();
            }
        }
        
        public bool Remove(Guid key)
        {
            StringSecret existing = _items.SingleOrDefault(x => x.Key == key);
            if (existing != null)
            {
                bool result = _items.Remove(existing);
                Save();
                return result;
            }
            return false;
        }

        private void Save()
        {
            using (Stream stream = File.Create(FilePath))
            {
                XmlSerializer serializer = new XmlSerializer(GetType());
                serializer.Serialize(stream, this);
            }
        }
    }
}
