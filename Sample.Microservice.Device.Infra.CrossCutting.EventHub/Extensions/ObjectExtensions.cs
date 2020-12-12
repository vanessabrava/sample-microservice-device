using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Sample.Microservice.Device.Infra.CrossCutting.EventHub.Extensions
{
    internal static class ObjectExtensions
    {
        internal static byte[] ToByteArray<TObject>(this TObject obj)
        {
            if (obj == null) return null;

            var bf = new BinaryFormatter();
            using var ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        internal static TObject FromByteArray<TObject>(this byte[] data)
        {
            if (data == null) return default;

            var bf = new BinaryFormatter();
            using var ms = new MemoryStream(data);
            object obj = bf.Deserialize(ms);
            return (TObject)obj;

        }
    }
}
