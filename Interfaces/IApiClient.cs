using System.Threading.Tasks;

namespace ApiTestingFramework.Interfaces
{
    public interface IApiClient
    {
        // HTTP Methods (Contract for all implementations)
        Task<object> GetAsync(string endpoint);
        Task<object> PostAsync(string endpoint, object body);
        Task<object> PostAsync(string endpoint, string jsonBody);
        Task<object> PutAsync(string endpoint, object body);
        Task<object> PutAsync(string endpoint, string jsonBody);
        Task<object> DeleteAsync(string endpoint);

        // Common configuration methods
        void SetBaseUrl(string baseUrl);
        void AddHeader(string name, string value);
        void AddAuthentication(string token);
        //void ClearHeaders();
        void SetTimeout(int milliseconds);
    }
}