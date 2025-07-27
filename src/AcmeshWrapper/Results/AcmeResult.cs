namespace AcmeshWrapper.Results
{
    public abstract class AcmeResult
    {
        public bool IsSuccess { get; internal set; }
        public string? RawOutput { get; internal set; }
        public string[]? ErrorOutput { get; internal set; }
    }
}
