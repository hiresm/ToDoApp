using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;

namespace Hires.ToDo.Services
{
    public class PersistationService : IPersistationService
    {
        public async Task<T> LoadData<T>(string fileName) where T : class
        {
            var file = await GetFile(fileName);
            
            using (StreamReader sr = new StreamReader(file.Path))
            {
                var serializer = new XmlSerializer(typeof(T));
                try
                {
                    return (T)serializer.Deserialize(sr);
                }
                catch (Exception)
                {
                }

                return Activator.CreateInstance<T>();
            }
        }

        private async Task<IStorageItem> GetFile(string fileName)
        {
            var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(fileName);

            if (file != null)
                return file;

            return await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName);
        }

        public async Task SaveData<T>(T data, string fileName) where T : class
        {
            var file = await GetFile(fileName);

            using (StreamWriter sw = new StreamWriter(file.Path))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(sw, data);
            }
        }
    }
}
