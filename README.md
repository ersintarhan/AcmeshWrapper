# AcmeshWrapper

[English](#english) | [Türkçe](#türkçe)

## English

### Description

AcmeshWrapper is a modern .NET library that provides a type-safe, async wrapper around the popular [acme.sh](https://github.com/acmesh-official/acme.sh) ACME protocol client. It simplifies the process of obtaining, renewing, and managing SSL/TLS certificates from Let's Encrypt and other ACME-compliant certificate authorities in your .NET applications.

### Features

- **Type-safe API**: Strongly-typed options and results for all acme.sh commands
- **Async/await support**: All operations are fully asynchronous
- **Comprehensive command coverage**:
  - `list` - List all certificates
  - `issue` - Issue new certificates
  - `renew` - Renew specific certificates
  - `renew-all` - Renew all certificates due for renewal
  - `install-cert` - Install certificates to specific locations
  - `revoke` - Revoke certificates
  - `remove` - Remove certificates from acme.sh management
  - `info` - Get detailed certificate information
  - `get-certificate` - Retrieve certificate file contents
- **Error handling**: Detailed error information with structured results
- **Cross-platform**: Works on Windows, Linux, and macOS
- **Minimal dependencies**: Built on ProcessX for reliable process execution

### Requirements

- .NET Standard 2.1 or .NET 9.0
- [acme.sh](https://github.com/acmesh-official/acme.sh) installed and accessible in PATH
- Appropriate permissions for certificate operations

### Installation

```bash
# Install via NuGet
dotnet add package AcmeshWrapper

# Or add project reference
dotnet add reference path/to/AcmeshWrapper.csproj
```

### Usage Examples

#### Creating an AcmeClient

```csharp
using AcmeshWrapper;

// Default constructor uses "acme.sh" from PATH
var client = new AcmeClient();

// Or specify custom path
var client = new AcmeClient("/usr/local/bin/acme.sh");
```

#### Listing Certificates

```csharp
var listOptions = new ListOptions
{
    Raw = true // Get raw output format
};

var result = await client.ListAsync(listOptions);

if (result.IsSuccess)
{
    foreach (var cert in result.Certificates)
    {
        Console.WriteLine($"Domain: {cert.Domain}");
        Console.WriteLine($"Next Renewal: {cert.Le_Next_Renew_Time}");
        Console.WriteLine($"Key Length: {cert.Le_Keylength}");
    }
}
else
{
    Console.WriteLine("Error listing certificates:");
    foreach (var error in result.ErrorOutput)
    {
        Console.WriteLine(error);
    }
}
```

#### Issuing a New Certificate

```csharp
var issueOptions = new IssueOptions
{
    Domains = new List<string> { "example.com", "www.example.com" },
    WebRoot = "/var/www/html",
    KeyLength = "4096",
    Staging = false // Use true for testing
};

var result = await client.IssueAsync(issueOptions);

if (result.IsSuccess)
{
    Console.WriteLine($"Certificate: {result.CertificateFile}");
    Console.WriteLine($"Key: {result.KeyFile}");
    Console.WriteLine($"CA: {result.CaFile}");
    Console.WriteLine($"Full Chain: {result.FullChainFile}");
}
```

#### Renewing a Certificate

```csharp
var renewOptions = new RenewOptions
{
    Domain = "example.com",
    Force = false, // Force renewal even if not due
    Ecc = false    // Use ECC certificate
};

var result = await client.RenewAsync(renewOptions);

if (result.IsSuccess)
{
    Console.WriteLine($"Renewed at: {result.RenewedAt}");
    Console.WriteLine($"Certificate: {result.CertificatePath}");
}
```

#### Installing Certificates

```csharp
var installOptions = new InstallCertOptions
{
    Domain = "example.com",
    CertFile = "/etc/nginx/ssl/cert.pem",
    KeyFile = "/etc/nginx/ssl/key.pem",
    FullChainFile = "/etc/nginx/ssl/fullchain.pem",
    ReloadCmd = "systemctl reload nginx"
};

var result = await client.InstallCertAsync(installOptions);

if (result.IsSuccess)
{
    Console.WriteLine($"Installed at: {result.InstalledAt}");
    Console.WriteLine($"Reload executed: {result.ReloadCommandExecuted}");
}
```

#### Revoking a Certificate

```csharp
var revokeOptions = new RevokeOptions
{
    Domain = "example.com",
    Reason = RevokeReason.KeyCompromise
};

var result = await client.RevokeAsync(revokeOptions);

if (result.IsSuccess)
{
    Console.WriteLine($"Revoked at: {result.RevokedAt}");
    Console.WriteLine($"Thumbprint: {result.CertificateThumbprint}");
}
```

#### Renewing All Certificates

```csharp
var renewAllOptions = new RenewAllOptions
{
    StopRenewOnError = true // Stop if any renewal fails
};

var result = await client.RenewAllAsync(renewAllOptions);

Console.WriteLine($"Total certificates: {result.TotalCertificates}");
Console.WriteLine($"Successful renewals: {result.SuccessfulRenewals}");
Console.WriteLine($"Skipped (not due): {result.SkippedRenewals}");
Console.WriteLine($"Failed: {result.FailedRenewals}");

if (result.FailedDomains.Any())
{
    Console.WriteLine("Failed domains:");
    foreach (var domain in result.FailedDomains)
    {
        Console.WriteLine($"  - {domain}");
    }
}
```

#### Removing a Certificate

```csharp
var removeOptions = new RemoveOptions
{
    Domain = "example.com",
    Ecc = false
};

var result = await client.RemoveAsync(removeOptions);

if (result.IsSuccess)
{
    Console.WriteLine($"Removed: {result.Domain} at {result.RemovedAt}");
}
```

#### Getting Certificate Information

```csharp
var infoOptions = new InfoOptions
{
    Domain = "example.com",
    Ecc = false // Set to true for ECC certificates
};

var result = await client.InfoAsync(infoOptions);

if (result.IsSuccess)
{
    Console.WriteLine($"Domain: {result.Domain}");
    Console.WriteLine($"Key Length: {result.KeyLength}");
    Console.WriteLine($"Alt Names: {result.AltNames}");
    Console.WriteLine($"Next Renewal: {result.NextRenewTimeStr}");
    Console.WriteLine($"API Endpoint: {result.ApiEndpoint}");
}
```

#### Retrieving Certificate Contents

```csharp
var getCertOptions = new GetCertificateOptions
{
    Domain = "example.com",
    Ecc = false,
    IncludeKey = true,        // Include private key
    IncludeFullChain = true,  // Include full certificate chain
    IncludeCa = true          // Include CA bundle
};

var result = await client.GetCertificateAsync(getCertOptions);

if (result.IsSuccess)
{
    Console.WriteLine($"Certificate: {result.Certificate}");
    Console.WriteLine($"Private Key: {result.PrivateKey}");
    Console.WriteLine($"Full Chain: {result.FullChain}");
    Console.WriteLine($"CA Bundle: {result.CaBundle}");
    
    // Save to files
    await File.WriteAllTextAsync("cert.pem", result.Certificate);
    await File.WriteAllTextAsync("key.pem", result.PrivateKey);
}
```

### Error Handling

All methods return result objects that include:
- `IsSuccess`: Boolean indicating operation success
- `RawOutput`: Complete output from acme.sh
- `ErrorOutput`: Array of error messages (if any)
- Command-specific properties (paths, timestamps, etc.)

```csharp
var result = await client.IssueAsync(options);

if (!result.IsSuccess)
{
    // Log raw output for debugging
    Console.WriteLine(result.RawOutput);
    
    // Display user-friendly errors
    foreach (var error in result.ErrorOutput)
    {
        Console.WriteLine($"Error: {error}");
    }
}
```

### License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

## Türkçe

### Açıklama

AcmeshWrapper, popüler [acme.sh](https://github.com/acmesh-official/acme.sh) ACME protokol istemcisi için tip güvenli, asenkron bir .NET sarmalayıcı kütüphanesidir. Let's Encrypt ve diğer ACME uyumlu sertifika otoritelerinden SSL/TLS sertifikalarını almayı, yenilemeyi ve yönetmeyi .NET uygulamalarınızda kolaylaştırır.

### Özellikler

- **Tip güvenli API**: Tüm acme.sh komutları için güçlü tipli seçenekler ve sonuçlar
- **Async/await desteği**: Tüm işlemler tamamen asenkron
- **Kapsamlı komut desteği**:
  - `list` - Tüm sertifikaları listele
  - `issue` - Yeni sertifika al
  - `renew` - Belirli sertifikaları yenile
  - `renew-all` - Yenilenmesi gereken tüm sertifikaları yenile
  - `install-cert` - Sertifikaları belirli konumlara kur
  - `revoke` - Sertifikaları iptal et
  - `remove` - Sertifikaları acme.sh yönetiminden kaldır
  - `info` - Detaylı sertifika bilgisi al
  - `get-certificate` - Sertifika dosya içeriklerini getir
- **Hata yönetimi**: Yapılandırılmış sonuçlarla detaylı hata bilgisi
- **Çapraz platform**: Windows, Linux ve macOS'ta çalışır
- **Minimal bağımlılıklar**: Güvenilir süreç yürütme için ProcessX üzerine inşa edilmiştir

### Gereksinimler

- .NET Standard 2.1 veya .NET 9.0
- [acme.sh](https://github.com/acmesh-official/acme.sh) kurulu ve PATH'te erişilebilir olmalı
- Sertifika işlemleri için uygun izinler

### Kurulum

```bash
# NuGet üzerinden kurulum
dotnet add package AcmeshWrapper

# Veya proje referansı ekleyin
dotnet add reference path/to/AcmeshWrapper.csproj
```

### Kullanım Örnekleri

#### AcmeClient Oluşturma

```csharp
using AcmeshWrapper;

// Varsayılan yapıcı PATH'ten "acme.sh" kullanır
var client = new AcmeClient();

// Veya özel yol belirtin
var client = new AcmeClient("/usr/local/bin/acme.sh");
```

#### Sertifikaları Listeleme

```csharp
var listOptions = new ListOptions
{
    Raw = true // Ham çıktı formatını al
};

var result = await client.ListAsync(listOptions);

if (result.IsSuccess)
{
    foreach (var cert in result.Certificates)
    {
        Console.WriteLine($"Alan adı: {cert.Domain}");
        Console.WriteLine($"Sonraki yenileme: {cert.Le_Next_Renew_Time}");
        Console.WriteLine($"Anahtar uzunluğu: {cert.Le_Keylength}");
    }
}
else
{
    Console.WriteLine("Sertifikaları listelerken hata:");
    foreach (var error in result.ErrorOutput)
    {
        Console.WriteLine(error);
    }
}
```

#### Yeni Sertifika Alma

```csharp
var issueOptions = new IssueOptions
{
    Domains = new List<string> { "example.com", "www.example.com" },
    WebRoot = "/var/www/html",
    KeyLength = "4096",
    Staging = false // Test için true kullanın
};

var result = await client.IssueAsync(issueOptions);

if (result.IsSuccess)
{
    Console.WriteLine($"Sertifika: {result.CertificateFile}");
    Console.WriteLine($"Anahtar: {result.KeyFile}");
    Console.WriteLine($"CA: {result.CaFile}");
    Console.WriteLine($"Tam zincir: {result.FullChainFile}");
}
```

#### Sertifika Yenileme

```csharp
var renewOptions = new RenewOptions
{
    Domain = "example.com",
    Force = false, // Süresi dolmasa bile yenilemeyi zorla
    Ecc = false    // ECC sertifikası kullan
};

var result = await client.RenewAsync(renewOptions);

if (result.IsSuccess)
{
    Console.WriteLine($"Yenilenme zamanı: {result.RenewedAt}");
    Console.WriteLine($"Sertifika: {result.CertificatePath}");
}
```

#### Sertifika Kurulumu

```csharp
var installOptions = new InstallCertOptions
{
    Domain = "example.com",
    CertFile = "/etc/nginx/ssl/cert.pem",
    KeyFile = "/etc/nginx/ssl/key.pem",
    FullChainFile = "/etc/nginx/ssl/fullchain.pem",
    ReloadCmd = "systemctl reload nginx"
};

var result = await client.InstallCertAsync(installOptions);

if (result.IsSuccess)
{
    Console.WriteLine($"Kurulum zamanı: {result.InstalledAt}");
    Console.WriteLine($"Yenileme komutu çalıştırıldı: {result.ReloadCommandExecuted}");
}
```

#### Sertifika İptali

```csharp
var revokeOptions = new RevokeOptions
{
    Domain = "example.com",
    Reason = RevokeReason.KeyCompromise
};

var result = await client.RevokeAsync(revokeOptions);

if (result.IsSuccess)
{
    Console.WriteLine($"İptal zamanı: {result.RevokedAt}");
    Console.WriteLine($"Parmak izi: {result.CertificateThumbprint}");
}
```

#### Tüm Sertifikaları Yenileme

```csharp
var renewAllOptions = new RenewAllOptions
{
    StopRenewOnError = true // Herhangi bir yenileme başarısız olursa dur
};

var result = await client.RenewAllAsync(renewAllOptions);

Console.WriteLine($"Toplam sertifika: {result.TotalCertificates}");
Console.WriteLine($"Başarılı yenilemeler: {result.SuccessfulRenewals}");
Console.WriteLine($"Atlananlar (süresi dolmamış): {result.SkippedRenewals}");
Console.WriteLine($"Başarısız: {result.FailedRenewals}");

if (result.FailedDomains.Any())
{
    Console.WriteLine("Başarısız alan adları:");
    foreach (var domain in result.FailedDomains)
    {
        Console.WriteLine($"  - {domain}");
    }
}
```

#### Sertifika Kaldırma

```csharp
var removeOptions = new RemoveOptions
{
    Domain = "example.com",
    Ecc = false
};

var result = await client.RemoveAsync(removeOptions);

if (result.IsSuccess)
{
    Console.WriteLine($"Kaldırıldı: {result.Domain} - {result.RemovedAt}");
}
```

#### Sertifika Bilgisi Alma

```csharp
var infoOptions = new InfoOptions
{
    Domain = "example.com",
    Ecc = false // ECC sertifikaları için true yapın
};

var result = await client.InfoAsync(infoOptions);

if (result.IsSuccess)
{
    Console.WriteLine($"Alan adı: {result.Domain}");
    Console.WriteLine($"Anahtar uzunluğu: {result.KeyLength}");
    Console.WriteLine($"Alternatif isimler: {result.AltNames}");
    Console.WriteLine($"Sonraki yenileme: {result.NextRenewTimeStr}");
    Console.WriteLine($"API endpoint: {result.ApiEndpoint}");
}
```

#### Sertifika İçeriğini Alma

```csharp
var getCertOptions = new GetCertificateOptions
{
    Domain = "example.com",
    Ecc = false,
    IncludeKey = true,        // Özel anahtarı dahil et
    IncludeFullChain = true,  // Tam sertifika zincirini dahil et
    IncludeCa = true          // CA paketini dahil et
};

var result = await client.GetCertificateAsync(getCertOptions);

if (result.IsSuccess)
{
    Console.WriteLine($"Sertifika: {result.Certificate}");
    Console.WriteLine($"Özel anahtar: {result.PrivateKey}");
    Console.WriteLine($"Tam zincir: {result.FullChain}");
    Console.WriteLine($"CA paketi: {result.CaBundle}");
    
    // Dosyalara kaydet
    await File.WriteAllTextAsync("cert.pem", result.Certificate);
    await File.WriteAllTextAsync("key.pem", result.PrivateKey);
}
```

### Hata Yönetimi

Tüm metodlar şunları içeren sonuç nesneleri döndürür:
- `IsSuccess`: İşlem başarısını gösteren boolean
- `RawOutput`: acme.sh'tan gelen tam çıktı
- `ErrorOutput`: Hata mesajları dizisi (varsa)
- Komuta özel özellikler (yollar, zaman damgaları vb.)

```csharp
var result = await client.IssueAsync(options);

if (!result.IsSuccess)
{
    // Hata ayıklama için ham çıktıyı kaydet
    Console.WriteLine(result.RawOutput);
    
    // Kullanıcı dostu hataları göster
    foreach (var error in result.ErrorOutput)
    {
        Console.WriteLine($"Hata: {error}");
    }
}
```

### Lisans

Bu proje MIT Lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.