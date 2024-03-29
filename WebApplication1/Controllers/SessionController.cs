using Microsoft.AspNetCore.Mvc;
using WebApplication1.Session;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _sessionService;

        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
            
        }

        [HttpGet]
        public async Task<List<SessionEntity>> Get()
        {
            return await _sessionService.SessionListAsync();
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<SessionEntity>> Get(string userId)
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            var session = await _sessionService.GetSessionDetailByIdAsync(userId, userAgent);
            if (session is null)
                return NotFound();

            return session;
        }

        [HttpPost("CreateSession")]
        public async Task<IActionResult> Post(SessionEntity session)
        {
            session.EndTime = session.StartTime.AddHours(1);
            await _sessionService.AddSessionAsync(session);
            return Ok();
        }

        [HttpPut("UpdateSession")]
        public async Task<IActionResult> Update(string userId, SessionEntity session)
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            var _session = await _sessionService.GetSessionDetailByIdAsync(userId, userAgent);
            if (_session is null)
                return NotFound();

            await _sessionService.UpdateSessionAsync(userId, userAgent, session);
            return Ok();
        }

        [HttpDelete("DeleteSession")]
        public async Task<IActionResult> Delete(string userId)
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            var session = await _sessionService.GetSessionDetailByIdAsync(userId, userAgent);
            if (session is null)
                return NotFound();

            await _sessionService.DeleteSessionAsync(userId, userAgent);
            return Ok();
        }
    }
}
