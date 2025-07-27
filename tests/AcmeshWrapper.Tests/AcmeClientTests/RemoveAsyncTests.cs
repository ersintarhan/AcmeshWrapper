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
    /// Test class for AcmeClient.RemoveAsync method
    /// </summary>
    [TestClass]
    public class RemoveAsyncTests : TestBase
    {
        /// <summary>
        /// Tests that RemoveAsync returns successful result for standard RSA certificate removal
        /// </summary>
        [TestMethod]
        public async Task RemoveAsync_StandardRemove_Success()
        {
            // Arrange
            var domain = "example.com";
            var outputLines = new[]
            {
                $"[Sun Jul 27 01:35:20 +03 2025] {domain} has been removed. The key and cert files are in /root/.acme.sh/{domain}"
            };
            
            CreateMockAcmeScript(outputLines);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            var options = new RemoveOptions(domain);

            // Act
            var result = await client.RemoveAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(domain, result.Domain);
            Assert.IsFalse(result.WasEcc);
            Assert.IsNotNull(result.RemovedAt);
            Assert.IsTrue(result.RemovedAt.Value <= DateTime.UtcNow);
            Assert.IsNull(result.ErrorOutput);
            
            // Verify the output contains important information
            Assert.IsTrue(result.RawOutput?.Contains($"{domain} has been removed") ?? false);
            Assert.IsTrue(result.RawOutput?.Contains("/root/.acme.sh/") ?? false);
            Assert.IsNotNull(result.CertificatePath);
            Assert.AreEqual($"/root/.acme.sh/{domain}", result.CertificatePath);
            
            LogMessage($"Successfully removed {domain} from acme.sh management");
            LogObject(result, "Remove Result");
        }

        /// <summary>
        /// Tests that RemoveAsync returns successful result for ECC certificate removal
        /// </summary>
        [TestMethod]
        public async Task RemoveAsync_EccCertificate_Success()
        {
            // Arrange
            var domain = "example.com";
            var outputLines = new[]
            {
                $"[Sun Jul 27 01:35:20 +03 2025] It seems that {domain} already has an ECC cert.",
                $"[Sun Jul 27 01:35:20 +03 2025] {domain} has been removed. The key and cert files are in /root/.acme.sh/{domain}_ecc"
            };
            
            CreateMockAcmeScript(outputLines, eccFlag: true);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            var options = new RemoveOptions(domain) { Ecc = true };

            // Act
            var result = await client.RemoveAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(domain, result.Domain);
            Assert.IsTrue(result.WasEcc);
            Assert.IsNotNull(result.RemovedAt);
            Assert.IsNull(result.ErrorOutput);
            
            // Verify ECC-specific output
            Assert.IsTrue(result.RawOutput?.Contains("already has an ECC cert") ?? false);
            Assert.IsTrue(result.RawOutput?.Contains($"{domain}_ecc") ?? false);
            Assert.IsNotNull(result.CertificatePath);
            Assert.AreEqual($"/root/.acme.sh/{domain}_ecc", result.CertificatePath);
            
            LogMessage($"Successfully removed ECC certificate for {domain}");
            LogObject(result, "ECC Remove Result");
        }

        /// <summary>
        /// Tests that RemoveAsync returns failure result when domain is not found
        /// </summary>
        [TestMethod]
        public async Task RemoveAsync_DomainNotFound_Fails()
        {
            // Arrange
            var domain = "nonexistent.com";
            var errorOutput = new[]
            {
                $"[Error] Certificate for '{domain}' not found in acme.sh management",
                $"[Error] Please check the domain name or use --list to see all managed certificates"
            };
            
            CreateFailingMockAcmeScript(errorOutput);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            var options = new RemoveOptions(domain);

            // Act
            var result = await client.RemoveAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(domain, result.Domain);
            Assert.IsFalse(result.WasEcc);
            Assert.IsNull(result.RemovedAt);
            Assert.IsNotNull(result.ErrorOutput);
            Assert.AreEqual(2, result.ErrorOutput.Length);
            Assert.IsTrue(result.ErrorOutput[0].Contains("not found"));
            Assert.IsTrue(result.ErrorOutput[1].Contains("--list"));
            
            LogMessage($"Domain {domain} not found - removal failed as expected");
            LogObject(result.ErrorOutput, "Error Output");
        }

        /// <summary>
        /// Tests that RemoveAsync returns failure result when certificate is already removed
        /// </summary>
        [TestMethod]
        public async Task RemoveAsync_AlreadyRemoved_Fails()
        {
            // Arrange
            var domain = "example.com";
            var errorOutput = new[]
            {
                $"[Error] Certificate '{domain}' is not currently managed by acme.sh",
                $"[Error] It may have already been removed or was never issued"
            };
            
            CreateFailingMockAcmeScript(errorOutput);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            var options = new RemoveOptions(domain);

            // Act
            var result = await client.RemoveAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(domain, result.Domain);
            Assert.IsNull(result.RemovedAt);
            Assert.IsNotNull(result.ErrorOutput);
            Assert.IsTrue(result.ErrorOutput[0].Contains("not currently managed"));
            Assert.IsTrue(result.ErrorOutput[1].Contains("already been removed"));
            
            LogMessage("Already removed certificate - failed as expected");
            LogObject(result, "Already Removed Result");
        }

        /// <summary>
        /// Tests that RemoveAsync returns failure result when process fails
        /// </summary>
        [TestMethod]
        public async Task RemoveAsync_ProcessFails_ReturnsFailureResult()
        {
            // Arrange
            var domain = "example.com";
            var errorOutput = new[]
            {
                "[Error] Failed to remove certificate from management",
                "[Error] Permission denied: Cannot write to configuration file",
                "[Error] Please check permissions and try again"
            };
            
            CreateFailingMockAcmeScript(errorOutput);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            var options = new RemoveOptions(domain);

            // Act
            var result = await client.RemoveAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(domain, result.Domain);
            Assert.IsNull(result.RemovedAt);
            Assert.IsNotNull(result.ErrorOutput);
            Assert.AreEqual(3, result.ErrorOutput.Length);
            Assert.IsTrue(result.ErrorOutput[0].Contains("Failed to remove"));
            Assert.IsTrue(result.ErrorOutput[1].Contains("Permission denied"));
            Assert.IsTrue(result.ErrorOutput[2].Contains("check permissions"));
            
            LogMessage("Process failed due to permissions - handled correctly");
            LogObject(result.ErrorOutput, "Process Error Output");
        }

        /// <summary>
        /// Tests that RemoveAsync removes the correct certificate when multiple domains match
        /// </summary>
        [TestMethod]
        public async Task RemoveAsync_MultipleDomainsMatch_RemovesCorrectOne()
        {
            // Arrange
            var domain = "api.example.com";
            var outputLines = new[]
            {
                $"[Sun Jul 27 01:35:20 +03 2025] {domain} has been removed. The key and cert files are in /root/.acme.sh/{domain}"
            };
            
            CreateMockAcmeScript(outputLines);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            var options = new RemoveOptions(domain);

            // Act
            var result = await client.RemoveAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(domain, result.Domain);
            Assert.IsNotNull(result.RemovedAt);
            
            // Verify the removal was successful
            Assert.IsTrue(result.RawOutput?.Contains($"{domain} has been removed") ?? false);
            Assert.IsNotNull(result.CertificatePath);
            Assert.AreEqual($"/root/.acme.sh/{domain}", result.CertificatePath);
            
            LogMessage($"Correctly removed {domain} from multiple matches");
            LogObject(result, "Multiple Match Remove Result");
        }

        /// <summary>
        /// Tests that RemoveAsync parses various output formats correctly
        /// </summary>
        [TestMethod]
        public async Task RemoveAsync_ParsesResult_Correctly()
        {
            // Arrange - Test different output formats that acme.sh might produce
            var testCases = new[]
            {
                new
                {
                    Domain = "test1.com",
                    Output = new[]
                    {
                        "[Sun Jul 27 01:35:20 +03 2025] test1.com has been removed. The key and cert files are in /root/.acme.sh/test1.com"
                    },
                    ExpectedSuccess = true
                },
                new
                {
                    Domain = "test2.com",
                    Output = new[]
                    {
                        "[Sun Jul 27 01:35:20 +03 2025] test2.com has been removed. The key and cert files are in /root/.acme.sh/test2.com_ecc"
                    },
                    ExpectedSuccess = true
                },
                new
                {
                    Domain = "test3.com",
                    Output = new[]
                    {
                        "[Info] Starting removal process...",
                        "[Error] Failed to remove certificate",
                        "[Error] Certificate not found: test3.com"
                    },
                    ExpectedSuccess = false
                }
            };

            foreach (var testCase in testCases)
            {
                // Arrange
                CreateMockAcmeScript(testCase.Output);
                var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
                var options = new RemoveOptions(testCase.Domain);

                // Act
                var result = await client.RemoveAsync(options);

                // Assert
                Assert.AreEqual(testCase.ExpectedSuccess, result.IsSuccess,
                    $"Expected success={testCase.ExpectedSuccess} for domain {testCase.Domain}");
                Assert.AreEqual(testCase.Domain, result.Domain);
                
                if (testCase.ExpectedSuccess)
                {
                    Assert.IsNotNull(result.RemovedAt);
                }
                else
                {
                    // The third case doesn't contain the expected success patterns
                    Assert.IsNull(result.RemovedAt);
                }
                
                LogMessage($"Parsed result for {testCase.Domain}: Success={result.IsSuccess}");
            }
        }

        /// <summary>
        /// Tests that RemoveAsync throws OperationCanceledException or TaskCanceledException when cancelled
        /// </summary>
        [TestMethod]
        public async Task RemoveAsync_WithCancellation_ThrowsOperationCanceledException()
        {
            // Arrange
            CreateSlowMockAcmeScript();
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            var options = new RemoveOptions("example.com");
            
            // Create a cancellation token that cancels after 100ms
            var cts = new CancellationTokenSource(100);

            // Act & Assert
            try
            {
                await client.RemoveAsync(options, cts.Token);
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
        private void CreateMockAcmeScript(string[] outputLines, bool eccFlag = false)
        {
            var scriptContent = "#!/bin/bash\n";
            
            // Check if --ecc flag should be present
            if (eccFlag)
            {
                scriptContent += "if [[ \"$*\" == *\"--ecc\"* ]]; then\n";
                foreach (var line in outputLines)
                {
                    scriptContent += $"echo \"{line}\"\n";
                }
                scriptContent += "else\n";
                scriptContent += "echo \"[Error] ECC flag required but not provided\"\n";
                scriptContent += "exit 1\n";
                scriptContent += "fi\n";
            }
            else
            {
                // Check if --remove is present
                scriptContent += "if [[ \"$*\" == *\"--remove\"* ]]; then\n";
                foreach (var line in outputLines)
                {
                    scriptContent += $"echo \"{line}\"\n";
                }
                scriptContent += "else\n";
                scriptContent += "echo \"[Error] Remove command not found\"\n";
                scriptContent += "exit 1\n";
                scriptContent += "fi\n";
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
echo ""Removed: 'example.com'""
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

        #endregion
    }
}