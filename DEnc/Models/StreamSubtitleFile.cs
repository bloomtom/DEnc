using DEnc.Models.Interfaces;

namespace DEnc.Models
{
    internal class StreamSubtitleFile : IStreamFile
    {
        public string Argument { get; set; }
        public int Index { get; set; }
        public string Language { get; set; }
        public string Path { get; set; }
        public StreamType Type { get; set; }
    }
}