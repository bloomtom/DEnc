using DEnc.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace DEnc
{
    internal class MediaMetadata
    {
        public string AudioFormat { get; set; }
        public string VideoFormat { get; set; }
        public IEnumerable<MediaStream> AudioStreams { get; set; }
        public IEnumerable<MediaStream> VideoStreams { get; set; }
        public long Bitrate { get; set; }
        public decimal Framerate { get; set; }
    }
}
