using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Exceptions;
using WebApplication1.Services;
using WebApplication1.Session;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _appDbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ISessionService _sessionService;
        private readonly IEmailService _emailService;
       
        public AuthenticationController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration, AppDbContext appDbContext, RoleManager<IdentityRole> roleManager,ISessionService sessionService,IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _appDbContext = appDbContext;
            _roleManager = roleManager;
            _sessionService = sessionService;
            _emailService = emailService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(string email, string username,string password)
        {
            var user = new IdentityUser { UserName = username, Email = email };

            var result = await _userManager.CreateAsync(user, password);

            if(result.Succeeded)
            {
                var doesRoleExist = await _roleManager.RoleExistsAsync("User");

                if (!doesRoleExist)
                {
                    var role = new IdentityRole("User");
                    await _roleManager.CreateAsync(role);
                }

                await _userManager.AddToRoleAsync(user, "User"); 

                await _signInManager.SignInAsync(user, isPersistent: false);

                return Ok(new { message = "Registration successful" });
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost("Login")]

        public async Task<IActionResult> Login(string username, string password)
        {
          
            var result = await _signInManager.PasswordSignInAsync(username, password,isPersistent: false, lockoutOnFailure: false);

            if(result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(username);

                var token = TokenHandler.CreateToken(_configuration, user.Id);
                var accessToken = token.AccessToken;
                var refreshToken = token.RefreshToken;

                var refreshTokenEntity = new RefreshToken { UserId = user.Id, Token = refreshToken };

                var session = new SessionEntity
                {
                    UserId = user.Id,
                    Token = token.AccessToken,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(1),
                    Device = Request.Headers["User-Agent"].ToString(),
                };

                await _sessionService.AddSessionAsync(session);

                await _appDbContext.RefreshTokens.AddAsync(refreshTokenEntity);
                await _appDbContext.SaveChangesAsync();

                return Ok(new { AccessToken = accessToken,
                               RefreshToken = refreshToken }); 
            }
            else
                return Unauthorized(new { message = "Invalid email or password" });
            
        }

        [HttpPost("Logout")]
        [Authorize]

        public async Task<IActionResult> Logout()
        {

            var user = await _userManager.GetUserAsync(User) ?? throw new NotFoundException("user not found");

            var refreshTokenEntity = _appDbContext.RefreshTokens.FirstOrDefault(r => r.UserId == user.Id) ?? throw new NotFoundException("user not found");

            var userAgent = Request.Headers["User-Agent"].ToString();

            var session = _sessionService.GetSessionDetailByIdAsync(user.Id, userAgent) ?? throw new UnauthorizedAccessException("Kullanıcı login degil");

            await _sessionService.DeleteSessionAsync(user.Id, userAgent);

            _appDbContext.RefreshTokens.Remove(refreshTokenEntity);

            await _appDbContext.SaveChangesAsync();

            await _signInManager.SignOutAsync();

            return Ok();
        }

        [HttpPost("ChangeUserRole"), Authorize(Roles = "Admin")]

        public async Task<IActionResult> ChangeUserRole(string username, string newRole )
        {
            var user = await _userManager.FindByNameAsync(username) ?? throw new NotFoundException("Role' u degisecek boyle bir user yok");

            var _user = await _userManager.GetUserAsync(User) ?? throw new NotFoundException("User bulunamadi");

            var device = Request.Headers["User-Agent"].ToString();

            var session = await _sessionService.GetSessionDetailByIdAsync(_user.Id, device);

            if (session == null)
                throw new UnauthorizedAccessException("User login degil");

            var userRole = await _userManager.GetRolesAsync(user);

            if (userRole.Contains(newRole))
                return BadRequest("User has this role already");

            var result =await _userManager.AddToRoleAsync(user, newRole);

            if(!result.Succeeded)
                return StatusCode(500, $"Failed to add user to role: {result.Errors}");

            return Ok("User has been added to this role sucessfully");
        }

        [HttpPost("SendForgetPasswordMail")]
        public async Task<IActionResult> SendForgetPasswordMail(string email)
        {
            var doesUserHavePassword = await _userManager.FindByEmailAsync(email);

            if (doesUserHavePassword == null)
                return Ok("Email gonderildi");

            var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(doesUserHavePassword);

            await _emailService.SendResetPasswordEmail(passwordResetToken, doesUserHavePassword.Email);

            return Ok("Email gonderildi");
        }

        [HttpPost("ResetPassword")]

        public async Task<IActionResult> ResetPassword(string resetCode, string newPassword, string username)
        {
            var user = await _userManager.FindByNameAsync(username) ?? throw new NotFoundException("User bulunamadi");

            var result = await _userManager.ResetPasswordAsync(user, resetCode, newPassword);

            if (result.Succeeded)
            {
                return Ok("Password reset successful");
            }
            else
            {
                return BadRequest("Password reset failed");
            }
        } 
    }
}
