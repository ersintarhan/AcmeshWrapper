using AcmeshWrapper.Enums;
using AcmeshWrapper.Options;
using AcmeshWrapper.Results;
using Cysharp.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AcmeshWrapper
{
    public class AcmeClient
    {
        private readonly string _acmeShPath;

        public AcmeClient(string acmeShPath = "acme.sh")
        {
            _acmeShPath = acmeShPath;
        }

        private async Task<string[]> ExecuteProcessAsync(List<string> arguments, CancellationToken cancellationToken = default)
        {
            var processStartInfo = new ProcessStartInfo(_acmeShPath);
            foreach (var arg in arguments)
            {
                processStartInfo.ArgumentList.Add(arg);
            }

            return await ProcessX.StartAsync(processStartInfo).ToTask(cancellationToken);
        }

        public async Task<ListResult> ListAsync(ListOptions options, CancellationToken cancellationToken = default)
        {
            var args = BuildListArgs(options);
            try
            {
                var result = await ExecuteProcessAsync(args, cancellationToken);
                return ParseListResult(string.Join("\n", result));
            }
            catch (ProcessErrorException ex)
            {
                return new ListResult
                {
                    IsSuccess = false,
                    ErrorOutput = ex.ErrorOutput.ToArray()
                };
            }
        }

        private List<string> BuildListArgs(ListOptions options)
        {
            var args = new List<string> { "--list" };
            if (options.Raw)
            {
                args.Add("--raw");
            }
            return args;
        }

        private ListResult ParseListResult(string output)
        {
            var result = new ListResult { IsSuccess = true, RawOutput = output };
            var lines = output.Split('\n').ToList();
            
            if (lines.Count < 2) 
            {
                return result;
            }

            var headerLine = lines[0];
            
            // Split by 2 or more spaces instead of tabs
            var headers = Regex.Split(headerLine, @"\s{2,}").Select(h => h.Trim()).ToList();

            for (int i = 1; i < lines.Count; i++)
            {
                var line = lines[i];
                
                if (string.IsNullOrWhiteSpace(line)) 
                {
                    continue;
                }

                // Split by 2 or more spaces instead of tabs
                var values = Regex.Split(line, @"\s{2,}").Select(v => v.Trim().Trim('"')).ToList();

                var certInfo = new CertificateInfo();

                for (int j = 0; j < headers.Count && j < values.Count; j++)
                {
                    var header = headers[j];
                    var value = values[j];

                    switch (header)
                    {
                        case "Main_Domain":
                            certInfo.Le_Main = value; // Use Le_Main for Main Domain
                            certInfo.Domain = value;  // Also set Domain for backward compatibility
                            break;
                        case "KeyLength":
                            certInfo.Le_Keylength = value;
                            break;
                        case "SAN_Domains":
                            certInfo.Le_SAN = value;
                            break;
                        case "CA":
                            certInfo.CA = value; // Store CA information
                            break;
                        case "Created":
                            certInfo.Le_Created_Time = value;
                            break;
                        case "Renew":
                            certInfo.Le_Next_Renew_Time = value;
                            break;
                    }
                }
                
                result.Certificates.Add(certInfo);
            }

            return result;
        }

        public async Task<IssueResult> IssueAsync(IssueOptions options, CancellationToken cancellationToken = default)
        {
            var args = BuildIssueArgs(options);
            try
            {
                var result = await ExecuteProcessAsync(args, cancellationToken);
                return ParseIssueResult(string.Join("\n", result));
            }
            catch (ProcessErrorException ex)
            {
                return new IssueResult
                {
                    IsSuccess = false,
                    ErrorOutput = ex.ErrorOutput.ToArray()
                };
            }
        }

        private List<string> BuildIssueArgs(IssueOptions options)
        {
            var args = new List<string> { "--issue" };

            foreach (var domain in options.Domains)
            {
                args.Add("-d");
                args.Add(domain);
            }

            if (!string.IsNullOrEmpty(options.WebRoot))
            {
                args.Add("-w");
                args.Add(options.WebRoot);
            }

            if (!string.IsNullOrEmpty(options.DnsProvider))
            {
                args.Add("--dns");
                args.Add(options.DnsProvider);
            }

            args.Add("--keylength");
            args.Add(options.KeyLength);

            if (options.Staging)
            {
                args.Add("--staging");
            }

            if (!string.IsNullOrEmpty(options.Server))
            {
                args.Add("--server");
                args.Add(options.Server);
            }

            return args;
        }

        private IssueResult ParseIssueResult(string output)
        {
            var result = new IssueResult { IsSuccess = false, RawOutput = output };

            var certFileMatch = Regex.Match(output, @"Your cert is in:\s*(.+)");
            if (certFileMatch.Success)
            {
                result.CertificateFile = certFileMatch.Groups[1].Value.Trim();
            }

            var keyFileMatch = Regex.Match(output, @"Your cert key is in:\s*(.+)");
            if (keyFileMatch.Success)
            {
                result.KeyFile = keyFileMatch.Groups[1].Value.Trim();
            }

            var caFileMatch = Regex.Match(output, @"The intermediate CA cert is in:\s*(.+)");
            if (caFileMatch.Success)
            {
                result.CaFile = caFileMatch.Groups[1].Value.Trim();
            }

            var fullChainFileMatch = Regex.Match(output, @"And the full chain certs is in:\s*(.+)");
            if (fullChainFileMatch.Success)
            {
                result.FullChainFile = fullChainFileMatch.Groups[1].Value.Trim();
            }

            // Success is only true if all file paths are found
            if (!string.IsNullOrEmpty(result.CertificateFile) &&
                !string.IsNullOrEmpty(result.KeyFile) &&
                !string.IsNullOrEmpty(result.CaFile) &&
                !string.IsNullOrEmpty(result.FullChainFile))
            {
                result.IsSuccess = true;
            }

            return result;
        }

        
        /// <summary>
        /// Renews an existing SSL/TLS certificate using acme.sh
        /// </summary>
        /// <param name="options">The renewal options containing domain and other settings</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation with the renewal result</returns>
        public async Task<RenewResult> RenewAsync(RenewOptions options, CancellationToken cancellationToken = default)
        {
            var args = BuildRenewArgs(options);
            try
            {
                var result = await ExecuteProcessAsync(args, cancellationToken);
                return ParseRenewResult(string.Join("\n", result));
            }
            catch (ProcessErrorException ex)
            {
                return new RenewResult
                {
                    IsSuccess = false,
                    ErrorOutput = ex.ErrorOutput.ToArray()
                };
            }
        }

        private List<string> BuildRenewArgs(RenewOptions options)
        {
            var args = new List<string> { "--renew" };

            // Add domain
            args.Add("-d");
            args.Add(options.Domain);

            // Add force flag if specified
            if (options.Force)
            {
                args.Add("--force");
            }

            // Add ecc flag if specified
            if (options.Ecc)
            {
                args.Add("--ecc");
            }

            // Add custom server if specified
            if (!string.IsNullOrEmpty(options.Server))
            {
                args.Add("--server");
                args.Add(options.Server);
            }

            return args;
        }

        private RenewResult ParseRenewResult(string output)
        {
            var result = new RenewResult
            {
                IsSuccess = false,
                RawOutput = output,
                RenewedAt = null
            };

            // A renewal that is skipped is also a success
            if (output.Contains("Skip, Next renewal time is"))
            {
                result.IsSuccess = true;
                return result;
            }

            // Parse certificate file path
            var certFileMatch = Regex.Match(output, @"Your cert is in:\s*(.+?\.cer)");
            if (certFileMatch.Success)
            {
                result.CertificatePath = certFileMatch.Groups[1].Value.Trim();
            }

            // Parse key file path
            var keyFileMatch = Regex.Match(output, @"Your cert key is in:\s*(.+?\.key)");
            if (keyFileMatch.Success)
            {
                result.KeyPath = keyFileMatch.Groups[1].Value.Trim();
            }

            // Parse CA file path
            var caFileMatch = Regex.Match(output, @"The intermediate CA cert is in:\s*(.+?\.cer)");
            if (caFileMatch.Success)
            {
                result.CaPath = caFileMatch.Groups[1].Value.Trim();
            }

            // Parse full chain file path
            var fullChainFileMatch = Regex.Match(output, @"And the full chain certs is in:\s*(.+?\.cer)");
            if (fullChainFileMatch.Success)
            {
                result.FullChainPath = fullChainFileMatch.Groups[1].Value.Trim();
            }

            // Check if renewal was successful by looking for success indicators
            if (output.Contains("Cert success") && !string.IsNullOrEmpty(result.CertificatePath))
            {
                result.IsSuccess = true;
                result.RenewedAt = DateTime.UtcNow;
            }
            else if (output.Contains("error") || output.Contains("failed"))
            {
                result.IsSuccess = false;
            }

            return result;
        }

        /// <summary>
        /// Installs SSL/TLS certificates to specified file paths using acme.sh
        /// </summary>
        /// <param name="options">The installation options containing domain and file paths</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation with the installation result</returns>
        public async Task<InstallCertResult> InstallCertAsync(InstallCertOptions options, CancellationToken cancellationToken = default)
        {
            var args = BuildInstallCertArgs(options);
            try
            {
                var result = await ExecuteProcessAsync(args, cancellationToken);
                return ParseInstallCertResult(string.Join("\n", result));
            }
            catch (ProcessErrorException ex)
            {
                return new InstallCertResult
                {
                    IsSuccess = false,
                    ErrorOutput = ex.ErrorOutput.ToArray()
                };
            }
        }

        private List<string> BuildInstallCertArgs(InstallCertOptions options)
        {
            var args = new List<string> { "--install-cert" };

            // Add domain
            args.Add("-d");
            args.Add(options.Domain);

            // Add ecc flag if specified
            if (options.Ecc)
            {
                args.Add("--ecc");
            }

            // Add certificate file path if specified
            if (!string.IsNullOrEmpty(options.CertFile))
            {
                args.Add("--cert-file");
                args.Add(options.CertFile);
            }

            // Add key file path if specified
            if (!string.IsNullOrEmpty(options.KeyFile))
            {
                args.Add("--key-file");
                args.Add(options.KeyFile);
            }

            // Add CA file path if specified
            if (!string.IsNullOrEmpty(options.CaFile))
            {
                args.Add("--ca-file");
                args.Add(options.CaFile);
            }

            // Add full chain file path if specified
            if (!string.IsNullOrEmpty(options.FullChainFile))
            {
                args.Add("--fullchain-file");
                args.Add(options.FullChainFile);
            }

            // Add reload command if specified
            if (!string.IsNullOrEmpty(options.ReloadCmd))
            {
                args.Add("--reloadcmd");
                args.Add(options.ReloadCmd);
            }

            return args;
        }

        private InstallCertResult ParseInstallCertResult(string output)
        {
            var result = new InstallCertResult
            {
                IsSuccess = false,
                RawOutput = output,
                InstalledAt = null
            };

            // Parse installed certificate file path
            var certFileMatch = Regex.Match(output, @"Installing cert to:\s*(.+)");
            if (certFileMatch.Success)
            {
                result.InstalledCertFile = certFileMatch.Groups[1].Value.Trim();
            }

            // Parse installed key file path
            var keyFileMatch = Regex.Match(output, @"Installing key to:\s*(.+)");
            if (keyFileMatch.Success)
            {
                result.InstalledKeyFile = keyFileMatch.Groups[1].Value.Trim();
            }

            // Parse installed CA file path
            var caFileMatch = Regex.Match(output, @"Installing CA to:\s*(.+)");
            if (caFileMatch.Success)
            {
                result.InstalledCaFile = caFileMatch.Groups[1].Value.Trim();
            }

            // Parse installed full chain file path
            var fullChainFileMatch = Regex.Match(output, @"Installing full chain to:\s*(.+)");
            if (fullChainFileMatch.Success)
            {
                result.InstalledFullChainFile = fullChainFileMatch.Groups[1].Value.Trim();
            }

            // Check if reload command was executed
            if (output.Contains("[Info] Run reload cmd:"))
            {
                result.ReloadCommandExecuted = true;

                // Extract reload command output
                var reloadCmdMatch = Regex.Match(output, @"\[Info\] Run reload cmd:\s*(.+)");
                if (reloadCmdMatch.Success)
                {
                    var reloadStartIndex = output.IndexOf("[Info] Run reload cmd:");
                    var reloadEndIndex = output.IndexOf("[Info] Reload success", reloadStartIndex);
                    
                    if (reloadEndIndex > reloadStartIndex)
                    {
                        result.ReloadCommandOutput = output.Substring(reloadStartIndex, reloadEndIndex - reloadStartIndex + "[Info] Reload success".Length).Trim();
                    }
                    else
                    {
                        // If "Reload success" is not found, capture everything after "Run reload cmd:"
                        result.ReloadCommandOutput = output.Substring(reloadStartIndex).Trim();
                    }
                }
            }

            // Check if installation was successful
            if (output.Contains("Certificate installation completed") ||
                output.Contains("[Info] Reload success") ||
                !string.IsNullOrEmpty(result.InstalledCertFile))
            {
                result.IsSuccess = true;
                result.InstalledAt = DateTime.UtcNow;
            }
            else if (output.Contains("error") || output.Contains("failed"))
            {
                result.IsSuccess = false;
            }

            return result;
        }

        /// <summary>
        /// Revokes an existing SSL/TLS certificate using acme.sh
        /// </summary>
        /// <param name="options">The revocation options containing domain and other settings</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation with the revocation result</returns>
        public async Task<RevokeResult> RevokeAsync(RevokeOptions options, CancellationToken cancellationToken = default)
        {
            var args = BuildRevokeArgs(options);
            try
            {
                var result = await ExecuteProcessAsync(args, cancellationToken);
                return ParseRevokeResult(string.Join("\n", result), options);
            }
            catch (ProcessErrorException ex)
            {
                return new RevokeResult
                {
                    IsSuccess = false,
                    ErrorOutput = ex.ErrorOutput.ToArray(),
                    Domain = options.Domain,
                    Reason = options.Reason,
                    WasEcc = options.Ecc
                };
            }
        }

        private List<string> BuildRevokeArgs(RevokeOptions options)
        {
            var args = new List<string> { "--revoke" };

            // Add domain
            args.Add("-d");
            args.Add(options.Domain);

            // Add ecc flag if specified
            if (options.Ecc)
            {
                args.Add("--ecc");
            }

            // Add revoke reason if specified
            if (options.Reason.HasValue)
            {
                args.Add("--revoke-reason");
                args.Add(((int)options.Reason.Value).ToString());
            }

            return args;
        }

        private RevokeResult ParseRevokeResult(string output, RevokeOptions options)
        {
            var result = new RevokeResult
            {
                IsSuccess = false,
                RawOutput = output,
                Domain = options.Domain,
                Reason = options.Reason,
                WasEcc = options.Ecc,
                RevokedAt = null
            };

            // Check if revocation was successful by looking for success indicators
            if (output.Contains("Revoke success") || output.Contains("Cert revoked"))
            {
                result.IsSuccess = true;
                result.RevokedAt = DateTime.UtcNow;
            }
            else if (output.Contains("error") || output.Contains("failed"))
            {
                result.IsSuccess = false;
                result.RevokedAt = null; // Clear revoked time if failed
            }

            // Try to extract certificate thumbprint if present in output
            var thumbprintMatch = Regex.Match(output, @"Certificate thumbprint:\s*([A-F0-9]+)", RegexOptions.IgnoreCase);
            if (thumbprintMatch.Success)
            {
                result.CertificateThumbprint = thumbprintMatch.Groups[1].Value;
            }

            return result;
        }

        /// <summary>
        /// Removes a certificate from acme.sh management using acme.sh --remove command
        /// </summary>
        /// <param name="options">The removal options containing domain and other settings</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation with the removal result</returns>
        public async Task<RemoveResult> RemoveAsync(RemoveOptions options, CancellationToken cancellationToken = default)
        {
            var args = BuildRemoveArgs(options);
            try
            {
                var result = await ExecuteProcessAsync(args, cancellationToken);
                return ParseRemoveResult(string.Join("\n", result), options);
            }
            catch (ProcessErrorException ex)
            {
                return new RemoveResult
                {
                    IsSuccess = false,
                    ErrorOutput = ex.ErrorOutput.ToArray(),
                    Domain = options.Domain,
                    WasEcc = options.Ecc
                };
            }
        }

        private List<string> BuildRemoveArgs(RemoveOptions options)
        {
            var args = new List<string> { "--remove" };

            // Add domain
            args.Add("-d");
            args.Add(options.Domain);

            // Add ecc flag if specified
            if (options.Ecc)
            {
                args.Add("--ecc");
            }

            return args;
        }

        private RemoveResult ParseRemoveResult(string output, RemoveOptions options)
        {
            var result = new RemoveResult
            {
                IsSuccess = false,
                RawOutput = output,
                Domain = options.Domain,
                WasEcc = options.Ecc,
                RemovedAt = null
            };

            // Check for the actual success pattern from acme.sh
            if (output.Contains($"'{options.Domain}' has been removed.") || output.Contains($"{options.Domain} has been removed"))
            {
                result.IsSuccess = true;
                result.RemovedAt = DateTime.UtcNow;
                
                // Try to extract the certificate path from the output
                var pathMatch = Regex.Match(output, @"The key and cert files are in\s+(.+)");
                if (pathMatch.Success)
                {
                    result.CertificatePath = pathMatch.Groups[1].Value.Trim();
                }
            }
            // Check for error conditions
            else if (output.Contains("error") || output.Contains("failed") || output.Contains("Error"))
            {
                result.IsSuccess = false;
                result.RemovedAt = null;
            }
            
            // Check if the output mentions ECC cert
            if (output.Contains("seems to already have an ECC cert"))
            {
                result.WasEcc = true;
            }

            return result;
        }

        /// <summary>
        /// Renews all SSL/TLS certificates that are due for renewal using acme.sh --renew-all command
        /// </summary>
        /// <param name="options">The renewal options containing settings for the operation</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation with the renewal result for all certificates</returns>
        public async Task<RenewAllResult> RenewAllAsync(RenewAllOptions options, CancellationToken cancellationToken = default)
        {
            var args = BuildRenewAllArgs(options);
            try
            {
                var result = await ExecuteProcessAsync(args, cancellationToken);
                return ParseRenewAllResult(string.Join("\n", result));
            }
            catch (ProcessErrorException ex)
            {
                return new RenewAllResult
                {
                    IsSuccess = false,
                    ErrorOutput = ex.ErrorOutput.ToArray()
                };
            }
        }

        /// <summary>
        /// Gets detailed information about a certificate using acme.sh --info command
        /// </summary>
        /// <param name="options">The info options containing domain and other settings</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation with the certificate information</returns>
        public async Task<InfoResult> InfoAsync(InfoOptions options, CancellationToken cancellationToken = default)
        {
            var args = BuildInfoArgs(options);
            try
            {
                var result = await ExecuteProcessAsync(args, cancellationToken);
                return ParseInfoResult(string.Join("\n", result));
            }
            catch (ProcessErrorException ex)
            {
                return new InfoResult
                {
                    IsSuccess = false,
                    ErrorOutput = ex.ErrorOutput.ToArray()
                };
            }
        }

        private List<string> BuildRenewAllArgs(RenewAllOptions options)
        {
            var args = new List<string> { "--renew-all" };

            // Add stop-renew-on-error flag if specified
            if (options.StopRenewOnError)
            {
                args.Add("--stop-renew-on-error");
            }

            // Add custom server if specified
            if (!string.IsNullOrEmpty(options.Server))
            {
                args.Add("--server");
                args.Add(options.Server);
            }

            return args;
        }

        private RenewAllResult ParseRenewAllResult(string output)
        {
            var result = new RenewAllResult
            {
                IsSuccess = true,
                RawOutput = output,
                CompletedAt = DateTime.UtcNow
            };

            var lines = output.Split('\n');
            string? currentDomain = null;

            foreach (var line in lines)
            {
                // Check for domain being processed
                var renewMatch = Regex.Match(line, @"Renew:\s*'(.+?)'");
                if (renewMatch.Success)
                {
                    currentDomain = renewMatch.Groups[1].Value;
                    result.TotalCertificates++;
                    continue;
                }

                // Check for skip message
                if (line.Contains("Skip, Next renewal time is:") && currentDomain != null)
                {
                    result.SkippedDomains.Add(currentDomain);
                    result.SkippedRenewals++;
                    currentDomain = null;
                    continue;
                }

                // Check for success
                if (line.Contains("Cert success") && currentDomain != null)
                {
                    result.RenewedDomains.Add(currentDomain);
                    result.SuccessfulRenewals++;
                    currentDomain = null;
                    continue;
                }

                // Check for error - both patterns from the example
                if ((line.Contains("Renew error for") || line.Contains("Error renew")) && currentDomain != null)
                {
                    // Extract domain from error message if not already set
                    if (currentDomain == null)
                    {
                        var errorMatch = Regex.Match(line, @"(?:Renew error for|Error renew)\s+(.+?)(?:\.|$)");
                        if (errorMatch.Success)
                        {
                            currentDomain = errorMatch.Groups[1].Value.Trim();
                        }
                    }
                    
                    if (currentDomain != null && !result.FailedDomains.Contains(currentDomain))
                    {
                        result.FailedDomains.Add(currentDomain);
                        result.FailedRenewals++;
                    }
                    currentDomain = null;
                    continue;
                }
            }

            // Set overall success based on whether there were any failures
            result.IsSuccess = result.FailedRenewals == 0;

            return result;
        }

        private List<string> BuildInfoArgs(InfoOptions options)
        {
            var args = new List<string> { "--info" };

            // Add domain
            args.Add("-d");
            args.Add(options.Domain);

            // Add ecc flag if specified
            if (options.Ecc)
            {
                args.Add("--ecc");
            }

            return args;
        }

        private InfoResult ParseInfoResult(string output)
        {
            var result = new InfoResult
            {
                IsSuccess = true,
                RawOutput = output
            };

            var lines = output.Split('\n');

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Parse key=value format
                var equalIndex = line.IndexOf('=');
                if (equalIndex > 0)
                {
                    var key = line.Substring(0, equalIndex).Trim();
                    var value = line.Substring(equalIndex + 1).Trim().Trim('\'', '"');

                    switch (key)
                    {
                        case "DOMAIN_CONF":
                            result.DomainConfigPath = value;
                            break;
                        case "Le_Domain":
                            result.Domain = value;
                            break;
                        case "Le_Alt":
                            result.AltNames = value;
                            break;
                        case "Le_Webroot":
                            result.Webroot = value;
                            break;
                        case "Le_PreHook":
                            result.PreHook = value;
                            break;
                        case "Le_PostHook":
                            result.PostHook = value;
                            break;
                        case "Le_RenewHook":
                            result.RenewHook = value;
                            break;
                        case "Le_API":
                            result.ApiEndpoint = value;
                            break;
                        case "Le_Keylength":
                            result.KeyLength = value;
                            break;
                        case "Le_OrderFinalize":
                            result.OrderFinalizeUrl = value;
                            break;
                        case "Le_LinkOrder":
                            result.LinkOrderUrl = value;
                            break;
                        case "Le_LinkCert":
                            result.LinkCertUrl = value;
                            break;
                        case "Le_CertCreateTime":
                            if (long.TryParse(value, out var certCreateTime))
                            {
                                result.CertCreateTime = certCreateTime;
                            }
                            break;
                        case "Le_CertCreateTimeStr":
                            result.CertCreateTimeStr = value;
                            break;
                        case "Le_NextRenewTimeStr":
                            result.NextRenewTimeStr = value;
                            break;
                        case "Le_NextRenewTime":
                            if (long.TryParse(value, out var nextRenewTime))
                            {
                                result.NextRenewTime = nextRenewTime;
                            }
                            break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Retrieves certificate files and their contents for a given domain
        /// </summary>
        /// <param name="options">The options containing domain and flags for which files to include</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation with the certificate files and contents</returns>
        public async Task<GetCertificateResult> GetCertificateAsync(GetCertificateOptions options, CancellationToken cancellationToken = default)
        {
            // First, get certificate information to find the paths
            var infoOptions = new InfoOptions
            {
                Domain = options.Domain,
                Ecc = options.Ecc
            };

            var infoResult = await InfoAsync(infoOptions, cancellationToken);
            
            var result = new GetCertificateResult
            {
                IsSuccess = infoResult.IsSuccess,
                RawOutput = infoResult.RawOutput,
                ErrorOutput = infoResult.ErrorOutput
            };

            if (!infoResult.IsSuccess || string.IsNullOrEmpty(infoResult.DomainConfigPath))
            {
                return result;
            }

            // Extract the base directory from the domain config path
            var domainConfigDir = Path.GetDirectoryName(infoResult.DomainConfigPath);
            if (string.IsNullOrEmpty(domainConfigDir))
            {
                result.IsSuccess = false;
                result.ErrorOutput = new[] { "Unable to determine certificate directory from domain config path" };
                return result;
            }

            try
            {
                // Determine certificate file names based on ECC flag
                var certFileName = options.Ecc ? $"{options.Domain}_ecc.cer" : $"{options.Domain}.cer";
                var keyFileName = options.Ecc ? $"{options.Domain}_ecc.key" : $"{options.Domain}.key";
                var caFileName = "ca.cer";
                var fullChainFileName = "fullchain.cer";

                // Build full paths
                result.CertificatePath = Path.Combine(domainConfigDir, certFileName);
                result.KeyPath = Path.Combine(domainConfigDir, keyFileName);
                result.CaPath = Path.Combine(domainConfigDir, caFileName);
                result.FullChainPath = Path.Combine(domainConfigDir, fullChainFileName);

                // Read certificate content (always included)
                if (File.Exists(result.CertificatePath))
                {
                    result.Certificate = await File.ReadAllTextAsync(result.CertificatePath, cancellationToken);
                }

                // Read private key if requested
                if (options.IncludeKey && File.Exists(result.KeyPath))
                {
                    result.PrivateKey = await File.ReadAllTextAsync(result.KeyPath, cancellationToken);
                }

                // Read full chain if requested
                if (options.IncludeFullChain && File.Exists(result.FullChainPath))
                {
                    result.FullChain = await File.ReadAllTextAsync(result.FullChainPath, cancellationToken);
                }

                // Read CA bundle if requested
                if (options.IncludeCa && File.Exists(result.CaPath))
                {
                    result.CaBundle = await File.ReadAllTextAsync(result.CaPath, cancellationToken);
                }

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorOutput = new[] { $"Error reading certificate files: {ex.Message}" };
            }

            return result;
        }
    }
}