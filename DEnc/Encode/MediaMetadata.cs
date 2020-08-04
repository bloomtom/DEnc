using DEnc.Serialization;
using System.Collections.Generic;

namespace DEnc
{
    /// <summary>
    /// Encapsulates parsed data regarding an input media file, generally yielded from interpreting <see cref="FFprobeData"/>.
    /// </summary>
    public class MediaMetadata
    {
        ///<inheritdoc cref="MediaMetadata"/>
        public MediaMetadata(
            string path,
            IEnumerable<MediaStream> videoStreams,
            IEnumerable<MediaStream> audioStreams,
            IEnumerable<MediaStream> subtitleStreams,
            IReadOnlyDictionary<string, string> metadata,
            long bitrate,
            decimal framerate,
            float duration)
        {
            Path = path;
            VideoStreams = videoStreams;
            AudioStreams = audioStreams;
            SubtitleStreams = subtitleStreams;
            Metadata = metadata;
            Bitrate = bitrate;
            KBitrate = (int)(bitrate / 1024);
            Framerate = framerate;
            Duration = duration;
        }

        /// <summary>
        /// A collection of the audio only streams contained in a media container.
        /// </summary>
        public IEnumerable<MediaStream> AudioStreams { get; private set; }

        /// <summary>
        /// Bitrate in bits per second.
        /// </summary>
        public long Bitrate { get; private set; }
        /// <summary>
        /// Bitrate in kb/s.
        /// </summary>
        public int KBitrate { get; private set; }

        /// <summary>
        /// The duration in seconds.
        /// </summary>
        public float Duration { get; private set; }

        /// <summary>
        /// The framerate in frames per second.
        /// </summary>
        public decimal Framerate { get; private set; }

        /// <summary>
        /// A collection of misc metadata from the ffprobe "format tags" in key/value form.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; private set; }

        /// <summary>
        /// The path to the origin media file on disk.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// A collection of the subtitle streams.
        /// </summary>
        public IEnumerable<MediaStream> SubtitleStreams { get; private set; }

        /// <summary>
        /// A collection of the video streams.
        /// </summary>
        public IEnumerable<MediaStream> VideoStreams { get; private set; }

        /// <summary>
        /// Uses Path for comparison.
        /// </summary>
        public override bool Equals(object obj)
        {
            return base.Equals(Path);
        }

        /// <summary>
        /// Yields the hash code for the Path.
        /// </summary>
        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }
    }
}