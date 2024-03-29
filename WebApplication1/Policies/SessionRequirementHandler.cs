using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using System.Security.Claims;
using WebApplication1.Session;

namespace WebApplication1.Policies
{
    public class SessionRequirementHandler : AuthorizationHandler<SessionRequirement>
    {
        private readonly IMongoCollection<SessionEntity> _sessionCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionRequirementHandler(IMongoDatabase database, IHttpContextAccessor httpContextAccessor)
        {
            _sessionCollection = database.GetCollection<SessionEntity>("Session");
            _httpContextAccessor = httpContextAccessor;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SessionRequirement requirement)
        {
            var userAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            var userId = userIdClaim?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                context.Fail();
                throw new UnauthorizedAccessException("User bulunamadi");
            }

            var session = await _sessionCollection.Find(s => s.UserId == userId && s.Device == userAgent).FirstOrDefaultAsync();

            if (session != null)
            {
                context.Succeed(requirement);
                return;
            }
            context.Fail();
        }
    }
}
