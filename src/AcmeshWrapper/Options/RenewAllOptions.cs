namespace AcmeshWrapper.Options
{
    /// <summary>
    /// Options for renewing all SSL/TLS certificates using acme.sh --renew-all command
    /// </summary>
    public class RenewAllOptions
    {
        /// <summary>
        /// Stop the renewal process on the first error encountered.
        /// If false (default), continues with remaining certificates even if some fail.
        /// </summary>
        public bool StopRenewOnError { get; set; } = false;

        /// <summary>
        /// Custom ACME server URL (optional). If not specified, uses the default acme.sh server
        /// </summary>
        public string? Server { get; set; }
    }
}