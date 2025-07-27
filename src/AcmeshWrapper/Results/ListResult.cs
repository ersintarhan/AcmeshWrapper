using System.Collections.Generic;

namespace AcmeshWrapper.Results
{
    public class ListResult : AcmeResult
    {
        public List<CertificateInfo> Certificates { get; set; } = new List<CertificateInfo>();
    }

    public class CertificateInfo
    {
        public string? Domain { get; set; }
        public string? Le_Main { get; set; }
        public string? Le_Alt { get; set; }
        public string? Le_Keylength { get; set; }
        public string? Le_Real_Keylength { get; set; }
        public string? Le_SAN { get; set; }
        public string? Le_Next_Renew_Time { get; set; }
        public string? Le_Next_Renew_Time_UTC { get; set; }
        public string? Le_Created_Time { get; set; }
        public string? Le_Created_Time_UTC { get; set; }
        public string? CA { get; set; }
    }
}
