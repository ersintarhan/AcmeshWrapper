using Cysharp.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AcmeshWrapper.Tests.Helpers
{
    /// <summary>
    /// Helper class for creating ProcessX mock objects and scenarios
    /// </summary>
    public static class ProcessXMockHelper
    {
        /// <summary>
        /// Creates a successful ProcessAsyncEnumerable with the given output lines
        /// </summary>
        public static async IAsyncEnumerable<string> CreateSuccessfulProcess(params string[] outputLines)
        {
            foreach (var line in outputLines)
            {
                yield return line;
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates a ProcessAsyncEnumerable that simulates a process error
        /// </summary>
        public static async IAsyncEnumerable<string> CreateFailedProcess(int exitCode, params string[] errorOutput)
        {
            await Task.Yield(); // Ensure the method is async
            throw new ProcessErrorException(exitCode, errorOutput);
            
            // This code is unreachable but required for the compiler
            #pragma warning disable CS0162 // Unreachable code detected
            yield break;
            #pragma warning restore CS0162 // Unreachable code detected
        }

        /// <summary>
        /// Creates a mock for acme.sh --list command success
        /// </summary>
        public static string[] CreateListSuccessOutput()
        {
            return new[]
            {
                "Main_Domain      KeyLength      SAN_Domains      CA               Created                   Renew",
                "example.com      ec-256         example.com      Lets Encrypt    2024-01-15 12:00:00      2024-03-15 12:00:00",
                "test.com         ec-256         test.com         Lets Encrypt    2024-01-20 15:30:00      2024-03-20 15:30:00"
            };
        }

        /// <summary>
        /// Creates a mock for acme.sh --issue command success
        /// </summary>
        public static string[] CreateIssueSuccessOutput(string domain)
        {
            return new[]
            {
                $"[Info] Processing {domain}",
                "[Info] Getting domain auth token for each domain",
                "[Info] Getting webroot for domain",
                "[Info] Verifying domain",
                "[Info] Verified!",
                "[Info] Creating domain key",
                "[Info] The domain key is here: /root/.acme.sh/" + domain + "/" + domain + ".key",
                "[Info] Single domain certificate",
                "[Info] Getting domain auth token for each domain",
                "[Info] Creating CSR",
                "[Info] The CSR is here: /root/.acme.sh/" + domain + "/" + domain + ".csr",
                "[Info] Your cert is in: /root/.acme.sh/" + domain + "/" + domain + ".cer",
                "[Info] Your cert key is in: /root/.acme.sh/" + domain + "/" + domain + ".key",
                "[Info] The intermediate CA cert is in: /root/.acme.sh/" + domain + "/ca.cer",
                "[Info] And the full chain certs is in: /root/.acme.sh/" + domain + "/fullchain.cer"
            };
        }

        /// <summary>
        /// Creates a mock for acme.sh --renew command success
        /// </summary>
        public static string[] CreateRenewSuccessOutput(string domain)
        {
            return new[]
            {
                $"[Info] Renew: '{domain}'",
                "[Info] Single domain certificate",
                "[Info] Getting domain auth token for each domain",
                "[Info] Getting webroot for domain",
                "[Info] Verifying domain",
                "[Info] Verified!",
                "[Info] Creating CSR",
                "[Info] The CSR is here: /root/.acme.sh/" + domain + "/" + domain + ".csr",
                "[Info] Cert success.",
                "[Info] Your cert is in: /root/.acme.sh/" + domain + "/" + domain + ".cer",
                "[Info] Your cert key is in: /root/.acme.sh/" + domain + "/" + domain + ".key",
                "[Info] The intermediate CA cert is in: /root/.acme.sh/" + domain + "/ca.cer",
                "[Info] And the full chain certs is in: /root/.acme.sh/" + domain + "/fullchain.cer"
            };
        }

        /// <summary>
        /// Creates a mock for acme.sh --renew command skip (not due)
        /// </summary>
        public static string[] CreateRenewSkipOutput(string domain)
        {
            return new[]
            {
                $"[Info] Renew: '{domain}'",
                "[Info] Skip, Next renewal time is: 2024-03-15 12:00:00",
                "[Info] Certificate is not due yet, skip renewing."
            };
        }

        /// <summary>
        /// Creates a mock for acme.sh --install-cert command success
        /// </summary>
        public static string[] CreateInstallCertSuccessOutput(InstallCertPaths paths)
        {
            var output = new List<string>();

            if (!string.IsNullOrEmpty(paths.CertFile))
                output.Add($"[Info] Installing cert to: {paths.CertFile}");
            
            if (!string.IsNullOrEmpty(paths.KeyFile))
                output.Add($"[Info] Installing key to: {paths.KeyFile}");
            
            if (!string.IsNullOrEmpty(paths.CaFile))
                output.Add($"[Info] Installing CA to: {paths.CaFile}");
            
            if (!string.IsNullOrEmpty(paths.FullChainFile))
                output.Add($"[Info] Installing full chain to: {paths.FullChainFile}");

            if (!string.IsNullOrEmpty(paths.ReloadCmd))
            {
                output.Add($"[Info] Run reload cmd: {paths.ReloadCmd}");
                output.Add("[Info] Reload success");
            }

            return output.ToArray();
        }

        /// <summary>
        /// Creates a mock for acme.sh --revoke command success
        /// </summary>
        public static string[] CreateRevokeSuccessOutput(string domain)
        {
            return new[]
            {
                $"[Info] Revoking certificate for domain: {domain}",
                "[Info] Certificate thumbprint: A1B2C3D4E5F6789012345678901234567890ABCD",
                "[Info] Revoke success!"
            };
        }

        /// <summary>
        /// Creates a mock for acme.sh --remove command success
        /// </summary>
        public static string[] CreateRemoveSuccessOutput(string domain)
        {
            return new[]
            {
                $"[Info] Removing certificate for domain: {domain}",
                $"[Info] Removed: '{domain}'"
            };
        }

        /// <summary>
        /// Creates a mock for acme.sh --renew-all command success
        /// </summary>
        public static string[] CreateRenewAllSuccessOutput(params RenewAllDomainResult[] domainResults)
        {
            var output = new List<string>();

            foreach (var result in domainResults)
            {
                output.Add($"[Info] Renew: '{result.Domain}'");
                
                switch (result.Status)
                {
                    case RenewStatus.Success:
                        output.AddRange(new[]
                        {
                            "[Info] Single domain certificate",
                            "[Info] Getting domain auth token for each domain",
                            "[Info] Verifying domain",
                            "[Info] Verified!",
                            "[Info] Cert success."
                        });
                        break;
                    
                    case RenewStatus.Skipped:
                        output.Add("[Info] Skip, Next renewal time is: 2024-03-15 12:00:00");
                        break;
                    
                    case RenewStatus.Failed:
                        output.Add($"[Error] Renew error for {result.Domain}.");
                        break;
                }
            }

            return output.ToArray();
        }

        /// <summary>
        /// Creates a mock for domain validation error
        /// </summary>
        public static string[] CreateDomainValidationError(string domain)
        {
            return new[]
            {
                $"[Error] Domain validation failed for {domain}",
                "[Error] Please check your DNS settings or webroot path"
            };
        }

        /// <summary>
        /// Creates a mock for rate limit error
        /// </summary>
        public static string[] CreateRateLimitError()
        {
            return new[]
            {
                "[Error] Rate limit exceeded",
                "[Error] Too many certificates already issued for exact set of domains",
                "[Error] Please try again later"
            };
        }
    }

    /// <summary>
    /// Helper class for install cert paths
    /// </summary>
    public class InstallCertPaths
    {
        public string? CertFile { get; set; }
        public string? KeyFile { get; set; }
        public string? CaFile { get; set; }
        public string? FullChainFile { get; set; }
        public string? ReloadCmd { get; set; }
    }

    /// <summary>
    /// Represents a domain result for renew-all command
    /// </summary>
    public class RenewAllDomainResult
    {
        public string Domain { get; set; } = string.Empty;
        public RenewStatus Status { get; set; }
    }

    /// <summary>
    /// Status of renewal for a domain
    /// </summary>
    public enum RenewStatus
    {
        Success,
        Skipped,
        Failed
    }
}