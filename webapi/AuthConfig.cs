public static class AuthConfig
{

    public static class EntraExternal
    {
        public const string CallbackPath = "/api/signin-oidc";
        public const string SignedOutCallbackPath = "/api/signout-callback-oidc";
          public const string Authority = "https://mathtablaexternal.ciamlogin.com/";
        public const string TenantId ="2dddf335-6391-4a0d-80ad-d2531b405291";
        public static class Production
        {
            public const string SourceType = "KeyVault";
            public const string KeyVaultUrl = "https://mathtablavault.vault.azure.net";
            public const string CertificateName = "AspireNuxt-Prod-Cert";
            public const string ManagedIdentityClientId = "a8aa5450-479c-4437-87f9-891d2755d1b2";
            public const string ClientId = "d5a4dfd5-5bd5-42b4-bb48-cad1217c34a1";
          

            public static bool SendX5C = true;
            
        }

        public static class Development
        {
            public const string ClientId = "178eb67d-a719-48ad-98c9-0f0b0417072a";
        }
    }
    public static class AzureServices
    {
        public static class SpeechService
        {
            public const string TenantId ="2dddf335-6391-4a0d-80ad-d2531b405291";
            public const string Region = "centralus";
            public const string ManagedIdentityClientId = "a8aa5450-479c-4437-87f9-891d2755d1b2";

            const string subdomain = "vue-mathtabla-voice";

            public const string FetchUri=$"https://{subdomain}.cognitiveservices.azure.com/sts/v1.0/issueToken";
            public const string Scope="https://cognitiveservices.azure.com/.default";

        }
    }

}