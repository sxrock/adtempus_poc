using BlazorApp1.Model;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace BlazorApp1
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly IConfiguration _configuration;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(ProtectedSessionStorage sessionStorage, IConfiguration configuration)
        {
            _sessionStorage = sessionStorage ?? throw new ArgumentNullException(nameof(sessionStorage));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Console.WriteLine("CustomAuthenticationStateProvider: Initialized");
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userSessionResult = await _sessionStorage.GetAsync<UserSession>("UserSession");
                Console.WriteLine($"CustomAuthenticationStateProvider: Session retrieval success={userSessionResult.Success}, Value={(userSessionResult.Success ? $"UserId={userSessionResult.Value?.UserId}, Role={userSessionResult.Value?.Role}" : "null")}");
                if (userSessionResult.Success && userSessionResult.Value != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, userSessionResult.Value.UserId),
                        new Claim(ClaimTypes.Role, userSessionResult.Value.Role)
                    };
                    var identity = new ClaimsIdentity(claims, authenticationType: "CustomAuth"); // Ensure non-empty authenticationType
                    var principal = new ClaimsPrincipal(identity);
                    Console.WriteLine($"CustomAuthenticationStateProvider: Returning authenticated state, IsAuthenticated={principal.Identity.IsAuthenticated}");
                    return new AuthenticationState(principal);
                }
                else
                {
                    Console.WriteLine("CustomAuthenticationStateProvider: No session found or session invalid");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CustomAuthenticationStateProvider: Error in GetAuthenticationStateAsync - {ex.Message}, StackTrace: {ex.StackTrace}");
            }
            Console.WriteLine("CustomAuthenticationStateProvider: Returning anonymous state");
            return new AuthenticationState(_anonymous);
        }

        public async Task AuthenticateUser(string userId, string password)
        {
            try
            {
                Console.WriteLine($"CustomAuthenticationStateProvider: AuthenticateUser called for UserId={userId}");
                var users = _configuration.GetSection("Users").Get<List<UserConfig>>();
                if (users == null)
                {
                    Console.WriteLine("CustomAuthenticationStateProvider: Users section is null or missing in appsettings.json");
                    await _sessionStorage.DeleteAsync("UserSession");
                    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
                    return;
                }

                Console.WriteLine($"CustomAuthenticationStateProvider: Loaded {users.Count} users from appsettings.json");
                var user = users.FirstOrDefault(u => u.UserId == userId && u.Password == password);
                if (user != null)
                {
                    Console.WriteLine($"CustomAuthenticationStateProvider: User authenticated - UserId={user.UserId}, Role={user.Role}");
                    var userSession = new UserSession { UserId = user.UserId, Role = user.Role };
                    await _sessionStorage.SetAsync("UserSession", userSession);
                    Console.WriteLine("CustomAuthenticationStateProvider: UserSession saved to ProtectedSessionStorage");

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserId),
                        new Claim(ClaimTypes.Role, user.Role)
                    };
                    var identity = new ClaimsIdentity(claims, authenticationType: "CustomAuth"); // Ensure non-empty authenticationType
                    var principal = new ClaimsPrincipal(identity);
                    Console.WriteLine($"CustomAuthenticationStateProvider: Created ClaimsPrincipal, IsAuthenticated={principal.Identity.IsAuthenticated}");
                    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
                }
                else
                {
                    Console.WriteLine("CustomAuthenticationStateProvider: Authentication failed - invalid credentials");
                    await _sessionStorage.DeleteAsync("UserSession");
                    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CustomAuthenticationStateProvider: Error in AuthenticateUser - {ex.Message}, StackTrace: {ex.StackTrace}");
                await _sessionStorage.DeleteAsync("UserSession");
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                Console.WriteLine("CustomAuthenticationStateProvider: LogoutAsync called");
                await _sessionStorage.DeleteAsync("UserSession");
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CustomAuthenticationStateProvider: Error in LogoutAsync - {ex.Message}, StackTrace: {ex.StackTrace}");
            }
        }
    }
}