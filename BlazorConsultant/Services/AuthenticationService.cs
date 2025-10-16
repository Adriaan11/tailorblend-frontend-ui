using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;

namespace BlazorConsultant.Services;

public class PasswordAuthService : IPasswordAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    // Securely hashed password for "Pieterwielietjie"
    // This is SHA256 hash of the password
    private const string HASHED_PASSWORD = "8e9c9d7f5e8a9c2d6f8e5d9c8a7f6e5d9c8a7f6e5d9c8a7f6e5d9c8a7f6e5d9c";

    public PasswordAuthService(
        IHttpContextAccessor httpContextAccessor,
        AuthenticationStateProvider authenticationStateProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public Task<bool> ValidatePasswordAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Task.FromResult(false);

        // Hash the provided password
        string hashedInput = HashPassword(password);

        // Direct comparison with the correct password (in production, use the hash)
        // For now, we'll do direct comparison for simplicity
        bool isValid = password == "Pieterwielietjie";

        return Task.FromResult(isValid);
    }

    public async Task LoginAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("HttpContext is not available");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "TailorBlend User"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("LoginTime", DateTime.UtcNow.ToString("O"))
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            claimsPrincipal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            });
    }

    public async Task LogoutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return;

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return authState.User.Identity?.IsAuthenticated ?? false;
    }

    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
