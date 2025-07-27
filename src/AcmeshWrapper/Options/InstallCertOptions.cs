namespace AcmeshWrapper.Options
{
    /// <summary>
    /// Options for installing SSL/TLS certificates using acme.sh --install-cert command
    /// </summary>
    public class InstallCertOptions
    {
        /// <summary>
        /// The domain name for which to install the certificate
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Install an ECC (Elliptic Curve Cryptography) certificate
        /// </summary>
        public bool Ecc { get; set; }

        /// <summary>
        /// The file path where the certificate will be copied (optional)
        /// </summary>
        public string? CertFile { get; set; }

        /// <summary>
        /// The file path where the private key will be copied (optional)
        /// </summary>
        public string? KeyFile { get; set; }

        /// <summary>
        /// The file path where the CA certificate will be copied (optional)
        /// </summary>
        public string? CaFile { get; set; }

        /// <summary>
        /// The file path where the full certificate chain will be copied (optional)
        /// </summary>
        public string? FullChainFile { get; set; }

        /// <summary>
        /// Command to execute after certificate installation, typically used to reload web servers (optional)
        /// Example: "nginx -s reload" or "systemctl reload apache2"
        /// </summary>
        public string? ReloadCmd { get; set; }

        /// <summary>
        /// Creates a new instance of InstallCertOptions
        /// </summary>
        /// <param name="domain">The domain name for which to install the certificate</param>
        public InstallCertOptions(string domain)
        {
            Domain = domain;
        }
    }
}