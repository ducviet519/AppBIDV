using DataBIDV.Models;
using DataBIDV.Services.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;

namespace DataBIDV.Services.Repositories
{
    public class ConnectAPIService : IConnectAPIService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string client_id = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\client_id.asc"));
        private readonly string client_secret = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\client_secret.asc"));
        public ConnectAPIService()
        {
            _httpClient.BaseAddress = new Uri("https://apithanhtoan.com/");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
            var query = new Dictionary<string, string>
            {
                ["grant_type"] = "",
                ["scope"] = "",
                ["client_id"] = "",
                ["client_secret"] = "",
            };
            string uri = QueryHelpers.AddQueryString("/openapi/oauth2/token/", query);
        }
        public async Task<TokenAPI> Get_API_Token(RequestModel request)
        {
            TokenAPI data = new TokenAPI();
            var query = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["scope"] = "read",
                ["client_id"] = client_id,
                ["client_secret"] = client_secret,
            };
            string uri = QueryHelpers.AddQueryString("/openapi/oauth2/token", query);
            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/openapi/oauth2/token");
                httpRequestMessage.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(request), Encoding.UTF8);
                httpRequestMessage.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded");               
                HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                }

                return data;
            }
            catch (Exception ex)
            {
                string Msg = ex.Message;
                throw;
            }
        }


        public async Task<List<GiaoDichModel>> Get_DanhSachGiaoDich(string token, RequestModel request)
        {
            
            List<GiaoDichModel> data = new List<GiaoDichModel>();
            
            string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
            var bodyData = System.Text.Json.JsonSerializer.Serialize(request);
            string requestID = Guid.NewGuid().ToString("N");
            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/iconnect/account/acctHis/v1.0");
                // Header
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpRequestMessage.Headers.Add("Timestamp", timestamp);
                httpRequestMessage.Headers.Add("X-API-Interaction-ID", requestID);
                httpRequestMessage.Content = new StringContent(bodyData, Encoding.UTF8);
                httpRequestMessage.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                // Body
                var content = new MultipartFormDataContent();
                //content.Add(new StringContent(timestamp), "Timestamp");
                httpRequestMessage.Content = content;
                //Sending request to find web api REST service resource GetDepartments using HttpClient  
                HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);

                //Checking the response is successful or not which is sent using HttpClient  
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
     
                }

                return data ;
            }
            catch (Exception ex)
            {
                string Msg = ex.Message;
                throw;
            }
        }
    }
}
