using Microsoft.AspNetCore.DataProtection;

namespace AuxApiGateway
{
    public class ApiGatewaySetting
    {
        public string Authority { get; set; }
        public string Secret { get; set; } 
        public string Audience { get; set; } 
        public bool ValidateIssuer { get; set; } 
        public bool ValidateLifetime { get; set; } 
        public bool ValidateIssuerSigningKey { get; set; } 
        public bool ValidateAudience { get; set; }
    }
}