namespace AcmeshWrapper.Options
{
    public class InfoOptions
    {
        /// <summary>
        /// The domain to get information for (required)
        /// </summary>
        public string Domain { get; set; } = string.Empty;

        /// <summary>
        /// Whether to get information for an ECC certificate (optional)
        /// </summary>
        public bool Ecc { get; set; }
    }
}