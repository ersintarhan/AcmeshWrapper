using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AcmeshWrapper;
using AcmeshWrapper.Options;
using AcmeshWrapper.Results;

namespace TestConsole
{
    class Program
    {
        private static AcmeClient _acmeClient;
        
        static async Task Main(string[] args)
        {
            // AcmeClient instance'ı oluştur
            var acmeShPath = "/Users/ersin/.acme.sh/acme.sh";
            _acmeClient = new AcmeClient(acmeShPath);

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
    _    ____ __  __ _____    ____  _   _ 
   / \  / ___|  \/  | ____|  / ___|| | | |
  / _ \| |   | |\/| |  _|    \___ \| |_| |
 / ___ \ |___| |  | | |___    ___) |  _  |
/_/   \_\____|_|  |_|_____|  |____/|_| |_|
                                           
        ");
            Console.ResetColor();
            
            Console.WriteLine("=== AcmeshWrapper Test Console ===");
            Console.WriteLine($"Using acme.sh at: {acmeShPath}");
            Console.WriteLine();

            bool exit = false;
            while (!exit)
            {
                ShowMenu();
                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            await TestListCommand();
                            break;
                        case "2":
                            await TestRemoveCommand();
                            break;
                        case "3":
                            await TestIssueCommand();
                            break;
                        case "4":
                            await TestRenewCommand();
                            break;
                        case "5":
                            await TestInfoCommand();
                            break;
                        case "6":
                            await TestGetCertificateCommand();
                            break;
                        case "7":
                            exit = true;
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nInvalid choice! Please select a valid option.");
                            Console.ResetColor();
                            break;
                    }

                    if (!exit)
                    {
                        Console.WriteLine("\nPress Enter to continue...");
                        Console.ReadLine();
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                    Console.WriteLine("\nPress Enter to continue...");
                    Console.ReadLine();
                }
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nThank you for using AcmeshWrapper Test Console!");
            Console.WriteLine("Goodbye!");
            Console.ResetColor();
        }

