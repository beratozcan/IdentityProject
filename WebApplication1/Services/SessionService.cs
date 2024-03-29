using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace WebApplication1.Session
{
    public class SessionService : ISessionService
    {
        private readonly IMongoCollection<SessionEntity> sessionCollection;

        public SessionService(IOptions<SessionDbSettings> sessionDbSettings)
        {
            var mongoClient = new MongoClient(sessionDbSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(sessionDbSettings.Value.DatabaseName);

            sessionCollection = mongoDatabase.GetCollection<SessionEntity>(sessionDbSettings.Value.SessionCollectionName);
        }
        public async Task AddSessionAsync(SessionEntity sessionDetails)
        {
            await sessionCollection.InsertOneAsync(sessionDetails);
        }
        public async Task DeleteSessionAsync(string userId, string device)
        {
            await sessionCollection.DeleteOneAsync(x => x.UserId == userId && x.Device == device);
        }
        public async Task<SessionEntity> GetSessionDetailByIdAsync(string userId , string device)
        {
            return await sessionCollection.Find(x => x.UserId == userId && x.Device == device).FirstOrDefaultAsync();
            
        }
        public async Task<List<SessionEntity>> SessionListAsync()
        {
            return await sessionCollection.Find(_ => true).ToListAsync();
        }
        public async Task UpdateSessionAsync(string userId,string device, SessionEntity sessionDetails)
        {
            await sessionCollection.ReplaceOneAsync(x => x.UserId == userId && x.Device == device, sessionDetails);
        }
    }
}
