using System;
using System.Text.Json;
using RFI.MicroserviceFramework._Cryptography;
using RFI.MicroserviceFramework._Environment;
using RFI.MicroserviceFramework._Helpers;
using RFI.MicroserviceFramework._Loggers;

namespace RFI.MicroserviceFramework._Api
{
    public static class ApiSsoToken
    {
        public static string GetUsernameFromToken(string token)
        {
            try
            {
                var parts = token.Split('.'); // 0 - header, 1 - body, 3 - signature

                var json = parts[1].Base64UrlDecode();

                using var jsonDocument = JsonDocument.Parse(json);

                var username = jsonDocument.RootElement.GetProperty("username").GetString();
                if(username.IsNullOrEmpty(true)) throw new ApiException("Token username is not set");

                var environment = json.Regex("(?<=environment\":\").*?(?=\")");
                if(SEnv.EnvironmentName != environment) throw new ApiException("Token environment is not correct");

                var exp = jsonDocument.RootElement.GetProperty("exp").GetInt32();
                if(SEnv.IsDebug.Not() && DateTime.UnixEpoch.AddSeconds(exp) < DateTime.UtcNow) throw new ApiException(CodeStatus.TokenExpired);

                if(Rsa.Verify(parts[0] + '.' + parts[1], parts[2].Base64UrlNormalize(), SEnv.SsoKeyPublic).Not()) throw new ApiException(CodeStatus.PermissionDenied, "Token signature is invalid");

                return username;
            }
            catch(ApiException)
            {
                throw;
            }
            catch(Exception ex)
            {
                ex.Log();
                throw new ApiException("Token is invalid");
            }
        }
    }
}