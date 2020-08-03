using DEnc.Models.Interfaces;

namespace DEnc.Models
{
    internal class StreamAudioFile : IStreamFile
    {
        public string Argument { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public StreamType Type { get; set; }
    }
}