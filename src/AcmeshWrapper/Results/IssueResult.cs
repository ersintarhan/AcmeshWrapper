namespace AcmeshWrapper.Results
{
    public class IssueResult : AcmeResult
    {
        public string? CertificateFile { get; internal set; }
        public string? KeyFile { get; internal set; }
        public string? CaFile { get; internal set; }
        public string? FullChainFile { get; internal set; }
    }
}
