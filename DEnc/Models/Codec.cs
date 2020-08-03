namespace DEnc.Models
{
    internal class Codec
    {
        public Codec(string name, string container, string extension)
        {
            Name = name;
            Container = container;
            Extension = extension;
        }

        public string Container { get; private set; }
        public string Extension { get; private set; }
        public string Name { get; private set; }
        public override string ToString()
        {
            return Name;
        }
    }
}