using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using Jose;
using DataBIDV.Models;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;

namespace DataBIDV.Extensions
{
    public class StaticHelper
    {
        #region Encode and Decode a base64 string
        //base64 = EncodeBase64(text, Encoding.ASCII);
        public static string EncodeBase64(string text, Encoding encoding = null)
        {
            if (text == null) return null;

            encoding = encoding ?? Encoding.UTF8;
            var bytes = encoding.GetBytes(text);
            return Convert.ToBase64String(bytes);
        }

        public static string DecodeBase64(string encodedText, Encoding encoding = null)
        {
            if (encodedText == null) return null;

            encoding = encoding ?? Encoding.UTF8;
            var bytes = Convert.FromBase64String(encodedText);
            return encoding.GetString(bytes);
        }
        #endregion

        #region Encrypt And Decrypt Using Public Key And Private Key
        
        public static string EncryptUsingPublicKey(string data)
        {
            try            
            {
                byte[] byteData = Encoding.UTF8.GetBytes(data);
                string publicKey = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\public.pem"));
                var output = String.Empty;
                var rsa = RSA.Create();
                rsa.ImportFromPem(publicKey.ToCharArray());
                byte[] bytesEncrypted = rsa.Encrypt(byteData, RSAEncryptionPadding.Pkcs1);
                output = Convert.ToBase64String(bytesEncrypted);
                return output;
            }
            catch (Exception ex)
            {
                string Msg = ex.Message;
                return "";
            }
        }

        public static string DecryptUsingPrivateKey(string data)
        {
            try
            {
                byte[] byteData = Convert.FromBase64String(data);
                
                RSACryptoServiceProvider RSAprivateKey = GetPrivateKey();
                var output = Encoding.UTF8.GetString(RSAprivateKey.Decrypt(byteData, RSAEncryptionPadding.Pkcs1));
                return output;
            }
            catch (Exception ex) {
                string Msg = ex.Message;
                return null;
            }           
        }
        public static RSACryptoServiceProvider GetPrivateKey()
        {
            string privateKey = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\private.pem"));
            //PemReader pr = new PemReader(new StringReader(privateKey), new PasswordFinder("bidv"));
            PemReader pr = new PemReader(new StringReader(privateKey), new PasswordFinder("bidv"));
            AsymmetricCipherKeyPair KeyPair = (AsymmetricCipherKeyPair)pr.ReadObject();
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)KeyPair.Private);

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();// cspParams);
            csp.ImportParameters(rsaParams);
            return csp;
        }

        public static RSA GetPublicKey()
        {
            string publicKey = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\public.pem"));
            var rsa = RSA.Create();
            rsa.ImportFromPem(publicKey.ToCharArray());
            return rsa;
        }
        private class PasswordFinder : IPasswordFinder
        {
            private string password;

            public PasswordFinder(string password)
            {
                this.password = password;
            }


            public char[] GetPassword()
            {
                return password.ToCharArray();
            }
        }
        #endregion

        #region General JWE JSON Serialization
        
        //JWT with a Private RSA Key
        public static string CreateTokenJWT(RequestBody request)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(request);
            string jwk = RSAKeys.ExportPrivateKey(GetPrivateKey());
            var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(GetPublicKey().ExportRSAPublicKey(), out _);
            rsa.ImportRSAPrivateKey(GetPrivateKey().ExportRSAPrivateKey(), out _);
            string token =  Jose.JWT.Encode(json, rsa, Jose.JwsAlgorithm.RS256, options: new JwtOptions { DetachPayload = true });
            return token;
        }

        public static string DecodeToken(string token)
        {
            return Jose.JWT.Decode(token, GetPublicKey(), Jose.JwsAlgorithm.RS256);
        }
        #endregion

        #region Get Certificate Only String
        public static string GetCertificateString()
        {
            string certPemString = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\cert.pem"));
            return certPemString.Replace("-----BEGIN CERTIFICATE-----", null)
                                        .Replace("-----END CERTIFICATE-----", null)
                                        .Replace(Environment.NewLine, null)
                                        .Trim();             
        }

        #endregion
    }
}

