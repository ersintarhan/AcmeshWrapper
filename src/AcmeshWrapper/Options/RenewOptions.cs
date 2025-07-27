namespace AcmeshWrapper.Options
{
    /// <summary>
    /// Options for renewing SSL/TLS certificates using acme.sh
    /// </summary>
    public class RenewOptions
    {
        /// <summary>
        /// The domain name for which to renew the certificate
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Force renewal even if the certificate is not due for renewal
        /// </summary>
        public bool Force { get; set; }

        /// <summary>
        /// Renew an ECC (Elliptic Curve Cryptography) certificate
        /// </summary>
        public bool Ecc { get; set; }

        /// <summary>
        /// Custom ACME server URL (optional). If not specified, uses the default acme.sh server
        /// </summary>
        public string? Server { get; set; }

        /// <summary>
        /// Creates a new instance of RenewOptions
        /// </summary>
        /// <param name="domain">The domain name for which to renew the certificate</param>
        public RenewOptions(string domain)
        {
            Domain = domain;
        }
    }
}
