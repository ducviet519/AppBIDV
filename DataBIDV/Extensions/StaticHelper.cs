using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;

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
                string privateKey = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\private.pem"));
                RSACryptoServiceProvider RSAprivateKey = ImportPrivateKey(privateKey);
                var output = Encoding.UTF8.GetString(RSAprivateKey.Decrypt(byteData, RSAEncryptionPadding.Pkcs1));
                
                //var rsa = RSA.Create();
                //rsa.ImportFromPem(privateKey.ToCharArray());
                //var rsa = ReadKeyFromFile();
                //var keys = Encoding.UTF8.GetString(rsa.Decrypt(byteData, RSAEncryptionPadding.OaepSHA256));
                return output;
            }
            catch (Exception ex) {
                string Msg = ex.Message;
                return null;
            }           
        }
        private static RSACryptoServiceProvider ImportPrivateKey(string pem)
        {
            PemReader pr = new PemReader(new StringReader(pem), new PasswordFinder("bidv"));
            AsymmetricCipherKeyPair KeyPair = (AsymmetricCipherKeyPair)pr.ReadObject();
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)KeyPair.Private);

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();// cspParams);
            csp.ImportParameters(rsaParams);
            return csp;
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
    }
}
