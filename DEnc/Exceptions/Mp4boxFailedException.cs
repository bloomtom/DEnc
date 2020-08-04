using DEnc.Commands;
using System;
using System.Text;

namespace DEnc.Exceptions
{
    /// <summary>
    /// Thrown when ffmpeg fails during execution or produced invalid output.
    /// </summary>
    public class Mp4boxFailedException : FFMpegFailedException
    {
        ///<inheritdoc cref="DashManifestNotCreatedException"/>
        public Mp4boxFailedException(FFmpegCommand ffmpegCommand, Mp4BoxRenderedCommand mp4boxCommand, StringBuilder log, string message) : base(message)
        {
            FFmpegCommand = ffmpegCommand;
            MP4BoxCommand = mp4boxCommand;
            Log = log;
        }

        ///<inheritdoc cref="DashManifestNotCreatedException"/>
        public Mp4boxFailedException(FFmpegCommand ffmpegCommand, Mp4BoxRenderedCommand mp4boxCommand, StringBuilder log, string message, Exception innerException) : base(message, innerException)
        {
            FFmpegCommand = ffmpegCommand;
            MP4BoxCommand = mp4boxCommand;
            Log = log;
        }

        ///<inheritdoc cref="DashManifestNotCreatedException"/>
        public Mp4boxFailedException()
        {
        }

        ///<inheritdoc cref="DashManifestNotCreatedException"/>
        public Mp4boxFailedException(string message) : base(message)
        {
        }

        ///<inheritdoc cref="DashManifestNotCreatedException"/>
        public Mp4boxFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// The file path the mpd file was expected to be generated at.
        /// </summary>
        public Mp4BoxRenderedCommand MP4BoxCommand { get; protected set; }
    }
}