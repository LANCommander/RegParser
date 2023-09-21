namespace RegParserDotNet
{
    public class RegistryKey
    {
        public RegistryValueType Type { get; set; }
        public string Path { get; set; }
        public object Value { get; set; }

        public RegistryKey(string path, RegistryValueType type, object value)
        {
            Path = path;
            Type = type;
            Value = value;
        }

        public RegistryKey(string path)
        {
            Path = path;
            Type = RegistryValueType.REG_KEY;
        }
    }
}
