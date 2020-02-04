using Newtonsoft.Json;
using RFI.MicroserviceFramework._Cache;
using RFI.MicroserviceFramework._Helpers;

// ReSharper disable All

namespace RFI.MicroserviceFramework._Api
{
    public class ApiUser
    {
        public string PublicKey { get; set; }

        [JsonIgnore, DBIgnoreParam]
        public string Username { get; set; }

        public int User_ID { get; set; }

        public int Partner_ID { get; set; }


        public ApiUser()
        {
        }

        public ApiUser(string username)
        {
            Username = username;

            if(MemCache.TryGetValue(Username, out var value))
            {
                this.PopulateFromJson(value);
                return;
            }

            var usr = ApiDbController.ExecStoredProcedure<ApiUser>("get_user_public_key", new { username });

            User_ID = usr.User_ID;
            PublicKey = usr.PublicKey;

            MemCache.Save(Username, this.JsonSerialize());

            if(PublicKey.IsNullOrEmpty()) throw new ApiException(CodeStatus.PermissionDenied, "PublicKey not found");
        }
    }
}