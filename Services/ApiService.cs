﻿using AppSnacks.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AppSnacks.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;
        private readonly string _baseUrl = "https://nbn754d3-7066.uks1.devtunnels.ms/";

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

        public async Task<ApiResponse<bool>> AddItemToCart(ShoppingCart shoppingCart)
        {
            try
            {
                var json = JsonSerializer.Serialize(shoppingCart, _serializerOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await PostRequest("api/ItemsShoppingCart", content);

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
                _logger.LogError($"Error adding item to cart: {ex.Message}");
                return new ApiResponse<bool> { ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> ConfirmOrder(Order order)
        {
            try
            {
                var json = JsonSerializer.Serialize(order, _serializerOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await PostRequest("api/Orders", content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = response.StatusCode == HttpStatusCode.Unauthorized
                        ? "Unauthorized"
                        : $"Error sending HTTP request: {response.StatusCode}";

                    _logger.LogError($"Error sending HTTP request: {response.StatusCode}");
                    return new ApiResponse<bool> { ErrorMessage = errorMessage };
                }
                return new ApiResponse<bool> { Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error confirming order: {ex.Message}");
                return new ApiResponse<bool> { ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> UploadUserImage(byte[] imageArray)
        {
            try
            {
                var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(imageArray), "image", "image.jpg");
                var response = await PostRequest("api/users/uploadimage", content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = response.StatusCode == HttpStatusCode.Unauthorized
                      ? "Unauthorized"
                      : $"Erro ao enviar requisição HTTP: {response.StatusCode}";

                    _logger.LogError($"Erro ao enviar requisição HTTP: {response.StatusCode}");
                    return new ApiResponse<bool> { ErrorMessage = errorMessage };
                }
                return new ApiResponse<bool> { Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao fazer upload da imagem do usuário: {ex.Message}");
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

        public async Task<(bool Data, string? ErrorMessage)> UpdateItemQuantityCart(int productId, string action)
        {
            try
            {
                var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                var response = await PutRequest($"api/ItemsShoppingCart?productId={productId}&action={action}", content);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        string errorMessage = "Unauthorized";
                        _logger.LogWarning(errorMessage);
                        return (false, errorMessage);
                    }
                    string generalErrorMessage = $"Request error: {response.ReasonPhrase}";
                    _logger.LogError(generalErrorMessage);
                    return (false, generalErrorMessage);
                }
            }
            catch (HttpRequestException ex)
            {
                string errorMessage = $"HTTP request error: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return (false, errorMessage);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return (false, errorMessage);
            }
        }

        private async Task<HttpResponseMessage> PutRequest(string uri, StringContent content)
        {
            var url = AppConfig.BaseUrl + uri;
            try
            {
                AddAuthorizationHeader();
                var result = await _httpClient.PutAsync(url, content);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending PUT request to {uri}: {ex.Message}");
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public async Task<(List<Category>? Categorias, string? ErrorMessage)> GetCategories()
        {
            return await GetAsync<List<Category>>("api/categories");
        }

        public async Task<(List<Product>? Produtos, string? ErrorMessage)> GetProducts(string typeProduct, string categoryId)
        {
            string endpoint = $"api/Products?typeProduct={typeProduct}&categoryId={categoryId}";
            return await GetAsync<List<Product>>(endpoint);
        }

        public async Task<(Product? ProductDetails, string? ErrorMessage)> GetProductDetails(int productId)
        {
            string endpoint = $"api/products/{productId}";
            return await GetAsync<Product>(endpoint);
        }

        public async Task<(List<CartOrderItem>? CartOrderItems, string? ErrorMessage)> GetCartOrderItems(int userId)
        {
            var endpoint = $"api/ItemsShoppingCart/{userId}";
            return await GetAsync<List<CartOrderItem>>(endpoint);
        }

        public async Task<(ProfileImage? ProfileImage, string? ErrorMessage)> GetUserProfileImage()
        {
            string endpoint = "api/Users/userimage";
            return await GetAsync<ProfileImage>(endpoint);
        }

        public async Task<(List<UserOrder>?, string? ErrorMessage)> GetUserOrder(int userId)
        {

            string endpoint = $"api/Orders/OrdersPerUser/{userId}";

            return await GetAsync<List<UserOrder>>(endpoint);
        }

        public async Task<(List<OrderDetails>?, string? ErrorMessage)> GetOrderDetails(int orderId)
        {
            string endpoint = $"api/Orders/OrderDetails/{orderId}";

            return await GetAsync<List<OrderDetails>>(endpoint);
        }

        private async Task<(T? Data, string? ErrorMessage)> GetAsync<T>(string endpoint)
        {
            try
            {
                AddAuthorizationHeader();

                var response = await _httpClient.GetAsync(AppConfig.BaseUrl + endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<T>(responseString, _serializerOptions);
                    return (data ?? Activator.CreateInstance<T>(), null);
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        string errorMessage = "Unauthorized";
                        _logger.LogWarning(errorMessage);
                        return (default, errorMessage);
                    }

                    string generalErrorMessage = $"Erro na requisição: {response.ReasonPhrase}";
                    _logger.LogError(generalErrorMessage);
                    return (default, generalErrorMessage);
                }
            }
            catch (HttpRequestException ex)
            {
                string errorMessage = $"Erro de requisição HTTP: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return (default, errorMessage);
            }
            catch (JsonException ex)
            {
                string errorMessage = $"Erro de desserialização JSON: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return (default, errorMessage);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Erro inesperado: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return (default, errorMessage);
            }
        }

        private void AddAuthorizationHeader()
        {
            var token = Preferences.Get("accesstoken", string.Empty);
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

    }
}
