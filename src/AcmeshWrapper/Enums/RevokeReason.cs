namespace AcmeshWrapper.Enums
{
    /// <summary>
    /// Specifies the reason for certificate revocation according to RFC 5280.
    /// </summary>
    public enum RevokeReason
    {
        /// <summary>
        /// No reason specified (0).
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// The certificate subject's private key or the CA's private key is suspected to have been compromised (1).
        /// </summary>
        KeyCompromise = 1,

        /// <summary>
        /// The CA's private key is suspected to have been compromised (2).
        /// </summary>
        CACompromise = 2,

        /// <summary>
        /// The subject's affiliation has changed (3).
        /// </summary>
        AffiliationChanged = 3,

        /// <summary>
        /// The certificate has been superseded by a new certificate (4).
        /// </summary>
        Superseded = 4,

        /// <summary>
        /// The certificate is no longer needed for its original purpose (5).
        /// </summary>
        CessationOfOperation = 5,

        /// <summary>
        /// The certificate should be temporarily suspended (6).
        /// </summary>
        CertificateHold = 6,

        // Note: Value 7 is not used according to RFC 5280

        /// <summary>
        /// Request to remove the certificate from CRL (8).
        /// </summary>
        RemoveFromCRL = 8,

        /// <summary>
        /// The privileges granted to the subject have been withdrawn (9).
        /// </summary>
        PrivilegeWithdrawn = 9,

        /// <summary>
        /// The AA (Attribute Authority) certificate has been compromised (10).
        /// </summary>
        AACompromise = 10
    }
}