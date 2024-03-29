using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.Session
{
    [BsonIgnoreExtraElements]
    public class SessionEntity
    {
        public string UserId { get; set; }

        public string Token { get; set; }

        public DateTime StartTime {  get; set; }

        public DateTime EndTime { get; set; }

        public string Device {  get; set; }
    }
}
