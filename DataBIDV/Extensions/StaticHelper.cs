using DataBIDV.Models;
using Jose;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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

        #region General JSON Web Signature (JWS)

        public static string CreateTokenJWS(string json)
        {            
            var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(GetPublicKey().ExportRSAPublicKey(), out _);
            rsa.ImportRSAPrivateKey(GetPrivateKey().ExportRSAPrivateKey(), out _);

            return Jose.JWT.Encode(json, rsa, Jose.JwsAlgorithm.RS256, options: new JwtOptions { DetachPayload = true });
        }

        #endregion

        #region General JSON Web Encryption (JWE)

        public static string EncryptionJWE(RequestBody request)
        {
            var symmetric_key = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\symmetric_key.asc"));
            string json = System.Text.Json.JsonSerializer.Serialize(request);

            var key = ConvertHexStringToByteArray(symmetric_key);
            var jwk = new Jwk(ConvertHexStringToByteArray(symmetric_key)); 

            var header = new Dictionary<string, object>()
            {
                //{ "alg", "dir" },
                //{ "enc", "A128CBC_HS256" }
            };

            return DataBIDV.Extensions.JWE.Encrypt(json, new[] { new JweRecipient(JweAlgorithm.DIR, key) }, JweEncryption.A128CBC_HS256, extraProtectedHeaders: header);
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

        #region ConvertHexStringToByteArray
        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }
        #endregion
    }
}

