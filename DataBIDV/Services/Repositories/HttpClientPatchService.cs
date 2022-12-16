using DataBIDV.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Services.Repositories
{
    public class HttpClientPatchService: IHttpClientServiceImplementation
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        public HttpClientPatchService()
        {
            _httpClient.BaseAddress = new Uri("https://localhost:5001/api/");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
            string rootPath = System.IO.Directory.GetCurrentDirectory();
            string client_id = File.ReadAllText(Path.Combine(rootPath, "client_id.asc"));
            string client_secret = File.ReadAllText(Path.Combine(rootPath, "client_secret.asc"));
        }


    }
}
