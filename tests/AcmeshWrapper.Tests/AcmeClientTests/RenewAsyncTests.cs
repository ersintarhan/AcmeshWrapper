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
    /// Test class for AcmeClient.RenewAsync method
    /// </summary>
    [TestClass]
    public class RenewAsyncTests : TestBase
    {
        /// <summary>
        /// Tests successful standard certificate renewal
        /// </summary>
        [TestMethod]
        public async Task RenewAsync_StandardRenewal_Success()
        {
            // Arrange
            var domain = TestConstants.Domains.ExampleCom;
            var outputLines = ProcessXMockHelper.CreateRenewSuccessOutput(domain);
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--renew", "-d example.com" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RenewOptions(domain);

            // Act
            var result = await client.RenewAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.CertificatePath);
            Assert.IsNotNull(result.KeyPath);
            Assert.IsNotNull(result.CaPath);
            Assert.IsNotNull(result.FullChainPath);
            Assert.IsNotNull(result.RenewedAt);
            
            Assert.AreEqual(TestConstants.Paths.GetCertPath(domain), result.CertificatePath);
            Assert.AreEqual(TestConstants.Paths.GetKeyPath(domain), result.KeyPath);
            Assert.AreEqual(TestConstants.Paths.GetCaPath(domain), result.CaPath);
            Assert.AreEqual(TestConstants.Paths.GetFullChainPath(domain), result.FullChainPath);
            
            LogObject(result, "RenewAsync Standard Renewal Result");
        }

        /// <summary>
        /// Tests certificate renewal with force flag
        /// </summary>
        [TestMethod]
        public async Task RenewAsync_ForceRenewal_Success()
        {
            // Arrange
            var domain = TestConstants.Domains.TestCom;
            var outputLines = ProcessXMockHelper.CreateRenewSuccessOutput(domain);
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--renew", "-d test.com", "--force" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RenewOptions(domain)
            {
                Force = true
            };

            // Act
            var result = await client.RenewAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.CertificatePath);
            Assert.IsNotNull(result.KeyPath);
            Assert.IsNotNull(result.CaPath);
            Assert.IsNotNull(result.FullChainPath);
            Assert.IsNotNull(result.RenewedAt);
            
            LogObject(result, "RenewAsync Force Renewal Result");
        }

        /// <summary>
        /// Tests ECC certificate renewal
        /// </summary>
        [TestMethod]
        public async Task RenewAsync_EccCertificate_Success()
        {
            // Arrange
            var domain = TestConstants.Domains.SubdomainExampleCom;
            var outputLines = new[]
            {
                $"[Info] Renew: '{domain}'",
                "[Info] Using ECC certificate",
                "[Info] Single domain certificate",
                "[Info] Getting domain auth token for each domain",
                "[Info] Verifying domain",
                "[Info] Verified!",
                "[Info] Creating CSR",
                $"[Info] The CSR is here: /root/.acme.sh/{domain}_ecc/{domain}.csr",
                "[Info] Cert success.",
                $"[Info] Your cert is in: /root/.acme.sh/{domain}_ecc/{domain}.cer",
                $"[Info] Your cert key is in: /root/.acme.sh/{domain}_ecc/{domain}.key",
                $"[Info] The intermediate CA cert is in: /root/.acme.sh/{domain}_ecc/ca.cer",
                $"[Info] And the full chain certs is in: /root/.acme.sh/{domain}_ecc/fullchain.cer"
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--renew", "-d subdomain.example.com", "--ecc" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RenewOptions(domain)
            {
                Ecc = true
            };

            // Act
            var result = await client.RenewAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.CertificatePath);
            Assert.IsTrue(result.CertificatePath.Contains("_ecc"));
            Assert.IsNotNull(result.KeyPath);
            Assert.IsNotNull(result.CaPath);
            Assert.IsNotNull(result.FullChainPath);
            
            LogObject(result, "RenewAsync ECC Certificate Result");
        }

        /// <summary>
        /// Tests certificate renewal with custom server
        /// </summary>
        [TestMethod]
        public async Task RenewAsync_WithCustomServer_Success()
        {
            // Arrange
            var domain = TestConstants.Domains.ExampleCom;
            var server = TestConstants.Servers.LetsEncryptStaging;
            var outputLines = ProcessXMockHelper.CreateRenewSuccessOutput(domain);
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--renew", "-d example.com", "--server", server });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RenewOptions(domain)
            {
                Server = server
            };

            // Act
            var result = await client.RenewAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.CertificatePath);
            Assert.IsNotNull(result.KeyPath);
            Assert.IsNotNull(result.CaPath);
            Assert.IsNotNull(result.FullChainPath);
            
            LogObject(result, "RenewAsync Custom Server Result");
        }

        /// <summary>
        /// Tests certificate renewal when not yet due
        /// </summary>
        [TestMethod]
        public async Task RenewAsync_NotYetDue_Skipped()
        {
            // Arrange
            var domain = TestConstants.Domains.ExampleCom;
            var outputLines = ProcessXMockHelper.CreateRenewSkipOutput(domain);
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--renew", "-d example.com" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RenewOptions(domain);

            // Act
            var result = await client.RenewAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess); // Skip is still considered success
            Assert.IsNull(result.CertificatePath); // No paths should be set when skipped
            Assert.IsNull(result.KeyPath);
            Assert.IsNull(result.CaPath);
            Assert.IsNull(result.FullChainPath);
            Assert.IsNotNull(result.RawOutput);
            Assert.IsTrue(result.RawOutput.Contains("Skip, Next renewal time is:"));
            
            LogObject(result, "RenewAsync Not Yet Due Result");
        }

        /// <summary>
        /// Tests certificate renewal when domain is not found
        /// </summary>
        [TestMethod]
        public async Task RenewAsync_DomainNotFound_Fails()
        {
            // Arrange
            var domain = "nonexistent.example.com";
            var errorOutput = new[]
            {
                $"[Error] Domain '{domain}' is not found",
                "[Error] Please check if the domain exists in your certificate list"
            };
            
            CreateMockAcmeScript(errorOutput, expectedArgs: new[] { "--renew", "-d nonexistent.example.com" }, exitCode: 1);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RenewOptions(domain);

            // Act
            var result = await client.RenewAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.ErrorOutput);
            Assert.IsTrue(result.ErrorOutput.Any(e => e.Contains($"Domain '{domain}' is not found")));
            Assert.IsNull(result.CertificatePath);
            Assert.IsNull(result.KeyPath);
            Assert.IsNull(result.CaPath);
            Assert.IsNull(result.FullChainPath);
            Assert.IsNull(result.RenewedAt);
            
            LogObject(result, "RenewAsync Domain Not Found Result");
        }

        /// <summary>
        /// Tests certificate renewal when process fails
        /// </summary>
        [TestMethod]
        public async Task RenewAsync_ProcessFails_ReturnsFailureResult()
        {
            // Arrange
            var domain = TestConstants.Domains.ExampleCom;
            var errorOutput = ProcessXMockHelper.CreateDomainValidationError(domain);
            
            CreateMockAcmeScript(errorOutput, expectedArgs: new[] { "--renew", "-d example.com" }, exitCode: 2);
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RenewOptions(domain);

            // Act
            var result = await client.RenewAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.ErrorOutput);
            Assert.IsTrue(result.ErrorOutput.Any(e => e.Contains("Domain validation failed")));
            Assert.IsNull(result.CertificatePath);
            Assert.IsNull(result.KeyPath);
            Assert.IsNull(result.CaPath);
            Assert.IsNull(result.FullChainPath);
            Assert.IsNull(result.RenewedAt);
            
            LogObject(result, "RenewAsync Process Failure Result");
        }

        /// <summary>
        /// Tests that certificate paths are parsed correctly from output
        /// </summary>
        [TestMethod]
        public async Task RenewAsync_ParsesCertificatePaths_Correctly()
        {
            // Arrange
            var domain = "path-test.example.com";
            var customPaths = new
            {
                Cert = "/custom/path/certs/path-test.example.com/cert.cer",
                Key = "/custom/path/certs/path-test.example.com/private.key",
                Ca = "/custom/path/certs/path-test.example.com/intermediate.cer",
                FullChain = "/custom/path/certs/path-test.example.com/fullchain.cer"
            };
            
            var outputLines = new[]
            {
                $"[Info] Renew: '{domain}'",
                "[Info] Cert success.",
                $"[Info] Your cert is in: {customPaths.Cert}",
                $"[Info] Your cert key is in: {customPaths.Key}",
                $"[Info] The intermediate CA cert is in: {customPaths.Ca}",
                $"[Info] And the full chain certs is in: {customPaths.FullChain}"
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--renew", "-d path-test.example.com" });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RenewOptions(domain);

            // Act
            var result = await client.RenewAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(customPaths.Cert, result.CertificatePath);
            Assert.AreEqual(customPaths.Key, result.KeyPath);
            Assert.AreEqual(customPaths.Ca, result.CaPath);
            Assert.AreEqual(customPaths.FullChain, result.FullChainPath);
            
            LogObject(result, "RenewAsync Parse Paths Result");
        }

        /// <summary>
        /// Tests certificate renewal with all options set
        /// </summary>
        [TestMethod]
        public async Task RenewAsync_AllOptionsSet_Success()
        {
            // Arrange
            var domain = TestConstants.Domains.WildcardExampleCom;
            var server = TestConstants.Servers.ZeroSSL;
            var outputLines = new[]
            {
                $"[Info] Renew: '{domain}'",
                "[Info] Force renewal enabled",
                "[Info] Using ECC certificate",
                $"[Info] Using server: {server}",
                "[Info] Single domain certificate",
                "[Info] Getting domain auth token for each domain",
                "[Info] Verifying domain",
                "[Info] Verified!",
                "[Info] Cert success.",
                $"[Info] Your cert is in: /root/.acme.sh/{domain}_ecc/{domain}.cer",
                $"[Info] Your cert key is in: /root/.acme.sh/{domain}_ecc/{domain}.key",
                $"[Info] The intermediate CA cert is in: /root/.acme.sh/{domain}_ecc/ca.cer",
                $"[Info] And the full chain certs is in: /root/.acme.sh/{domain}_ecc/fullchain.cer"
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { "--renew", "-d *.example.com", "--force", "--ecc", "--server", server });
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new RenewOptions(domain)
            {
                Force = true,
                Ecc = true,
                Server = server
            };

            // Act
            var result = await client.RenewAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.CertificatePath);
            Assert.IsTrue(result.CertificatePath.Contains("_ecc"));
            Assert.IsNotNull(result.KeyPath);
            Assert.IsNotNull(result.CaPath);
            Assert.IsNotNull(result.FullChainPath);
            Assert.IsNotNull(result.RenewedAt);
            
            LogObject(result, "RenewAsync All Options Set Result");
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
            
            // Make the script executable (only works on Unix-like systems)
            if (Environment.OSVersion.Platform == PlatformID.Unix || 
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                System.Diagnostics.Process.Start("chmod", $"+x {scriptPath}").WaitForExit();
            }
        }
    }
}