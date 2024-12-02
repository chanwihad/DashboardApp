using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DashboardApp.Models;

namespace DashboardApp.Services
{

    public class AuthApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> RegisterAsync(string username, string fullName, string email, string password)
        {
            var registerModel = new
            {
                Username = username,
                FullName = fullName,
                Email = email,
                Password = password
            };

            var content = new StringContent(JsonSerializer.Serialize(registerModel), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("http://localhost:5117/api/auth/register", content);

            return response.IsSuccessStatusCode;
        }


        public async Task<bool> LoginAsync(string username, string password)
        {
            var loginModel = new
            {
                Username = username,
                Password = password
            };

            var content = new StringContent(JsonSerializer.Serialize(loginModel), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("http://localhost:5117/api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseModel>();
                if (result != null)
                {
                    _httpContextAccessor.HttpContext.Session.SetString("Token", result.Token);
                    _httpContextAccessor.HttpContext.Session.SetString("Username", result.Username);
                    _httpContextAccessor.HttpContext.Session.SetString("ClientId", result.ClientId);
                    _httpContextAccessor.HttpContext.Session.SetString("UserMenus", JsonSerializer.Serialize(result.Menus));
                    _httpContextAccessor.HttpContext.Session.SetString("CanCreate", result.Permissions.CanCreate.ToString().ToLower());
                    _httpContextAccessor.HttpContext.Session.SetString("CanView", result.Permissions.CanView.ToString().ToLower());
                    _httpContextAccessor.HttpContext.Session.SetString("CanUpdate", result.Permissions.CanUpdate.ToString().ToLower());
                    _httpContextAccessor.HttpContext.Session.SetString("CanDelete", result.Permissions.CanDelete.ToString().ToLower());

                    return true;
                }
            }

            return false;
        }

        public async Task<bool> ChangePasswordAsync(string oldPassword, string newPassword)
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("Token");

            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("User is not logged in.");

            var changePasswordModel = new
            {
                CurrentPassword = oldPassword,
                NewPassword = newPassword
            };

            var content = new StringContent(JsonSerializer.Serialize(changePasswordModel), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync("http://localhost:5117/api/auth/change-password", content);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var forgotPasswordModel = new
            {
                Email = email
            };

            var content = new StringContent(JsonSerializer.Serialize(forgotPasswordModel), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("http://localhost:5117/api/auth/reset-password", content);

            return response.IsSuccessStatusCode;
        }


    }
}