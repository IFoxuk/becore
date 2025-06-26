using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using becore.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace becore.Services;

public class AuthService : AuthenticationStateProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthService> _logger;
    private readonly IJSRuntime _jsRuntime;
    private HttpClient? _httpClient;
    private readonly string TokenKey = "authToken";
    private readonly string UserKey = "currentUser";
    private readonly JsonSerializerOptions _jsonOptions;
    
    private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
    private UserResponse? _cachedUser;
    private readonly TaskCompletionSource<bool> _initializationTcs = new();
    private bool _isInitialized = false;
    
    public event Action<bool> AuthStateChanged = delegate { };
    
    public bool IsAuthenticated => _currentUser.Identity?.IsAuthenticated ?? false;
    public UserResponse? CurrentUser => _cachedUser;

    public AuthService(IHttpClientFactory httpClientFactory, ILogger<AuthService> logger, IJSRuntime jsRuntime)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsRuntime = jsRuntime;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        
        // Инициализация в фоновом режиме
        _ = InitializeAsync();
    }

    private HttpClient GetHttpClient()
    {
        if (_httpClient == null)
        {
            _httpClient = _httpClientFactory.CreateClient("ApiClient");
        }
        return _httpClient;
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest loginRequest)
    {
        try
        {
            var httpClient = GetHttpClient();
            var response = await httpClient.PostAsJsonAsync("api/auth/login", loginRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    await SetAuthenticationAsync(authResponse, loginRequest.RememberMe);
                    return new ApiResponse<AuthResponse> 
                    { 
                        IsSuccess = true, 
                        Message = "Вход выполнен успешно", 
                        Data = authResponse 
                    };
                }
            }
            
            // Более точная обработка ошибок на основе кода ответа
            var errorContent = await response.Content.ReadAsStringAsync();
            var message = response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "Неверное имя пользователя или пароль",
                System.Net.HttpStatusCode.BadRequest => "Некорректные данные для входа",
                System.Net.HttpStatusCode.InternalServerError => "Сервер временно недоступен. Попробуйте позже",
                System.Net.HttpStatusCode.ServiceUnavailable => "Сервис временно недоступен",
                _ => "Произошла ошибка при входе в систему"
            };
            
            _logger.LogWarning("Ошибка входа: {StatusCode}, Content: {Content}", response.StatusCode, errorContent);
            
            return new ApiResponse<AuthResponse> 
            { 
                IsSuccess = false, 
                Message = message 
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка сети при входе в систему");
            return new ApiResponse<AuthResponse> 
            { 
                IsSuccess = false, 
                Message = "Не удается подключиться к серверу. Проверьте подключение к интернету" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при входе в систему");
            return new ApiResponse<AuthResponse> 
            { 
                IsSuccess = false, 
                Message = "Произошла неожиданная ошибка. Попробуйте позже" 
            };
        }
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest registerRequest)
    {
        try
        {
            var httpClient = GetHttpClient();
            var response = await httpClient.PostAsJsonAsync("api/auth/register", registerRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    await SetAuthenticationAsync(authResponse, false);
                    return new ApiResponse<AuthResponse> 
                    { 
                        IsSuccess = true, 
                        Message = "Регистрация прошла успешно", 
                        Data = authResponse 
                    };
                }
            }
            
            // Более точная обработка ошибок на основе кода ответа
            var errorContent = await response.Content.ReadAsStringAsync();
            var message = response.StatusCode switch
            {
                System.Net.HttpStatusCode.Conflict => "Пользователь с таким именем или email уже существует",
                System.Net.HttpStatusCode.BadRequest => "Некорректные данные для регистрации",
                System.Net.HttpStatusCode.InternalServerError => "Сервер временно недоступен. Попробуйте позже",
                System.Net.HttpStatusCode.ServiceUnavailable => "Сервис временно недоступен",
                _ => "Произошла ошибка при регистрации"
            };
            
            _logger.LogWarning("Ошибка регистрации: {StatusCode}, Content: {Content}", response.StatusCode, errorContent);
            
            return new ApiResponse<AuthResponse> 
            { 
                IsSuccess = false, 
                Message = message 
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка сети при регистрации");
            return new ApiResponse<AuthResponse> 
            { 
                IsSuccess = false, 
                Message = "Не удается подключиться к серверу. Проверьте подключение к интернету" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при регистрации");
            return new ApiResponse<AuthResponse> 
            { 
                IsSuccess = false, 
                Message = "Произошла неожиданная ошибка. Попробуйте позже" 
            };
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return false;
            
        try
        {
            await SetAuthorizationHeaderAsync(token);
            var httpClient = GetHttpClient();
            var response = await httpClient.GetAsync("api/auth/me");
            
            // Если токен недействителен (401), только тогда выходим
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Token is unauthorized, logging out");
                await LogoutAsync();
                return false;
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            // Сетевые ошибки не должны приводить к сбросу аутентификации
            _logger.LogWarning(ex, "Network error during authentication check");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication check");
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserKey);
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", TokenKey);
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", UserKey);
        
        GetHttpClient().DefaultRequestHeaders.Authorization = null;
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        AuthStateChanged.Invoke(false);
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetTokenAsync();
        
        if (string.IsNullOrEmpty(token))
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            return new AuthenticationState(_currentUser);
        }

        try
        {
            // Устанавливаем токен в заголовки
            await SetAuthorizationHeaderAsync(token);
            
            // Парсим JWT токен локально для получения Claims
            var claims = ParseJwtToken(token);
            if (claims != null && claims.Any())
            {
                var identity = new ClaimsIdentity(claims, "jwt");
                _currentUser = new ClaimsPrincipal(identity);
                
                // Проверяем, не истек ли токен
                var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
                if (expClaim != null && long.TryParse(expClaim.Value, out var exp))
                {
                    var expDate = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
                    if (expDate <= DateTime.UtcNow)
                    {
                        _logger.LogWarning("JWT token has expired");
                        await LogoutAsync();
                        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                    }
                }
                
                // Кешируем пользователя из токена
                if (_cachedUser == null)
                {
                    var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                    var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                    
                    if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
                    {
                        _cachedUser = new UserResponse
                        {
                            Id = userGuid,
                            Username = username ?? "Unknown",
                            Email = email ?? "Unknown",
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };
                    }
                }
                
                return new AuthenticationState(_currentUser);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении состояния аутентификации");
            await LogoutAsync();
        }

        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        return new AuthenticationState(_currentUser);
    }

    private async Task SetAuthenticationAsync(AuthResponse authResponse, bool rememberMe)
    {
        var storage = rememberMe ? "localStorage" : "sessionStorage";
        
        // Сохраняем токен
        await _jsRuntime.InvokeVoidAsync($"{storage}.setItem", TokenKey, authResponse.Token);
        
        // Устанавливаем токен в заголовки глобально
        await SetAuthorizationHeaderAsync(authResponse.Token);
        
        // Получаем полную информацию о пользователе с сервера
        try
        {
            var userResponse = await GetCurrentUserAsync();
            if (userResponse != null)
            {
                _cachedUser = userResponse;
                await _jsRuntime.InvokeVoidAsync($"{storage}.setItem", UserKey, JsonSerializer.Serialize(userResponse, _jsonOptions));
                
                // Создаем Claims для пользователя
                var identity = CreateClaimsIdentity(userResponse);
                _currentUser = new ClaimsPrincipal(identity);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось получить информацию о пользователе после аутентификации");
            
            // Создаем минимальную информацию на основе ответа аутентификации
            var basicUserInfo = new UserResponse
            {
                Id = Guid.Empty, // Будет обновлен при следующем запросе
                Username = authResponse.Username,
                Email = authResponse.Email,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            
            _cachedUser = basicUserInfo;
            await _jsRuntime.InvokeVoidAsync($"{storage}.setItem", UserKey, JsonSerializer.Serialize(basicUserInfo, _jsonOptions));
            
            var identity = CreateClaimsIdentity(basicUserInfo);
            _currentUser = new ClaimsPrincipal(identity);
        }
        
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        AuthStateChanged.Invoke(true);
    }

    private async Task<string?> GetTokenAsync()
    {
        try
        {
            // Сначала проверяем sessionStorage, потом localStorage
            var token = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", TokenKey);
            if (string.IsNullOrEmpty(token))
            {
                token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
            }
            return token;
        }
        catch
        {
            return null;
        }
    }

    public async Task<UserResponse?> GetCurrentUserAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("GetCurrentUserAsync: Token is null or empty");
                return null;
            }

            _logger.LogInformation("GetCurrentUserAsync: Token found, length: {TokenLength}", token.Length);
            
            // Получаем информацию о пользователе с сервера
            await SetAuthorizationHeaderAsync(token);
            var httpClient = GetHttpClient();
            var response = await httpClient.GetAsync("api/auth/me");
            
            _logger.LogInformation("GetCurrentUserAsync: API response status: {StatusCode}", response.StatusCode);
            
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<UserResponse>();
                _cachedUser = user;
                _logger.LogInformation("GetCurrentUserAsync: User found: {Username}", user?.Username);
                return user;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Пользователь не найден в базе данных - это нормальная ситуация
                // которая может произойти если аккаунт был удален
                _logger.LogWarning("GetCurrentUserAsync: User not found in database (404)");
                return null;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Токен недействителен - нужно выйти из системы
                _logger.LogWarning("GetCurrentUserAsync: Unauthorized (401) - token might be invalid");
                await LogoutAsync();
                return null;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GetCurrentUserAsync: API error {StatusCode}: {Content}", response.StatusCode, errorContent);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении информации о пользователе");
            return null;
        }
    }

    private async Task InitializeAsync()
    {
        try
        {
            if (_isInitialized)
                return;

            _logger.LogInformation("Initializing AuthService...");
            
            var token = await GetTokenAsync();
            _logger.LogInformation("Token from storage: {TokenExists}", !string.IsNullOrEmpty(token));
            
            if (!string.IsNullOrEmpty(token))
            {
                _logger.LogInformation("Setting authorization header...");
                await SetAuthorizationHeaderAsync(token);
                
                // Проверяем валидность токена через прямой запрос к API
                try
                {
                    var httpClient = GetHttpClient();
                    var response = await httpClient.GetAsync("api/auth/me");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
                        if (user != null)
                        {
                            _logger.LogInformation("User authenticated successfully: {Username}", user.Username);
                            
                            var identity = CreateClaimsIdentity(user);
                            _currentUser = new ClaimsPrincipal(identity);
                            _cachedUser = user;
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        _logger.LogWarning("Token is invalid, clearing authentication");
                        await LogoutAsync();
                    }
                    else
                    {
                        _logger.LogWarning("API response: {StatusCode}", response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to validate token during initialization");
                }
            }
            else
            {
                _logger.LogInformation("No token found in storage");
            }

            _isInitialized = true;
            _initializationTcs.SetResult(true);
            
            _logger.LogInformation("AuthService initialization completed. IsAuthenticated: {IsAuthenticated}", IsAuthenticated);
            
            // Уведомляем об изменении состояния
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            AuthStateChanged.Invoke(IsAuthenticated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при инициализации AuthService");
            _isInitialized = true;
            _initializationTcs.SetResult(false);
        }
    }

    public async Task<bool> WaitForInitializationAsync()
    {
        return await _initializationTcs.Task;
    }

    public async Task<bool> CheckUsernameAvailabilityAsync(string username)
    {
        try
        {
            var httpClient = GetHttpClient();
            var response = await httpClient.GetAsync($"api/auth/check-username/{Uri.EscapeDataString(username)}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CheckExistsResponse>();
                return !(result?.Exists ?? true);
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CheckEmailAvailabilityAsync(string email)
    {
        try
        {
            var httpClient = GetHttpClient();
            var response = await httpClient.GetAsync($"api/auth/check-email/{Uri.EscapeDataString(email)}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CheckExistsResponse>();
                return !(result?.Exists ?? true);
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> DebugAuthAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                await SetAuthorizationHeaderAsync(token);
            }
            
            var httpClient = GetHttpClient();
            var response = await httpClient.GetAsync("api/auth/debug-auth");
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Debug Auth Response: {Content}", content);
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отладке авторизации");
            return $"Error: {ex.Message}";
        }
    }

    private async Task SetAuthorizationHeaderAsync(string token)
    {
        try
        {
            GetHttpClient().DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _logger.LogInformation("Authorization header set successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set authorization header");
        }
        await Task.CompletedTask;
    }

    private List<Claim>? ParseJwtToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            
            var claims = new List<Claim>();
            
            foreach (var claim in jsonToken.Claims)
            {
                // Конвертируем стандартные JWT claims в .NET ClaimTypes
                var claimType = claim.Type switch
                {
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" => ClaimTypes.NameIdentifier,
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" => ClaimTypes.Name,
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" => ClaimTypes.Email,
                    "username" => ClaimTypes.Name,
                    "email" => ClaimTypes.Email,
                    _ => claim.Type
                };
                
                claims.Add(new Claim(claimType, claim.Value));
            }
            
            return claims;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse JWT token");
            return null;
        }
    }

    private static ClaimsIdentity CreateClaimsIdentity(UserResponse user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("IsActive", user.IsActive.ToString())
        };

        if (user.LastLoginAt.HasValue)
        {
            claims.Add(new Claim("LastLoginAt", user.LastLoginAt.Value.ToString("O")));
        }

        return new ClaimsIdentity(claims, "jwt");
    }
}
