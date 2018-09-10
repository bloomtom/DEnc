using System.Collections.Generic;

namespace DEnc
{
    public interface IEncodeOptions
    {
        ICollection<string> AdditionalAudioFlags { get; set; }
        ICollection<string> AdditionalFlags { get; set; }
        ICollection<string> AdditionalVideoFlags { get; set; }
    }
}