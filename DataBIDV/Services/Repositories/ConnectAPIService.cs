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
using PgpCore;
using DataBIDV.Extensions;
using System.Net.Http.Json;

namespace DataBIDV.Services.Repositories
{
    public class ConnectAPIService : IConnectAPIService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string client_id = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\client_id.asc"));
        private readonly string client_secret = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\client_secret.asc"));
        private readonly string public_key  = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\test_key.asc"));
        public ConnectAPIService()
        {
            _httpClient.BaseAddress = new Uri("https://apithanhtoan.com/");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
        }
        public async Task<TokenAPI> Get_API_Token()
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
            var request = new RequestModel { grant_type = "client_credentials", scope = "read", client_id = client_id, client_secret = client_secret };

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

        //API Vấn tin danh sách giao dịch (Có mã hóa dữ liệu)
        public async Task<List<GiaoDichModel>> Get_DanhSachGiaoDich_Encrypt(string token, RequestBody request)
        {
            
            List<GiaoDichModel> data = new List<GiaoDichModel>();
            
            string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
            string requestID = Guid.NewGuid().ToString("D");
            try
            {
                //Encrypt
                EncryptionKeys encryptionKeys = new EncryptionKeys(public_key);
                PGP pgp = new PGP(encryptionKeys);
                string encryptedContent = StaticHelper.EncodeBase64(await pgp.EncryptArmoredStringAsync(System.Text.Json.JsonSerializer.Serialize(request)));
                string jsonContent = System.Text.Json.JsonSerializer.Serialize(new { data = encryptedContent });

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/iconnect/account/acctHis/v1.0");
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpRequestMessage.Headers.Add("Timestamp", timestamp);
                httpRequestMessage.Headers.Add("X-API-Interaction-ID", requestID);
                httpRequestMessage.Content = new StringContent(jsonContent, Encoding.UTF8);
                httpRequestMessage.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

                HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);
 
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

        //API Vấn tin danh sách giao dịch (Không mã hóa dữ liệu)
        public async Task<List<GiaoDichModel>> Get_DanhSachGiaoDich(string token, RequestBody request)
        {

            List<GiaoDichModel> data = new List<GiaoDichModel>();

            string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
            string requestID = Guid.NewGuid().ToString("D");
            try
            {
                string jsonContent = System.Text.Json.JsonSerializer.Serialize(request);

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/iconnect/account/getAcctHis/v1.1");
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpRequestMessage.Headers.Add("Timestamp", timestamp);
                httpRequestMessage.Headers.Add("X-API-Interaction-ID", requestID);
                httpRequestMessage.Content = new StringContent(jsonContent, Encoding.UTF8);
                httpRequestMessage.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

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

        //API Vấn tin số dư đầu ngày
        public async Task<List<GiaoDichModel>> Get_TruyVanSoDu_DauNgay(string token, RequestBody request)
        {

            List<GiaoDichModel> data = new List<GiaoDichModel>();

            string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
            string requestID = Guid.NewGuid().ToString("D");
            try
            {
                string jsonContent = System.Text.Json.JsonSerializer.Serialize(new { pageNum = request.pageNum });

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/iconnect/account/openningBal/v1");
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpRequestMessage.Headers.Add("Timestamp", timestamp);
                httpRequestMessage.Headers.Add("X-API-Interaction-ID", requestID);
                httpRequestMessage.Content = new StringContent(jsonContent, Encoding.UTF8);
                httpRequestMessage.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

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

        //API Vấn tin số dư tài khoản
        public async Task<List<GiaoDichModel>> Get_TruyVanSoDu_TaiKhoan(string token, RequestBody request)
        {

            List<GiaoDichModel> data = new List<GiaoDichModel>();

            string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
            string requestID = Guid.NewGuid().ToString("D");
            try
            {
                string jsonContent = System.Text.Json.JsonSerializer.Serialize(new { accountNo = request.accountNo });

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/iconnect/account/getAcctDetail/v1");
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpRequestMessage.Headers.Add("Timestamp", timestamp);
                httpRequestMessage.Headers.Add("X-API-Interaction-ID", requestID);
                httpRequestMessage.Content = new StringContent(jsonContent, Encoding.UTF8);
                httpRequestMessage.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

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
    }
}
