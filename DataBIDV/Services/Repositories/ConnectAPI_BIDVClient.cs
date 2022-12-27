using DataBIDV.Extensions;
using DataBIDV.Models;
using DataBIDV.Services.Interfaces;
using Jose;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Services.Repositories
{
    public class ConnectAPI_BIDVClient : IConnectAPI_BIDVClient
    {
        #region Kết nối API BIDV
        private readonly string baseUrl = "https://bidv.net:9303/bidvorg/service";
        private readonly string client_id = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\client_id.asc"));
        private readonly string client_secret = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\client_secret.asc"));

        private static HttpClient Get_HttpClient(string token = null){            
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
            return _httpClient;
        }
        private async Task<TokenAPI> Get_API_Token()
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
                HttpResponseMessage response = await Get_HttpClient().SendAsync(httpRequestMessage);

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
        private async Task<string> Get_API_Data(string uri, string jsonContent)
        {
            var tokenData = await Get_API_Token();
            string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
            string requestID = Guid.NewGuid().ToString("N");
            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}{uri}");
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(tokenData.token_type ?? "Bearer", tokenData.access_token ?? "");
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpRequestMessage.Headers.Add("Channel", "ICONNECT");
                httpRequestMessage.Headers.Add("Timestamp", timestamp);
                httpRequestMessage.Headers.Add("X-API-Interaction-ID", requestID);
                httpRequestMessage.Headers.Add("X-JWS-Signature", StaticHelper.CreateTokenJWS(jsonContent));
                httpRequestMessage.Headers.Add("X-Client-Certificate", StaticHelper.GetCertificateString());
                httpRequestMessage.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await Get_HttpClient().SendAsync(httpRequestMessage);
               
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                return null;
            }
            catch (Exception ex)
            {
                string Msg = ex.Message;
                throw;
            }
        }
        #endregion
        
        //API Vấn tin danh sách giao dịch (Có mã hóa dữ liệu)
        public async Task<List<GiaoDich>> Get_DanhSachGiaoDich_Encrypt(RequestBody request)
        {          
            List<GiaoDich> data = new List<GiaoDich>();           
            try
            {
                string jsonContent = StaticHelper.EncryptionJWE(request);
                var responseData = await Get_API_Data("/iconnect/account/getAcctHis/v1.1", jsonContent);
                var model = JsonConvert.DeserializeObject<Root_GiaoDich>(responseData);
                foreach (var item in model.data.rows)
                {
                    var row = new GiaoDich()
                    {
                        requestId = model.requestId,
                        accountNo = request.accountNo,
                        amount = item.amount,
                        curr = item.curr,
                        dorc = item.dorc,
                        remark = item.remark,
                        transDate = item.transDate,
                        transTime = item.transTime,
                    };
                    data.Add(row);
                }
                return data;
            }
            catch (Exception ex)
            {
                string Msg = ex.Message;
                throw;
            }
        }

        //API Vấn tin danh sách giao dịch (Không mã hóa dữ liệu)
        public async Task<List<GiaoDich>> Get_DanhSachGiaoDich(RequestBody request)
        {
            List<GiaoDich> data = new List<GiaoDich>();
            try
            {
                string jsonContent = JsonConvert.SerializeObject(request);
                var responseData = await Get_API_Data("/iconnect/account/getAcctHis/v1.1", jsonContent);
                var json = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\getAcctHis_v1.1.json"));
                Root_GiaoDich model = JsonConvert.DeserializeObject<Root_GiaoDich>(json);
                foreach (var item in model.data.rows)
                {
                    var row = new GiaoDich()
                    {
                        requestId = model.requestId,
                        accountNo = request.accountNo,
                        amount = item.amount,
                        curr = item.curr,
                        dorc = item.dorc,
                        remark = item.remark,
                        transDate = item.transDate,
                        transTime = item.transTime,
                    };
                    data.Add(row);
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
        public async Task<List<GiaoDich>> Get_TruyVanSoDu_DauNgay(RequestBody request)
        {
            List<GiaoDich> data = new List<GiaoDich>();

            string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
            string requestID = Guid.NewGuid().ToString("N");
            try
            {
                string jsonContent = JsonConvert.SerializeObject(new { pageNum = request.pageNum });
                var responseData = await Get_API_Data("/iconnect/account/openningBal/v1", jsonContent);
                return data;
            }
            catch (Exception ex)
            {
                string Msg = ex.Message;
                throw;
            }
        }

        //API Vấn tin số dư tài khoản
        public async Task<List<GiaoDich>> Get_TruyVanSoDu_TaiKhoan(RequestBody request)
        {           
            List<GiaoDich> data = new List<GiaoDich>();
            try
            {
                string jsonContent = JsonConvert.SerializeObject(new { accountNo = request.accountNo });
                var responseData = await Get_API_Data("/iconnect/account/getAcctDetail/v1", jsonContent);
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
