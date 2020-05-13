using System;
using System.Collections.Generic;
using System.Text;

namespace DEnc.Models.Interfaces
{
    internal interface IStreamFile
    {
        StreamType Type { get; set; }
        int Index { get; set; }
        string Path { get; set; }
        string Argument { get; set; }
    }
}
