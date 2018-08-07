using DEnc.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace DEnc
{
    internal class MediaMetadata
    {
        public IEnumerable<MediaStream> AudioStreams { get; private set; }
        public IEnumerable<MediaStream> VideoStreams { get; private set; }
        public IEnumerable<MediaStream> SubtitleStreams { get; private set; }
        public long Bitrate { get; private set; }
        public decimal Framerate { get; private set; }

        internal MediaMetadata(
            IEnumerable<MediaStream> videoStreams,
            IEnumerable<MediaStream> audioStreams,
            IEnumerable<MediaStream> subtitleStreams,
            long bitrate,
            decimal framerate)
        {
            VideoStreams = videoStreams;
            AudioStreams = audioStreams;
            SubtitleStreams = subtitleStreams;
            Bitrate = bitrate;
            Framerate = framerate;
        }
    }
}
