using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_Desktop_lib.Services
{
    internal class SelfSetHttpClientFactory : IHttpClientFactory
    {
        private HttpClient _client;

        public SelfSetHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        public HttpClient GetHttpClient()
        {
            return _client;
        }

        public void SetHttpClient(HttpClient httpClient)
        {
            _client = httpClient;
        }
    }
}
