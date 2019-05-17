using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Models;

namespace User.Identity
{
    public static class Config
    {
        public static IEnumerable<Client> GetClients()
        {

            return new List<Client>
            {
                new Client
                {
                    ClientId = "iphone",
                    ClientSecrets = new List<Secret>{ new Secret("secret".Sha256()) },
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    AllowOfflineAccess = true,
                    RequireClientSecret = false,
                    AllowedGrantTypes = new List<string> {"sms_auth_code"},
                    AlwaysIncludeUserClaimsInIdToken = true,

                    AllowedScopes = new List<string>
                    {
                        "api_gateway",
                        "contact_api",
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,

                    }
                },
                new Client
                {
                    ClientId = "android",
                    ClientSecrets = new List<Secret>{ new Secret("secret".Sha256()) },
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    AllowOfflineAccess = true,
                    RequireClientSecret = false,
                    AllowedGrantTypes = new List<string> {"sms_auth_code"},
                    AlwaysIncludeUserClaimsInIdToken = true,

                    AllowedScopes = new List<string>
                    {
                        "api_gateway",
                        "contact_api",
                        "user_api",
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,

                    }
                }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api_gateway", "gateway"),
                new ApiResource("contact_api", "contact service"),
                new ApiResource("user_api", "user service")

            };
        }
    }
}
