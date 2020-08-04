using DEnc.Commands;
using System;

namespace DEnc.Models
{
    /// <summary>
    /// Thrown when MP4Box fails to generate a DASH manifest MPD file.
    /// </summary>
    public class DashManifestNotCreatedException : Exception
    {
        ///<inheritdoc cref="DashManifestNotCreatedException"/>
        public DashManifestNotCreatedException(string expectedMpdPath, FFmpegCommand ffmpegCommand, Mp4BoxRenderedCommand mp4boxCommand, string message) : base(message)
        {
            ExpectedMpdPath = expectedMpdPath;
            FFmpegCommand = ffmpegCommand;
            MP4BoxCommand = mp4boxCommand;
        }

        ///<inheritdoc cref="DashManifestNotCreatedException"/>
        public DashManifestNotCreatedException()
        {
        }

        ///<inheritdoc cref="DashManifestNotCreatedException"/>
        public DashManifestNotCreatedException(string message) : base(message)
        {
        }

        ///<inheritdoc cref="DashManifestNotCreatedException"/>
        public DashManifestNotCreatedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// The file path the mpd file was expected to be generated at.
        /// </summary>
        public string ExpectedMpdPath { get; private set; }

        /// <summary>
        /// The file path the mpd file was expected to be generated at.
        /// </summary>
        public FFmpegCommand FFmpegCommand { get; private set; }

        /// <summary>
        /// The file path the mpd file was expected to be generated at.
        /// </summary>
        public Mp4BoxRenderedCommand MP4BoxCommand { get; private set; }
    }
}