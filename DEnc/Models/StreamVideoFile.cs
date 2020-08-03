using DEnc.Models.Interfaces;

namespace DEnc.Models
{
    internal class StreamVideoFile : IStreamFile
    {
        public string Argument { get; set; }
        public string Bitrate { get; set; }
        public int Index { get; set; }
        public string Path { get; set; }
        public StreamType Type { get; set; }
    }
}