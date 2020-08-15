using DEnc.Commands;
using DEnc.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DEnc
{
    /// <summary>
    /// A representation of the output from DASHing a file.
    /// </summary>
    public class DashEncodeResult
    {
        /// <summary>
        /// Create a typical instance of DashEncodeResult.
        /// </summary>
        /// <param name="mpdPath">The exact path to the mpd file.</param>
        /// <param name="mpdContent">The exact mpd content deserialized from XML.</param>
        /// <param name="ffmpegCommand">The generated ffmpeg command used when </param>
        /// <param name="inputMetadata">Metadata about the DASHed input file.</param>
        public DashEncodeResult(string mpdPath, MPD mpdContent, FFmpegCommand ffmpegCommand, MediaMetadata inputMetadata)
        {
            DashFileContent = mpdContent;
            DashFilePath = mpdPath;
            FFmpegCommand = ffmpegCommand;
            InputMetadata = inputMetadata;
        }

        /// <summary>
        /// This is the exact mpd content deserialized from XML.
        /// </summary>
        public MPD DashFileContent { get; protected set; }

        /// <summary>
        /// The exact path to the mpd file.
        /// </summary>
        public string DashFilePath { get; protected set; }

        /// <summary>
        /// The result yielded from probing the input file.
        /// </summary>
        public FFmpegCommand FFmpegCommand { get; protected set; }

        /// <summary>
        /// The play duration of the media computed on the fly from <see cref="InputMetadata"/>.
        /// </summary>
        public TimeSpan FileDuration => InputMetadata != null ? TimeSpan.FromMilliseconds((InputMetadata.VideoStreams.FirstOrDefault()?.duration ?? 0) * 1000) : TimeSpan.Zero;

        /// <summary>
        /// The result yielded from probing the input file.
        /// </summary>
        public MediaMetadata InputMetadata { get; protected set; }
        /// <summary>
        /// Returns the list of media filenames from the DashFileContent. This operation scans the MPD object and isn't cached. Does not return filenames when a live profile is used.
        /// </summary>
        public IEnumerable<string> MediaFiles => DashFileContent?.Period.SelectMany(x => x.AdaptationSet.SelectMany(y => y.Representation.SelectMany(z => z.BaseURL)));
    }
}