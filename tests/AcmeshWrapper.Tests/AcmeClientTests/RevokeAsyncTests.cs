using AcmeshWrapper.Enums;
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
    /// Test class for AcmeClient.RevokeAsync method
    /// </summary>
    [TestClass]
    public class RevokeAsyncTests : TestBase
    {
        /// <summary>
        /// Tests successful standard certificate revocation
        /// </summary>
        [TestMethod]
        public async Task RevokeAsync_StandardRevoke_Success()
        {
            // Arrange
            var domain = TestConstants.Domains.ExampleCom;
            var outputLines = ProcessXMockHelper.CreateRevokeSuccessOutput(domain);
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--revoke", "-d example.com" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RevokeOptions
            {
                Domain = domain
            };

            // Act
            var result = await client.RevokeAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(domain, result.Domain);
            Assert.IsNotNull(result.RevokedAt);
            Assert.IsNull(result.Reason); // No reason specified
            Assert.AreEqual("A1B2C3D4E5F6789012345678901234567890ABCD", result.CertificateThumbprint);
            Assert.IsNotNull(result.RawOutput);
            Assert.IsTrue(result.RawOutput.Contains("Revoke success"));
            
            LogObject(result, "RevokeAsync Standard Revoke Result");
        }

        /// <summary>
        /// Tests certificate revocation with reason specified
        /// </summary>
        [TestMethod]
        public async Task RevokeAsync_WithReason_Success()
        {
            // Arrange
            var domain = TestConstants.Domains.TestCom;
            var reason = RevokeReason.KeyCompromise;
            var outputLines = new[]
            {
                $"[Info] Revoking certificate for domain: {domain}",
                $"[Info] Revoke reason: {(int)reason} (Key Compromise)",
                "[Info] Certificate thumbprint: A1B2C3D4E5F6789012345678901234567890ABCD",
                "[Info] Revoke success!"
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--revoke", "-d test.com", "--revoke-reason 1" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RevokeOptions
            {
                Domain = domain,
                Reason = reason
            };

            // Act
            var result = await client.RevokeAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(domain, result.Domain);
            Assert.IsNotNull(result.RevokedAt);
            Assert.AreEqual(reason, result.Reason);
            Assert.AreEqual("A1B2C3D4E5F6789012345678901234567890ABCD", result.CertificateThumbprint);
            
            LogObject(result, "RevokeAsync With Reason Result");
        }

        /// <summary>
        /// Tests ECC certificate revocation
        /// </summary>
        [TestMethod]
        public async Task RevokeAsync_EccCertificate_Success()
        {
            // Arrange
            var domain = TestConstants.Domains.SubdomainExampleCom;
            var outputLines = new[]
            {
                $"[Info] Revoking ECC certificate for domain: {domain}",
                "[Info] Using ECC certificate",
                "[Info] Certificate thumbprint: E1F2A3B4C5D6789012345678901234567890EF",
                "[Info] Cert revoked."
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--revoke", "-d subdomain.example.com", "--ecc" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RevokeOptions
            {
                Domain = domain,
                Ecc = true
            };

            // Act
            var result = await client.RevokeAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(domain, result.Domain);
            Assert.IsNotNull(result.RevokedAt);
            Assert.AreEqual("E1F2A3B4C5D6789012345678901234567890EF", result.CertificateThumbprint);
            Assert.IsTrue(result.RawOutput?.Contains("Cert revoked") ?? false);
            
            LogObject(result, "RevokeAsync ECC Certificate Result");
        }

        /// <summary>
        /// Tests revocation with all possible revoke reasons
        /// </summary>
        [TestMethod]
        public async Task RevokeAsync_AllRevokeReasons_Success()
        {
            // Test all enum values except RemoveFromCRL (which is typically not used for revocation)
            var reasons = new[]
            {
                RevokeReason.Unspecified,
                RevokeReason.KeyCompromise,
                RevokeReason.CACompromise,
                RevokeReason.AffiliationChanged,
                RevokeReason.Superseded,
                RevokeReason.CessationOfOperation,
                RevokeReason.CertificateHold,
                RevokeReason.PrivilegeWithdrawn,
                RevokeReason.AACompromise
            };

            foreach (var reason in reasons)
            {
                // Arrange
                var domain = $"reason-test-{(int)reason}.example.com";
                var outputLines = new[]
                {
                    $"[Info] Revoking certificate for domain: {domain}",
                    $"[Info] Revoke reason: {(int)reason}",
                    $"[Info] Certificate thumbprint: {TestConstants.CertificateInfo.Thumbprint}",
                    "[Info] Revoke success!"
                };
                
                CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--revoke", $"-d {domain}", $"--revoke-reason {(int)reason}" });
                var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
                
                var options = new RevokeOptions
                {
                    Domain = domain,
                    Reason = reason
                };

                // Act
                var result = await client.RevokeAsync(options);

                // Assert
                Assert.IsTrue(result.IsSuccess, $"Failed for reason: {reason}");
                Assert.AreEqual(domain, result.Domain);
                Assert.AreEqual(reason, result.Reason);
                Assert.IsNotNull(result.RevokedAt);
                
                LogMessage($"Successfully tested revoke reason: {reason} ({(int)reason})");
            }
        }

        /// <summary>
        /// Tests revocation when domain is not found
        /// </summary>
        [TestMethod]
        public async Task RevokeAsync_DomainNotFound_Fails()
        {
            // Arrange
            var domain = "nonexistent.example.com";
            var errorOutput = new[]
            {
                $"[Error] Domain '{domain}' is not found",
                "[Error] Please check if the domain exists in your certificate list"
            };
            
            CreateMockAcmeScript(errorOutput, expectedArgs: new[] { "--revoke", "-d nonexistent.example.com" }, exitCode: 1);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RevokeOptions
            {
                Domain = domain
            };

            // Act
            var result = await client.RevokeAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.ErrorOutput);
            Assert.IsTrue(result.ErrorOutput.Any(e => e.Contains("Domain") && e.Contains("not found")));
            Assert.IsNull(result.RevokedAt);
            Assert.IsNull(result.CertificateThumbprint);
            
            LogObject(result, "RevokeAsync Domain Not Found Result");
        }

        /// <summary>
        /// Tests revocation when certificate is already revoked
        /// </summary>
        [TestMethod]
        public async Task RevokeAsync_AlreadyRevoked_Fails()
        {
            // Arrange
            var domain = TestConstants.Domains.ExampleCom;
            var errorOutput = new[]
            {
                $"[Error] Certificate for domain '{domain}' is already revoked",
                "[Error] Certificate was revoked on 2024-01-01",
                "[Error] Revocation failed"
            };
            
            CreateMockAcmeScript(errorOutput, expectedArgs: new[] { "--revoke", "-d example.com" }, exitCode: 1);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RevokeOptions
            {
                Domain = domain
            };

            // Act
            var result = await client.RevokeAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.ErrorOutput);
            Assert.IsTrue(result.ErrorOutput.Any(e => e.Contains("already revoked")));
            Assert.IsNull(result.RevokedAt);
            Assert.AreEqual(domain, result.Domain); // Domain should still be set
            
            LogObject(result, "RevokeAsync Already Revoked Result");
        }

        /// <summary>
        /// Tests revocation when process fails
        /// </summary>
        [TestMethod]
        public async Task RevokeAsync_ProcessFails_ReturnsFailureResult()
        {
            // Arrange
            var domain = TestConstants.Domains.ExampleCom;
            var errorOutput = new[]
            {
                "[Error] Failed to connect to ACME server",
                "[Error] Network timeout"
            };
            
            CreateMockAcmeScript(errorOutput, expectedArgs: new[] { "--revoke", "-d example.com" }, exitCode: 2);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RevokeOptions
            {
                Domain = domain
            };

            // Act
            var result = await client.RevokeAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.ErrorOutput);
            Assert.IsTrue(result.ErrorOutput.Length > 0);
            Assert.IsNull(result.RevokedAt);
            
            LogObject(result, "RevokeAsync Process Failure Result");
        }

        /// <summary>
        /// Tests that certificate thumbprint is parsed correctly
        /// </summary>
        [TestMethod]
        public async Task RevokeAsync_ParsesThumbprint_Correctly()
        {
            // Arrange
            var domain = "thumbprint-test.example.com";
            var customThumbprint = "1234567890ABCDEF1234567890ABCDEF12345678";
            var outputLines = new[]
            {
                $"[Info] Revoking certificate for domain: {domain}",
                $"[Info] Certificate thumbprint: {customThumbprint}",
                "[Info] Processing revocation request",
                "[Info] Revoke success!"
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--revoke", "-d thumbprint-test.example.com" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RevokeOptions
            {
                Domain = domain
            };

            // Act
            var result = await client.RevokeAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(customThumbprint, result.CertificateThumbprint);
            Assert.AreEqual(domain, result.Domain);
            Assert.IsNotNull(result.RevokedAt);
            
            LogObject(result, "RevokeAsync Parse Thumbprint Result");
        }

        /// <summary>
        /// Tests revocation without specifying a reason
        /// </summary>
        [TestMethod]
        public async Task RevokeAsync_WithoutReason_Success()
        {
            // Arrange
            var domain = TestConstants.Domains.WildcardExampleCom;
            var outputLines = new[]
            {
                $"[Info] Revoking certificate for domain: {domain}",
                "[Info] No revocation reason specified",
                "[Info] Certificate thumbprint: FEDCBA0987654321FEDCBA0987654321FEDCBA09",
                "[Info] Revoke success!"
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--revoke", "-d *.example.com" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RevokeOptions
            {
                Domain = domain
            };

            // Act
            var result = await client.RevokeAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(domain, result.Domain);
            Assert.IsNull(result.Reason);
            Assert.IsNotNull(result.RevokedAt);
            Assert.AreEqual("FEDCBA0987654321FEDCBA0987654321FEDCBA09", result.CertificateThumbprint);
            
            LogObject(result, "RevokeAsync Without Reason Result");
        }

        /// <summary>
        /// Tests revocation with cancellation token
        /// </summary>
        [TestMethod]
        public async Task RevokeAsync_WithCancellationToken_CanBeCancelled()
        {
            // Arrange
            var domain = TestConstants.Domains.ExampleCom;
            var outputLines = new[]
            {
                "[Info] Starting revocation process...",
                "[Info] This will take a while...",
                "[Info] Still processing...",
                "[Info] Almost done...",
                "[Info] Revoke success!"
            };
            
            CreateMockAcmeScriptWithDelay(outputLines, expectedArgs: new[] { "--revoke", "-d example.com" }, delayMs: 5000);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RevokeOptions
            {
                Domain = domain
            };

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(100); // Cancel after 100ms

            // Act & Assert
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () => await client.RevokeAsync(options, cts.Token)
            );
            
            LogMessage("RevokeAsync cancellation test completed successfully");
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
            
            var scriptPath = CreateTestFile("mock-acme.sh", scriptContent);
            
            // Make the script executable on Unix-like systems
            if (Environment.OSVersion.Platform == PlatformID.Unix || 
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                System.Diagnostics.Process.Start("chmod", $"+x {scriptPath}").WaitForExit();
            }
        }

        /// <summary>
        /// Creates a mock acme.sh script with a delay for testing cancellation
        /// </summary>
        private void CreateMockAcmeScriptWithDelay(string[] outputLines, string[]? expectedArgs = null, int delayMs = 1000)
        {
            var scriptContent = "#!/bin/bash\n";
            
            // If expected args are provided, validate them
            if (expectedArgs != null)
            {
                scriptContent += "# Expected args validation\n";
                scriptContent += "ARGS=\"$@\"\n";
                
                var expectedArgsString = string.Join(" ", expectedArgs);
                scriptContent += $"EXPECTED=\"{expectedArgsString}\"\n";
                
                scriptContent += "if [[ \"$ARGS\" != \"$EXPECTED\" ]]; then\n";
                scriptContent += "    echo \"Error: Unexpected arguments\" >&2\n";
                scriptContent += "    echo \"Expected: $EXPECTED\" >&2\n";
                scriptContent += "    echo \"Actual: $ARGS\" >&2\n";
                scriptContent += "    exit 1\n";
                scriptContent += "fi\n\n";
            }
            
            // Add delay
            scriptContent += $"sleep {delayMs / 1000.0}\n";
            
            // Output the lines
            foreach (var line in outputLines)
            {
                scriptContent += $"echo \"{line}\"\n";
            }
            
            scriptContent += "exit 0\n";
            
            var scriptPath = CreateTestFile("mock-acme.sh", scriptContent);
            
            // Make the script executable on Unix-like systems
            if (Environment.OSVersion.Platform == PlatformID.Unix || 
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                System.Diagnostics.Process.Start("chmod", $"+x {scriptPath}").WaitForExit();
            }
        }
    }
}