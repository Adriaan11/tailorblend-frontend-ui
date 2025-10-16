namespace BlazorConsultant.Services;

public interface IPasswordAuthService
{
    Task<bool> ValidatePasswordAsync(string password);
    Task LoginAsync();
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
}
