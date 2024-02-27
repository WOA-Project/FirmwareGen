using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace FirmwareGen
{
    public static class XmlUtils
    {
        public static void Serialize<T>(this T type, string path) where T : class
        {
            XmlSerializer serializer = new(type.GetType());
            using FileStream writer = new(path, FileMode.Create);
            serializer.Serialize(writer, type);
        }

        public static T Deserialize<T>(string path) where T : class
        {
            T type;
            XmlSerializer serializer = new(typeof(T));
            using (XmlReader reader = XmlReader.Create(path))
            {
                type = serializer.Deserialize(reader) as T;
            }
            return type;
        }
    }
}