namespace AcmeshWrapper.Results
{
    public class InfoResult : AcmeResult
    {
        /// <summary>
        /// The domain configuration file path
        /// </summary>
        public string? DomainConfigPath { get; internal set; }

        /// <summary>
        /// The domain name
        /// </summary>
        public string? Domain { get; internal set; }

        /// <summary>
        /// Alternative names (SANs) for the certificate
        /// </summary>
        public string? AltNames { get; internal set; }

        /// <summary>
        /// The webroot path or DNS provider used
        /// </summary>
        public string? Webroot { get; internal set; }

        /// <summary>
        /// Pre-hook command
        /// </summary>
        public string? PreHook { get; internal set; }

        /// <summary>
        /// Post-hook command
        /// </summary>
        public string? PostHook { get; internal set; }

        /// <summary>
        /// Renew-hook command
        /// </summary>
        public string? RenewHook { get; internal set; }

        /// <summary>
        /// The ACME API endpoint URL
        /// </summary>
        public string? ApiEndpoint { get; internal set; }

        /// <summary>
        /// The key length (e.g., "4096", "ec-256")
        /// </summary>
        public string? KeyLength { get; internal set; }

        /// <summary>
        /// The order finalize URL
        /// </summary>
        public string? OrderFinalizeUrl { get; internal set; }

        /// <summary>
        /// The link order URL
        /// </summary>
        public string? LinkOrderUrl { get; internal set; }

        /// <summary>
        /// The link certificate URL
        /// </summary>
        public string? LinkCertUrl { get; internal set; }

        /// <summary>
        /// Certificate creation time as Unix timestamp
        /// </summary>
        public long? CertCreateTime { get; internal set; }

        /// <summary>
        /// Certificate creation time as formatted string
        /// </summary>
        public string? CertCreateTimeStr { get; internal set; }

        /// <summary>
        /// Next renewal time as formatted string
        /// </summary>
        public string? NextRenewTimeStr { get; internal set; }

        /// <summary>
        /// Next renewal time as Unix timestamp
        /// </summary>
        public long? NextRenewTime { get; internal set; }
    }
}