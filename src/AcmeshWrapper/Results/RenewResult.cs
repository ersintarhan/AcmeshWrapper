using System;

namespace AcmeshWrapper.Results
{
    /// <summary>
    /// Represents the result of a certificate renewal operation.
    /// </summary>
    public class RenewResult : AcmeResult
    {
        /// <summary>
        /// Gets or sets the path to the renewed certificate file.
        /// </summary>
        public string? CertificatePath { get; internal set; }

        /// <summary>
        /// Gets or sets the path to the private key file.
        /// </summary>
        public string? KeyPath { get; internal set; }

        /// <summary>
        /// Gets or sets the path to the CA certificate file.
        /// </summary>
        public string? CaPath { get; internal set; }

        /// <summary>
        /// Gets or sets the path to the full chain certificate file.
        /// </summary>
        public string? FullChainPath { get; internal set; }

        /// <summary>
        /// Gets or sets the date and time when the certificate was renewed.
        /// </summary>
        public DateTime? RenewedAt { get; internal set; }
    }
}