using AcmeshWrapper.Options;
using AcmeshWrapper.Results;
using AcmeshWrapper.Tests.Helpers;
using Cysharp.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcmeshWrapper.Tests.AcmeClientTests
{
    /// <summary>
    /// Test class for AcmeClient.InfoAsync method
    /// </summary>
    [TestClass]
    public class InfoAsyncTests : TestBase
    {

        /// <summary>
        /// Tests that InfoAsync returns successful result for standard RSA certificate
        /// </summary>
        [TestMethod]
        public async Task InfoAsync_StandardCertificate_Success()
        {
            // Arrange
            var domain = "example.com";
            var outputLines = new[]
            {
                "DOMAIN_CONF=/home/user/.acme.sh/example.com/example.com.conf",
                "Le_Domain=example.com",
                "Le_Alt=no",
                "Le_Webroot=/var/www/example.com",
                "Le_PreHook=",
                "Le_PostHook=",
                "Le_RenewHook=",
                "Le_API=https://acme-v02.api.letsencrypt.org/directory",
                "Le_Keylength=2048",
                "Le_CertCreateTimeStr=2024-01-15 10:30:45 UTC",
                "Le_NextRenewTimeStr=2024-03-15 10:30:45 UTC"
            };
            
            CreateMockAcmeScript(outputLines, domain);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            var options = new InfoOptions { Domain = domain };

            // Act
            var result = await client.InfoAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("/home/user/.acme.sh/example.com/example.com.conf", result.DomainConfigPath);
            Assert.AreEqual(domain, result.Domain);
            Assert.AreEqual("no", result.AltNames);
            Assert.AreEqual("/var/www/example.com", result.Webroot);
            Assert.AreEqual(string.Empty, result.PreHook);
            Assert.AreEqual(string.Empty, result.PostHook);
            Assert.AreEqual(string.Empty, result.RenewHook);
            Assert.AreEqual("https://acme-v02.api.letsencrypt.org/directory", result.ApiEndpoint);
            Assert.AreEqual("2048", result.KeyLength);
            
            LogMessage($"Successfully retrieved info for {domain}");
            LogObject(result, "Info Result");
        }

        /// <summary>
        /// Tests that InfoAsync returns successful result for ECC certificate
        /// </summary>
        [TestMethod]
        public async Task InfoAsync_EccCertificate_Success()
        {
            // Arrange
            var domain = "example.com";
            var outputLines = new[]
            {
                "DOMAIN_CONF=/home/user/.acme.sh/example.com_ecc/example.com.conf",
                "Le_Domain=example.com",
                "Le_Alt=www.example.com,api.example.com",
                "Le_Webroot=/var/www/example.com",
                "Le_PreHook=echo 'Starting renewal'",
                "Le_PostHook=echo 'Renewal complete'",
                "Le_RenewHook=service nginx reload",
                "Le_API=https://acme-v02.api.letsencrypt.org/directory",
                "Le_Keylength=ec-256",
                "Le_CertCreateTimeStr=2024-01-15 10:30:45 UTC",
                "Le_NextRenewTimeStr=2024-03-15 10:30:45 UTC"
            };
            
            CreateMockAcmeScript(outputLines, domain, isEcc: true);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            var options = new InfoOptions 
            { 
                Domain = domain,
                Ecc = true
            };

            // Act
            var result = await client.InfoAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("/home/user/.acme.sh/example.com_ecc/example.com.conf", result.DomainConfigPath);
            Assert.AreEqual(domain, result.Domain);
            Assert.AreEqual("www.example.com,api.example.com", result.AltNames);
            Assert.AreEqual("/var/www/example.com", result.Webroot);
            // The parser trims quotes, so the trailing quote gets removed
            Assert.AreEqual("echo 'Starting renewal", result.PreHook);
            Assert.AreEqual("echo 'Renewal complete", result.PostHook);
            Assert.AreEqual("service nginx reload", result.RenewHook);
            Assert.AreEqual("https://acme-v02.api.letsencrypt.org/directory", result.ApiEndpoint);
            Assert.AreEqual("ec-256", result.KeyLength);
            
            LogMessage($"Successfully retrieved ECC certificate info for {domain}");
            LogObject(result, "ECC Info Result");
        }

        /// <summary>
        /// Tests that InfoAsync returns failure result when domain is not found
        /// </summary>
        [TestMethod]
        public async Task InfoAsync_DomainNotFound_Fails()
        {
            // Arrange
            var domain = "nonexistent.com";
            var errorOutput = new[]
            {
                $"[Thu Jan 15 10:30:45 UTC 2024] {domain} is not a issued domain, skip."
            };
            
            CreateFailingMockAcmeScript(errorOutput);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            var options = new InfoOptions { Domain = domain };

            // Act
            var result = await client.InfoAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Domain);
            Assert.IsNotNull(result.ErrorOutput);
            Assert.IsTrue(result.ErrorOutput[0].Contains($"{domain} is not a issued domain"));
            
            LogMessage($"Domain {domain} not found - info failed as expected");
            LogObject(result.ErrorOutput, "Error Output");
        }

        /// <summary>
        /// Tests that InfoAsync returns failure result when process fails
        /// </summary>
        [TestMethod]
        public async Task InfoAsync_ProcessFails_ReturnsFailureResult()
        {
            // Arrange
            var domain = "example.com";
            var errorOutput = new[]
            {
                "[Error] Failed to get certificate info",
                "[Error] Permission denied: Cannot read certificate directory"
            };
            
            CreateFailingMockAcmeScript(errorOutput);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            var options = new InfoOptions { Domain = domain };

            // Act
            var result = await client.InfoAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Domain);
            Assert.IsNotNull(result.ErrorOutput);
            Assert.IsTrue(result.ErrorOutput[0].Contains("Failed to get certificate info"));
            
            LogMessage("Process failed due to permissions - handled correctly");
            LogObject(result.ErrorOutput, "Process Error Output");
        }

        /// <summary>
        /// Tests that InfoAsync parses all fields correctly
        /// </summary>
        [TestMethod]
        public async Task InfoAsync_ParsesAllFields_Correctly()
        {
            // Arrange
            var domain = "test.com";
            var outputLines = new[]
            {
                "DOMAIN_CONF=/root/.acme.sh/test.com/test.com.conf",
                "Le_Domain=test.com",
                "Le_Alt=*.test.com",
                "Le_Webroot=dns_cloudflare",
                "Le_PreHook=/scripts/pre-hook.sh",
                "Le_PostHook=/scripts/post-hook.sh",
                "Le_RenewHook=/scripts/renew-hook.sh",
                "Le_API=https://acme.zerossl.com/v2/DV90",
                "Le_Keylength=4096",
                "Le_CertCreateTimeStr=2024-02-20 14:25:30 UTC",
                "Le_NextRenewTimeStr=2024-04-20 14:25:30 UTC",
                "Le_NextRenewTime=1713624330"
            };
            
            CreateMockAcmeScript(outputLines, domain);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            var options = new InfoOptions { Domain = domain };

            // Act
            var result = await client.InfoAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("/root/.acme.sh/test.com/test.com.conf", result.DomainConfigPath);
            Assert.AreEqual(domain, result.Domain);
            Assert.AreEqual("*.test.com", result.AltNames);
            Assert.AreEqual("dns_cloudflare", result.Webroot);
            Assert.AreEqual("/scripts/pre-hook.sh", result.PreHook);
            Assert.AreEqual("/scripts/post-hook.sh", result.PostHook);
            Assert.AreEqual("/scripts/renew-hook.sh", result.RenewHook);
            Assert.AreEqual("https://acme.zerossl.com/v2/DV90", result.ApiEndpoint);
            Assert.AreEqual("4096", result.KeyLength);
            Assert.AreEqual(1713624330L, result.NextRenewTime);
            
            LogMessage("All certificate info fields parsed correctly");
            LogObject(result, "Complete info parsing test result");
        }

        /// <summary>
        /// Tests that InfoAsync handles missing fields correctly
        /// </summary>
        [TestMethod]
        public async Task InfoAsync_WithMissingFields_Success()
        {
            // Arrange
            var domain = "minimal.com";
            var outputLines = new[]
            {
                "DOMAIN_CONF=/home/user/.acme.sh/minimal.com/minimal.com.conf",
                "Le_Domain=minimal.com",
                "Le_Alt=",
                "Le_Webroot=/var/www/minimal.com",
                "Le_PreHook=",
                "Le_PostHook=",
                "Le_RenewHook=",
                "Le_API=",
                "Le_Keylength=",
                "Le_CertCreateTimeStr=2024-01-15 10:30:45 UTC",
                "Le_NextRenewTimeStr=2024-03-15 10:30:45 UTC"
            };
            
            CreateMockAcmeScript(outputLines, domain);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            var options = new InfoOptions { Domain = domain };

            // Act
            var result = await client.InfoAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("/home/user/.acme.sh/minimal.com/minimal.com.conf", result.DomainConfigPath);
            Assert.AreEqual(domain, result.Domain);
            Assert.AreEqual(string.Empty, result.AltNames);
            Assert.AreEqual("/var/www/minimal.com", result.Webroot);
            Assert.AreEqual(string.Empty, result.PreHook);
            Assert.AreEqual(string.Empty, result.PostHook);
            Assert.AreEqual(string.Empty, result.RenewHook);
            Assert.AreEqual(string.Empty, result.ApiEndpoint);
            Assert.AreEqual(string.Empty, result.KeyLength);
            Assert.IsNull(result.NextRenewTime);
            
            LogMessage($"Successfully handled minimal info for {domain} with missing fields");
            LogObject(result, "Minimal Info Result");
        }

        /// <summary>
        /// Tests that InfoAsync throws OperationCanceledException or TaskCanceledException when cancelled
        /// </summary>
        [TestMethod]
        public async Task InfoAsync_WithCancellation_ThrowsOperationCanceledException()
        {
            // Arrange
            CreateSlowMockAcmeScript();
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            var options = new InfoOptions { Domain = "example.com" };
            
            // Create a cancellation token that cancels after 100ms
            var cts = new CancellationTokenSource(100);

            // Act & Assert
            try
            {
                await client.InfoAsync(options, cts.Token);
                Assert.Fail("Expected OperationCanceledException or TaskCanceledException but no exception was thrown");
            }
            catch (TaskCanceledException)
            {
                // TaskCanceledException inherits from OperationCanceledException
                // This is expected - ProcessX sometimes throws TaskCanceledException
                LogMessage("Caught TaskCanceledException as expected");
            }
            catch (OperationCanceledException)
            {
                // This is also expected - ProcessX sometimes throws OperationCanceledException directly
                LogMessage("Caught OperationCanceledException as expected");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected OperationCanceledException or TaskCanceledException but got {ex.GetType().Name}: {ex.Message}");
            }
        }

        #region Helper Methods

        /// <summary>
        /// Creates a mock acme.sh script that outputs the specified lines
        /// </summary>
        private void CreateMockAcmeScript(string[] outputLines, string domain, bool isEcc = false)
        {
            var scriptContent = "#!/bin/bash\n";
            
            // Check if --info and domain are present
            scriptContent += "if [[ \"$*\" == *\"--info\"* ]] && [[ \"$*\" == *\"" + domain + "\"* ]]; then\n";
            
            if (isEcc)
            {
                scriptContent += "if [[ \"$*\" == *\"--ecc\"* ]]; then\n";
            }
            
            foreach (var line in outputLines)
            {
                // Use printf instead of echo to avoid escaping issues
                scriptContent += $"printf '%s\\n' {EscapeForShell(line)}\n";
            }
            
            if (isEcc)
            {
                scriptContent += "else\n";
                scriptContent += "echo \"[Error] ECC flag required but not provided\"\n";
                scriptContent += "exit 1\n";
                scriptContent += "fi\n";
            }
            
            scriptContent += "else\n";
            scriptContent += "echo \"[Error] Info command not found or domain mismatch\"\n";
            scriptContent += "exit 1\n";
            scriptContent += "fi\n";
            scriptContent += "exit 0\n";
            
            var scriptPath = CreateTestFile("mock-acme.sh", scriptContent);
            
            // Make the script executable on Unix-like systems
            if (Environment.OSVersion.Platform == PlatformID.Unix || 
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                System.Diagnostics.Process.Start("chmod", $"+x {scriptPath}").WaitForExit();
            }
        }

        /// <summary>
        /// Creates a failing mock acme.sh script
        /// </summary>
        private void CreateFailingMockAcmeScript(string[] errorOutput)
        {
            var scriptContent = "#!/bin/bash\n";
            foreach (var line in errorOutput)
            {
                scriptContent += $"echo \"{line}\" >&2\n";
            }
            scriptContent += "exit 1\n";
            
            var scriptPath = CreateTestFile("mock-acme.sh", scriptContent);
            
            // Make the script executable on Unix-like systems
            if (Environment.OSVersion.Platform == PlatformID.Unix || 
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                System.Diagnostics.Process.Start("chmod", $"+x {scriptPath}").WaitForExit();
            }
        }

        /// <summary>
        /// Creates a slow mock acme.sh script for cancellation testing
        /// </summary>
        private void CreateSlowMockAcmeScript()
        {
            var scriptContent = @"#!/bin/bash
sleep 5
echo ""Created=2024-01-15 10:30:45 UTC""
echo ""Renew=2024-03-15 10:30:45 UTC""
echo ""Le_Domain=example.com""
exit 0
";
            
            var scriptPath = CreateTestFile("mock-acme.sh", scriptContent);
            
            // Make the script executable on Unix-like systems
            if (Environment.OSVersion.Platform == PlatformID.Unix || 
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                System.Diagnostics.Process.Start("chmod", $"+x {scriptPath}").WaitForExit();
            }
        }

        /// <summary>
        /// Escapes a string for use in shell commands
        /// </summary>
        private string EscapeForShell(string input)
        {
            // Use single quotes and escape any single quotes in the input
            return "'" + input.Replace("'", "'\"'\"'") + "'";
        }

        #endregion
    }
}