using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace FirmwareGen
{
    public static class XmlUtils
    {
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