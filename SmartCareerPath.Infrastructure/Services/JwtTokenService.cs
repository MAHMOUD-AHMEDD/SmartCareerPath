using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartCareerPath.Domain.Entites.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartCareerPath.Infrastructure.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _config;
        private readonly SigningCredentials _credentials;

        public JwtTokenService(IConfiguration config)
        {

            _config = config;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:Secret"]!));

            _credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        }


        public string GenerateAccessToken(AppUser appUser, string role)
        {
            var jwtSettings = _config.GetSection("JwtSettings");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,appUser.Id),
                new Claim(ClaimTypes.Email,appUser.Email!),
                new Claim(ClaimTypes.Role,role),
                new Claim("first name",appUser.FirstName),
                new Claim("last name",appUser.LastName),
                new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    _config.GetValue<int>("JwtSettings:AccessTokenExpiryMinutes", 15)),
                signingCredentials: _credentials // reused — no allocation per call
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        public string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }


    }
}
