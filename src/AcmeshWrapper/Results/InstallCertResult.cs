using System;

namespace AcmeshWrapper.Results
{
    /// <summary>
    /// Represents the result of a certificate installation operation.
    /// </summary>
    public class InstallCertResult : AcmeResult
    {
        /// <summary>
        /// Gets or sets the path to the installed certificate file.
        /// </summary>
        public string? InstalledCertFile { get; internal set; }

        /// <summary>
        /// Gets or sets the path to the installed private key file.
        /// </summary>
        public string? InstalledKeyFile { get; internal set; }

        /// <summary>
        /// Gets or sets the path to the installed CA certificate file.
        /// </summary>
        public string? InstalledCaFile { get; internal set; }

        /// <summary>
        /// Gets or sets the path to the installed full chain certificate file.
        /// </summary>
        public string? InstalledFullChainFile { get; internal set; }

        /// <summary>
        /// Gets or sets whether a reload command was executed after installation.
        /// </summary>
        public bool ReloadCommandExecuted { get; internal set; }

        /// <summary>
        /// Gets or sets the output from the reload command execution.
        /// </summary>
        public string? ReloadCommandOutput { get; internal set; }

        /// <summary>
        /// Gets or sets the date and time when the certificate was installed.
        /// </summary>
        public DateTime? InstalledAt { get; internal set; }
    }
}