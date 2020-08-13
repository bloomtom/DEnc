using DEnc.Commands;
using System;

namespace DEnc.Exceptions
{
    /// <summary>
    /// Thrown when MP4Box fails to generate a DASH manifest MPD file.
    /// </summary>
    public class DashManifestNotCreatedException : Mp4boxFailedException
    {
        ///<inheritdoc cref="DashManifestNotCreatedException"/>
        public DashManifestNotCreatedException(string expectedMpdPath, FFmpegCommand ffmpegCommand, Mp4BoxCommand mp4boxCommand, string message) : base(message)
        {
            ExpectedMpdPath = expectedMpdPath;
            FFmpegCommand = ffmpegCommand;
            MP4BoxCommand = mp4boxCommand;
        }

        ///<inheritdoc cref="DashManifestNotCreatedException"/>
        public DashManifestNotCreatedException(string expectedMpdPath, FFmpegCommand ffmpegCommand, Mp4BoxCommand mp4boxCommand, string message, Exception innerException) : base(message, innerException)
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
        public string ExpectedMpdPath { get; protected set; }
    }
}