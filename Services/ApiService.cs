using AppSnacks.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AppSnacks.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;
        private readonly string _baseUrl = "url";

        JsonSerializerOptions _serializerOptions;
        public ApiService(HttpClient httpClient,
            ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ApiResponse<bool>> UserRegister(string name, string email, string phone, string password)
        {
            try
            {
                var register = new Register()
                {
                    Name = name,
                    Email = email,
                    Phone = phone,
                    Password = password
                };

                var json = JsonSerializer.Serialize(register, _serializerOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await PostRequest("api/Users/Register", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Erro ao enviar requisição HTTP: {response.StatusCode}");
                    return new ApiResponse<bool>
                    {
                        ErrorMessage = $"Erro ao enviar requisição HTTP: {response.StatusCode}"
                    };
                }

                return new ApiResponse<bool> { Data = true };
            }
            catch (Exception ex) 
            {
                _logger.LogError($"Erro ao registar o user: {ex.Message}");
                return new ApiResponse<bool> { ErrorMessage = ex.Message };
            }

        }

        public async Task<ApiResponse<bool>> Login(string email, string password)
        {
            try
            {
                var login = new Login()
                {
                    Email = email,
                    Password = password
                };

                var json = JsonSerializer.Serialize(login, _serializerOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await PostRequest("api/Users/Login", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Erro ao enviar requisição HTTP: {response.StatusCode}");
                    return new ApiResponse<bool>
                    {
                        ErrorMessage = $"Erro ao enviar requisição HTTP: {response.StatusCode}"
                    };
                }

                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Token>(jsonResult, _serializerOptions);

                Preferences.Set("accesstoken", result!.AccessToken);
                Preferences.Set("userid", (int)result.UserId!);
                Preferences.Set("username", result.UserName);

                return new ApiResponse<bool> { Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to login {ex.Message}");
                return new ApiResponse<bool> { ErrorMessage = ex.Message };
            }
        }


        public async Task<HttpResponseMessage> PostRequest(string uri, HttpContent content)
        {
            var Url = _baseUrl + uri;
            try
            {
                var result = await _httpClient.PostAsync(Url, content);
                return result;
            }
            catch (Exception ex) 
            {
                _logger.LogError($"Erro ao enviar requisição POST para {uri}: {ex.Message}");
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

    }
}
