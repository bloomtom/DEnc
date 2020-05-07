using System;
using System.Collections.Generic;
using System.Text;

namespace DEnc.Commands
{
    internal class StreamFile
    {
        public StreamType Type { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Argument { get; set; }
    }
}
