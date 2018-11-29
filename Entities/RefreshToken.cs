using System;

namespace Entities
{
    public class RefreshToken
    {
        public int TokenID { get; set; }
        public string ClientID { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }
        public bool Expired { get; set; }
        public int ExpiresInMinutes { get; set; }
    }
}
