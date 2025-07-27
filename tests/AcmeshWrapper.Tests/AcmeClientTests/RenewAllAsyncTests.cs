using AcmeshWrapper.Options;
using AcmeshWrapper.Results;
using AcmeshWrapper.Tests.Helpers;
using Cysharp.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcmeshWrapper.Tests.AcmeClientTests
{
    /// <summary>
    /// Test class for AcmeClient.RenewAllAsync method
    /// </summary>
    [TestClass]
    public class RenewAllAsyncTests : TestBase
    {
        /// <summary>
        /// Tests successful renewal of all certificates
        /// </summary>
        [TestMethod]
        public async Task RenewAllAsync_AllCertificatesRenewed_Success()
        {
            // Arrange
            var outputLines = new[]
            {
                "[Info] Renew: 'example.com'",
                "[Info] Single domain certificate",
                "[Info] Getting domain auth token for each domain",
                "[Info] Verifying domain",
                "[Info] Verified!",
                "[Info] Cert success.",
                "[Info] Your cert is in: /root/.acme.sh/example.com/example.com.cer",
                "[Info] Your cert key is in: /root/.acme.sh/example.com/example.com.key",
                "[Info] Renew: 'test.com'",
                "[Info] Single domain certificate",
                "[Info] Getting domain auth token for each domain",
                "[Info] Verifying domain",
                "[Info] Verified!",
                "[Info] Cert success.",
                "[Info] Your cert is in: /root/.acme.sh/test.com/test.com.cer",
                "[Info] Your cert key is in: /root/.acme.sh/test.com/test.com.key",
                "[Info] Renew: 'subdomain.example.com'",
                "[Info] Single domain certificate",
                "[Info] Getting domain auth token for each domain",
                "[Info] Verifying domain",
                "[Info] Verified!",
                "[Info] Cert success.",
                "[Info] Your cert is in: /root/.acme.sh/subdomain.example.com/subdomain.example.com.cer"
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--renew-all" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RenewAllOptions();

            // Act
            var result = await client.RenewAllAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(3, result.TotalCertificates);
            Assert.AreEqual(3, result.SuccessfulRenewals);
            Assert.AreEqual(0, result.FailedRenewals);
            Assert.AreEqual(0, result.SkippedRenewals);
            
            Assert.AreEqual(3, result.RenewedDomains.Count);
            Assert.IsTrue(result.RenewedDomains.Contains("example.com"));
            Assert.IsTrue(result.RenewedDomains.Contains("test.com"));
            Assert.IsTrue(result.RenewedDomains.Contains("subdomain.example.com"));
            
            Assert.AreEqual(0, result.FailedDomains.Count);
            Assert.AreEqual(0, result.SkippedDomains.Count);
            
            Assert.IsNotNull(result.CompletedAt);
            Assert.IsNotNull(result.RawOutput);
            
            LogObject(result, "RenewAllAsync All Certificates Renewed Result");
        }

        /// <summary>
        /// Tests renewal with some certificates skipped (not yet due for renewal)
        /// </summary>
        [TestMethod]
        public async Task RenewAllAsync_SomeCertificatesSkipped_Success()
        {
            // Arrange
            var outputLines = new[]
            {
                "[Info] Renew: 'example.com'",
                "[Info] Single domain certificate",
                "[Info] Getting domain auth token for each domain",
                "[Info] Verifying domain",
                "[Info] Verified!",
                "[Info] Cert success.",
                "[Info] Your cert is in: /root/.acme.sh/example.com/example.com.cer",
                "[Info] Renew: 'test.com'",
                "[Info] Skip, Next renewal time is: Fri Mar 15 12:00:00 UTC 2024",
                "[Info] Renew: 'subdomain.example.com'",
                "[Info] Skip, Next renewal time is: Mon Mar 20 15:30:00 UTC 2024"
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--renew-all" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RenewAllOptions();

            // Act
            var result = await client.RenewAllAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(3, result.TotalCertificates);
            Assert.AreEqual(1, result.SuccessfulRenewals);
            Assert.AreEqual(0, result.FailedRenewals);
            Assert.AreEqual(2, result.SkippedRenewals);
            
            Assert.AreEqual(1, result.RenewedDomains.Count);
            Assert.IsTrue(result.RenewedDomains.Contains("example.com"));
            
            Assert.AreEqual(2, result.SkippedDomains.Count);
            Assert.IsTrue(result.SkippedDomains.Contains("test.com"));
            Assert.IsTrue(result.SkippedDomains.Contains("subdomain.example.com"));
            
            Assert.AreEqual(0, result.FailedDomains.Count);
            
            LogObject(result, "RenewAllAsync Some Certificates Skipped Result");
        }

        /// <summary>
        /// Tests renewal with some certificates failed
        /// </summary>
        [TestMethod]
        public async Task RenewAllAsync_SomeCertificatesFailed_PartialSuccess()
        {
            // Arrange
            var outputLines = new[]
            {
                "[Info] Renew: 'example.com'",
                "[Info] Single domain certificate",
                "[Info] Getting domain auth token for each domain",
                "[Info] Verifying domain",
                "[Info] Verified!",
                "[Info] Cert success.",
                "[Info] Your cert is in: /root/.acme.sh/example.com/example.com.cer",
                "[Info] Renew: 'test.com'",
                "[Error] Verification failed for test.com",
                "[Error] Renew error for test.com",
                "[Info] Renew: 'subdomain.example.com'",
                "[Info] Single domain certificate",
                "[Info] Getting domain auth token for each domain",
                "[Error] DNS problem: NXDOMAIN looking up TXT for _acme-challenge.subdomain.example.com",
                "[Error] Error renew subdomain.example.com."
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--renew-all" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RenewAllOptions();

            // Act
            var result = await client.RenewAllAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess); // Should be false when there are failures
            Assert.AreEqual(3, result.TotalCertificates);
            Assert.AreEqual(1, result.SuccessfulRenewals);
            Assert.AreEqual(2, result.FailedRenewals);
            Assert.AreEqual(0, result.SkippedRenewals);
            
            Assert.AreEqual(1, result.RenewedDomains.Count);
            Assert.IsTrue(result.RenewedDomains.Contains("example.com"));
            
            Assert.AreEqual(2, result.FailedDomains.Count);
            Assert.IsTrue(result.FailedDomains.Contains("test.com"));
            Assert.IsTrue(result.FailedDomains.Contains("subdomain.example.com"));
            
            Assert.AreEqual(0, result.SkippedDomains.Count);
            
            LogObject(result, "RenewAllAsync Some Certificates Failed Result");
        }

        /// <summary>
        /// Tests renewal with stop-on-error flag set, stopping on first error
        /// </summary>
        [TestMethod]
        public async Task RenewAllAsync_WithStopOnError_StopsOnFirstError()
        {
            // Arrange
            var outputLines = new[]
            {
                "[Info] Renew: 'example.com'",
                "[Info] Single domain certificate",
                "[Info] Getting domain auth token for each domain",
                "[Info] Verifying domain",
                "[Info] Verified!",
                "[Info] Cert success.",
                "[Info] Your cert is in: /root/.acme.sh/example.com/example.com.cer",
                "[Info] Renew: 'test.com'",
                "[Error] Verification failed for test.com",
                "[Error] Renew error for test.com",
                "[Info] Stopping due to --stop-renew-on-error flag"
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--renew-all", "--stop-renew-on-error" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RenewAllOptions
            {
                StopRenewOnError = true
            };

            // Act
            var result = await client.RenewAllAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(2, result.TotalCertificates); // Only processed 2 before stopping
            Assert.AreEqual(1, result.SuccessfulRenewals);
            Assert.AreEqual(1, result.FailedRenewals);
            Assert.AreEqual(0, result.SkippedRenewals);
            
            Assert.AreEqual(1, result.RenewedDomains.Count);
            Assert.IsTrue(result.RenewedDomains.Contains("example.com"));
            
            Assert.AreEqual(1, result.FailedDomains.Count);
            Assert.IsTrue(result.FailedDomains.Contains("test.com"));
            
            LogObject(result, "RenewAllAsync With Stop On Error Result");
        }

        /// <summary>
        /// Tests renewal with custom server
        /// </summary>
        [TestMethod]
        public async Task RenewAllAsync_WithCustomServer_Success()
        {
            // Arrange
            var server = TestConstants.Servers.LetsEncryptStaging;
            var outputLines = new[]
            {
                $"[Info] Using server: {server}",
                "[Info] Renew: 'example.com'",
                "[Info] Single domain certificate",
                "[Info] Getting domain auth token for each domain",
                "[Info] Verifying domain",
                "[Info] Verified!",
                "[Info] Cert success.",
                "[Info] Your cert is in: /root/.acme.sh/example.com/example.com.cer",
                "[Info] Renew: 'test.com'",
                "[Info] Skip, Next renewal time is: Fri Mar 15 12:00:00 UTC 2024"
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--renew-all", "--server", server });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RenewAllOptions
            {
                Server = server
            };

            // Act
            var result = await client.RenewAllAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(2, result.TotalCertificates);
            Assert.AreEqual(1, result.SuccessfulRenewals);
            Assert.AreEqual(0, result.FailedRenewals);
            Assert.AreEqual(1, result.SkippedRenewals);
            
            Assert.AreEqual(1, result.RenewedDomains.Count);
            Assert.IsTrue(result.RenewedDomains.Contains("example.com"));
            
            Assert.AreEqual(1, result.SkippedDomains.Count);
            Assert.IsTrue(result.SkippedDomains.Contains("test.com"));
            
            LogObject(result, "RenewAllAsync With Custom Server Result");
        }

        /// <summary>
        /// Tests renewal when there are no certificates to renew
        /// </summary>
        [TestMethod]
        public async Task RenewAllAsync_NoCertificatesToRenew_Success()
        {
            // Arrange
            var outputLines = new[]
            {
                "[Info] No certificates found.",
                "[Info] Nothing to renew."
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--renew-all" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RenewAllOptions();

            // Act
            var result = await client.RenewAllAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.TotalCertificates);
            Assert.AreEqual(0, result.SuccessfulRenewals);
            Assert.AreEqual(0, result.FailedRenewals);
            Assert.AreEqual(0, result.SkippedRenewals);
            
            Assert.AreEqual(0, result.RenewedDomains.Count);
            Assert.AreEqual(0, result.FailedDomains.Count);
            Assert.AreEqual(0, result.SkippedDomains.Count);
            
            LogObject(result, "RenewAllAsync No Certificates To Renew Result");
        }

        /// <summary>
        /// Tests renewal when process fails completely
        /// </summary>
        [TestMethod]
        public async Task RenewAllAsync_ProcessFails_ReturnsFailureResult()
        {
            // Arrange
            var errorOutput = new[]
            {
                "Error: Unable to access acme.sh configuration",
                "Error: Please check your installation"
            };
            
            CreateMockAcmeScript(errorOutput, expectedArgs: new[] { "--renew-all" }, exitCode: 1);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RenewAllOptions();

            // Act
            var result = await client.RenewAllAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.ErrorOutput);
            Assert.AreEqual(2, result.ErrorOutput.Length);
            Assert.IsTrue(result.ErrorOutput.Any(e => e.Contains("Unable to access acme.sh configuration")));
            
            // When process fails, counts should be 0
            Assert.AreEqual(0, result.TotalCertificates);
            Assert.AreEqual(0, result.SuccessfulRenewals);
            Assert.AreEqual(0, result.FailedRenewals);
            Assert.AreEqual(0, result.SkippedRenewals);
            
            LogObject(result, "RenewAllAsync Process Fails Result");
        }

        /// <summary>
        /// Tests parsing of multiple domains including wildcards
        /// </summary>
        [TestMethod]
        public async Task RenewAllAsync_ParsesMultipleDomains_Correctly()
        {
            // Arrange
            var outputLines = new[]
            {
                "[Info] Renew: '*.example.com'",
                "[Info] Wildcard domain certificate",
                "[Info] Getting domain auth token for each domain",
                "[Info] Verifying domain",
                "[Info] Verified!",
                "[Info] Cert success.",
                "[Info] Your cert is in: /root/.acme.sh/*.example.com/*.example.com.cer",
                "[Info] Renew: 'example.com,www.example.com'",
                "[Info] Multi domain certificate",
                "[Info] Getting domain auth token for each domain",
                "[Info] Verifying domain",
                "[Info] Verified!",
                "[Info] Cert success.",
                "[Info] Your cert is in: /root/.acme.sh/example.com/example.com.cer",
                "[Info] Renew: 'api.example.com'",
                "[Info] Skip, Next renewal time is: Fri Mar 15 12:00:00 UTC 2024"
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--renew-all" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RenewAllOptions();

            // Act
            var result = await client.RenewAllAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(3, result.TotalCertificates);
            Assert.AreEqual(2, result.SuccessfulRenewals);
            Assert.AreEqual(0, result.FailedRenewals);
            Assert.AreEqual(1, result.SkippedRenewals);
            
            Assert.AreEqual(2, result.RenewedDomains.Count);
            Assert.IsTrue(result.RenewedDomains.Contains("*.example.com"));
            Assert.IsTrue(result.RenewedDomains.Contains("example.com,www.example.com"));
            
            Assert.AreEqual(1, result.SkippedDomains.Count);
            Assert.IsTrue(result.SkippedDomains.Contains("api.example.com"));
            
            LogObject(result, "RenewAllAsync Parses Multiple Domains Result");
        }

        /// <summary>
        /// Tests mixed results with correct counts
        /// </summary>
        [TestMethod]
        public async Task RenewAllAsync_MixedResults_CorrectCounts()
        {
            // Arrange
            var outputLines = new[]
            {
                // First cert - success
                "[Info] Renew: 'success1.example.com'",
                "[Info] Single domain certificate",
                "[Info] Cert success.",
                // Second cert - skip
                "[Info] Renew: 'skip1.example.com'",
                "[Info] Skip, Next renewal time is: Fri Mar 15 12:00:00 UTC 2024",
                // Third cert - fail
                "[Info] Renew: 'fail1.example.com'",
                "[Error] Verification failed",
                "[Error] Renew error for fail1.example.com",
                // Fourth cert - success
                "[Info] Renew: 'success2.example.com'",
                "[Info] Single domain certificate",
                "[Info] Cert success.",
                // Fifth cert - skip
                "[Info] Renew: 'skip2.example.com'",
                "[Info] Skip, Next renewal time is: Mon Mar 20 15:30:00 UTC 2024",
                // Sixth cert - fail
                "[Info] Renew: 'fail2.example.com'",
                "[Error] DNS problem",
                "[Error] Error renew fail2.example.com.",
                // Seventh cert - success
                "[Info] Renew: 'success3.example.com'",
                "[Info] Single domain certificate",
                "[Info] Cert success."
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--renew-all" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RenewAllOptions();

            // Act
            var result = await client.RenewAllAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess); // Should be false because of failures
            Assert.AreEqual(7, result.TotalCertificates);
            Assert.AreEqual(3, result.SuccessfulRenewals);
            Assert.AreEqual(2, result.FailedRenewals);
            Assert.AreEqual(2, result.SkippedRenewals);
            
            // Check successful domains
            Assert.AreEqual(3, result.RenewedDomains.Count);
            Assert.IsTrue(result.RenewedDomains.Contains("success1.example.com"));
            Assert.IsTrue(result.RenewedDomains.Contains("success2.example.com"));
            Assert.IsTrue(result.RenewedDomains.Contains("success3.example.com"));
            
            // Check failed domains
            Assert.AreEqual(2, result.FailedDomains.Count);
            Assert.IsTrue(result.FailedDomains.Contains("fail1.example.com"));
            Assert.IsTrue(result.FailedDomains.Contains("fail2.example.com"));
            
            // Check skipped domains
            Assert.AreEqual(2, result.SkippedDomains.Count);
            Assert.IsTrue(result.SkippedDomains.Contains("skip1.example.com"));
            Assert.IsTrue(result.SkippedDomains.Contains("skip2.example.com"));
            
            LogObject(result, "RenewAllAsync Mixed Results");
        }

        /// <summary>
        /// Creates a mock acme.sh script for testing
        /// </summary>
        private void CreateMockAcmeScript(string[] outputLines, string[]? expectedArgs = null, int exitCode = 0)
        {
            var scriptContent = "#!/bin/bash\n";
            
            // If expected args are provided, validate them
            if (expectedArgs != null)
            {
                scriptContent += "# Expected args validation\n";
                scriptContent += "ARGS=\"$@\"\n";
                
                // Build expected args string
                var expectedArgsString = string.Join(" ", expectedArgs);
                scriptContent += $"EXPECTED=\"{expectedArgsString}\"\n";
                
                scriptContent += "if [[ \"$ARGS\" != \"$EXPECTED\" ]]; then\n";
                scriptContent += "    echo \"Error: Unexpected arguments\" >&2\n";
                scriptContent += "    echo \"Expected: $EXPECTED\" >&2\n";
                scriptContent += "    echo \"Actual: $ARGS\" >&2\n";
                scriptContent += "    exit 1\n";
                scriptContent += "fi\n\n";
            }
            
            // Output the lines
            if (exitCode == 0)
            {
                foreach (var line in outputLines)
                {
                    scriptContent += $"echo \"{line}\"\n";
                }
            }
            else
            {
                // For error cases, output to stderr
                foreach (var line in outputLines)
                {
                    scriptContent += $"echo \"{line}\" >&2\n";
                }
            }
            
            scriptContent += $"exit {exitCode}\n";
            
            // Create the script file
            var scriptPath = GetTestFilePath("mock-acme.sh");
            CreateTestFile("mock-acme.sh", scriptContent);
            
            // Make it executable on Unix-like systems
            if (Environment.OSVersion.Platform == PlatformID.Unix || 
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                System.Diagnostics.Process.Start("chmod", $"+x {scriptPath}").WaitForExit();
            }
        }
    }
}