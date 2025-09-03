using System;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ApiTestingFramework.Interfaces;
using ApiTestingFramework.Models;
using Newtonsoft.Json;
using RestSharp;

namespace ApiTestingFramework.StepDefinitions
{
    [Binding]
    public class UserAPISteps
    {
        private readonly ScenarioContext _scenarioContext;
        private object _response;
        private readonly IApiClient _apiClient;
        private readonly IResponseHandler _responseHandler;
        private readonly IJsonExtractor _jsonExtractor;
        private readonly ILogger _logger;

        public UserAPISteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            var serviceProvider = scenarioContext.Get<IServiceProvider>("ServiceProvider");
            _apiClient = serviceProvider.GetRequiredService<IApiClient>();
            _responseHandler = serviceProvider.GetRequiredService<IResponseHandler>();
            _jsonExtractor = serviceProvider.GetRequiredService<IJsonExtractor>();
            _logger = serviceProvider.GetRequiredService<ILogger>();
        }

        [Given(@"I set the base URL to ""(.*)""")]
        public void GivenISetTheBaseURLTo(string baseUrl)
        {
            _apiClient.SetBaseUrl(baseUrl);
        }

        [Given(@"I set the authorization token to ""(.*)""")]
        public void GivenISetTheAuthorizationTokenTo(string token)
        {
            _apiClient.AddAuthentication(token);
        }

        [Given(@"I add header ""(.*)"" with value ""(.*)""")]
        public void GivenIAddHeaderWithValue(string headerName, string headerValue)
        {
            _apiClient.AddHeader(headerName, headerValue);
        }

/*         [Given(@"I clear all headers")]
        public void GivenIClearAllHeaders()
        {
            _apiClient.ClearHeaders();
        } */

        [Given(@"I set timeout to (\d+) milliseconds")]
        public void GivenISetTimeoutToMilliseconds(int milliseconds)
        {
            _apiClient.SetTimeout(milliseconds);
        }

        [When(@"I send a GET request to ""(.*)""")]
        public async Task WhenISendAGETRequestTo(string endpoint)
        {
            _response = await _apiClient.GetAsync(endpoint);
            _scenarioContext.Set(_response, "Response");
        }

        [When(@"I send a POST request to ""(.*)"" with the user data")]
        public async Task WhenISendAPOSTRequestToWithTheUserData(string endpoint)
        {
            var table = _scenarioContext.Get<Table>("UserDataTable");
            var userData = table.Rows[0];
            var user = new User
            {
                Name = userData["name"],
                Username = userData["username"],
                Email = userData["email"],
                Phone = userData["phone"],
                Website = userData["website"]
            };

            _response = await _apiClient.PostAsync(endpoint, user);
            _scenarioContext.Set(_response, "Response");
        }

        [When(@"I send a POST request to ""(.*)"" with JSON body:")]
        public async Task WhenISendAPOSTRequestToWithJSONBody(string endpoint, string jsonBody)
        {
            _response = await _apiClient.PostAsync(endpoint, jsonBody);
            _scenarioContext.Set(_response, "Response");
        }

        [When(@"I send a PUT request to ""(.*)"" with the user data")]
        public async Task WhenISendAPUTRequestToWithTheUserData(string endpoint)
        {
            var table = _scenarioContext.Get<Table>("UserDataTable");
            var userData = table.Rows[0];
            var user = new User
            {
                Name = userData["name"],
                Username = userData["username"],
                Email = userData["email"],
                Phone = userData["phone"],
                Website = userData["website"]
            };

            _response = await _apiClient.PutAsync(endpoint, user);
            _scenarioContext.Set(_response, "Response");
        }

        [When(@"I send a DELETE request to ""(.*)""")]
        public async Task WhenISendADELETERequestTo(string endpoint)
        {
            _response = await _apiClient.DeleteAsync(endpoint);
            _scenarioContext.Set(_response, "Response");
        }

        [Then(@"the response status code should be (\d+)")]
        public void ThenTheResponseStatusCodeShouldBe(int statusCode)
        {
            _responseHandler.VerifyStatusCode(_response, statusCode);
        }

        [Then(@"the response should be successful")]
        public void ThenTheResponseShouldBeSuccessful()
        {
            _responseHandler.VerifyResponseIsSuccessful(_response);
        }

        [Then(@"the response should contain ""(.*)""")]
        public void ThenTheResponseShouldContain(string expectedText)
        {
            _responseHandler.VerifyResponseContains(_response, expectedText);
        }

        [Then(@"the response should contain a list of users")]
        public async Task ThenTheResponseShouldContainAListOfUsers()
        {
            var users = await _responseHandler.DeserializeResponseAsync<User[]>(_response);
            users.Should().NotBeNull();
            users.Should().NotBeEmpty();
            _scenarioContext.Set(users, "UsersList");
            
            _logger.Info($"Retrieved {users.Length} users");
            foreach (var user in users)
            {
                _logger.Debug($"User: {user.Name} ({user.Email})");
            }
        }

        [Then(@"I should be able to extract the first user's name")]
        public void ThenIShouldBeAbleToExtractTheFirstUsersName()
        {
            var users = _scenarioContext.Get<User[]>("UsersList");
            users[0].Name.Should().NotBeNullOrEmpty();
            _logger.Info($"First user's name: {users[0].Name}");
        }
    }
}