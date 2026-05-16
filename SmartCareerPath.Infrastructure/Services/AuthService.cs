using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartCareerPath.Application.Constants;
using SmartCareerPath.Application.DTOs.Auth;
using SmartCareerPath.Application.Interfaces;
using SmartCareerPath.Domain.Entites;
using SmartCareerPath.Domain.Entites.Identity;
using SmartCareerPath.Infrastructure.Persistence;

namespace SmartCareerPath.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _db;
        private readonly JwtTokenService _jwt;
        private readonly IConfiguration _config;

        public AuthService(
            UserManager<AppUser> userManager,
            AppDbContext db,
            JwtTokenService jwt,
            IConfiguration config)
        {
            _userManager = userManager;
            _db = db;
            _jwt = jwt;
            _config = config;
        }


        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedAccessException("Invalid credentials.");


            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault()
                ?? throw new UnauthorizedAccessException("User has no assigned role.");
            return await GenerateAndSaveTokensAsync(user, role);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var hashed = _jwt.HashToken(refreshToken);
            var storedToken = await _db.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == hashed)
                ?? throw new UnauthorizedAccessException("Invalid refresh token.");

            if (storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token expired or revoked.");

            storedToken.IsRevoked = true;
            await _db.SaveChangesAsync();

            // guard against roleless users
            var roles = await _userManager.GetRolesAsync(storedToken.User);
            var role = roles.FirstOrDefault()
                ?? throw new UnauthorizedAccessException("User has no assigned role.");
            return await GenerateAndSaveTokensAsync(storedToken.User, role);
        }

        public async Task<AuthResponseDto> RegisterMentorAsync(RegisterMentorDto dto)
        {
            // 1. Check if email already exists
            if (await _userManager.FindByEmailAsync(dto.Email) is not null)
                throw new InvalidOperationException("Email already registered.");

            // 2. Create Mentor entity
            var mentor = new Mentor
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                YearsOfExperience = dto.YearsOfExperience,
                Company = dto.Company,
                Description = dto.Description,
                LinkedIn = dto.LinkedIn,
                CurrentJobId = dto.CurrentJobId,
                TrackId = dto.TrackId
            };

            // 3. Create via UserManager (handles password hashing)
            var result = await _userManager.CreateAsync(mentor, dto.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            // 4. Assign role
            await _userManager.AddToRoleAsync(mentor, Roles.Mentor);

            // 5. Generate tokens and return
            return await GenerateAndSaveTokensAsync(mentor, Roles.Mentor);
        }

        public async Task<AuthResponseDto> RegisterSeekerAsync(RegisterSeekerDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) is not null)
                throw new InvalidOperationException("Email already registered");


            var seeker = new Seeker
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                LinkedIn = dto.LinkedIn,
                CurrentJobId = dto.CurrentJobId
            };

            var result = await _userManager.CreateAsync(seeker, dto.Password);

            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join(',', result.Errors.Select(r => r.Description)));

            await _userManager.AddToRoleAsync(seeker, Roles.Seeker);

            return await GenerateAndSaveTokensAsync(seeker, Roles.Seeker);


        }

        public async Task RevokeTokenAsync(string userId)
        {
            var token = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.UserId == userId);
            if (token is not null)
            {
                token.IsRevoked = true;
                await _db.SaveChangesAsync();
            }
        }



        private async Task<AuthResponseDto> GenerateAndSaveTokensAsync(AppUser user, string role)
        {
            var accessToken = _jwt.GenerateAccessToken(user, role);
            var refreshToken = _jwt.GenerateRefreshToken();
            var hashedRefresh = _jwt.HashToken(refreshToken);
            var expiry = DateTime.UtcNow.AddDays(
            _config.GetValue<int>("JwtSettings:RefreshTokenExpiryDays", 7));

            var existing = _db.RefreshTokens.FirstOrDefault(r => r.UserId == user.Id);


            if (existing is null)
                _db.RefreshTokens.Add(new RefreshToken
                { UserId = user.Id, Token = hashedRefresh, ExpiresAt = expiry });
            else
            {
                existing.Token = hashedRefresh;
                existing.ExpiresAt = expiry;
                existing.IsRevoked = false;
            }

            await _db.SaveChangesAsync();

            // Fix: read expiry from config — must match the actual JWT token expiry
            var accessExpiryMinutes = _config.GetValue<int>("JwtSettings:AccessTokenExpiryMinutes", 15);

            return new AuthResponseDto(
                AccessToken: accessToken,
                RefreshToken: refreshToken,   // raw token returned to client
                AccessTokenExpiry: DateTime.UtcNow.AddMinutes(accessExpiryMinutes),
                UserId: user.Id,
                Email: user.Email!,
                Role: role
            );


        }


    }
}
