using DataBIDV.Extensions;
using DataBIDV.Models;
using DataBIDV.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Services.Repositories
{
    public class ConnectAPI_BIDVClient : IConnectAPI_BIDVClient
    {

        private readonly string baseUrl = "https://bidv.net:9303/bidvorg/service";
        private readonly string client_id = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\client_id.asc")) ?? "5fdc43607aba07a73c6fe5a62066f6e0";
        private readonly string client_secret = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\client_secret.asc")) ?? "7ed7a5a3ca071c83d5e977863bfc537e";
        
        public ConnectAPI_BIDVClient()
        {
            
        }

        public static HttpClient GetHttpClient(string token = null){
            
            var handler = new HttpClientHandler();
            handler.SslProtocols = SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13;
            var certificate = new X509Certificate2(File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\cert.pem")));
            handler.ClientCertificates.Add(certificate);
            
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => { return true; };
            var _httpClient = new HttpClient(handler);
            _httpClient.BaseAddress = new Uri("https://bidv.net:9303/");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
            if(!String.IsNullOrEmpty(token))
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
                string requestID = Guid.NewGuid().ToString("N");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Timestamp", timestamp);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-API-Interaction-ID", requestID);
            }    

            return _httpClient;
        }
        public async Task<TokenAPI> Get_API_Token()
        {
            TokenAPI data = new TokenAPI();
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "read"),
                new KeyValuePair<string, string>("client_id", client_id),
                new KeyValuePair<string, string>("client_secret", client_secret),
            });

            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/openapi/oauth2/token");
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                httpRequestMessage.Content = requestContent;             
                HttpResponseMessage response = await GetHttpClient().SendAsync(httpRequestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    data = JsonConvert.DeserializeObject<TokenAPI>(responseContent);
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
        public async Task<List<GiaoDich>> Get_DanhSachGiaoDich_Encrypt(string token, RequestBody request)
        {           
            List<GiaoDich> data = new List<GiaoDich>();           
            try
            {
                //Encrypt
                string encryptedContent = StaticHelper.EncryptUsingPublicKey(System.Text.Json.JsonSerializer.Serialize(request));
                string jsonContent = JsonConvert.SerializeObject(new { data = encryptedContent });

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/iconnect/account/acctHis/v1.0");

                httpRequestMessage.Content = new StringContent(jsonContent, Encoding.UTF8);
                httpRequestMessage.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
               
                HttpResponseMessage response = await GetHttpClient().SendAsync(httpRequestMessage);
 
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();  
                }
                var json = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\acctHis_v1.0.json"));
                var res = JsonConvert.DeserializeObject<ResponseGiaoDich_Encrypt>(json);
                var decode = StaticHelper.DecodeBase64(res.data);
                Data_GiaoDich list = JsonConvert.DeserializeObject<Data_GiaoDich>(decode);
                
                return data ;
            }
            catch (Exception ex)
            {
                string Msg = ex.Message;
                throw;
            }
        }

        //API Vấn tin danh sách giao dịch (Không mã hóa dữ liệu)
        public async Task<List<GiaoDich>> Get_DanhSachGiaoDich(string token, RequestBody request)
        {

            List<GiaoDich> data = new List<GiaoDich>();

            string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
            string requestID = Guid.NewGuid().ToString("N");
            try
            {
                string jsonContent = JsonConvert.SerializeObject(request);

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/iconnect/account/getAcctHis/v1.1");
                httpRequestMessage.Content = new StringContent(jsonContent, Encoding.UTF8);
                httpRequestMessage.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

                HttpResponseMessage response = await GetHttpClient().SendAsync(httpRequestMessage);
 
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                }
                var json = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\getAcctHis_v1.1.json"));
                Root_GiaoDich myDeserializedClass = JsonConvert.DeserializeObject<Root_GiaoDich>(json);
                return data;
            }
            catch (Exception ex)
            {
                string Msg = ex.Message;
                throw;
            }
        }

        //API Vấn tin số dư đầu ngày
        public async Task<List<GiaoDich>> Get_TruyVanSoDu_DauNgay(string token, RequestBody request)
        {
            List<GiaoDich> data = new List<GiaoDich>();

            string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
            string requestID = Guid.NewGuid().ToString("N");
            try
            {
                string jsonContent = JsonConvert.SerializeObject(new { pageNum = request.pageNum });

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/iconnect/account/openningBal/v1");

                httpRequestMessage.Content = new StringContent(jsonContent, Encoding.UTF8);
                httpRequestMessage.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

                HttpResponseMessage response = await GetHttpClient(token).SendAsync(httpRequestMessage);

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
        public async Task<List<GiaoDich>> Get_TruyVanSoDu_TaiKhoan(string token, RequestBody request)
        {

            List<GiaoDich> data = new List<GiaoDich>();

            string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
            string requestID = Guid.NewGuid().ToString("N");
            try
            {
                string jsonContent = JsonConvert.SerializeObject(new { accountNo = request.accountNo });

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/iconnect/account/getAcctDetail/v1");
                httpRequestMessage.Content = new StringContent(jsonContent, Encoding.UTF8);
                httpRequestMessage.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

                HttpResponseMessage response = await GetHttpClient().SendAsync(httpRequestMessage);

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
