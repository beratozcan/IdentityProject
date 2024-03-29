namespace WebApplication1.Session
{
    public interface ISessionService
    {
        public Task<List<SessionEntity>> SessionListAsync();
        public Task<SessionEntity> GetSessionDetailByIdAsync(string userId, string device);
        public Task AddSessionAsync(SessionEntity sessionDetails);
        public Task UpdateSessionAsync(string userId,string device, SessionEntity sessionDetails);
        public Task DeleteSessionAsync(String userId, string device);
    }
}
