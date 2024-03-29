using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Session;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _appDbContext;
        private readonly ISessionService _sessionService;
        public WeatherForecastController(ILogger<WeatherForecastController> logger, UserManager<IdentityUser> userManager, AppDbContext appDbContext, ISessionService sessionService)
        {
            _logger = logger;
            _userManager = userManager;
            _appDbContext = appDbContext;
            _sessionService = sessionService;
        }

        [HttpGet(Name = "GetWeatherForecast"), Authorize(Roles="Admin")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {            
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast

            {
               Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
               TemperatureC = Random.Shared.Next(-20, 55),
               Summary = Summaries[Random.Shared.Next(Summaries.Length)]
             }).ToArray();
        }
    }
}
