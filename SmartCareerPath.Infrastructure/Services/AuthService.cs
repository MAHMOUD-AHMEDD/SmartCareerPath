using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCareerPath.Application;
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
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<AppUser> userManager,
    AppDbContext db,
    JwtTokenService jwt,
    IConfiguration config,
    IEmailService emailService,
    IOptions<EmailSettings> emailSettings,
    ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _db = db;
            _jwt = jwt;
            _config = config;
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }



        public async Task ConfirmEmailAsync(string email, string token)
        {
            // Bug fix 1: null guard — FluentValidation only covers [FromBody] DTOs, not [FromQuery]
            // params. Without this, a request with no query string binds null, and
            // token.Replace(" ", "+") throws NullReferenceException → 500 instead of a clean 400.
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Email and token are required.");

            var user = await _userManager.FindByEmailAsync(email);

            // Bug fix 2: uniform generic error for both "user not found" and "bad token" cases.
            // Throwing KeyNotFoundException (→ 404) for unknown emails and InvalidOperationException
            // (→ 400) for bad tokens allows user enumeration purely from the status code.
            // ForgotPasswordAsync and ResendConfirmationEmailAsync correctly avoid this — same pattern here.
            const string genericError =
                "Email confirmation failed. The link may have expired. " +
                "Request a new one via POST /api/auth/resend-confirmation.";

            if (user is null)
                throw new InvalidOperationException(genericError);

            if (user.EmailConfirmed)
                throw new InvalidOperationException("Email is already confirmed.");

            // Identity tokens use Base64 which contains '+'. Some HTTP clients decode
            // '+' in query strings as a space. Replace spaces back to '+' as a safety net.
            var safeToken = token.Replace(" ", "+");
            var result = await _userManager.ConfirmEmailAsync(user, safeToken);

            if (!result.Succeeded)
                throw new InvalidOperationException(genericError);
        }

        public async Task ResendConfirmationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            // Security: same 200 response whether user exists or not — prevents enumeration.
            // Also silently skip already-confirmed accounts.
            if (user is null || user.EmailConfirmed) return;

            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _emailService.SendConfirmationEmailAsync(
                    user.Email!,
                    $"{user.FirstName} {user.LastName}",
                    BuildConfirmUrl(user.Email!, token));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend confirmation email to {Email}", email);
            }
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            // Security: always return the same response whether the user exists or not.
            if (user is null) return;

            try
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _emailService.SendPasswordResetEmailAsync(
                    user.Email!, $"{user.FirstName} {user.LastName}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            }
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);


            const string genericError = "Password reset failed. The token may be invalid or expired.";

            if (user is null)
                throw new InvalidOperationException(genericError);

            var safeToken = dto.Token.Replace(" ", "+");
            var result = await _userManager.ResetPasswordAsync(user, safeToken, dto.NewPassword);

            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        private string BuildConfirmUrl(string email, string token)
        {
            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(email);
            return $"{_emailSettings.BaseUrl.TrimEnd('/')}/api/auth/confirm-email" +
                   $"?email={encodedEmail}&token={encodedToken}";
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
                TrackId = dto.TrackId,
                Phone = dto.Phone
            };

            // 3. Create via UserManager (handles password hashing)
            var result = await _userManager.CreateAsync(mentor, dto.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            // 4. Assign role
            await _userManager.AddToRoleAsync(mentor, Roles.Mentor);



            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(mentor);
                await _emailService.SendConfirmationEmailAsync(
                    mentor.Email!,
                    $"{mentor.FirstName} {mentor.LastName}",
                    BuildConfirmUrl(mentor.Email!, token));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email}", mentor.Email);
            }

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
                CurrentJobId = dto.CurrentJobId,
                Phone = dto.Phone
            };

            var result = await _userManager.CreateAsync(seeker, dto.Password);

            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join(',', result.Errors.Select(r => r.Description)));

            await _userManager.AddToRoleAsync(seeker, Roles.Seeker);

            // Soft email send — registration succeeds even if delivery fails
            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(seeker);
                await _emailService.SendConfirmationEmailAsync(
                    seeker.Email!,
                    $"{seeker.FirstName} {seeker.LastName}",
                    BuildConfirmUrl(seeker.Email!, token));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email}", seeker.Email);
            }

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
                Role: role,
                EmailConfirmed: user.EmailConfirmed
            );


        }



        public async Task LogoutAsync(string userId)
        {
            var token = await _db.RefreshTokens
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (token is not null)
            {
                _db.RefreshTokens.Remove(token);
                await _db.SaveChangesAsync();
            }
        }


    }
}
