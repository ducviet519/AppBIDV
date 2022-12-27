using Jose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Extensions
{    
    /// <summary>
    /// Provides methods for encoding and decoding JSON Web Tokens.
    /// </summary>
    public static class JWT
    {
        private static readonly JwtSettings defaultSettings = new JwtSettings();

        /// <summary>
        /// Global default settings for JWT.
        /// </summary>
        public static JwtSettings DefaultSettings
        {
            get { return defaultSettings; }
        }

        [Obsolete("Custom JsonMappers should be set in DefaultSettings")]
        public static IJsonMapper JsonMapper
        {
            set { defaultSettings.RegisterMapper(value); }
        }

        /// <summary>
        /// Parses JWT token, extracts and unmarshal headers as IDictionary<string, object>.
        /// This method is NOT performing integrity checking.
        /// </summary>
        /// <param name="token">signed JWT token</param>
        /// <param name="settings">optional settings to override global DefaultSettings</param>
        /// <returns>unmarshalled headers</returns>
        public static IDictionary<string, object> Headers(string token, JwtSettings settings = null)
        {
            return Headers<IDictionary<string, object>>(token, settings);
        }

        /// <summary>
        /// Parses JWT token, extracts and attempts to unmarshal headers to requested type
        /// This method is NOT performing integrity checking.
        /// </summary>
        /// <param name="token">signed JWT token</param>
        /// <param name="settings">optional settings to override global DefaultSettings</param>
        /// <typeparam name="T">desired type after unmarshalling</typeparam>
        /// <returns>unmarshalled headers</returns>
        public static T Headers<T>(string token, JwtSettings settings = null)
        {
            var parts = Compact.Iterate(token);

            return GetSettings(settings).JsonMapper.Parse<T>(Encoding.UTF8.GetString(parts.Next()));
        }

        /// <summary>
        /// Parses signed JWT token, extracts and returns payload part as string
        /// This method is NOT supported for encrypted JWT tokens.
        /// This method is NOT performing integrity checking.
        /// </summary>
        /// <param name="token">signed JWT token</param>
        /// <returns>unmarshalled payload</returns>
        /// <exception cref="JoseException">if encrypted JWT token is provided</exception>
        public static string Payload(string token, bool b64 = true)
        {
            var bytes = PayloadBytes(token, b64);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Parses signed JWT token, extracts and returns payload part as binary data.
        /// This method is NOT supported for encrypted JWT tokens.
        /// This method is NOT performing integrity checking.
        /// </summary>
        /// <param name="token">signed JWT token</param>
        /// <returns>unmarshalled payload</returns>
        /// <exception cref="JoseException">if encrypted JWT token is provided</exception>
        public static byte[] PayloadBytes(string token, bool b64 = true)
        {
            var parts = Compact.Iterate(token);

            if (parts.Count < 3)
            {
                throw new JoseException(
                    "The given token doesn't follow JWT format and must contains at least three parts.");
            }

            if (parts.Count > 3)
            {
                throw new JoseException(
                    "Getting payload for encrypted tokens is not supported. Please use Jose.JWT.Decode() method instead.");
            }

            parts.Next(false); //skip header
            return parts.Next(b64);
        }

        /// <summary>
        /// Parses signed JWT token, extracts payload part and attempts to unmarshal string to requested type with configured json mapper.
        /// This method is NOT supported for encrypted JWT tokens.
        /// This method is NOT performing integrity checking.
        /// </summary>
        /// <typeparam name="T">desired type after unmarshalling</typeparam>
        /// <param name="token">signed JWT token</param>
        /// <param name="settings">optional settings to override global DefaultSettings</param>
        /// <returns>unmarshalled payload</returns>
        /// <exception cref="JoseException">if encrypted JWT token is provided</exception>
        public static T Payload<T>(string token, JwtSettings settings = null)
        {
            return GetSettings(settings).JsonMapper.Parse<T>(Payload(token));
        }

        /// <summary>
        /// Serialize and encodes object to JWT token and applies requested encryption/compression algorithms.
        /// </summary>
        /// <param name="payload">json string to encode</param>
        /// <param name="key">key for encryption, suitable for provided JWS algorithm, can be null.</param>
        /// <param name="alg">JWT algorithm to be used.</param>
        /// <param name="enc">encryption algorithm to be used.</param>
        /// <param name="compression">optional compression type to use.</param>
        /// <param name="extraHeaders">optional extra headers to pass along with the payload.</param>
        /// <param name="settings">optional settings to override global DefaultSettings</param>
        /// <returns>JWT in compact serialization form, encrypted and/or compressed.</returns>
        public static string Encode(object payload, object key, JweAlgorithm alg, JweEncryption enc, JweCompression? compression = null, IDictionary<string, object> extraHeaders = null, JwtSettings settings = null)
        {
            return Encode(GetSettings(settings).JsonMapper.Serialize(payload), key, alg, enc, compression, extraHeaders, settings);
        }

        /// <summary>
        /// Encodes given json string to JWT token and applies requested encryption/compression algorithms.
        /// Json string to encode will be obtained via configured IJsonMapper implementation.
        /// </summary>
        /// <param name="payload">json string to encode (not null or whitespace)</param>
        /// <param name="key">key for encryption, suitable for provided JWS algorithm, can be null.</param>
        /// <param name="alg">JWT algorithm to be used.</param>
        /// <param name="enc">encryption algorithm to be used.</param>
        /// <param name="compression">optional compression type to use.</param>
        /// <param name="extraHeaders">optional extra headers to pass along with the payload.</param>
        /// <param name="settings">optional settings to override global DefaultSettings</param>
        /// <returns>JWT in compact serialization form, encrypted and/or compressed.</returns>
        public static string Encode(string payload, object key, JweAlgorithm alg, JweEncryption enc, JweCompression? compression = null, IDictionary<string, object> extraHeaders = null, JwtSettings settings = null)
        {
            byte[] plainText = Encoding.UTF8.GetBytes(payload);

            return EncodeBytes(plainText, key, alg, enc, compression, extraHeaders, settings);
        }

        /// <summary>
        /// Encodes given binary data to JWT token and applies requested encryption/compression algorithms.
        /// </summary>
        /// <param name="payload">Binary data to encode (not null)</param>
        /// <param name="key">key for encryption, suitable for provided JWS algorithm, can be null.</param>
        /// <param name="alg">JWT algorithm to be used.</param>
        /// <param name="enc">encryption algorithm to be used.</param>
        /// <param name="compression">optional compression type to use.</param>
        /// <param name="extraHeaders">optional extra headers to pass along with the payload.</param>
        /// <param name="settings">optional settings to override global DefaultSettings</param>
        /// <returns>JWT in compact serialization form, encrypted and/or compressed.</returns>
        public static string EncodeBytes(byte[] payload, object key, JweAlgorithm alg, JweEncryption enc, JweCompression? compression = null, IDictionary<string, object> extraHeaders = null, JwtSettings settings = null)
        {
            return JWE.EncryptBytes(payload, new JweRecipient[] { new JweRecipient(alg, key) }, enc, aad: null, SerializationMode.Compact, compression, extraHeaders, null, settings);
        }

        /// <summary>
        /// Serialize and encodes object to JWT token and sign it using given algorithm.
        /// Json string to encode will be obtained via configured IJsonMapper implementation.
        /// </summary>
        /// <param name="payload">object to map to json string and encode</param>
        /// <param name="key">key for signing, suitable for provided JWS algorithm, can be null.</param>
        /// <param name="algorithm">JWT algorithm to be used.</param>
        /// <param name="extraHeaders">optional extra headers to pass along with the payload.</param>
        /// <param name="settings">optional settings to override global DefaultSettings</param>
        /// <param name="options">additional encoding options</param>
        /// <returns>JWT in compact serialization form, digitally signed.</returns>
        public static string Encode(object payload, object key, JwsAlgorithm algorithm, IDictionary<string, object> extraHeaders = null, JwtSettings settings = null, JwtOptions options = null)
        {
            return Encode(GetSettings(settings).JsonMapper.Serialize(payload), key, algorithm, extraHeaders, settings, options);
        }

        /// <summary>
        /// Encodes given json string to JWT token and sign it using given algorithm.
        /// </summary>
        /// <param name="payload">json string to encode (not null or whitespace)</param>
        /// <param name="key">key for signing, suitable for provided JWS algorithm, can be null.</param>
        /// <param name="algorithm">JWT algorithm to be used.</param>
        /// <param name="extraHeaders">optional extra headers to pass along with the payload.</param>
        /// <param name="settings">optional settings to override global DefaultSettings</param>
        /// <param name="options">additional encoding options</param>
        /// <returns>JWT in compact serialization form, digitally signed.</returns>
        public static string Encode(string payload, object key, JwsAlgorithm algorithm, IDictionary<string, object> extraHeaders = null, JwtSettings settings = null, JwtOptions options = null)
        {
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            return EncodeBytes(payloadBytes, key, algorithm, extraHeaders, settings, options);
        }

        /// <summary>
        /// Encodes given binary data to JWT token and sign it using given algorithm.
        /// </summary>
        /// <param name="payload">Binary data to encode (not null)</param>
        /// <param name="key">key for signing, suitable for provided JWS algorithm, can be null.</param>
        /// <param name="algorithm">JWT algorithm to be used.</param>
        /// <param name="extraHeaders">optional extra headers to pass along with the payload.</param>
        /// <param name="settings">optional settings to override global DefaultSettings</param>
        /// <param name="options">additional encoding options</param>
        /// <returns>JWT in compact serialization form, digitally signed.</returns>
        public static string EncodeBytes(byte[] payload, object key, JwsAlgorithm algorithm, IDictionary<string, object> extraHeaders = null, JwtSettings settings = null, JwtOptions options = null)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            var jwtSettings = GetSettings(settings);
            var jwtOptions = options ?? JwtOptions.Default;

            var jwtHeader = new Dictionary<string, object> { { "alg", jwtSettings.JwsHeaderValue(algorithm) } };

            if (extraHeaders == null) //allow overload, but keep backward compatible defaults
            {
                extraHeaders = new Dictionary<string, object> { { "typ", "JWT" } };
            }

            if (!jwtOptions.EncodePayload)
            {
                jwtHeader["b64"] = false;
                jwtHeader["crit"] = Collections.Union(new[] { "b64" }, Dictionaries.Get<object>(extraHeaders, "crit"));
            }

            Dictionaries.Append(jwtHeader, extraHeaders);
            byte[] headerBytes = Encoding.UTF8.GetBytes(jwtSettings.JsonMapper.Serialize(jwtHeader));

            var jwsAlgorithm = jwtSettings.Jws(algorithm);

            if (jwsAlgorithm == null)
            {
                throw new JoseException(string.Format("Unsupported JWS algorithm requested: {0}", algorithm));
            }

            byte[] signature = jwsAlgorithm.Sign(securedInput(headerBytes, payload, jwtOptions.EncodePayload), key);

            byte[] payloadBytes = jwtOptions.DetachPayload ? new byte[0] : payload;

            return jwtOptions.EncodePayload
                ? Compact.Serialize(headerBytes, payloadBytes, signature)
                : Compact.Serialize(headerBytes, Encoding.UTF8.GetString(payloadBytes), signature);
        }

        /// <summary>
        /// Decodes JWT token by performing necessary decompression/decryption and signature verification as defined in JWT token header.
        /// Resulting json string is returned untouched (e.g. no parsing or mapping)
        /// </summary>
        /// <param name="token">JWT token in compact serialization form.</param>
        /// <param name="key">key for decoding suitable for JWT algorithm used.</param>
        /// <param name="alg">The algorithm type that we expect to receive in the header.</param>
        /// <param name="enc">The encryption type that we expect to receive in the header.</param>
        /// <param name="settings">optional settings to override global DefaultSettings</param>
        /// <returns>decoded json string</returns>
        /// <exception cref="IntegrityException">if signature validation failed</exception>
        /// <exception cref="EncryptionException">if JWT token can't be decrypted</exception>
        /// <exception cref="InvalidAlgorithmException">if JWT signature, encryption or compression algorithm is not supported</exception>
        public static string Decode(string token, object key, JweAlgorithm alg, JweEncryption enc, JwtSettings settings = null)
        {
            return Decode(token, key, null, alg, enc, settings);
        }

        /// <summary>
        /// Decodes JWT token by performing necessary decompression/decryption and signature verification as defined in JWT token header.
        /// Resulting bytes of the payload are returned untouched (e.g. no parsing or mapping)
        /// </summary>
        /// <param name="token">JWT token in compact serialization form.</param>
        /// <param name="key">key for decoding suitable for JWT algorithm used.</param>
        /// <param name="alg">The algorithm type that we expect to receive in the header.</param>
        /// <param name="enc">The encryption type that we expect to receive in the header.</param>
        /// <param name="settings">optional settings to override global DefaultSettings</param>
        /// <returns>Decrypted payload as binary data</returns>
        /// <exception cref="IntegrityException">if signature validation failed</exception>
        /// <exception cref="EncryptionException">if JWT token can't be decrypted</exception>
        /// <exception cref="InvalidAlgorithmException">if JWT signature, encryption or compression algorithm is not supported</exception>
        public static byte[] DecodeBytes(string token, object key, JweAlgorithm alg, JweEncryption enc, JwtSettings settings = null)
        {
            return DecodeBytes(token, key, null, alg, enc, settings);
        }

        /// <summary>
        /// Decodes JWT token by performing necessary decompression/decryption and signature verification as defined in JWT token header.
        /// Resulting json string is returned untouched (e.g. no parsing or mapping)
        /// </summary>
        /// <param name="token">JWT token in compact serialization form.</param>
        /// <param name="key">key for decoding suitable for JWT algorithm used.</param>
        /// <param name="alg">The algorithm type that we expect to receive in the header.</param>
        /// <param name="settings">optional settings to override global DefaultSettings</param>
        /// <param name="payload">optional detached payload</param>
        /// <returns>decoded json string</returns>
        /// <exception cref="IntegrityException">if signature validation failed</exception>
        /// <exception cref="EncryptionException">if JWT token can't be decrypted</exception>
        /// <exception cref="InvalidAlgorithmException">if JWT signature, encryption or compression algorithm is not supported</exception>
        public static string Decode(string token, object key, JwsAlgorithm alg, JwtSettings settings = null, string payload = null)
        {
            return Decode(token, key, alg, null, null, settings, payload);
        }

        /// <summary>
        /// Decodes JWT token by performing necessary decompression/decryption and signature verification as defined in JWT token header.
        /// Resulting bytes of the payload are returned untouched (e.g. no parsing or mapping)
        /// </summary>
        /// <param name="token">JWT token in compact serialization form.</param>
        /// <param name="key">key for decoding suitable for JWT algorithm used.</param>
        /// <param name="alg">The algorithm type that we expect to receive in the header.</param>
        /// <param name="settings">optional settings to override global DefaultSettings</param>
        /// <param name="payload">optional detached payload</param>
        /// <returns>The payload as binary data</returns>
        /// <exception cref="IntegrityException">if signature validation failed</exception>
        /// <exception cref="EncryptionException">if JWT token can't be decrypted</exception>
        /// <exception cref="InvalidAlgorithmException">if JWT signature, encryption or compression algorithm is not supported</exception>
        public static byte[] DecodeBytes(string token, object key, JwsAlgorithm alg, JwtSettings settings = null, byte[] payload = null)
        {
            return DecodeBytes(token, key, alg, null, null, settings, payload);
        }

        /// <summary>
        /// Decodes JWT token by performing necessary decompression/decryption and signature verification as defined in JWT token header.
        /// Resulting json string is returned untouched (e.g. no parsing or mapping)
        /// </summary>
        /// <param name="token">JWT token in compact serialization form.</param>
        /// <param name="key">key for decoding suitable for JWT algorithm used, can be null.</param>
        /// <param name="settings">optional settings to override global DefaultSettings</param>
        /// <param name="payload">optional detached payload</param>
        /// <returns>decoded json string</returns>
        /// <exception cref="IntegrityException">if signature validation failed</exception>
        /// <exception cref="EncryptionException">if JWT token can't be decrypted</exception>
        /// <exception cref="InvalidAlgorithmException">if JWT signature, encryption or compression algorithm is not supported</exception>
        public static string Decode(string token, object key = null, JwtSettings settings = null, string payload = null)
        {
            return Decode(token, key, null, null, null, settings, payload);
        }

        /// <summary>
        /// Decodes JWT token by performing necessary decompression/decryption and signature verification as defined in JWT token header.
        /// Resulting binary payload is returned untouched (e.g. no parsing or mapping)
        /// </summary>
        /// <param name="token">JWT token in compact serialization form.</param>
        /// <param name="key">key for decoding suitable for JWT algorithm used, can be null.</param>
        /// <param name="settings">optional settings to override global DefaultSettings</param>
        /// <param name="payload">optional detached payload</param>
        /// <returns>The payload as binary data</returns>
        /// <exception cref="IntegrityException">if signature validation failed</exception>
        /// <exception cref="EncryptionException">if JWT token can't be decrypted</exception>
        /// <exception cref="InvalidAlgorithmException">if JWT signature, encryption or compression algorithm is not supported</exception>
        public static byte[] DecodeBytes(string token, object key = null, JwtSettings settings = null, byte[] payload = null)
        {
            return DecodeBytes(token, key, null, null, null, settings, payload);
        }

        /// <summary>
        /// Decodes JWT token by performing necessary decompression/decryption and signature verification as defined in JWT token header.
        /// Resulting json string will be parsed and mapped to desired type via configured IJsonMapper implementation.
        /// </summary>
        /// <typeparam name="T">Deserid object type after json mapping</typeparam>
        /// <param name="token">JWT token in compact serialization form.</param>
        /// <param name="key">key for decoding suitable for JWT algorithm used.</param>
        /// <param name="alg">The algorithm type that we expect to receive in the header.</param>
        /// <param name="enc">The encryption type that we expect to receive in the header.</param>
        /// <param name="settings">optional settings to override global DefaultSettings</param>
        /// <returns>object of provided T, result of decoded json mapping</returns>
        /// <exception cref="IntegrityException">if signature validation failed</exception>
        /// <exception cref="EncryptionException">if JWT token can't be decrypted</exception>
        /// <exception cref="InvalidAlgorithmException">if JWT signature, encryption or compression algorithm is not supported</exception>
        public static T Decode<T>(string token, object key, JweAlgorithm alg, JweEncryption enc, JwtSettings settings = null)
        {
            return GetSettings(settings).JsonMapper.Parse<T>(Decode(token, key, alg, enc, settings));
        }

        /// <summary>
        /// Decodes JWT token by performing necessary decompression/decryption and signature verification as defined in JWT token header.
        /// Resulting json string will be parsed and mapped to desired type via configured IJsonMapper implementation.
        /// </summary>
        /// <typeparam name="T">Deserid object type after json mapping</typeparam>
        /// <param name="token">JWT token in compact serialization form.</param>
        /// <param name="key">key for decoding suitable for JWT algorithm used.</param>
        /// <param name="alg">The algorithm type that we expect to receive in the header.</param>
        /// <param name="settings">optional settings to override global DefaultSettings</param>
        /// <returns>object of provided T, result of decoded json mapping</returns>
        /// <exception cref="IntegrityException">if signature validation failed</exception>
        /// <exception cref="EncryptionException">if JWT token can't be decrypted</exception>
        /// <exception cref="InvalidAlgorithmException">if JWT signature, encryption or compression algorithm is not supported</exception>
        public static T Decode<T>(string token, object key, JwsAlgorithm alg, JwtSettings settings = null)
        {
            return GetSettings(settings).JsonMapper.Parse<T>(Decode(token, key, alg, settings));
        }

        /// <summary>
        /// Decodes JWT token by performing necessary decompression/decryption and signature verification as defined in JWT token header.
        /// Resulting json string will be parsed and mapped to desired type via configured IJsonMapper implementation.
        /// </summary>
        /// <typeparam name="T">Deserid object type after json mapping</typeparam>
        /// <param name="token">JWT token in compact serialization form.</param>
        /// <param name="key">key for decoding suitable for JWT algorithm used, can be null.</param>
        /// <param name="settings">optional settings to override global DefaultSettings</param>
        /// <returns>object of provided T, result of decoded json mapping</returns>
        /// <exception cref="IntegrityException">if signature validation failed</exception>
        /// <exception cref="EncryptionException">if JWT token can't be decrypted</exception>
        /// <exception cref="InvalidAlgorithmException">if JWT signature, encryption or compression algorithm is not supported</exception>
        public static T Decode<T>(string token, object key = null, JwtSettings settings = null)
        {
            return GetSettings(settings).JsonMapper.Parse<T>(Decode(token, key, settings));
        }

        private static byte[] DecodeBytes(string token, object key = null, JwsAlgorithm? expectedJwsAlg = null, JweAlgorithm? expectedJweAlg = null, JweEncryption? expectedJweEnc = null, JwtSettings settings = null, byte[] payload = null)
        {
            Ensure.IsNotEmpty(token, "Incoming token expected to be in compact serialization form, not empty, whitespace or null.");

            var parts = Compact.Iterate(token);

            if (parts.Count == 5) //encrypted JWT
            {
                return JWE.Decrypt(token, key, expectedJweAlg, expectedJweEnc, settings).PlaintextBytes;
            }
            else
            {
                //signed or plain JWT
                var jwtSettings = GetSettings(settings);

                byte[] header = parts.Next();

                var headerData = jwtSettings.JsonMapper.Parse<IDictionary<string, object>>(Encoding.UTF8.GetString(header));

                bool b64 = true;

                object value;
                if (headerData.TryGetValue("b64", out value))
                {
                    b64 = (bool)value;
                }

                byte[] contentPayload = parts.Next(b64);
                byte[] signature = parts.Next();

                var effectivePayload = payload ?? contentPayload;

                var algorithm = (string)headerData["alg"];
                var jwsAlgorithm = jwtSettings.JwsAlgorithmFromHeader(algorithm);
                if (expectedJwsAlg != null && expectedJwsAlg != jwsAlgorithm)
                {
                    throw new InvalidAlgorithmException(
                        "The algorithm type passed to the Decode method did not match the algorithm type in the header.");
                }

                var jwsAlgorithmImpl = jwtSettings.Jws(jwsAlgorithm);

                if (jwsAlgorithmImpl == null)
                {
                    throw new JoseException(string.Format("Unsupported JWS algorithm requested: {0}", algorithm));
                }

                if (!jwsAlgorithmImpl.Verify(signature, securedInput(header, effectivePayload, b64), key))
                {
                    throw new IntegrityException("Invalid signature.");
                }

                return effectivePayload;
            }
        }

        private static string Decode(string token, object key = null, JwsAlgorithm? jwsAlg = null, JweAlgorithm? jweAlg = null, JweEncryption? jweEnc = null, JwtSettings settings = null, string payload = null)
        {
            var detached = payload != null ? Encoding.UTF8.GetBytes(payload) : null;

            var payloadBytes = DecodeBytes(token, key, jwsAlg, jweAlg, jweEnc, settings, detached);

            return Encoding.UTF8.GetString(payloadBytes);
        }

        private static byte[] DecryptBytes(Compact.Iterator parts, object key, JweAlgorithm? jweAlg, JweEncryption? jweEnc, JwtSettings settings = null)
        {
            byte[] header = parts.Next();
            byte[] encryptedCek = parts.Next();
            byte[] iv = parts.Next();
            byte[] cipherText = parts.Next();
            byte[] authTag = parts.Next();

            JwtSettings jwtSettings = GetSettings(settings);
            IDictionary<string, object> jwtHeader = jwtSettings.JsonMapper.Parse<IDictionary<string, object>>(Encoding.UTF8.GetString(header));

            JweAlgorithm headerAlg = jwtSettings.JwaAlgorithmFromHeader((string)jwtHeader["alg"]);
            JweEncryption headerEnc = jwtSettings.JweAlgorithmFromHeader((string)jwtHeader["enc"]);

            IKeyManagement keys = jwtSettings.Jwa(headerAlg);
            IJweAlgorithm enc = jwtSettings.Jwe(headerEnc);

            if (keys == null)
            {
                throw new JoseException(string.Format("Unsupported JWA algorithm requested: {0}", headerAlg));
            }

            if (enc == null)
            {
                throw new JoseException(string.Format("Unsupported JWE algorithm requested: {0}", headerEnc));
            }

            if (jweAlg != null && (JweAlgorithm)jweAlg != headerAlg)
            {
                throw new InvalidAlgorithmException("The algorithm type passed to the Decrypt method did not match the algorithm type in the header.");
            }

            if (jweEnc != null && (JweEncryption)jweEnc != headerEnc)
            {
                throw new InvalidAlgorithmException("The encryption type passed to the Decrypt method did not match the encryption type in the header.");
            }

            byte[] cek = keys.Unwrap(encryptedCek, key, enc.KeySize, jwtHeader);
            byte[] aad = Encoding.UTF8.GetBytes(Compact.Serialize(header));

            byte[] plainText = enc.Decrypt(aad, cek, iv, cipherText, authTag);

            if (jwtHeader.ContainsKey("zip"))
            {
                var compression = jwtSettings.Compression((string)jwtHeader["zip"]);

                plainText = compression.Decompress(plainText);
            }

            return plainText;
        }

        private static JwtSettings GetSettings(JwtSettings settings)
        {
            return settings ?? defaultSettings;
        }

        private static byte[] securedInput(byte[] header, byte[] payload, bool b64)
        {
            return b64
                ? Encoding.UTF8.GetBytes(Compact.Serialize(header, payload))
                : Arrays.Concat(Encoding.UTF8.GetBytes(Compact.Serialize(header)),
                                Encoding.UTF8.GetBytes("."),
                                payload);
        }
    }

}
