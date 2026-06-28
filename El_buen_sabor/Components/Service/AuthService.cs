using El_buen_sabor.Components.Models;
using static System.Net.WebRequestMethods;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
        public async Task<List<UserDto>> GetUsersAsync()
        {
            try
            {
                string url = $"{_http.BaseAddress}api/v1/Auth/users";
                var token = await _localStorage.GetItemAsync<string>("token");
                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                using var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var users = await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? new List<UserDto>();
                    foreach(var user in users)
                    {
                        string nameUpper = user.Role.ToUpper().Trim();
                        if (DictionaryRoles.ContainsKey(nameUpper))
                        {
                            user.Role = DictionaryRoles[nameUpper];
                        }
                    }
                    return users;
                }
                else
                {
                    Console.WriteLine($"Error en la API de usuarios. Código de estado: {response.StatusCode}");
                    return new List<UserDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de comunicación al obtener usuarios: {ex.Message}");
                return new List<UserDto>();
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

        public async Task<bool> RegisterUserAsync(RegisterDto registerData)
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
                string rolEnEspanol = registerData.Role?.ToUpper().Trim() ?? string.Empty;
                foreach (var entry in DictionaryRoles)
                {
                    if (entry.Value == rolEnEspanol)
                    {
                        registerData.Role = entry.Key;
                        break;
                    }
                }
                request.Content = JsonContent.Create(registerData);
                using var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de red al registrar usuario: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUser(string id, UpdateUserDto dto)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("token");
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string url = $"{_http.BaseAddress}api/v1/Auth/user/{id}";
                var response = await _http.PatchAsJsonAsync(url, dto);

                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"STATUS: {response.StatusCode}");
                Console.WriteLine($"BODY: {body}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR UPDATE USER: {ex.Message}");
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
    }
}
