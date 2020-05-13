using System;
using System.Collections.Generic;
using System.Text;

namespace DEnc.Commands
{
    internal interface IStreamFile
    {
        StreamType Type { get; set; }
        int Index { get; set; }
        string Path { get; set; }
        string Argument { get; set; }
    }

    internal class StreamVideoFile : IStreamFile
    {
        public StreamType Type { get; set; }
        public int Index { get; set; }
        public string Bitrate { get; set; }
        public string Path { get; set; }
        public string Argument { get; set; }
    }

    internal class StreamAudioFile : IStreamFile
    {
        public StreamType Type { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Argument { get; set; }
    }

    internal class StreamSubtitleFile : IStreamFile
    {
        public StreamType Type { get; set; }
        public int Index { get; set; }
        public string Language { get; set; }
        public string Path { get; set; }
        public string Argument { get; set; }
    }
}
