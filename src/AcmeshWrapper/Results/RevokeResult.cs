using System;
using AcmeshWrapper.Enums;

namespace AcmeshWrapper.Results
{
    /// <summary>
    /// Represents the result of a certificate revocation operation.
    /// </summary>
    public class RevokeResult : AcmeResult
    {
        /// <summary>
        /// Gets or sets the domain of the revoked certificate.
        /// </summary>
        public string? Domain { get; internal set; }

        /// <summary>
        /// Gets or sets the date and time when the certificate was revoked.
        /// </summary>
        public DateTime? RevokedAt { get; internal set; }

        /// <summary>
        /// Gets or sets the reason for certificate revocation.
        /// </summary>
        public RevokeReason? Reason { get; internal set; }

        /// <summary>
        /// Gets or sets the thumbprint of the revoked certificate, if available.
        /// </summary>
        public string? CertificateThumbprint { get; internal set; }

        /// <summary>
        /// Gets or sets whether the revoked certificate was an ECC certificate.
        /// </summary>
        public bool WasEcc { get; internal set; }
    }
}