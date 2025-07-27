namespace AcmeshWrapper.Results
{
    /// <summary>
    /// Result containing certificate files and their contents
    /// </summary>
    public class GetCertificateResult : AcmeResult
    {
        /// <summary>
        /// Path to the certificate file
        /// </summary>
        public string? CertificatePath { get; internal set; }

        /// <summary>
        /// Certificate content in PEM format
        /// </summary>
        public string? Certificate { get; internal set; }

        /// <summary>
        /// Path to the private key file
        /// </summary>
        public string? KeyPath { get; internal set; }

        /// <summary>
        /// Private key content in PEM format (only populated if IncludeKey was true)
        /// </summary>
        public string? PrivateKey { get; internal set; }

        /// <summary>
        /// Path to the full chain certificate file
        /// </summary>
        public string? FullChainPath { get; internal set; }

        /// <summary>
        /// Full chain certificate content in PEM format (only populated if IncludeFullChain was true)
        /// </summary>
        public string? FullChain { get; internal set; }

        /// <summary>
        /// Path to the CA bundle file
        /// </summary>
        public string? CaPath { get; internal set; }

        /// <summary>
        /// CA bundle content in PEM format (only populated if IncludeCa was true)
        /// </summary>
        public string? CaBundle { get; internal set; }
    }
}