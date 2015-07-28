using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace HeapSurf
{
    public static class FileHelper
    {
        public static void Save<T>(T obj, string filePath)
        {
            try
            {
                using (var writer = new System.IO.StreamWriter(filePath))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(writer, obj);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving: ", ex);
            }
        }

        public static T Load<T>(string filePath)
        {
            if (File.Exists(filePath))
            {
                var serializer = new XmlSerializer(typeof(T));
                using (TextReader reader = new StreamReader(filePath))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
            return default(T);
        }
    }
}
