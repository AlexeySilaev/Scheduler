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
        private string _filePath;

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

        /// <summary>
        /// Конструктор для десериализации из xml.
        /// </summary>
        public StringSecrets() : this(string.Empty)
        {
        }

        internal StringSecrets(string filePath)
        { 
            _filePath = filePath;
            _items = new List<StringSecret>();
        }

        internal void OnDeserialized(string fromFilePath)
        {
            _filePath = fromFilePath;
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

        internal void Import(StringSecrets importSecrets, bool skipExisting)
        {
            importSecrets._items.ForEach(importSecret =>
            {
                StringSecret thisSecret = _items.SingleOrDefault(s => s.Key == importSecret.Key);
                if (thisSecret == null)
                    _items.Add(importSecret);
                else if (skipExisting == false)
                    thisSecret.Value = importSecret.Value;
            });
            Save();
        }

        internal void RemoveExcept(Guid[] stringSecrets)
        {
            StringSecret[] keepExisting = _items
                .Where(item => stringSecrets.Contains(item.Key)).ToArray();
            Items = keepExisting;
            Save();
        }

        private void Save()
        {
            using (Stream stream = File.Create(_filePath))
            {
                XmlSerializer serializer = new XmlSerializer(GetType());
                serializer.Serialize(stream, this);
            }
        }
    }
}
