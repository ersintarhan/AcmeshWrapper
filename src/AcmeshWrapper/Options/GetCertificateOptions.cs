namespace AcmeshWrapper.Options
{
    /// <summary>
    /// Options for retrieving certificate files and their contents
    /// </summary>
    public class GetCertificateOptions
    {
        /// <summary>
        /// The domain name for which to retrieve the certificate (required)
        /// </summary>
        public string Domain { get; set; } = string.Empty;

        /// <summary>
        /// Whether to get the ECC certificate instead of RSA (optional)
        /// </summary>
        public bool Ecc { get; set; }

        /// <summary>
        /// Whether to include the private key content. Default: false
        /// </summary>
        public bool IncludeKey { get; set; } = false;

        /// <summary>
        /// Whether to include the full chain certificate content. Default: false
        /// </summary>
        public bool IncludeFullChain { get; set; } = false;

        /// <summary>
        /// Whether to include the CA bundle content. Default: false
        /// </summary>
        public bool IncludeCa { get; set; } = false;
    }
}