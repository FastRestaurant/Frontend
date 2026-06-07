using El_buen_sabor.Components.Models;
using static System.Net.WebRequestMethods;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace El_buen_sabor.Components.Service
{
    public class AuthService
    {
        private readonly HttpClient _http;

        public AuthService(HttpClient http)
        {
            _http = http;
        }

        public async Task<LoginResponse?> Login(LoginRequest request)
        {
            var response = await _http.PostAsJsonAsync(
                "api/Auth/login",
                request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }
        public async Task<bool> LogoutAsync(string token)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{_http.BaseAddress}api/Auth/logout");

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
    }
}
