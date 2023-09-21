namespace RegParserDotNet
{
    public class RegistryEntry
    {
        public RegistryValueType Type { get; set; }
        public string Path { get; set; }
        public string Property { get; set; }
        public object Value { get; set; }

        public RegistryEntry(string path, string property, RegistryValueType type, object value)
        {
            Path = path;
            Property = property;
            Type = type;
            Value = value;
        }

        public RegistryEntry(string path)
        {
            Path = path;
            Type = RegistryValueType.REG_KEY;
        }
    }
}
