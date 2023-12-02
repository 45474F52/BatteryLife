using System.IO;
using System.Runtime.Serialization.Json;

namespace BatteryLife.Serializer
{
    public class JSONSerializer<T> : Serializer<T>
    {
        private readonly DataContractJsonSerializer _jsonSerializer;
        private readonly string _path;

        public JSONSerializer(string path)
        {
            _jsonSerializer = new DataContractJsonSerializer(typeof(T));
            _path = path;
        }

        public override T Deserialize()
        {
            if (File.Exists(_path))
            {
                using (FileStream stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return (T)_jsonSerializer.ReadObject(stream);
                }
            }
            else
                return default;
        }

        public override void Serialize(T value)
        {
            FileStream stream = new FileStream(
                    _path,
                    File.Exists(_path) ? FileMode.Create : FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None);

            try
            {
                _jsonSerializer.WriteObject(stream, value);
            }
            catch
            {
                throw;
            }
            finally
            {
                stream.Close();
            }
        }
    }
}