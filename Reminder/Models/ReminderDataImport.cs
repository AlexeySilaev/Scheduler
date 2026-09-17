using Reminder.Resources.Storage;
using System.IO;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace Reminder.Models
{
    internal class ReminderDataImport : DataImport
    {
        public ReminderDataImport(string importFilePath, bool skipExisting)
            : base(importFilePath, skipExisting)
        {
        }

        protected override void MergeData(string unpackFolderPath)
        {
            base.MergeData(unpackFolderPath);
            XmlSerializer xmlSerializer = XmlSerializer.FromTypes(new[] { typeof(Data) })[0];
            string dataPath = Path.Combine(unpackFolderPath, Consts.DATA_FILE_NAME);
            using (FileStream fs = new FileStream(dataPath, FileMode.OpenOrCreate))
            {
                Data data = xmlSerializer.Deserialize(fs) as Data;
                ReminderModel.Instance.Merge(data, _skipExisting);
            }
        }

        protected override void ClearUnusedData()
        {
            ReminderModel.Instance.ClearUnusedData();
        }
    }
}
