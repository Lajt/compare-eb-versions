namespace Default.MapBot
{
    public class AffixData
    {
        public string Name { get; }
        public string Description { get; }
        public bool RerollMagic { get; set; }
        public bool RerollRare { get; set; }

        public AffixData(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}