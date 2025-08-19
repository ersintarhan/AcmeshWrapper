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
    /// Test class for AcmeClient.IssueAsync method
    /// </summary>
    [TestClass]
    public class IssueAsyncTests : TestBase
    {
        /// <summary>
        /// Tests successful certificate issuance with WebRoot validation
        /// </summary>
        [TestMethod]
        public async Task IssueAsync_WithWebRoot_Success()
        {
            // Arrange
            var domain = "example.com";
            var webRoot = "/var/www/html";
            var outputLines = ProcessXMockHelper.CreateIssueSuccessOutput(domain);
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--issue", "-d example.com", "-w /var/www/html", "--keylength 4096" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new IssueOptions
            {
                Domains = new List<string> { domain },
                WebRoot = webRoot,
                KeyLength = "4096"
            };

            // Act
            var result = await client.IssueAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.CertificateFile);
            Assert.IsNotNull(result.KeyFile);
            Assert.IsNotNull(result.CaFile);
            Assert.IsNotNull(result.FullChainFile);
            
            Assert.AreEqual($"/root/.acme.sh/{domain}/{domain}.cer", result.CertificateFile);
            Assert.AreEqual($"/root/.acme.sh/{domain}/{domain}.key", result.KeyFile);
            Assert.AreEqual($"/root/.acme.sh/{domain}/ca.cer", result.CaFile);
            Assert.AreEqual($"/root/.acme.sh/{domain}/fullchain.cer", result.FullChainFile);
            
            LogObject(result, "IssueAsync WebRoot Result");
        }

        /// <summary>
        /// Tests successful certificate issuance with DNS provider validation
        /// </summary>
        [TestMethod]
        public async Task IssueAsync_WithDnsProvider_Success()
        {
            // Arrange
            var domain = "example.com";
            var dnsProvider = "dns_cf";
            var outputLines = ProcessXMockHelper.CreateIssueSuccessOutput(domain);
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--issue", "-d example.com", "--dns dns_cf", "--keylength 4096" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new IssueOptions
            {
                Domains = new List<string> { domain },
                DnsProvider = dnsProvider,
                KeyLength = "4096"
            };

            // Act
            var result = await client.IssueAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.CertificateFile);
            Assert.IsNotNull(result.KeyFile);
            Assert.IsNotNull(result.CaFile);
            Assert.IsNotNull(result.FullChainFile);
            
            LogObject(result, "IssueAsync DNS Provider Result");
        }

        /// <summary>
        /// Tests successful certificate issuance with staging server
        /// </summary>
        [TestMethod]
        public async Task IssueAsync_WithStaging_Success()
        {
            // Arrange
            var domain = "test.example.com";
            var webRoot = "/var/www/test";
            var outputLines = ProcessXMockHelper.CreateIssueSuccessOutput(domain);
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--issue", "-d test.example.com", "-w /var/www/test", "--keylength 4096", "--staging" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new IssueOptions
            {
                Domains = new List<string> { domain },
                WebRoot = webRoot,
                KeyLength = "4096",
                Staging = true
            };

            // Act
            var result = await client.IssueAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.CertificateFile);
            Assert.IsNotNull(result.KeyFile);
            
            LogMessage("Certificate issued successfully with staging server");
            LogObject(result, "IssueAsync Staging Result");
        }

        /// <summary>
        /// Tests successful certificate issuance with custom key length
        /// </summary>
        [TestMethod]
        public async Task IssueAsync_WithKeyLength_Success()
        {
            // Arrange
            var domain = "secure.example.com";
            var webRoot = "/var/www/secure";
            var keyLength = "2048";
            var outputLines = ProcessXMockHelper.CreateIssueSuccessOutput(domain);
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--issue", "-d secure.example.com", "-w /var/www/secure", "--keylength 2048" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new IssueOptions
            {
                Domains = new List<string> { domain },
                WebRoot = webRoot,
                KeyLength = keyLength
            };

            // Act
            var result = await client.IssueAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.CertificateFile);
            Assert.IsNotNull(result.KeyFile);
            
            LogMessage($"Certificate issued with custom key length: {keyLength}");
            LogObject(result, "IssueAsync Custom Key Length Result");
        }

        /// <summary>
        /// Tests successful certificate issuance for multiple domains
        /// </summary>
        [TestMethod]
        public async Task IssueAsync_MultipleDomainsSuccess()
        {
            // Arrange
            var domains = new List<string> { "example.com", "www.example.com", "api.example.com" };
            var webRoot = "/var/www/html";
            var mainDomain = domains[0];
            
            var outputLines = new[]
            {
                $"[Info] Processing {domains[0]}",
                $"[Info] Processing {domains[1]}",
                $"[Info] Processing {domains[2]}",
                "[Info] Getting domain auth token for each domain",
                "[Info] Getting webroot for domain",
                "[Info] Verifying domain",
                "[Info] Verified!",
                "[Info] Creating domain key",
                $"[Info] The domain key is here: /root/.acme.sh/{mainDomain}/{mainDomain}.key",
                "[Info] Multi domain certificate",
                "[Info] Getting domain auth token for each domain",
                "[Info] Creating CSR",
                $"[Info] The CSR is here: /root/.acme.sh/{mainDomain}/{mainDomain}.csr",
                $"[Info] Your cert is in: /root/.acme.sh/{mainDomain}/{mainDomain}.cer",
                $"[Info] Your cert key is in: /root/.acme.sh/{mainDomain}/{mainDomain}.key",
                $"[Info] The intermediate CA cert is in: /root/.acme.sh/{mainDomain}/ca.cer",
                $"[Info] And the full chain certs is in: /root/.acme.sh/{mainDomain}/fullchain.cer"
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--issue", "-d example.com", "-d www.example.com", "-d api.example.com", "-w /var/www/html", "--keylength 4096" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new IssueOptions
            {
                Domains = domains,
                WebRoot = webRoot,
                KeyLength = "4096"
            };

            // Act
            var result = await client.IssueAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.CertificateFile);
            Assert.IsNotNull(result.KeyFile);
            Assert.IsNotNull(result.CaFile);
            Assert.IsNotNull(result.FullChainFile);
            
            // Certificate files should be under the main domain
            Assert.IsTrue(result.CertificateFile.Contains(mainDomain));
            Assert.IsTrue(result.KeyFile.Contains(mainDomain));
            
            LogMessage($"Multi-domain certificate issued for {domains.Count} domains");
            LogObject(result, "IssueAsync Multiple Domains Result");
        }

        /// <summary>
        /// Tests certificate issuance failure due to domain validation error
        /// </summary>
        [TestMethod]
        public async Task IssueAsync_DomainValidationFails()
        {
            // Arrange
            var domain = "invalid.example.com";
            var webRoot = "/var/www/invalid";
            var errorOutput = ProcessXMockHelper.CreateDomainValidationError(domain);
            
            CreateFailingMockAcmeScript(errorOutput);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new IssueOptions
            {
                Domains = new List<string> { domain },
                WebRoot = webRoot,
                KeyLength = "4096"
            };

            // Act
            var result = await client.IssueAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.ErrorOutput);
            Assert.IsTrue(result.ErrorOutput.Length > 0);
            Assert.IsTrue(result.ErrorOutput.Any(e => e.Contains("Domain validation failed")));
            Assert.IsNull(result.CertificateFile);
            Assert.IsNull(result.KeyFile);
            
            LogObject(result.ErrorOutput, "Domain Validation Error");
        }

        /// <summary>
        /// Tests certificate issuance failure due to rate limit exceeded
        /// </summary>
        [TestMethod]
        public async Task IssueAsync_RateLimitExceeded()
        {
            // Arrange
            var domain = "ratelimited.example.com";
            var webRoot = "/var/www/ratelimited";
            var errorOutput = ProcessXMockHelper.CreateRateLimitError();
            
            CreateFailingMockAcmeScript(errorOutput);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new IssueOptions
            {
                Domains = new List<string> { domain },
                WebRoot = webRoot,
                KeyLength = "4096"
            };

            // Act
            var result = await client.IssueAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.ErrorOutput);
            Assert.IsTrue(result.ErrorOutput.Any(e => e.Contains("Rate limit exceeded")));
            Assert.IsNull(result.CertificateFile);
            Assert.IsNull(result.KeyFile);
            
            LogObject(result.ErrorOutput, "Rate Limit Error");
        }

        /// <summary>
        /// Tests that IssueAsync returns failure result when process fails
        /// </summary>
        [TestMethod]
        public async Task IssueAsync_ProcessFails_ReturnsFailureResult()
        {
            // Arrange
            var domain = "fail.example.com";
            var errorOutput = new[] 
            { 
                "[Error] Failed to issue certificate",
                "[Error] Internal server error",
                "[Error] Please check your configuration"
            };
            
            CreateFailingMockAcmeScript(errorOutput);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new IssueOptions
            {
                Domains = new List<string> { domain },
                WebRoot = "/var/www/fail",
                KeyLength = "4096"
            };

            // Act
            var result = await client.IssueAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.ErrorOutput);
            Assert.AreEqual(3, result.ErrorOutput.Length);
            Assert.IsTrue(result.ErrorOutput[0].Contains("Failed to issue certificate"));
            Assert.IsNull(result.CertificateFile);
            Assert.IsNull(result.KeyFile);
            Assert.IsNull(result.CaFile);
            Assert.IsNull(result.FullChainFile);
            
            LogObject(result.ErrorOutput, "Process Failure Error");
        }

        /// <summary>
        /// Tests that IssueAsync correctly parses certificate paths from output
        /// </summary>
        [TestMethod]
        public async Task IssueAsync_ParsesCertificatePaths_Correctly()
        {
            // Arrange
            var domain = "parse-test.example.com";
            var customPath = "/custom/path/certs";
            
            // Test with custom paths in output
            var outputLines = new[]
            {
                $"[Info] Processing {domain}",
                "[Info] Getting domain auth token for each domain",
                "[Info] Verifying domain",
                "[Info] Verified!",
                "[Info] Creating domain key",
                $"[Info] Your cert is in: {customPath}/{domain}/{domain}.cer",
                $"[Info] Your cert key is in: {customPath}/{domain}/{domain}.key",
                $"[Info] The intermediate CA cert is in: {customPath}/{domain}/ca.cer",
                $"[Info] And the full chain certs is in: {customPath}/{domain}/fullchain.cer"
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--issue", "-d parse-test.example.com", "-w /var/www/html", "--keylength 4096" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new IssueOptions
            {
                Domains = new List<string> { domain },
                WebRoot = "/var/www/html",
                KeyLength = "4096"
            };

            // Act
            var result = await client.IssueAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual($"{customPath}/{domain}/{domain}.cer", result.CertificateFile);
            Assert.AreEqual($"{customPath}/{domain}/{domain}.key", result.KeyFile);
            Assert.AreEqual($"{customPath}/{domain}/ca.cer", result.CaFile);
            Assert.AreEqual($"{customPath}/{domain}/fullchain.cer", result.FullChainFile);
            
            LogMessage("Certificate paths parsed correctly from custom output");
            LogObject(result, "Parse Test Result");
        }

        /// <summary>
        /// Tests that issuance fails if the command produces empty output, even with exit code 0
        /// </summary>
        [TestMethod]
        public async Task IssueAsync_EmptyOutput_ReturnsFailure()
        {
            // Arrange
            var domain = "empty.example.com";

            // Create a mock script that produces no output but exits successfully
            CreateMockAcmeScript(new string[0], new[] { "--issue", "-d", domain, "--keylength", "4096" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));

            var options = new IssueOptions
            {
                Domains = new List<string> { domain },
                KeyLength = "4096"
            };

            // Act
            var result = await client.IssueAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess, "Result should be unsuccessful with empty output.");
            Assert.IsNull(result.CertificateFile);
            Assert.IsNull(result.KeyFile);

            LogMessage("Issuance correctly failed for empty output.");
            LogObject(result, "Empty Output Result");
        }

        #region Helper Methods

        /// <summary>
        /// Creates a mock acme.sh script that outputs the specified lines
        /// </summary>
        private void CreateMockAcmeScript(string[] outputLines, string[]? expectedArgs = null)
        {
            var scriptContent = "#!/bin/bash\n";
            
            // If expected args are provided, validate them
            if (expectedArgs != null)
            {
                scriptContent += "# Expected args validation\n";
                scriptContent += "ARGS=\"$@\"\n";
                
                // Build expected args string
                var expectedArgsStr = string.Join(" ", expectedArgs);
                scriptContent += $"EXPECTED=\"{expectedArgsStr}\"\n";
                
                scriptContent += "if [[ \"$ARGS\" != \"$EXPECTED\" ]]; then\n";
                scriptContent += "  echo \"[Error] Unexpected arguments: $ARGS\" >&2\n";
                scriptContent += "  echo \"[Error] Expected: $EXPECTED\" >&2\n";
                scriptContent += "  exit 1\n";
                scriptContent += "fi\n\n";
            }
            
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

        #endregion
    }
}