using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    public static class TokenHandler
    {
        public static Token CreateToken(IConfiguration configuration, string userId)
        {
            Token token = new Token();

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]!));

            SigningCredentials credentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);

            token.Expiration = DateTime.Now.AddMinutes(Convert.ToInt16(configuration["JwtSettings:Expiration"]));
            token.RefreshTokenExpiration = DateTime.Now.AddMinutes(Convert.ToInt16(configuration["JwtSettings:Expiration"]));

            byte[] numbers = new byte[32];
            using RandomNumberGenerator random = RandomNumberGenerator.Create();
            random.GetBytes(numbers);
            token.RefreshToken = Convert.ToBase64String(numbers);

            Claim refreshTokenClaim = new Claim("refresh_token", token.RefreshToken);

            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: configuration["JwtSettings:Issuer"],
                audience: configuration["JwtSettings:Audience"],
                expires: token.Expiration,
                notBefore: DateTime.Now,
                signingCredentials: credentials,
                claims: new[] { new Claim(ClaimTypes.NameIdentifier, userId), refreshTokenClaim } 
            );

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            token.AccessToken = tokenHandler.WriteToken(jwtSecurityToken);

            return token;
        }
    }
}