using DEnc.Commands;
using System;
using System.Text;

namespace DEnc.Exceptions
{
    /// <summary>
    /// Thrown when MP4Box fails during execution or produced invalid output. The ffmpeg command used to generate files input to MP4Box is also included.
    /// </summary>
    public class Mp4boxFailedException : FFMpegFailedException
    {
        ///<inheritdoc cref="DashManifestNotCreatedException"/>
        public Mp4boxFailedException(FFmpegCommand ffmpegCommand, Mp4BoxCommand mp4boxCommand, StringBuilder log, string message) : base(message)
        {
            FFmpegCommand = ffmpegCommand;
            MP4BoxCommand = mp4boxCommand;
            Log = log;
        }

        ///<inheritdoc cref="DashManifestNotCreatedException"/>
        public Mp4boxFailedException(FFmpegCommand ffmpegCommand, Mp4BoxCommand mp4boxCommand, StringBuilder log, string message, Exception innerException) : base(message, innerException)
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
        public Mp4BoxCommand MP4BoxCommand { get; protected set; }
    }
}