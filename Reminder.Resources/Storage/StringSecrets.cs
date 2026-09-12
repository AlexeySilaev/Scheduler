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
        public const string FILE_NAME = "strings.xml";

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

        public void Save(Stream stream)
        {
            XmlSerializer serializer = new XmlSerializer(GetType());
            serializer.Serialize(stream, this);
        }

        public string Get(Guid key)
        {
            StringSecret existing = _items.SingleOrDefault(x => x.Key == key);
            return existing?.Value;
        }

        public void Add(StringSecret secret)
        {
            _items.Add(secret);
        }

        public void Update(Guid key, StringSecret secret)
        {
            StringSecret existing = _items.SingleOrDefault(x => x.Key == key);
            if (existing != null)
            {
                existing.Key = secret.Key;
                existing.Value = secret.Value;
            }
        }
        
        public bool Remove(Guid key)
        {
            StringSecret existing = _items.SingleOrDefault(x => x.Key == key);
            if (existing != null)
                return _items.Remove(existing);
            return false;
        }
    }
}
