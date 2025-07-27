namespace AcmeshWrapper.Tests.Helpers
{
    /// <summary>
    /// Constants used throughout the test suite
    /// </summary>
    public static class TestConstants
    {
        /// <summary>
        /// Test domain names
        /// </summary>
        public static class Domains
        {
            public const string ExampleCom = "example.com";
            public const string TestCom = "test.com";
            public const string SubdomainExampleCom = "subdomain.example.com";
            public const string WildcardExampleCom = "*.example.com";
            public const string MultiDomain1 = "multi1.com";
            public const string MultiDomain2 = "multi2.com";
            public const string InvalidDomain = "invalid_domain";
        }

        /// <summary>
        /// Test certificate paths
        /// </summary>
        public static class Paths
        {
            public const string AcmeShPath = "/usr/local/bin/acme.sh";
            public const string TestAcmeShPath = "acme.sh";
            public const string CertBasePath = "/root/.acme.sh";
            public const string WebRoot = "/var/www/html";
            
            public static string GetCertPath(string domain) => $"{CertBasePath}/{domain}/{domain}.cer";
            public static string GetKeyPath(string domain) => $"{CertBasePath}/{domain}/{domain}.key";
            public static string GetCaPath(string domain) => $"{CertBasePath}/{domain}/ca.cer";
            public static string GetFullChainPath(string domain) => $"{CertBasePath}/{domain}/fullchain.cer";
            
            // Install paths
            public const string InstallCertPath = "/etc/ssl/certs/example.crt";
            public const string InstallKeyPath = "/etc/ssl/private/example.key";
            public const string InstallCaPath = "/etc/ssl/certs/ca.crt";
            public const string InstallFullChainPath = "/etc/ssl/certs/fullchain.crt";
        }

        /// <summary>
        /// Test DNS providers
        /// </summary>
        public static class DnsProviders
        {
            public const string Cloudflare = "dns_cf";
            public const string Route53 = "dns_aws";
            public const string DigitalOcean = "dns_digitalocean";
            public const string GoDaddy = "dns_godaddy";
        }

        /// <summary>
        /// Test key lengths
        /// </summary>
        public static class KeyLengths
        {
            public const string Ec256 = "ec-256";
            public const string Ec384 = "ec-384";
            public const string Rsa2048 = "2048";
            public const string Rsa4096 = "4096";
        }

        /// <summary>
        /// Test servers
        /// </summary>
        public static class Servers
        {
            public const string LetsEncryptStaging = "https://acme-staging-v02.api.letsencrypt.org/directory";
            public const string LetsEncryptProduction = "https://acme-v02.api.letsencrypt.org/directory";
            public const string ZeroSSL = "https://acme.zerossl.com/v2/DV90";
        }

        /// <summary>
        /// Test commands
        /// </summary>
        public static class Commands
        {
            public const string NginxReload = "systemctl reload nginx";
            public const string ApacheReload = "systemctl reload apache2";
            public const string CustomReload = "/usr/local/bin/reload-certificates.sh";
        }

        /// <summary>
        /// Test error messages
        /// </summary>
        public static class ErrorMessages
        {
            public const string DomainValidationFailed = "Domain validation failed";
            public const string RateLimitExceeded = "Rate limit exceeded";
            public const string InvalidDnsProvider = "Invalid DNS provider";
            public const string CertificateNotFound = "Certificate not found";
            public const string PermissionDenied = "Permission denied";
        }

        /// <summary>
        /// Test certificate info
        /// </summary>
        public static class CertificateInfo
        {
            public const string Thumbprint = "A1B2C3D4E5F6789012345678901234567890ABCD";
            public const string SerialNumber = "0123456789ABCDEF";
            public const string Issuer = "Let's Encrypt Authority X3";
            public const string Subject = "CN=example.com";
        }

        /// <summary>
        /// Test dates
        /// </summary>
        public static class Dates
        {
            public const string CreatedTime = "2024-01-15 12:00:00";
            public const string CreatedTimeUtc = "2024-01-15T12:00:00Z";
            public const string NextRenewTime = "2024-03-15 12:00:00";
            public const string NextRenewTimeUtc = "2024-03-15T12:00:00Z";
        }

        /// <summary>
        /// Test exit codes
        /// </summary>
        public static class ExitCodes
        {
            public const int Success = 0;
            public const int GeneralError = 1;
            public const int ValidationError = 2;
            public const int RateLimitError = 3;
            public const int PermissionError = 126;
        }
    }
}