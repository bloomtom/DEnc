﻿using DEnc.Serialization;
using System.Collections.Generic;

namespace DEnc
{
    internal class MediaMetadata
    {
        internal MediaMetadata(
            IEnumerable<MediaStream> videoStreams,
            IEnumerable<MediaStream> audioStreams,
            IEnumerable<MediaStream> subtitleStreams,
            IReadOnlyDictionary<string, string> metadata,
            long bitrate,
            decimal framerate,
            float duration)
        {
            VideoStreams = videoStreams;
            AudioStreams = audioStreams;
            SubtitleStreams = subtitleStreams;
            Metadata = metadata;
            Bitrate = bitrate;
            Framerate = framerate;
            Duration = duration;
        }

        public IEnumerable<MediaStream> AudioStreams { get; private set; }
        /// <summary>
        /// Bitrate in bits per second.
        /// </summary>
        public long Bitrate { get; private set; }

        public float Duration { get; private set; }
        public decimal Framerate { get; private set; }
        public IReadOnlyDictionary<string, string> Metadata { get; private set; }
        public IEnumerable<MediaStream> SubtitleStreams { get; private set; }
        public IEnumerable<MediaStream> VideoStreams { get; private set; }
    }
}