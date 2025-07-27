using System;

namespace AcmeshWrapper.Results
{
    /// <summary>
    /// Represents the result of a certificate removal operation.
    /// </summary>
    public class RemoveResult : AcmeResult
    {
        /// <summary>
        /// Gets or sets the domain of the removed certificate.
        /// </summary>
        public string? Domain { get; internal set; }

        /// <summary>
        /// Gets or sets the date and time when the certificate was removed.
        /// </summary>
        public DateTime? RemovedAt { get; internal set; }

        /// <summary>
        /// Gets or sets whether the removed certificate was an ECC certificate.
        /// </summary>
        public bool WasEcc { get; internal set; }

        /// <summary>
        /// Gets or sets the path where the certificate files were stored before removal.
        /// </summary>
        public string? CertificatePath { get; internal set; }
    }
}