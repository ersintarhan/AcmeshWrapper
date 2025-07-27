using System.Collections.Generic;

namespace AcmeshWrapper.Options
{
    public class IssueOptions
    {
        public List<string> Domains { get; set; } = new List<string>();
        public string? WebRoot { get; set; }
        public string? DnsProvider { get; set; }
        public string KeyLength { get; set; } = "4096";
        public bool Staging { get; set; }
        public string? Server { get; set; }
    }
}
