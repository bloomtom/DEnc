using DEnc.Commands;
using System;
using System.Text;

namespace DEnc.Exceptions
{
    /// <summary>
    /// Thrown when ffmpeg fails during execution or produced invalid output.
    /// </summary>
    public class FFMpegFailedException : Exception
    {
        ///<inheritdoc cref="DashManifestNotCreatedException"/>
        public FFMpegFailedException(FFmpegCommand ffmpegCommand, StringBuilder log, string message) : base(message)
        {
            FFmpegCommand = ffmpegCommand;
            Log = log;
        }

        ///<inheritdoc cref="DashManifestNotCreatedException"/>
        public FFMpegFailedException(FFmpegCommand ffmpegCommand, StringBuilder log, string message, Exception innerException) : base(message, innerException)
        {
            FFmpegCommand = ffmpegCommand;
            Log = log;
        }

        ///<inheritdoc cref="DashManifestNotCreatedException"/>
        public FFMpegFailedException()
        {
        }

        ///<inheritdoc cref="DashManifestNotCreatedException"/>
        public FFMpegFailedException(string message) : base(message)
        {
        }

        ///<inheritdoc cref="DashManifestNotCreatedException"/>
        public FFMpegFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// The command used to run ffmpeg.
        /// </summary>
        public FFmpegCommand FFmpegCommand { get; protected set; }

        /// <summary>
        /// A text log detailing events leading up to the exception.
        /// </summary>
        public StringBuilder Log { get; protected set; } = new StringBuilder();
    }
}