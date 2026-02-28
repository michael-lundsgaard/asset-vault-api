namespace AssetVault.Domain.ValueObjects
{
    public record StoragePath
    {
        public string Value { get; }
        public string BucketKey => Value.Split('/').Last();

        // Private constructor to enforce validation via the Create method
        private StoragePath(string value) => Value = value;

        public static StoragePath Create(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Storage path cannot be empty.", nameof(path));
            return new StoragePath(path);
        }

        public override string ToString() => Value;
    }
}
