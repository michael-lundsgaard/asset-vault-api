namespace AssetVault.Domain.ValueObjects
{
    public record FileSize
    {
        public long Bytes { get; }
        public double Kilobytes => Bytes / 1024.0;
        public double Megabytes => Bytes / (1024.0 * 1024);
        public double Gigabytes => Bytes / (1024.0 * 1024 * 1024);

        // Private constructor to enforce validation via the Create method
        private FileSize(long bytes) => Bytes = bytes;

        public static FileSize Create(long bytes)
        {
            if (bytes < 0) throw new ArgumentException("File size cannot be negative.", nameof(bytes));
            return new FileSize(bytes);
        }

        public override string ToString() => Gigabytes >= 1
            ? $"{Gigabytes:F2} GB"
            : Megabytes >= 1
                ? $"{Megabytes:F2} MB"
                : $"{Kilobytes:F2} KB";
    }
}
