// Copyright (c) 2026 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Xsolla.AccelByte
{
    /// <summary>
    /// Pure helpers for the Xsolla ID authorization code flow with PKCE (RFC 7636, OpenID Connect Core).
    /// No Unity API is used here, so the class can be tested outside the engine.
    /// </summary>
    internal static class XsollaIdProtocol
    {
        // 32 random bytes give a 43-character code_verifier, the minimum length of RFC 7636 section 4.1.
        private const int RandomValueByteCount = 32;

        // Allowance for a wrong clock on the player's machine. AGS IAM checks exp again with its own clock.
        private const long ExpiryLeewaySeconds = 300;

        public static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        public static byte[] Base64UrlDecode(string encoded)
        {
            string standard = encoded.Replace('-', '+').Replace('_', '/');
            switch (standard.Length % 4)
            {
                case 2: standard += "=="; break;
                case 3: standard += "="; break;
            }
            return Convert.FromBase64String(standard);
        }

        public static string CreateRandomValue()
        {
            var bytes = new byte[RandomValueByteCount];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(bytes);
            }
            return Base64UrlEncode(bytes);
        }

        /// <summary>BASE64URL(SHA256(ASCII(codeVerifier))), the S256 method of RFC 7636 section 4.2.</summary>
        public static string CreateCodeChallenge(string codeVerifier)
        {
            using (var sha256 = SHA256.Create())
            {
                return Base64UrlEncode(sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier)));
            }
        }

        public static string NormalizeIssuer(string issuer)
        {
            return (issuer ?? string.Empty).TrimEnd('/');
        }

        public static string BuildRedirectUri(string host, int port, string path)
        {
            return $"http://{host}:{port}{path}";
        }

        public static string BuildAuthorizeUrl(string issuer, string clientId, string redirectUri, string scope, string state, string nonce, string codeChallenge)
        {
            var parameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("response_type", "code"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("scope", scope),
                new KeyValuePair<string, string>("state", state),
                new KeyValuePair<string, string>("nonce", nonce),
                new KeyValuePair<string, string>("code_challenge", codeChallenge),
                new KeyValuePair<string, string>("code_challenge_method", "S256")
            };
            return NormalizeIssuer(issuer) + "/oauth2/auth?" + string.Join("&",
                parameters.Select(p => Uri.EscapeDataString(p.Key) + "=" + Uri.EscapeDataString(p.Value)));
        }

        public static string BuildTokenEndpoint(string issuer)
        {
            return NormalizeIssuer(issuer) + "/oauth2/token";
        }

        public static Dictionary<string, string> BuildTokenRequestForm(string clientId, string code, string redirectUri, string codeVerifier)
        {
            return new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", clientId },
                { "code", code },
                { "redirect_uri", redirectUri },
                { "code_verifier", codeVerifier }
            };
        }

        /// <summary>Splits "GET /callback?code=...&amp;state=... HTTP/1.1" into method, path and query.</summary>
        public static bool TryParseRequestLine(string requestLine, out string method, out string path, out string query)
        {
            method = path = query = string.Empty;
            string[] tokens = (requestLine ?? string.Empty).Split(' ');
            if (tokens.Length < 2)
            {
                return false;
            }

            method = tokens[0];
            string target = tokens[1];
            int queryStart = target.IndexOf('?');
            path = queryStart < 0 ? target : target.Substring(0, queryStart);
            query = queryStart < 0 ? string.Empty : target.Substring(queryStart + 1);
            return true;
        }

        public static Dictionary<string, string> ParseQuery(string query)
        {
            var result = new Dictionary<string, string>();
            foreach (string pair in (query ?? string.Empty).Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int separator = pair.IndexOf('=');
                string key = separator < 0 ? pair : pair.Substring(0, separator);
                string value = separator < 0 ? string.Empty : pair.Substring(separator + 1);
                result[Uri.UnescapeDataString(key.Replace('+', ' '))] = Uri.UnescapeDataString(value.Replace('+', ' '));
            }
            return result;
        }

        /// <summary>
        /// Checks iss, aud, azp, exp and nonce of an id_token, as in OpenID Connect Core section 3.1.3.7.
        /// The signature is not checked: the token comes straight from the token endpoint over TLS, and AGS IAM verifies it.
        /// </summary>
        public static bool ValidateIdTokenClaims(string idToken, string issuer, string clientId, string expectedNonce, long nowUnixSeconds, out string error)
        {
            error = null;
            string[] parts = (idToken ?? string.Empty).Split('.');
            if (parts.Length != 3)
            {
                error = "id_token is not a JWT.";
                return false;
            }

            JObject claims;
            try
            {
                claims = JObject.Parse(Encoding.UTF8.GetString(Base64UrlDecode(parts[1])));
            }
            catch (Exception)
            {
                error = "id_token payload is not valid base64url JSON.";
                return false;
            }

            if (NormalizeIssuer((string)claims["iss"]) != NormalizeIssuer(issuer))
            {
                error = "id_token iss does not match the configured issuer.";
                return false;
            }

            JToken audience = claims["aud"];
            List<string> audiences = audience == null
                ? new List<string>()
                : audience.Type == JTokenType.Array
                    ? audience.Values<string>().ToList()
                    : new List<string> { (string)audience };
            if (!audiences.Contains(clientId))
            {
                error = "id_token aud does not contain the configured client ID.";
                return false;
            }
            if (audiences.Count > 1 && (string)claims["azp"] != clientId)
            {
                error = "id_token azp does not match the configured client ID.";
                return false;
            }

            JToken expiry = claims["exp"];
            if (expiry == null || expiry.Type != JTokenType.Integer || (long)expiry + ExpiryLeewaySeconds < nowUnixSeconds)
            {
                error = "id_token is expired.";
                return false;
            }

            if ((string)claims["nonce"] != expectedNonce)
            {
                error = "id_token nonce does not match the login request.";
                return false;
            }

            return true;
        }
    }
}
