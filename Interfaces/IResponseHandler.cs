using System.Threading.Tasks;

namespace ApiTestingFramework.Interfaces
{
    public interface IResponseHandler
    {
        Task<T> DeserializeResponseAsync<T>(object response);
        string GetResponseContent(object response);
        void VerifyStatusCode(object response, int expectedStatusCode);
        void VerifyResponseIsSuccessful(object response);
        void VerifyResponseContains(object response, string expectedText);
        T ExtractFromResponse<T>(object response, string jsonPath);
    }
}