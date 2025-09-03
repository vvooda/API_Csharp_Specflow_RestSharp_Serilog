using System;
using System.Threading.Tasks;
using RestSharp;
using ApiTestingFramework.Interfaces;
using Newtonsoft.Json;

namespace ApiTestingFramework.Implementations
{
    public class RestSharpClientService : IApiClient, IDisposable
    {
        private RestClient _client;
        private readonly ILogger _logger;
        private string _baseUrl;

        public RestSharpClientService(ILogger logger)
        {
            _logger = logger;
            
            // Initialize with empty options, we'll set base URL later
            var options = new RestClientOptions
            {
                ThrowOnAnyError = false,
                MaxTimeout = 30000 // milliseconds
            };
            
            _client = new RestClient(options);
            
            _logger.Info("RestSharp client initialized");
        }

        public void SetBaseUrl(string baseUrl)
        {
            _logger.Info($"Setting base URL: {baseUrl}");
            _baseUrl = baseUrl;
            
            // Recreate the client with the new base URL
            var options = new RestClientOptions(baseUrl)
            {
                ThrowOnAnyError = false,
                MaxTimeout = 30000
            };
            
            var oldClient = _client;
            _client = new RestClient(options);
            oldClient?.Dispose();
        }

        public void AddHeader(string name, string value)
        {
            _logger.Debug($"Adding header: {name} = {value}");
            _client.AddDefaultHeader(name, value);
        }

        public void AddAuthentication(string token)
        {
            _logger.Debug("Adding authentication token");
            _client.AddDefaultHeader("Authorization", $"Bearer {token}");
        }

/*         public void ClearHeaders()
        {
            _logger.Debug("Clearing all headers");
            _client.DefaultParameters.Clear();
        } */

        public void SetTimeout(int milliseconds)
        {
            _logger.Debug($"Setting timeout: {milliseconds}ms");
            
            // Recreate client with new timeout
            var options = new RestClientOptions(_baseUrl)
            {
                ThrowOnAnyError = false,
                MaxTimeout = milliseconds
            };
            
            var oldClient = _client;
            _client = new RestClient(options);
            oldClient?.Dispose();
        }

        private string GetFullUrl(string endpoint)
        {
            if (string.IsNullOrEmpty(_baseUrl))
            {
                return endpoint;
            }
            
            return _baseUrl.EndsWith("/") ? _baseUrl + endpoint.TrimStart('/') 
                 : endpoint.StartsWith("/") ? _baseUrl + endpoint
                 : _baseUrl + "/" + endpoint;
        }

        public async Task<object> GetAsync(string endpoint)
        {
            var fullUrl = GetFullUrl(endpoint);
            _logger.ApiRequest("GET", fullUrl);
            
            var request = new RestRequest(endpoint, Method.Get);
            var response = await _client.ExecuteAsync(request);
            
            LogResponseDetails(response);
            return response;
        }

        public async Task<object> PostAsync(string endpoint, object body)
        {
            var fullUrl = GetFullUrl(endpoint);
            var jsonBody = body != null ? JsonConvert.SerializeObject(body, Formatting.Indented) : null;
            _logger.ApiRequest("POST", fullUrl, jsonBody);
            
            var request = new RestRequest(endpoint, Method.Post);
            
            if (body != null)
            {
                request.AddJsonBody(body);
            }
            
            var response = await _client.ExecuteAsync(request);
            LogResponseDetails(response);
            
            return response;
        }

        public async Task<object> PostAsync(string endpoint, string jsonBody)
        {
            var fullUrl = GetFullUrl(endpoint);
            _logger.ApiRequest("POST", fullUrl, jsonBody);
            
            var request = new RestRequest(endpoint, Method.Post);
            
            if (!string.IsNullOrEmpty(jsonBody))
            {
                request.AddJsonBody(jsonBody);
            }
            
            var response = await _client.ExecuteAsync(request);
            LogResponseDetails(response);
            
            return response;
        }

        public async Task<object> PutAsync(string endpoint, object body)
        {
            var fullUrl = GetFullUrl(endpoint);
            var jsonBody = body != null ? JsonConvert.SerializeObject(body, Formatting.Indented) : null;
            _logger.ApiRequest("PUT", fullUrl, jsonBody);
            
            var request = new RestRequest(endpoint, Method.Put);
            
            if (body != null)
            {
                request.AddJsonBody(body);
            }
            
            var response = await _client.ExecuteAsync(request);
            LogResponseDetails(response);
            
            return response;
        }

        public async Task<object> PutAsync(string endpoint, string jsonBody)
        {
            var fullUrl = GetFullUrl(endpoint);
            _logger.ApiRequest("PUT", fullUrl, jsonBody);
            
            var request = new RestRequest(endpoint, Method.Put);
            
            if (!string.IsNullOrEmpty(jsonBody))
            {
                request.AddJsonBody(jsonBody);
            }
            
            var response = await _client.ExecuteAsync(request);
            LogResponseDetails(response);
            
            return response;
        }

        public async Task<object> DeleteAsync(string endpoint)
        {
            var fullUrl = GetFullUrl(endpoint);
            _logger.ApiRequest("DELETE", fullUrl);
            
            var request = new RestRequest(endpoint, Method.Delete);
            var response = await _client.ExecuteAsync(request);
            
            LogResponseDetails(response);
            return response;
        }

        private void LogResponseDetails(RestResponse response)
        {
            _logger.ApiResponse((int)response.StatusCode, response.Content);
            
            if (response.ErrorException != null)
            {
                _logger.Error(response.ErrorException, "Request failed with exception");
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
            _logger.Info("RestSharp client disposed");
        }
    }
}