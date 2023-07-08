using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Server.Helper
{
    public static class Serializer
    {
        public static byte[] Serialize<T>(T thingy)
        {
            MemoryStream ms = new MemoryStream();
            using (var writer = new BsonWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, thingy);
            }

            return ms.ToArray();
        }

        public static T Deserialize<T>(byte[] body)
        {
            MemoryStream ms = new MemoryStream(body);
            using (var reader = new BsonReader(ms))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader)!;
            }
        }
    }
}
