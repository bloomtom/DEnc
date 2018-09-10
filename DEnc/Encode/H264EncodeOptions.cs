using System;
using System.Collections.Generic;
using System.Text;

namespace DEnc
{
    /// <summary>
    /// A set of encoder options for h.264 encoding in ffmpeg using sane defaults. Default values may change over time, so if you require 
    /// </summary>
    public class H264EncodeOptions : IEncodeOptions
    {
        /// <summary>
        /// Flags for ffmpeg itself, not individual encode streams.
        /// </summary>
        public ICollection<string> AdditionalFlags { get; set; } = new List<string>();
        /// <summary>
        /// Flags to be applied to every audio stream. Default is "-sn -map_metadata -1".
        /// </summary>
        public ICollection<string> AdditionalAudioFlags { get; set; } = new List<string>() { "-sn", "-map_metadata -1" };
        /// <summary>
        /// Flags to be applied to every video stream. Default is "-sn -map_metadata -1".
        /// </summary>
        public ICollection<string> AdditionalVideoFlags { get; set; } = new List<string>() { "-sn", "-map_metadata -1", };

        public H264EncodeOptions()
        {
        }
    }
}
