using System.ComponentModel.DataAnnotations;
using AcmeshWrapper.Enums;

namespace AcmeshWrapper.Options
{
    /// <summary>
    /// Represents options for revoking a certificate using acme.sh.
    /// </summary>
    public class RevokeOptions
    {
        /// <summary>
        /// Gets or sets the domain name for which to revoke the certificate.
        /// This parameter is required.
        /// </summary>
        [Required]
        public string Domain { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether to revoke an ECC (Elliptic Curve Cryptography) certificate.
        /// If false or not specified, revokes the RSA certificate.
        /// </summary>
        public bool Ecc { get; set; }

        /// <summary>
        /// Gets or sets the reason for certificate revocation.
        /// If not specified, no reason will be provided to acme.sh.
        /// Valid values are defined in <see cref="RevokeReason"/> enum (0-10, excluding 7).
        /// </summary>
        public RevokeReason? Reason { get; set; }
    }
}