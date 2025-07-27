using AcmeshWrapper.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace AcmeshWrapper.Tests
{
    /// <summary>
    /// Base class for all test classes providing common setup and utilities
    /// </summary>
    [TestClass]
    public abstract class TestBase
    {
        /// <summary>
        /// Test context for accessing test-specific information
        /// </summary>
        public TestContext TestContext { get; set; } = null!;

        /// <summary>
        /// Temporary directory for test files
        /// </summary>
        protected string TestTempDirectory { get; private set; } = string.Empty;

        /// <summary>
        /// Default AcmeClient instance for tests
        /// </summary>
        protected AcmeClient DefaultAcmeClient { get; private set; } = null!;

        /// <summary>
        /// Initializes the test class before any tests run
        /// </summary>
        [TestInitialize]
        public virtual void TestInitialize()
        {
            // Create a unique temp directory for each test
            TestTempDirectory = Path.Combine(Path.GetTempPath(), "AcmeshWrapperTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(TestTempDirectory);

            // Create default AcmeClient instance
            DefaultAcmeClient = CreateAcmeClient();

            // Log test start
            TestContext.WriteLine($"Starting test: {TestContext.TestName}");
            TestContext.WriteLine($"Test temp directory: {TestTempDirectory}");
        }

        /// <summary>
        /// Cleans up after each test
        /// </summary>
        [TestCleanup]
        public virtual void TestCleanup()
        {
            // Clean up temp directory
            if (Directory.Exists(TestTempDirectory))
            {
                try
                {
                    Directory.Delete(TestTempDirectory, recursive: true);
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Failed to delete temp directory: {ex.Message}");
                }
            }

            // Log test completion
            TestContext.WriteLine($"Completed test: {TestContext.TestName}");
        }

        /// <summary>
        /// Creates an AcmeClient instance with the specified path
        /// </summary>
        /// <param name="acmeShPath">Path to acme.sh script</param>
        /// <returns>AcmeClient instance</returns>
        protected AcmeClient CreateAcmeClient(string acmeShPath = TestConstants.Paths.TestAcmeShPath)
        {
            return new AcmeClient(acmeShPath);
        }

        /// <summary>
        /// Creates a test file in the temp directory
        /// </summary>
        /// <param name="fileName">Name of the file to create</param>
        /// <param name="content">Content to write to the file</param>
        /// <returns>Full path to the created file</returns>
        protected string CreateTestFile(string fileName, string content = "")
        {
            var filePath = Path.Combine(TestTempDirectory, fileName);
            var directory = Path.GetDirectoryName(filePath);
            
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, content);
            return filePath;
        }

        /// <summary>
        /// Creates a test directory structure
        /// </summary>
        /// <param name="relativePath">Relative path within the temp directory</param>
        /// <returns>Full path to the created directory</returns>
        protected string CreateTestDirectory(string relativePath)
        {
            var fullPath = Path.Combine(TestTempDirectory, relativePath);
            Directory.CreateDirectory(fullPath);
            return fullPath;
        }

        /// <summary>
        /// Asserts that a file exists
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="message">Optional assertion message</param>
        protected void AssertFileExists(string filePath, string? message = null)
        {
            Assert.IsTrue(File.Exists(filePath), 
                message ?? $"Expected file to exist: {filePath}");
        }

        /// <summary>
        /// Asserts that a directory exists
        /// </summary>
        /// <param name="directoryPath">Path to the directory</param>
        /// <param name="message">Optional assertion message</param>
        protected void AssertDirectoryExists(string directoryPath, string? message = null)
        {
            Assert.IsTrue(Directory.Exists(directoryPath), 
                message ?? $"Expected directory to exist: {directoryPath}");
        }

        /// <summary>
        /// Asserts that a file contains specific text
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="expectedContent">Expected content</param>
        /// <param name="message">Optional assertion message</param>
        protected void AssertFileContains(string filePath, string expectedContent, string? message = null)
        {
            AssertFileExists(filePath);
            var actualContent = File.ReadAllText(filePath);
            StringAssert.Contains(actualContent, expectedContent, 
                message ?? $"Expected file {filePath} to contain: {expectedContent}");
        }

        /// <summary>
        /// Gets a test-specific file path
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Full path within the test temp directory</returns>
        protected string GetTestFilePath(string fileName)
        {
            return Path.Combine(TestTempDirectory, fileName);
        }

        /// <summary>
        /// Logs a message to the test output
        /// </summary>
        /// <param name="message">Message to log</param>
        protected void LogMessage(string message)
        {
            TestContext.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        }

        /// <summary>
        /// Logs an object as JSON to the test output
        /// </summary>
        /// <param name="obj">Object to log</param>
        /// <param name="label">Optional label for the object</param>
        protected void LogObject(object obj, string? label = null)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(obj, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            if (!string.IsNullOrEmpty(label))
            {
                LogMessage($"{label}:");
            }
            LogMessage(json);
        }

        /// <summary>
        /// Creates a mock certificate structure in the temp directory
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <returns>Path to the certificate directory</returns>
        protected string CreateMockCertificateStructure(string domain)
        {
            var certDir = CreateTestDirectory(Path.Combine("certs", domain));
            
            CreateTestFile(Path.Combine("certs", domain, $"{domain}.cer"), "-----BEGIN CERTIFICATE-----\nMOCK CERTIFICATE\n-----END CERTIFICATE-----");
            CreateTestFile(Path.Combine("certs", domain, $"{domain}.key"), "-----BEGIN PRIVATE KEY-----\nMOCK PRIVATE KEY\n-----END PRIVATE KEY-----");
            CreateTestFile(Path.Combine("certs", domain, "ca.cer"), "-----BEGIN CERTIFICATE-----\nMOCK CA CERTIFICATE\n-----END CERTIFICATE-----");
            CreateTestFile(Path.Combine("certs", domain, "fullchain.cer"), "-----BEGIN CERTIFICATE-----\nMOCK FULLCHAIN CERTIFICATE\n-----END CERTIFICATE-----");
            
            return certDir;
        }
    }
}