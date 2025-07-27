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
    /// Test class for AcmeClient.InstallCertAsync method
    /// </summary>
    [TestClass]
    public class InstallCertAsyncTests : TestBase
    {
        /// <summary>
        /// Tests successful installation of all certificate files
        /// </summary>
        [TestMethod]
        public async Task InstallCertAsync_AllFilesInstalled_Success()
        {
            // Arrange
            var domain = TestConstants.Domains.ExampleCom;
            var installPaths = new InstallCertPaths
            {
                CertFile = TestConstants.Paths.InstallCertPath,
                KeyFile = TestConstants.Paths.InstallKeyPath,
                CaFile = TestConstants.Paths.InstallCaPath,
                FullChainFile = TestConstants.Paths.InstallFullChainPath
            };
            
            var outputLines = ProcessXMockHelper.CreateInstallCertSuccessOutput(installPaths);
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { 
                "--install-cert", 
                "-d",
                "example.com",
                "--cert-file",
                "/etc/ssl/certs/example.crt",
                "--key-file",
                "/etc/ssl/private/example.key",
                "--ca-file",
                "/etc/ssl/certs/ca.crt",
                "--fullchain-file",
                "/etc/ssl/certs/fullchain.crt"
            });
            
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new InstallCertOptions(domain)
            {
                CertFile = installPaths.CertFile,
                KeyFile = installPaths.KeyFile,
                CaFile = installPaths.CaFile,
                FullChainFile = installPaths.FullChainFile
            };

            // Act
            var result = await client.InstallCertAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(installPaths.CertFile, result.InstalledCertFile);
            Assert.AreEqual(installPaths.KeyFile, result.InstalledKeyFile);
            Assert.AreEqual(installPaths.CaFile, result.InstalledCaFile);
            Assert.AreEqual(installPaths.FullChainFile, result.InstalledFullChainFile);
            Assert.IsFalse(result.ReloadCommandExecuted);
            Assert.IsNull(result.ReloadCommandOutput);
            Assert.IsNotNull(result.InstalledAt);
            
            LogObject(result, "InstallCertAsync All Files Installed Result");
        }

        /// <summary>
        /// Tests successful partial installation (only cert and key files)
        /// </summary>
        [TestMethod]
        public async Task InstallCertAsync_PartialInstall_Success()
        {
            // Arrange
            var domain = TestConstants.Domains.TestCom;
            var installPaths = new InstallCertPaths
            {
                CertFile = "/etc/nginx/ssl/test.crt",
                KeyFile = "/etc/nginx/ssl/test.key"
            };
            
            var outputLines = ProcessXMockHelper.CreateInstallCertSuccessOutput(installPaths);
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { 
                "--install-cert", 
                "-d",
                "test.com",
                "--cert-file",
                "/etc/nginx/ssl/test.crt",
                "--key-file",
                "/etc/nginx/ssl/test.key"
            });
            
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new InstallCertOptions(domain)
            {
                CertFile = installPaths.CertFile,
                KeyFile = installPaths.KeyFile
            };

            // Act
            var result = await client.InstallCertAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(installPaths.CertFile, result.InstalledCertFile);
            Assert.AreEqual(installPaths.KeyFile, result.InstalledKeyFile);
            Assert.IsNull(result.InstalledCaFile);
            Assert.IsNull(result.InstalledFullChainFile);
            Assert.IsFalse(result.ReloadCommandExecuted);
            Assert.IsNotNull(result.InstalledAt);
            
            LogObject(result, "InstallCertAsync Partial Install Result");
        }

        /// <summary>
        /// Tests successful installation with reload command execution
        /// </summary>
        [TestMethod]
        public async Task InstallCertAsync_WithReloadCommand_Success()
        {
            // Arrange
            var domain = TestConstants.Domains.SubdomainExampleCom;
            var reloadCmd = TestConstants.Commands.NginxReload;
            var installPaths = new InstallCertPaths
            {
                CertFile = "/etc/nginx/ssl/subdomain.example.com.crt",
                KeyFile = "/etc/nginx/ssl/subdomain.example.com.key",
                FullChainFile = "/etc/nginx/ssl/subdomain.example.com.fullchain.crt",
                ReloadCmd = reloadCmd
            };
            
            var outputLines = ProcessXMockHelper.CreateInstallCertSuccessOutput(installPaths);
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { 
                "--install-cert", 
                "-d subdomain.example.com",
                "--cert-file",
                "/etc/nginx/ssl/subdomain.example.com.crt",
                "--key-file",
                "/etc/nginx/ssl/subdomain.example.com.key",
                "--fullchain-file",
                "/etc/nginx/ssl/subdomain.example.com.fullchain.crt",
                "--reloadcmd",
                reloadCmd
            });
            
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new InstallCertOptions(domain)
            {
                CertFile = installPaths.CertFile,
                KeyFile = installPaths.KeyFile,
                FullChainFile = installPaths.FullChainFile,
                ReloadCmd = reloadCmd
            };

            // Act
            var result = await client.InstallCertAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(installPaths.CertFile, result.InstalledCertFile);
            Assert.AreEqual(installPaths.KeyFile, result.InstalledKeyFile);
            Assert.IsNull(result.InstalledCaFile);
            Assert.AreEqual(installPaths.FullChainFile, result.InstalledFullChainFile);
            Assert.IsTrue(result.ReloadCommandExecuted);
            Assert.IsNotNull(result.ReloadCommandOutput);
            Assert.IsTrue(result.ReloadCommandOutput.Contains("Reload success"));
            Assert.IsNotNull(result.InstalledAt);
            
            LogObject(result, "InstallCertAsync With Reload Command Result");
        }

        /// <summary>
        /// Tests installation when reload command fails
        /// </summary>
        [TestMethod]
        public async Task InstallCertAsync_ReloadCommandFails_PartialSuccess()
        {
            // Arrange
            var domain = TestConstants.Domains.ExampleCom;
            var reloadCmd = "/usr/bin/failing-reload-command";
            var installPaths = new InstallCertPaths
            {
                CertFile = "/etc/ssl/certs/example.crt",
                KeyFile = "/etc/ssl/private/example.key"
            };
            
            var outputLines = new[]
            {
                $"[Info] Installing cert to: {installPaths.CertFile}",
                $"[Info] Installing key to: {installPaths.KeyFile}",
                $"[Info] Run reload cmd: {reloadCmd}",
                "[Error] Reload command failed: Service not found"
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { 
                "--install-cert", 
                "-d example.com",
                "--cert-file /etc/ssl/certs/example.crt",
                "--key-file /etc/ssl/private/example.key",
                "--reloadcmd",
                reloadCmd
            });
            
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new InstallCertOptions(domain)
            {
                CertFile = installPaths.CertFile,
                KeyFile = installPaths.KeyFile,
                ReloadCmd = reloadCmd
            };

            // Act
            var result = await client.InstallCertAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess); // Installation succeeded even if reload failed
            Assert.AreEqual(installPaths.CertFile, result.InstalledCertFile);
            Assert.AreEqual(installPaths.KeyFile, result.InstalledKeyFile);
            Assert.IsTrue(result.ReloadCommandExecuted);
            Assert.IsNotNull(result.ReloadCommandOutput);
            Assert.IsTrue(result.ReloadCommandOutput.Contains("Reload command failed"));
            Assert.IsNotNull(result.InstalledAt);
            
            LogObject(result, "InstallCertAsync Reload Command Fails Result");
        }

        /// <summary>
        /// Tests ECC certificate installation
        /// </summary>
        [TestMethod]
        public async Task InstallCertAsync_EccCertificate_Success()
        {
            // Arrange
            var domain = TestConstants.Domains.WildcardExampleCom;
            var installPaths = new InstallCertPaths
            {
                CertFile = "/etc/ssl/certs/wildcard.example.com_ecc.crt",
                KeyFile = "/etc/ssl/private/wildcard.example.com_ecc.key",
                CaFile = "/etc/ssl/certs/wildcard.example.com_ecc_ca.crt",
                FullChainFile = "/etc/ssl/certs/wildcard.example.com_ecc_fullchain.crt"
            };
            
            var outputLines = new[]
            {
                "[Info] Using ECC certificate",
                $"[Info] Installing cert to: {installPaths.CertFile}",
                $"[Info] Installing key to: {installPaths.KeyFile}",
                $"[Info] Installing CA to: {installPaths.CaFile}",
                $"[Info] Installing full chain to: {installPaths.FullChainFile}"
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { 
                "--install-cert", 
                "-d *.example.com",
                "--ecc",
                "--cert-file /etc/ssl/certs/wildcard.example.com_ecc.crt",
                "--key-file /etc/ssl/private/wildcard.example.com_ecc.key",
                "--ca-file /etc/ssl/certs/wildcard.example.com_ecc_ca.crt",
                "--fullchain-file /etc/ssl/certs/wildcard.example.com_ecc_fullchain.crt"
            });
            
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new InstallCertOptions(domain)
            {
                Ecc = true,
                CertFile = installPaths.CertFile,
                KeyFile = installPaths.KeyFile,
                CaFile = installPaths.CaFile,
                FullChainFile = installPaths.FullChainFile
            };

            // Act
            var result = await client.InstallCertAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(installPaths.CertFile, result.InstalledCertFile);
            Assert.AreEqual(installPaths.KeyFile, result.InstalledKeyFile);
            Assert.AreEqual(installPaths.CaFile, result.InstalledCaFile);
            Assert.AreEqual(installPaths.FullChainFile, result.InstalledFullChainFile);
            Assert.IsFalse(result.ReloadCommandExecuted);
            Assert.IsNotNull(result.InstalledAt);
            
            LogObject(result, "InstallCertAsync ECC Certificate Result");
        }

        /// <summary>
        /// Tests installation with no files specified (only domain)
        /// </summary>
        [TestMethod]
        public async Task InstallCertAsync_NoFilesSpecified_Success()
        {
            // Arrange
            var domain = TestConstants.Domains.ExampleCom;
            
            var outputLines = new[]
            {
                "[Info] No installation paths specified",
                "[Info] Certificate installation completed"
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { 
                "--install-cert", 
                "-d example.com"
            });
            
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new InstallCertOptions(domain);

            // Act
            var result = await client.InstallCertAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.InstalledCertFile);
            Assert.IsNull(result.InstalledKeyFile);
            Assert.IsNull(result.InstalledCaFile);
            Assert.IsNull(result.InstalledFullChainFile);
            Assert.IsFalse(result.ReloadCommandExecuted);
            Assert.IsNotNull(result.InstalledAt);
            
            LogObject(result, "InstallCertAsync No Files Specified Result");
        }

        /// <summary>
        /// Tests installation when domain is not found
        /// </summary>
        [TestMethod]
        public async Task InstallCertAsync_DomainNotFound_Fails()
        {
            // Arrange
            var domain = "nonexistent.example.com";
            var errorOutput = new[]
            {
                $"[Error] Domain '{domain}' is not found",
                "[Error] Please check if the certificate exists for this domain"
            };
            
            CreateMockAcmeScript(errorOutput, expectedArgs: new[] { 
                "--install-cert", 
                "-d nonexistent.example.com"
            }, exitCode: 1);
            
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new InstallCertOptions(domain);

            // Act
            var result = await client.InstallCertAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.ErrorOutput);
            Assert.IsTrue(result.ErrorOutput.Any(e => e.Contains($"Domain '{domain}' is not found")));
            Assert.IsNull(result.InstalledCertFile);
            Assert.IsNull(result.InstalledKeyFile);
            Assert.IsNull(result.InstalledCaFile);
            Assert.IsNull(result.InstalledFullChainFile);
            Assert.IsFalse(result.ReloadCommandExecuted);
            Assert.IsNull(result.InstalledAt);
            
            LogObject(result, "InstallCertAsync Domain Not Found Result");
        }

        /// <summary>
        /// Tests installation when process fails
        /// </summary>
        [TestMethod]
        public async Task InstallCertAsync_ProcessFails_ReturnsFailureResult()
        {
            // Arrange
            var domain = TestConstants.Domains.ExampleCom;
            var errorOutput = new[]
            {
                $"[Error] Failed to install certificate for {domain}",
                "[Error] Permission denied: Cannot write to /etc/ssl/certs/",
                "[Error] Please run with appropriate permissions"
            };
            
            CreateMockAcmeScript(errorOutput, expectedArgs: new[] { 
                "--install-cert", 
                "-d example.com",
                "--cert-file /etc/ssl/certs/example.crt"
            }, exitCode: TestConstants.ExitCodes.PermissionError);
            
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new InstallCertOptions(domain)
            {
                CertFile = TestConstants.Paths.InstallCertPath
            };

            // Act
            var result = await client.InstallCertAsync(options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.ErrorOutput);
            Assert.IsTrue(result.ErrorOutput.Any(e => e.Contains("Permission denied")));
            Assert.IsNull(result.InstalledCertFile);
            Assert.IsNull(result.InstalledKeyFile);
            Assert.IsNull(result.InstalledCaFile);
            Assert.IsNull(result.InstalledFullChainFile);
            Assert.IsFalse(result.ReloadCommandExecuted);
            Assert.IsNull(result.InstalledAt);
            
            LogObject(result, "InstallCertAsync Process Failure Result");
        }

        /// <summary>
        /// Tests that installed file paths are parsed correctly from output
        /// </summary>
        [TestMethod]
        public async Task InstallCertAsync_ParsesInstalledPaths_Correctly()
        {
            // Arrange
            var domain = "path-test.example.com";
            var customPaths = new
            {
                Cert = "/custom/ssl/path/path-test.example.com/certificate.crt",
                Key = "/custom/ssl/path/path-test.example.com/private_key.key",
                Ca = "/custom/ssl/path/path-test.example.com/ca_bundle.crt",
                FullChain = "/custom/ssl/path/path-test.example.com/full_chain.crt"
            };
            
            var outputLines = new[]
            {
                $"[Info] Installing cert to: {customPaths.Cert}",
                $"[Info] Installing key to: {customPaths.Key}",
                $"[Info] Installing CA to: {customPaths.Ca}",
                $"[Info] Installing full chain to: {customPaths.FullChain}",
                "[Info] Run reload cmd: /usr/local/bin/custom-reload.sh",
                "[Info] Custom reload output: Service reloaded successfully",
                "[Info] Reload success"
            };
            
            CreateMockAcmeScript(outputLines, expectedArgs: new[] { 
                "--install-cert", 
                "-d path-test.example.com",
                $"--cert-file {customPaths.Cert}",
                $"--key-file {customPaths.Key}",
                $"--ca-file {customPaths.Ca}",
                $"--fullchain-file {customPaths.FullChain}",
                "--reloadcmd",
                "/usr/local/bin/custom-reload.sh"
            });
            
            var client = CreateAcmeClient(GetTestFilePath("mock-acme.sh"));
            
            var options = new InstallCertOptions(domain)
            {
                CertFile = customPaths.Cert,
                KeyFile = customPaths.Key,
                CaFile = customPaths.Ca,
                FullChainFile = customPaths.FullChain,
                ReloadCmd = "/usr/local/bin/custom-reload.sh"
            };

            // Act
            var result = await client.InstallCertAsync(options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(customPaths.Cert, result.InstalledCertFile);
            Assert.AreEqual(customPaths.Key, result.InstalledKeyFile);
            Assert.AreEqual(customPaths.Ca, result.InstalledCaFile);
            Assert.AreEqual(customPaths.FullChain, result.InstalledFullChainFile);
            Assert.IsTrue(result.ReloadCommandExecuted);
            Assert.IsNotNull(result.ReloadCommandOutput);
            Assert.IsTrue(result.ReloadCommandOutput.Contains("Custom reload output"));
            Assert.IsTrue(result.ReloadCommandOutput.Contains("Reload success"));
            Assert.IsNotNull(result.InstalledAt);
            
            LogObject(result, "InstallCertAsync Parse Paths Result");
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