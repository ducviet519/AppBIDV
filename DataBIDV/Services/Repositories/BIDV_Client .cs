using DataBIDV.Extensions;
using DataBIDV.Models;
using DataBIDV.Services.Interfaces;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Utilities.Encoders;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Services.Repositories
{
    public class BIDV_Client : IBIDV_Client
    {
        readonly RestClient _client;
        readonly string baseUrl = "https://bidv.net:9303/bidvorg/service";
        readonly string client_id = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\client_id.asc")) ?? "5fdc43607aba07a73c6fe5a62066f6e0";
        readonly string client_secret = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\client_secret.asc")) ?? "7ed7a5a3ca071c83d5e977863bfc537e";
        public BIDV_Client()
        {                      
            var options = new RestClientOptions(baseUrl)
            {
                ClientCertificates = new X509CertificateCollection() { new X509Certificate2(File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\cert.pem"))) },
                RemoteCertificateValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => { return true; },
            };

            _client = new RestClient(options)
            {
                Authenticator = new BIDV_Authenticator($"{baseUrl}/openapi/oauth2/token", client_id, client_secret),              
            };         
        }

        //API Vấn tin danh sách giao dịch (Có mã hóa dữ liệu)
        public async Task<List<GiaoDich>> Get_DanhSachGiaoDich_Encrypt(RequestBody requestBody)
        {

            List<GiaoDich> data = new List<GiaoDich>();

            string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
            string requestID = Guid.NewGuid().ToString("N");

            try
            {
                string encryptedContent = StaticHelper.EncryptUsingPublicKey(System.Text.Json.JsonSerializer.Serialize(requestBody));
                
                string decryptedConten = StaticHelper.DecryptUsingPrivateKey(encryptedContent);
                string jsonContent = System.Text.Json.JsonSerializer.Serialize(new { data = encryptedContent });

                RestRequest request = new RestRequest($"{baseUrl}/iconnect/account/acctHis/v1.0");
                request.Method = Method.Post;
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Timestamp", timestamp);
                request.AddHeader("X-API-Interaction-ID", requestID);
                //request.AddParameter("application/json", jsonContent, ParameterType.RequestBody);
                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;
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
        public async Task<List<GiaoDich>> Get_DanhSachGiaoDich(RequestBody requestBody)
        {
            List<GiaoDich> data = new List<GiaoDich>();

            string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
            string requestID = Guid.NewGuid().ToString("N");
            try
            {
                string jsonContent = System.Text.Json.JsonSerializer.Serialize(requestBody);
                RestRequest request = new RestRequest($"{baseUrl}/iconnect/account/getAcctHis/v1.1");
                request.Method = Method.Post;
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Timestamp", timestamp);
                request.AddHeader("X-API-Interaction-ID", requestID);
                request.AddParameter("application/json", jsonContent, ParameterType.RequestBody);
                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;

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
        public async Task<List<GiaoDich>> Get_TruyVanSoDu_DauNgay(RequestBody requestBody)
        {
            List<GiaoDich> data = new List<GiaoDich>();

            string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
            string requestID = Guid.NewGuid().ToString("N");
            try
            {             
                string jsonContent = System.Text.Json.JsonSerializer.Serialize(new { pageNum = requestBody.pageNum });
                RestRequest request = new RestRequest($"{baseUrl}/iconnect/account/openningBal/v1");
                request.Method = Method.Post;
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Timestamp", timestamp);
                request.AddHeader("X-API-Interaction-ID", requestID);               
                request.AddParameter("application/json", jsonContent, ParameterType.RequestBody);
                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;
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
        public async Task<GiaoDich> Get_TruyVanSoDu_TaiKhoan(RequestBody requestBody)
        {
            GiaoDich data = new GiaoDich();

            string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
            string requestID = Guid.NewGuid().ToString("N");
            try
            {
                string jsonContent = System.Text.Json.JsonSerializer.Serialize(new { accountNo = requestBody.accountNo });
                RestRequest request = new RestRequest($"{baseUrl}/iconnect/account/getAcctDetail/v1");
                request.Method = Method.Post;
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Timestamp", timestamp);
                request.AddHeader("X-API-Interaction-ID", requestID);
                request.AddParameter("application/json", jsonContent, ParameterType.RequestBody);
                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;
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
