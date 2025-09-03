using System;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using ApiTestingFramework.Interfaces;

namespace ApiTestingFramework.Implementations
{
    public class RestSharpResponseHandler : IResponseHandler
    {
        private readonly ILogger _logger;

        public RestSharpResponseHandler(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<T> DeserializeResponseAsync<T>(object response)
        {
            try
            {
                if (response is not RestResponse restResponse)
                {
                    throw new ArgumentException("Response must be a RestResponse", nameof(response));
                }

                if (string.IsNullOrEmpty(restResponse.Content))
                {
                    throw new InvalidOperationException("Response content is empty");
                }

                var result = JsonConvert.DeserializeObject<T>(restResponse.Content);
                _logger.Debug($"Deserialized response to type: {typeof(T).Name}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to deserialize response to type {typeof(T).Name}");
                throw;
            }
        }

        public string GetResponseContent(object response)
        {
            if (response is RestResponse restResponse)
            {
                return restResponse.Content;
            }
            throw new ArgumentException("Response must be a RestResponse", nameof(response));
        }

        public void VerifyStatusCode(object response, int expectedStatusCode)
        {
            if (response is not RestResponse restResponse)
            {
                throw new ArgumentException("Response must be a RestResponse", nameof(response));
            }

            _logger.Info($"Verifying status code: Expected {expectedStatusCode}, Actual {(int)restResponse.StatusCode}");
            
            ((int)restResponse.StatusCode).Should().Be(expectedStatusCode, 
                $"Expected status code {expectedStatusCode} but got {(int)restResponse.StatusCode}");
        }

        public void VerifyResponseIsSuccessful(object response)
        {
            if (response is not RestResponse restResponse)
            {
                throw new ArgumentException("Response must be a RestResponse", nameof(response));
            }

            _logger.Info("Verifying response is successful");
            restResponse.IsSuccessful.Should().BeTrue($"Request failed with status {restResponse.StatusCode}: {restResponse.ErrorMessage}");
        }

        public void VerifyResponseContains(object response, string expectedText)
        {
            if (response is not RestResponse restResponse)
            {
                throw new ArgumentException("Response must be a RestResponse", nameof(response));
            }

            _logger.Info($"Verifying response contains: {expectedText}");
            restResponse.Content.Should().Contain(expectedText, 
                $"Response content should contain '{expectedText}' but was: {restResponse.Content}");
        }

        public T ExtractFromResponse<T>(object response, string jsonPath)
        {
            try
            {
                if (response is not RestResponse restResponse)
                {
                    throw new ArgumentException("Response must be a RestResponse", nameof(response));
                }

                if (string.IsNullOrEmpty(restResponse.Content))
                {
                    throw new InvalidOperationException("Response content is empty");
                }

                JToken token;
                
                if (restResponse.Content.Trim().StartsWith('['))
                {
                    var jArray = JArray.Parse(restResponse.Content);
                    token = jArray.SelectToken(jsonPath);
                }
                else
                {
                    var jObject = JObject.Parse(restResponse.Content);
                    token = jObject.SelectToken(jsonPath);
                }

                if (token == null)
                {
                    throw new InvalidOperationException($"JSON path '{jsonPath}' not found in response");
                }

                var value = token.ToObject<T>();
                _logger.Debug($"Extracted value from JSON path '{jsonPath}': {value}");
                return value;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to extract value from JSON path '{jsonPath}'");
                throw;
            }
        }
    }
}