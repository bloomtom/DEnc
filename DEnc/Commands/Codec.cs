using System;
using System.Collections.Generic;
using System.Text;

namespace DEnc.Commands
{
    internal class Codec
    {
        public string Name { get; private set; }
        public string Container { get; private set; }
        public string Extension { get; private set; }

        public Codec(string name, string container, string extension)
        {
            Name = name;
            Container = container;
            Extension = extension;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
