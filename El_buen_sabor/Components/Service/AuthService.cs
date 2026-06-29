using El_buen_sabor.Components.Models;
using static System.Net.WebRequestMethods;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Blazored.LocalStorage;
using System.Data;


namespace El_buen_sabor.Components.Service
{    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;
        private static readonly Dictionary<string, string> DictionaryRoles =new()
        {
            { AppRoles.Kitchen, AppRoles.KitchenDisplay },
            { AppRoles.Cashier, AppRoles.CashierDisplay },
            { AppRoles.Admin, AppRoles.AdminDisplay },
            { AppRoles.Waitress, AppRoles.WaitressDisplay }
        };

        public AuthService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        public async Task<LoginResponse?> Login(LoginRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/v1/Auth/login",request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }
        public async Task<bool> LogoutAsync(string token)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{_http.BaseAddress}api/v1/Auth/logout");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                using var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de red en AuthService.LogoutAsync: {ex.Message}");
                return false;
            }
        }
        public async Task<UserPageResponseDto> GetUsersAsync(int pageNumber = 1, int pageSize = 10, string? search = null, string? role = null)
        {
            try
            {
                var query = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Add($"search={Uri.EscapeDataString(search.Trim())}");
                }

                var backendRole = ToBackendRole(role);
                if (!string.IsNullOrWhiteSpace(backendRole))
                {
                    query.Add($"role={Uri.EscapeDataString(backendRole)}");
                }

                string url = $"{_http.BaseAddress}api/v1/Auth/users?{string.Join("&", query)}";
                var token = await _localStorage.GetItemAsync<string>("token");
                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                using var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UserPageResponseDto>() ?? new UserPageResponseDto();
                    var users = result.Data;
                    foreach(var user in users)
                    {
                        user.Role = ToDisplayRole(user.Role);
                    }

                    result.RoleCounts = result.RoleCounts
                        .GroupBy(item => ToDisplayRole(item.Key))
                        .ToDictionary(group => group.Key, group => group.Sum(item => item.Value));

                    return result;
                }
                else
                {
                    Console.WriteLine($"Error en la API de usuarios. Código de estado: {response.StatusCode}");
                    return new UserPageResponseDto();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al obtener usuarios: {ex.Message}");
                return new UserPageResponseDto();
            }
        }

        public async Task<List<RoleDto>> GetRolesAsync()
        {
            try
            {
                string url = $"{_http.BaseAddress}api/v1/Auth/roles";
                var token = await _localStorage.GetItemAsync<string>("token");
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                using var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var rolesBackend = await response.Content.ReadFromJsonAsync<List<RoleDto>>() ?? new List<RoleDto>();
                    foreach (var role in rolesBackend)
                    {
                        string nameUpper = role.NormalizedName.ToUpper().Trim();
                        if (DictionaryRoles.ContainsKey(nameUpper))
                        {
                            role.NormalizedName = DictionaryRoles[nameUpper];
                        }
                    }
                    return rolesBackend;
                }
                return new List<RoleDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener roles: {ex.Message}");
                return new List<RoleDto>();
            }
        }

        public async Task<AuthActionResult> RegisterUserAsync(RegisterDto registerData)
        {
            try
            {
                string url = $"{_http.BaseAddress}api/v1/Auth/register";
                var token = await _localStorage.GetItemAsync<string>("token");
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                var backendRole = ToBackendRole(registerData.Role);
                var payload = new RegisterDto
                {
                    Email = registerData.Email.Trim(),
                    UserName = registerData.UserName.Trim(),
                    Password = registerData.Password,
                    FirstName = registerData.FirstName.Trim(),
                    LastName = registerData.LastName.Trim(),
                    Role = backendRole ?? string.Empty
                };

                request.Content = JsonContent.Create(payload);
                using var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return new AuthActionResult(true, string.Empty);
                }

                var content = await response.Content.ReadAsStringAsync();
                return new AuthActionResult(false, ReadErrorMessage(content, "No se pudo crear el usuario."));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de red al registrar usuario: {ex.Message}");
                return new AuthActionResult(false, "No se pudo conectar con el servicio de usuarios.");
            }
        }

        public async Task<bool> UpdateUser(string id, UpdateUserDto dto)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("token");
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string url = $"{_http.BaseAddress}api/v1/Auth/user/{id}";
                var payload = new UpdateUserDto
                {
                    UserName = dto.UserName?.Trim(),
                    Email = dto.Email?.Trim(),
                    FirstName = dto.FirstName?.Trim(),
                    LastName = dto.LastName?.Trim(),
                    Role = ToBackendRole(dto.Role),
                    NewPassword = dto.NewPassword
                };

                var response = await _http.PatchAsJsonAsync(url, payload);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar al usuario: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteUser(string id)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("token");
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string url = $"{_http.BaseAddress}api/v1/Auth/user/{id}";
                var response = await _http.DeleteAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar al usuario: {ex.Message}");
                return false;
            }
        }

        private static string ToDisplayRole(string? role)
        {
            var nameUpper = role?.ToUpper().Trim() ?? string.Empty;
            return DictionaryRoles.TryGetValue(nameUpper, out var displayRole) ? displayRole : nameUpper;
        }

        private static string? ToBackendRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return null;
            }

            var normalizedRole = role.ToUpper().Trim();
            var backendRole = DictionaryRoles.FirstOrDefault(item => item.Value == normalizedRole).Key;
            return string.IsNullOrWhiteSpace(backendRole) ? normalizedRole : backendRole;
        }

        private static string ReadErrorMessage(string content, string fallback)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return fallback;
            }

            try
            {
                using var document = JsonDocument.Parse(content);
                var messages = new List<string>();
                CollectErrorMessages(document.RootElement, messages);
                return messages.Count > 0 ? string.Join(" ", messages.Distinct()) : fallback;
            }
            catch (JsonException)
            {
                return content.Trim('"', ' ', '\r', '\n');
            }
        }

        private static void CollectErrorMessages(JsonElement element, List<string> messages)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    if (element.TryGetProperty("message", out var message))
                    {
                        CollectErrorMessages(message, messages);
                    }

                    if (element.TryGetProperty("description", out var description))
                    {
                        CollectErrorMessages(description, messages);
                    }

                    if (element.TryGetProperty("code", out var code))
                    {
                        var translated = TranslateIdentityError(code.GetString());
                        if (!string.IsNullOrWhiteSpace(translated))
                        {
                            messages.Add(translated);
                            return;
                        }
                    }

                    if (element.TryGetProperty("errors", out var errors))
                    {
                        CollectErrorMessages(errors, messages);
                    }
                    break;
                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                    {
                        CollectErrorMessages(item, messages);
                    }
                    break;
                case JsonValueKind.String:
                    var value = element.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        messages.Add(TranslateKnownMessage(value));
                    }
                    break;
            }
        }

        private static string TranslateIdentityError(string? code)
        {
            return code switch
            {
                "DuplicateUserName" => "El nombre de usuario ya existe.",
                "DuplicateEmail" => "El email ya esta registrado.",
                "PasswordTooShort" => "La contraseña debe tener al menos 8 caracteres.",
                "PasswordRequiresNonAlphanumeric" => "La contraseña debe incluir al menos un símbolo.",
                "PasswordRequiresDigit" => "La contraseña debe incluir al menos un número.",
                "PasswordRequiresLower" => "La contraseña debe incluir al menos una minúscula.",
                "PasswordRequiresUpper" => "La contraseña debe incluir al menos una mayúscula.",
                _ => string.Empty
            };
        }

        private static string TranslateKnownMessage(string message)
        {
            return message switch
            {
                "Rol inválido" => "Debe seleccionar un rol valido.",
                "Rol invalido" => "Debe seleccionar un rol valido.",
                _ => message
            };
        }
    }

    public sealed record AuthActionResult(bool Success, string Message);
}
