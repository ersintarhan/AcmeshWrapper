using System.ComponentModel.DataAnnotations;

namespace AcmeshWrapper.Options
{
    /// <summary>
    /// Represents options for removing a certificate from acme.sh management using acme.sh --remove command.
    /// Note: This command removes the certificate from acme.sh's management list but does not delete the actual certificate files.
    /// </summary>
    public class RemoveOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveOptions"/> class.
        /// </summary>
        /// <param name="domain">The domain name of the certificate to remove from acme.sh management.</param>
        public RemoveOptions(string domain)
        {
            Domain = domain;
        }

        /// <summary>
        /// Gets or sets the domain name of the certificate to remove from acme.sh management.
        /// This parameter is required.
        /// </summary>
        [Required]
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to remove an ECC (Elliptic Curve Cryptography) certificate.
        /// If false or not specified, removes the RSA certificate from management.
        /// </summary>
        public bool Ecc { get; set; }
    }
}