        private static void ShowMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔═══════════════════════════════════╗");
            Console.WriteLine("║         MAIN MENU                 ║");
            Console.WriteLine("╠═══════════════════════════════════╣");
            Console.WriteLine("║  1. List certificates             ║");
            Console.WriteLine("║  2. Remove certificate            ║");
            Console.WriteLine("║  3. Issue new certificate         ║");
            Console.WriteLine("║  4. Renew certificate             ║");
            Console.WriteLine("║  5. Certificate info              ║");
            Console.WriteLine("║  6. Get certificate contents      ║");
            Console.WriteLine("║  7. Exit                          ║");
            Console.WriteLine("╚═══════════════════════════════════╝");
            Console.ResetColor();
            Console.Write("\nPlease select an option (1-7): ");
        }

        private static async Task TestListCommand()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== LIST CERTIFICATES ===");
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Listing all certificates...");
            Console.WriteLine("-".PadRight(60, '-'));

            var listOptions = new ListOptions();
            var listResult = await _acmeClient.ListAsync(listOptions);

            if (listResult.IsSuccess)
            {
                if (listResult.Certificates.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("No certificates found.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Found {listResult.Certificates.Count} certificate(s):");
                    Console.ResetColor();
                    Console.WriteLine();

                    int certNumber = 1;
                    foreach (var cert in listResult.Certificates)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"Certificate #{certNumber}");
                        Console.ResetColor();
                        
                        Console.WriteLine($"  Main Domain:    {cert.Le_Main ?? "(not set)"}");
                        Console.WriteLine($"  CA:             {cert.CA ?? "(not set)"}");
                        Console.WriteLine($"  Key Length:     {cert.Le_Keylength ?? "(not set)"}");
                        Console.WriteLine($"  SAN Domains:    {cert.Le_SAN ?? "(not set)"}");
                        Console.WriteLine($"  Created:        {cert.Le_Created_Time ?? "(not set)"}");
                        Console.WriteLine($"  Next Renewal:   {cert.Le_Next_Renew_Time ?? "(not set)"}");
                        Console.WriteLine("-".PadRight(60, '-'));
                        certNumber++;
                    }
                }
            }
            else
            {
                DisplayError("Failed to list certificates!", listResult);
            }
        }

        private static async Task TestIssueCommand()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== ISSUE NEW CERTIFICATE ===");
            Console.ResetColor();
            Console.WriteLine();

            // 1. Domain adını al
            Console.Write("Enter the domain name (e.g., example.com): ");
            var domain = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrEmpty(domain))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Domain name cannot be empty!");
                Console.ResetColor();
                return;
            }

            var domains = new List<string> { domain };

            // 2. Wildcard sertifikası sor
            Console.WriteLine();
            Console.Write("Do you want to include wildcard certificate? (y/n): ");
            var wildcardChoice = Console.ReadLine()?.Trim().ToLower();
            
            if (wildcardChoice == "y" || wildcardChoice == "yes")
            {
                domains.Add($"*.{domain}");
            }

            // 3. DNS Provider seçimi
            Console.WriteLine();
            Console.WriteLine("Select DNS Provider:");
            Console.WriteLine("  1. Cloudflare (dns_cf)");
            Console.WriteLine("  2. Route53 (dns_aws)");
            Console.WriteLine("  3. DigitalOcean (dns_dgon)");
            Console.WriteLine("  4. GoDaddy (dns_gd)");
            Console.WriteLine("  5. Google Cloud DNS (dns_gcloud)");
            Console.WriteLine("  6. Azure DNS (dns_azure)");
            Console.WriteLine("  7. Manual DNS (dns_manual)");
            Console.WriteLine("  8. Other (custom)");
            Console.Write("\nSelect provider (1-8): ");
            
            var providerChoice = Console.ReadLine()?.Trim();
            string dnsProvider = "";
            
            switch (providerChoice)
            {
                case "1": dnsProvider = "dns_cf"; break;
                case "2": dnsProvider = "dns_aws"; break;
                case "3": dnsProvider = "dns_dgon"; break;
                case "4": dnsProvider = "dns_gd"; break;
                case "5": dnsProvider = "dns_gcloud"; break;
                case "6": dnsProvider = "dns_azure"; break;
                case "7": dnsProvider = "dns_manual"; break;
                case "8":
                    Console.Write("Enter custom DNS provider (e.g., dns_myapi): ");
                    dnsProvider = Console.ReadLine()?.Trim() ?? "";
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid provider selection!");
                    Console.ResetColor();
                    return;
            }

            // 4. Key type seçimi
            Console.WriteLine();
            Console.WriteLine("Select Key Type:");
            Console.WriteLine("  1. RSA 2048 (2048)");
            Console.WriteLine("  2. RSA 4096 (4096) - Recommended");
            Console.WriteLine("  3. ECC 256 (ec-256) - Modern & Fast");
            Console.WriteLine("  4. ECC 384 (ec-384) - High Security");
            Console.Write("\nSelect key type (1-4): ");
            
            var keyChoice = Console.ReadLine()?.Trim();
            string keyLength = "";
            
            switch (keyChoice)
            {
                case "1": keyLength = "2048"; break;
                case "2": keyLength = "4096"; break;
                case "3": keyLength = "ec-256"; break;
                case "4": keyLength = "ec-384"; break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid key type selection!");
                    Console.ResetColor();
                    return;
            }

            // 5. Staging/Production seçimi
            Console.WriteLine();
            Console.WriteLine("Select Environment:");
            Console.WriteLine("  1. Staging (Test) - Recommended for testing");
            Console.WriteLine("  2. Production (Live) - Real certificate");
            Console.Write("\nSelect environment (1-2): ");
            
            var envChoice = Console.ReadLine()?.Trim();
            bool staging = false;
            
            switch (envChoice)
            {
                case "1": staging = true; break;
                case "2": staging = false; break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid environment selection!");
                    Console.ResetColor();
                    return;
            }

            // 6. CA Provider seçimi
            Console.WriteLine();
            Console.WriteLine("Select CA Provider:");
            Console.WriteLine("  1. Let's Encrypt (Recommended)");
            Console.WriteLine("  2. ZeroSSL");
            Console.WriteLine("  3. Buypass");
            Console.Write("\nSelect CA provider (1-3): ");
            
            var caChoice = Console.ReadLine()?.Trim();
            string? caServer = null;
            string caProviderName = "";
            
            switch (caChoice)
            {
                case "1": 
                    caServer = "letsencrypt";
                    caProviderName = "Let's Encrypt";
                    break;
                case "2": 
                    caServer = null; // ZeroSSL is the default when no server is specified
                    caProviderName = "ZeroSSL";
                    break;
                case "3": 
                    caServer = "buypass";
                    caProviderName = "Buypass";
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid CA provider selection!");
                    Console.ResetColor();
                    return;
            }

            // 7. Özet göster ve onay al
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== CONFIGURATION SUMMARY ===");
            Console.ResetColor();
            Console.WriteLine($"  Domains:      {string.Join(", ", domains)}");
            Console.WriteLine($"  DNS Provider: {dnsProvider}");
            Console.WriteLine($"  Key Type:     {keyLength}");
            Console.WriteLine($"  Environment:  {(staging ? "Staging (Test)" : "Production (Live)")}");
            Console.WriteLine($"  CA Provider:  {caProviderName}");
            Console.WriteLine();
            
            if (!string.IsNullOrEmpty(dnsProvider) && dnsProvider != "dns_manual")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Note: Make sure {dnsProvider} API credentials are configured in ~/.acme.sh/acme.sh.env");
                Console.ResetColor();
                Console.WriteLine();
            }

            Console.Write("Do you want to proceed with this configuration? (yes/no): ");
            var confirmation = Console.ReadLine()?.Trim().ToLower();
            
            if (confirmation != "yes" && confirmation != "y")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Operation cancelled.");
                Console.ResetColor();
                return;
            }

            // 8. Sertifika oluştur
            Console.WriteLine();
            Console.WriteLine("Issuing certificate...");
            Console.WriteLine("-".PadRight(60, '-'));

            var options = new IssueOptions
            {
                Domains = domains,
                DnsProvider = dnsProvider,
                KeyLength = keyLength,
                Staging = staging,
                Server = caServer
            };

            try
            {
                var issueResult = await _acmeClient.IssueAsync(options);

                if (issueResult.IsSuccess)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n✓ Certificate issued successfully!");
                    Console.ResetColor();
                    Console.WriteLine();

                    // Display certificate paths
                    if (!string.IsNullOrEmpty(issueResult.CertificateFile))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Certificate Information:");
                        Console.ResetColor();
                        Console.WriteLine($"  Certificate: {issueResult.CertificateFile}");
                    }
                    
                    if (!string.IsNullOrEmpty(issueResult.KeyFile))
                    {
                        Console.WriteLine($"  Private Key: {issueResult.KeyFile}");
                    }
                    
                    if (!string.IsNullOrEmpty(issueResult.CaFile))
                    {
                        Console.WriteLine($"  CA Bundle:   {issueResult.CaFile}");
                    }
                    
                    if (!string.IsNullOrEmpty(issueResult.FullChainFile))
                    {
                        Console.WriteLine($"  Full Chain:  {issueResult.FullChainFile}");
                    }

                    // Show raw output if needed
                    if (!string.IsNullOrEmpty(issueResult.RawOutput))
                    {
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("Raw Output:");
                        Console.WriteLine(issueResult.RawOutput);
                        Console.ResetColor();
                    }
                }
                else
                {
                    DisplayError("Failed to issue certificate!", issueResult);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private static async Task TestRenewCommand()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== RENEW CERTIFICATE ===");
            Console.ResetColor();
            Console.WriteLine();

            // 1. List existing certificates
            Console.WriteLine("Fetching available certificates for renewal...");
            var listOptions = new ListOptions();
            var listResult = await _acmeClient.ListAsync(listOptions);

            if (!listResult.IsSuccess || listResult.Certificates.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No certificates available to renew.");
                Console.ResetColor();
                return;
            }

            // Display available certificates with next renewal date
            Console.WriteLine("\nAvailable certificates:");
            Console.WriteLine("-".PadRight(80, '-'));
            
            for (int i = 0; i < listResult.Certificates.Count; i++)
            {
                var cert = listResult.Certificates[i];
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{i + 1}. ");
                Console.ResetColor();
                Console.Write($"{cert.Le_Main ?? "Unknown domain"}");
                
                if (!string.IsNullOrEmpty(cert.Le_Next_Renew_Time))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($" (Next renewal: {cert.Le_Next_Renew_Time})");
                    Console.ResetColor();
                }
                Console.WriteLine();
            }

            // 2. Get domain to renew
            Console.WriteLine();
            Console.Write("Enter the number or domain name to renew (or 'cancel' to go back): ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input) || input.Equals("cancel", StringComparison.OrdinalIgnoreCase))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Operation cancelled.");
                Console.ResetColor();
                return;
            }

            string selectedDomain = "";
            
            // Check if input is a number
            if (int.TryParse(input, out int certNumber) && certNumber > 0 && certNumber <= listResult.Certificates.Count)
            {
                selectedDomain = listResult.Certificates[certNumber - 1].Le_Main ?? "";
            }
            else
            {
                // Treat input as domain name
                selectedDomain = input;
                
                // Verify domain exists in our certificate list
                bool domainExists = listResult.Certificates.Any(c => 
                    c.Le_Main?.Equals(selectedDomain, StringComparison.OrdinalIgnoreCase) == true);
                
                if (!domainExists)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nDomain '{selectedDomain}' not found in certificate list!");
                    Console.ResetColor();
                    return;
                }
            }

            // 3. Force renewal option
            Console.WriteLine();
            Console.WriteLine($"Selected domain: {selectedDomain}");
            Console.WriteLine();
            Console.Write("Force renewal even if not due? (y/n) [default: n]: ");
            var forceChoice = Console.ReadLine()?.Trim().ToLower();
            bool forceRenewal = (forceChoice == "y" || forceChoice == "yes");

            // 4. CA Provider selection
            Console.WriteLine();
            Console.WriteLine("Select CA Provider:");
            Console.WriteLine("  1. Keep current CA provider");
            Console.WriteLine("  2. Let's Encrypt");
            Console.WriteLine("  3. ZeroSSL");
            Console.WriteLine("  4. Buypass");
            Console.Write("\nSelect CA provider (1-4) [default: 1]: ");
            
            var caChoice = Console.ReadLine()?.Trim();
            string? caServer = null;
            string caProviderName = "Current";
            
            switch (caChoice)
            {
                case "2": 
                    caServer = "letsencrypt";
                    caProviderName = "Let's Encrypt";
                    break;
                case "3": 
                    caServer = null; // ZeroSSL is the default when no server is specified
                    caProviderName = "ZeroSSL";
                    break;
                case "4": 
                    caServer = "buypass";
                    caProviderName = "Buypass";
                    break;
                case "1":
                case "":
                default:
                    // Keep current CA provider
                    caServer = null;
                    break;
            }

            // Check if this is an ECC certificate
            var selectedCert = listResult.Certificates.FirstOrDefault(c => 
                c.Le_Main?.Equals(selectedDomain, StringComparison.OrdinalIgnoreCase) == true);
            bool isEcc = selectedCert?.Le_Keylength?.Contains("ec-", StringComparison.OrdinalIgnoreCase) == true;

            // 5. Configuration summary
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== CONFIGURATION SUMMARY ===");
            Console.ResetColor();
            Console.WriteLine($"  Domain:       {selectedDomain}");
            Console.WriteLine($"  Force Renew:  {(forceRenewal ? "Yes" : "No")}");
            Console.WriteLine($"  CA Provider:  {caProviderName}");
            if (isEcc)
            {
                Console.WriteLine($"  Certificate:  ECC (Elliptic Curve)");
            }
            Console.WriteLine();

            Console.Write("Do you want to proceed with renewal? (yes/no): ");
            var confirmation = Console.ReadLine()?.Trim().ToLower();
            
            if (confirmation != "yes" && confirmation != "y")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Operation cancelled.");
                Console.ResetColor();
                return;
            }

            // 6. Renew certificate
            Console.WriteLine();
            Console.WriteLine("Renewing certificate...");
            Console.WriteLine("-".PadRight(60, '-'));

            var renewOptions = new RenewOptions(selectedDomain)
            {
                Force = forceRenewal,
                Ecc = isEcc,
                Server = caServer
            };

            try
            {
                var renewResult = await _acmeClient.RenewAsync(renewOptions);

                if (renewResult.IsSuccess)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n✓ Certificate renewed successfully!");
                    Console.ResetColor();
                    Console.WriteLine();

                    // Display certificate paths
                    if (!string.IsNullOrEmpty(renewResult.CertificatePath))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("New Certificate Information:");
                        Console.ResetColor();
                        Console.WriteLine($"  Certificate: {renewResult.CertificatePath}");
                    }
                    
                    if (!string.IsNullOrEmpty(renewResult.KeyPath))
                    {
                        Console.WriteLine($"  Private Key: {renewResult.KeyPath}");
                    }
                    
                    if (!string.IsNullOrEmpty(renewResult.CaPath))
                    {
                        Console.WriteLine($"  CA Bundle:   {renewResult.CaPath}");
                    }
                    
                    if (!string.IsNullOrEmpty(renewResult.FullChainPath))
                    {
                        Console.WriteLine($"  Full Chain:  {renewResult.FullChainPath}");
                    }

                    if (renewResult.RenewedAt.HasValue)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"  Renewed at:  {renewResult.RenewedAt.Value:yyyy-MM-dd HH:mm:ss}");
                    }

                    // Show raw output if needed
                    if (!string.IsNullOrEmpty(renewResult.RawOutput))
                    {
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("Raw Output:");
                        Console.WriteLine(renewResult.RawOutput);
                        Console.ResetColor();
                    }
                }
                else
                {
                    DisplayError("Failed to renew certificate!", renewResult);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private static async Task TestRemoveCommand()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== REMOVE CERTIFICATE ===");
            Console.ResetColor();
            Console.WriteLine();

            // Önce mevcut sertifikaları listele
            Console.WriteLine("Fetching available certificates...");
            var listOptions = new ListOptions();
            var listResult = await _acmeClient.ListAsync(listOptions);

            if (!listResult.IsSuccess || listResult.Certificates.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No certificates available to remove.");
                Console.ResetColor();
                return;
            }

            // Mevcut sertifikaları göster
            Console.WriteLine("\nAvailable certificates:");
            Console.WriteLine("-".PadRight(40, '-'));
            
            for (int i = 0; i < listResult.Certificates.Count; i++)
            {
                var cert = listResult.Certificates[i];
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{i + 1}. ");
                Console.ResetColor();
                Console.WriteLine($"{cert.Le_Main ?? "Unknown domain"}");
            }

            // Kullanıcıdan domain adını al
            Console.WriteLine();
            Console.Write("Enter the domain name to remove (or 'cancel' to go back): ");
            var domain = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(domain) || domain.Equals("cancel", StringComparison.OrdinalIgnoreCase))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Operation cancelled.");
                Console.ResetColor();
                return;
            }

            // Onay iste
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"WARNING: This will remove the certificate for '{domain}'");
            Console.ResetColor();
            Console.Write("Are you sure? (yes/no): ");
            
            var confirmation = Console.ReadLine()?.Trim().ToLower();
            if (confirmation != "yes" && confirmation != "y")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Operation cancelled.");
                Console.ResetColor();
                return;
            }

            // Sertifikayı sil
            Console.WriteLine();
            Console.WriteLine($"Removing certificate for '{domain}'...");
            
            var removeOptions = new RemoveOptions(domain);

            var removeResult = await _acmeClient.RemoveAsync(removeOptions);

            if (removeResult.IsSuccess)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ Certificate for '{domain}' has been successfully removed!");
                Console.ResetColor();
                
                // Show certificate path if available
                if (!string.IsNullOrEmpty(removeResult.CertificatePath))
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠️  Important Notice:");
                    Console.ResetColor();
                    Console.WriteLine($"   Certificate files were located at: {removeResult.CertificatePath}");
                    Console.WriteLine("   The files are still on disk and must be manually deleted if needed.");
                    Console.WriteLine("   This is by design to prevent accidental data loss.");
                }
                
                if (!string.IsNullOrEmpty(removeResult.RawOutput))
                {
                    Console.WriteLine("\nOutput:");
                    Console.WriteLine(removeResult.RawOutput);
                }
                
                // Show updated certificate list
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Updated certificate list:");
                Console.ResetColor();
                Console.WriteLine("-".PadRight(60, '-'));
                
                var updatedListResult = await _acmeClient.ListAsync(new ListOptions());
                if (updatedListResult.IsSuccess)
                {
                    if (updatedListResult.Certificates.Count == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("No certificates remaining.");
                        Console.ResetColor();
                    }
                    else
                    {
                        foreach (var cert in updatedListResult.Certificates)
                        {
                            Console.WriteLine($"• {cert.Le_Main ?? "Unknown domain"} ({cert.CA ?? "Unknown CA"})");
                        }
                    }
                }
            }
            else
            {
                DisplayError($"Failed to remove certificate for '{domain}'!", removeResult);
            }
        }

        private static async Task TestInfoCommand()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== CERTIFICATE INFO ===");
            Console.ResetColor();
            Console.WriteLine();

            // 1. List existing certificates
            Console.WriteLine("Fetching available certificates...");
            var listOptions = new ListOptions();
            var listResult = await _acmeClient.ListAsync(listOptions);

            if (!listResult.IsSuccess || listResult.Certificates.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No certificates available.");
                Console.ResetColor();
                return;
            }

            // Display available certificates
            Console.WriteLine("\nAvailable certificates:");
            Console.WriteLine("-".PadRight(60, '-'));
            
            for (int i = 0; i < listResult.Certificates.Count; i++)
            {
                var cert = listResult.Certificates[i];
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{i + 1}. ");
                Console.ResetColor();
                Console.Write($"{cert.Le_Main ?? "Unknown domain"}");
                
                if (!string.IsNullOrEmpty(cert.CA))
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($" ({cert.CA})");
                    Console.ResetColor();
                }
                Console.WriteLine();
            }

            // 2. Get domain to show info for
            Console.WriteLine();
            Console.Write("Enter the number or domain name to get info for (or 'cancel' to go back): ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input) || input.Equals("cancel", StringComparison.OrdinalIgnoreCase))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Operation cancelled.");
                Console.ResetColor();
                return;
            }

            string selectedDomain = "";
            
            // Check if input is a number
            if (int.TryParse(input, out int certNumber) && certNumber > 0 && certNumber <= listResult.Certificates.Count)
            {
                selectedDomain = listResult.Certificates[certNumber - 1].Le_Main ?? "";
            }
            else
            {
                // Treat input as domain name
                selectedDomain = input;
                
                // Verify domain exists in our certificate list
                bool domainExists = listResult.Certificates.Any(c => 
                    c.Le_Main?.Equals(selectedDomain, StringComparison.OrdinalIgnoreCase) == true);
                
                if (!domainExists)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nDomain '{selectedDomain}' not found in certificate list!");
                    Console.ResetColor();
                    return;
                }
            }

            // 3. Get certificate info
            Console.WriteLine();
            Console.WriteLine($"Getting info for '{selectedDomain}'...");
            Console.WriteLine("-".PadRight(60, '-'));

            try
            {
                var infoResult = await _acmeClient.InfoAsync(new InfoOptions { Domain = selectedDomain });

                if (infoResult.IsSuccess)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n✓ Certificate information retrieved successfully!");
                    Console.ResetColor();
                    Console.WriteLine();

                    // Display Certificate Details
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("CERTIFICATE DETAILS");
                    Console.ResetColor();
                    Console.WriteLine($"  Domain:          {infoResult.Domain ?? "(not set)"}");
                    
                    if (!string.IsNullOrEmpty(infoResult.AltNames))
                    {
                        Console.WriteLine($"  Alt Names (SAN): {infoResult.AltNames}");
                    }
                    
                    if (!string.IsNullOrEmpty(infoResult.DomainConfigPath))
                    {
                        Console.WriteLine($"  Config Path:     {infoResult.DomainConfigPath}");
                    }
                    Console.WriteLine();

                    // Display CA Information
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("CA INFORMATION");
                    Console.ResetColor();
                    
                    if (!string.IsNullOrEmpty(infoResult.ApiEndpoint))
                    {
                        Console.WriteLine($"  API Endpoint:    {infoResult.ApiEndpoint}");
                    }
                    
                    if (!string.IsNullOrEmpty(infoResult.KeyLength))
                    {
                        Console.WriteLine($"  Key Type:        {infoResult.KeyLength}");
                    }
                    
                    if (!string.IsNullOrEmpty(infoResult.Webroot))
                    {
                        Console.WriteLine($"  Validation:      {infoResult.Webroot}");
                    }
                    Console.WriteLine();

                    // Display Important Dates
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("IMPORTANT DATES");
                    Console.ResetColor();
                    
                    if (!string.IsNullOrEmpty(infoResult.CertCreateTimeStr))
                    {
                        Console.WriteLine($"  Created:         {infoResult.CertCreateTimeStr}");
                    }
                    else if (infoResult.CertCreateTime.HasValue)
                    {
                        var createdDate = DateTimeOffset.FromUnixTimeSeconds(infoResult.CertCreateTime.Value).LocalDateTime;
                        Console.WriteLine($"  Created:         {createdDate:yyyy-MM-dd HH:mm:ss}");
                    }
                    
                    if (!string.IsNullOrEmpty(infoResult.NextRenewTimeStr))
                    {
                        Console.WriteLine($"  Next Renewal:    {infoResult.NextRenewTimeStr}");
                        
                        // Calculate days until renewal
                        if (infoResult.NextRenewTime.HasValue)
                        {
                            var renewalDate = DateTimeOffset.FromUnixTimeSeconds(infoResult.NextRenewTime.Value).LocalDateTime;
                            var daysUntilRenewal = (renewalDate - DateTime.Now).Days;
                            
                            Console.ForegroundColor = daysUntilRenewal <= 30 ? ConsoleColor.Yellow : ConsoleColor.Green;
                            Console.WriteLine($"  Days Until:      {daysUntilRenewal} days");
                            Console.ResetColor();
                        }
                    }
                    else if (infoResult.NextRenewTime.HasValue)
                    {
                        var renewalDate = DateTimeOffset.FromUnixTimeSeconds(infoResult.NextRenewTime.Value).LocalDateTime;
                        Console.WriteLine($"  Next Renewal:    {renewalDate:yyyy-MM-dd HH:mm:ss}");
                        
                        var daysUntilRenewal = (renewalDate - DateTime.Now).Days;
                        Console.ForegroundColor = daysUntilRenewal <= 30 ? ConsoleColor.Yellow : ConsoleColor.Green;
                        Console.WriteLine($"  Days Until:      {daysUntilRenewal} days");
                        Console.ResetColor();
                    }
                    Console.WriteLine();

                    // Display URLs if available
                    if (!string.IsNullOrEmpty(infoResult.OrderFinalizeUrl) || 
                        !string.IsNullOrEmpty(infoResult.LinkOrderUrl) || 
                        !string.IsNullOrEmpty(infoResult.LinkCertUrl))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("ACME URLs");
                        Console.ResetColor();
                        
                        if (!string.IsNullOrEmpty(infoResult.OrderFinalizeUrl))
                        {
                            Console.WriteLine($"  Order Finalize:  {infoResult.OrderFinalizeUrl}");
                        }
                        
                        if (!string.IsNullOrEmpty(infoResult.LinkOrderUrl))
                        {
                            Console.WriteLine($"  Link Order:      {infoResult.LinkOrderUrl}");
                        }
                        
                        if (!string.IsNullOrEmpty(infoResult.LinkCertUrl))
                        {
                            Console.WriteLine($"  Link Cert:       {infoResult.LinkCertUrl}");
                        }
                        Console.WriteLine();
                    }

                    // Display Hooks if available
                    if (!string.IsNullOrEmpty(infoResult.PreHook) || 
                        !string.IsNullOrEmpty(infoResult.PostHook) || 
                        !string.IsNullOrEmpty(infoResult.RenewHook))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("HOOKS");
                        Console.ResetColor();
                        
                        if (!string.IsNullOrEmpty(infoResult.PreHook))
                        {
                            Console.WriteLine($"  Pre-Hook:        {infoResult.PreHook}");
                        }
                        
                        if (!string.IsNullOrEmpty(infoResult.PostHook))
                        {
                            Console.WriteLine($"  Post-Hook:       {infoResult.PostHook}");
                        }
                        
                        if (!string.IsNullOrEmpty(infoResult.RenewHook))
                        {
                            Console.WriteLine($"  Renew-Hook:      {infoResult.RenewHook}");
                        }
                        Console.WriteLine();
                    }

                    // Show raw output if in debug mode
                    #if DEBUG
                    if (!string.IsNullOrEmpty(infoResult.RawOutput))
                    {
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("Raw Output:");
                        Console.WriteLine(infoResult.RawOutput);
                        Console.ResetColor();
                    }
                    #endif
                }
                else
                {
                    DisplayError($"Failed to get info for '{selectedDomain}'!", infoResult);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private static async Task TestGetCertificateCommand()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== GET CERTIFICATE CONTENTS ===");
            Console.ResetColor();
            Console.WriteLine();

            // 1. List existing certificates
            Console.WriteLine("Fetching available certificates...");
            var listOptions = new ListOptions();
            var listResult = await _acmeClient.ListAsync(listOptions);

            if (!listResult.IsSuccess || listResult.Certificates.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No certificates available.");
                Console.ResetColor();
                return;
            }

            // Display available certificates
            Console.WriteLine("\nAvailable certificates:");
            Console.WriteLine("-".PadRight(60, '-'));
            
            for (int i = 0; i < listResult.Certificates.Count; i++)
            {
                var cert = listResult.Certificates[i];
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{i + 1}. ");
                Console.ResetColor();
                Console.Write($"{cert.Le_Main ?? "Unknown domain"}");
                
                if (!string.IsNullOrEmpty(cert.CA))
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($" ({cert.CA})");
                    Console.ResetColor();
                }
                Console.WriteLine();
            }

            // 2. Get domain to read certificate for
            Console.WriteLine();
            Console.Write("Enter the number or domain name to get certificate for (or 'cancel' to go back): ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input) || input.Equals("cancel", StringComparison.OrdinalIgnoreCase))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Operation cancelled.");
                Console.ResetColor();
                return;
            }

            string selectedDomain = "";
            
            // Check if input is a number
            if (int.TryParse(input, out int certNumber) && certNumber > 0 && certNumber <= listResult.Certificates.Count)
            {
                selectedDomain = listResult.Certificates[certNumber - 1].Le_Main ?? "";
            }
            else
            {
                // Treat input as domain name
                selectedDomain = input;
                
                // Verify domain exists in our certificate list
                bool domainExists = listResult.Certificates.Any(c => 
                    c.Le_Main?.Equals(selectedDomain, StringComparison.OrdinalIgnoreCase) == true);
                
                if (!domainExists)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nDomain '{selectedDomain}' not found in certificate list!");
                    Console.ResetColor();
                    return;
                }
            }

            // 3. Ask which files to read
            Console.WriteLine();
            Console.WriteLine("What would you like to read?");
            Console.WriteLine("  1. Certificate only");
            Console.WriteLine("  2. Certificate + Private Key");
            Console.WriteLine("  3. Certificate + Full Chain");
            Console.WriteLine("  4. All files (cert, key, fullchain, ca)");
            Console.Write("\nSelect option (1-4): ");
            
            var fileChoice = Console.ReadLine()?.Trim();
            var options = new GetCertificateOptions { Domain = selectedDomain };
            
            switch (fileChoice)
            {
                case "1":
                    // Certificate is always included
                    break;
                case "2":
                    options.IncludeKey = true;
                    break;
                case "3":
                    options.IncludeFullChain = true;
                    break;
                case "4":
                    options.IncludeKey = true;
                    options.IncludeFullChain = true;
                    options.IncludeCa = true;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid option!");
                    Console.ResetColor();
                    return;
            }

            // 4. Get certificate contents
            Console.WriteLine();
            Console.WriteLine($"Reading certificate files for '{selectedDomain}'...");
            Console.WriteLine("-".PadRight(60, '-'));

            try
            {
                var certResult = await _acmeClient.GetCertificateAsync(options);

                if (certResult.IsSuccess)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n✓ Certificate files retrieved successfully!");
                    Console.ResetColor();
                    Console.WriteLine();

                    // Display Certificate content
                    if (!string.IsNullOrEmpty(certResult.Certificate))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("CERTIFICATE CONTENT:");
                        Console.ResetColor();
                        DisplayTruncatedPem(certResult.Certificate);
                        Console.WriteLine();
                    }

                    // Display Private Key content
                    if (!string.IsNullOrEmpty(certResult.PrivateKey))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("PRIVATE KEY CONTENT:");
                        Console.ResetColor();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("⚠️  WARNING: This is your private key. Keep it secure!");
                        Console.ResetColor();
                        DisplayTruncatedPem(certResult.PrivateKey);
                        Console.WriteLine();
                    }

                    // Display Full Chain content
                    if (!string.IsNullOrEmpty(certResult.FullChain))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("FULL CHAIN CONTENT:");
                        Console.ResetColor();
                        DisplayTruncatedPem(certResult.FullChain);
                        Console.WriteLine();
                    }

                    // Display CA Bundle content
                    if (!string.IsNullOrEmpty(certResult.CaBundle))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("CA BUNDLE CONTENT:");
                        Console.ResetColor();
                        DisplayTruncatedPem(certResult.CaBundle);
                        Console.WriteLine();
                    }

                    // Display file paths
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("FILE PATHS:");
                    Console.ResetColor();
                    
                    if (!string.IsNullOrEmpty(certResult.CertificatePath))
                    {
                        Console.WriteLine($"  Certificate: {certResult.CertificatePath}");
                    }
                    
                    if (!string.IsNullOrEmpty(certResult.KeyPath))
                    {
                        Console.WriteLine($"  Private Key: {certResult.KeyPath}");
                    }
                    
                    if (!string.IsNullOrEmpty(certResult.FullChainPath))
                    {
                        Console.WriteLine($"  Full Chain:  {certResult.FullChainPath}");
                    }
                    
                    if (!string.IsNullOrEmpty(certResult.CaPath))
                    {
                        Console.WriteLine($"  CA Bundle:   {certResult.CaPath}");
                    }
                }
                else
                {
                    DisplayError($"Failed to get certificate contents for '{selectedDomain}'!", certResult);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private static void DisplayTruncatedPem(string pemContent)
        {
            var lines = pemContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            if (lines.Length <= 10)
            {
                // If content is short, display all
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(pemContent);
                Console.ResetColor();
            }
            else
            {
                // Display first 3 lines
                Console.ForegroundColor = ConsoleColor.DarkGray;
                for (int i = 0; i < 3 && i < lines.Length; i++)
                {
                    Console.WriteLine(lines[i]);
                }
                
                // Display omission message
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"... ({lines.Length - 6} lines omitted) ...");
                
                // Display last 3 lines
                Console.ForegroundColor = ConsoleColor.DarkGray;
                for (int i = lines.Length - 3; i < lines.Length; i++)
                {
                    if (i >= 0)
                    {
                        Console.WriteLine(lines[i]);
                    }
                }
                Console.ResetColor();
            }
        }

        private static void DisplayError(string message, AcmeResult result)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n✗ {message}");
            Console.ResetColor();
            
            if (result.ErrorOutput != null && result.ErrorOutput.Length > 0)
            {
                Console.WriteLine("\nError Output:");
                foreach (var error in result.ErrorOutput)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  {error}");
                    Console.ResetColor();
                }
            }
            
            if (!string.IsNullOrEmpty(result.RawOutput))
            {
                Console.WriteLine("\nRaw Output:");
                Console.WriteLine(result.RawOutput);
            }
        }

        private static void HandleException(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n✗ An error occurred: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            
            Console.ResetColor();
            
            #if DEBUG
            Console.WriteLine($"\nStack Trace: {ex.StackTrace}");
            #endif
        }
    }
}