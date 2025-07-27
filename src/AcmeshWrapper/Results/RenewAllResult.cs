using System;
using System.Collections.Generic;

namespace AcmeshWrapper.Results
{
    /// <summary>
    /// Represents the result of a renew all certificates operation.
    /// </summary>
    public class RenewAllResult : AcmeResult
    {
        private List<string>? _renewedDomains;
        private List<string>? _failedDomains;
        private List<string>? _skippedDomains;

        /// <summary>
        /// Gets or sets the total number of certificates processed.
        /// </summary>
        public int TotalCertificates { get; internal set; }

        /// <summary>
        /// Gets or sets the number of certificates successfully renewed.
        /// </summary>
        public int SuccessfulRenewals { get; internal set; }

        /// <summary>
        /// Gets or sets the number of certificates that failed to renew.
        /// </summary>
        public int FailedRenewals { get; internal set; }

        /// <summary>
        /// Gets or sets the number of certificates skipped (not yet due for renewal).
        /// </summary>
        public int SkippedRenewals { get; internal set; }

        /// <summary>
        /// Gets or sets the list of domains that were successfully renewed.
        /// </summary>
        public List<string> RenewedDomains
        {
            get => _renewedDomains ?? (_renewedDomains = new List<string>());
            internal set => _renewedDomains = value;
        }

        /// <summary>
        /// Gets or sets the list of domains that failed to renew.
        /// </summary>
        public List<string> FailedDomains
        {
            get => _failedDomains ?? (_failedDomains = new List<string>());
            internal set => _failedDomains = value;
        }

        /// <summary>
        /// Gets or sets the list of domains that were skipped.
        /// </summary>
        public List<string> SkippedDomains
        {
            get => _skippedDomains ?? (_skippedDomains = new List<string>());
            internal set => _skippedDomains = value;
        }

        /// <summary>
        /// Gets or sets the date and time when the operation completed.
        /// </summary>
        public DateTime? CompletedAt { get; internal set; }
    }
}