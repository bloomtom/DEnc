using DEnc.Models.Interfaces;

namespace DEnc.Models
{
    /// <summary>
    /// The definition for how to produce a video stream during encoding.
    /// </summary>
    public class VideoStreamCommand : IStreamCommand
    {
        ///<inheritdoc cref="IStreamCommand.Argument"/>
        public string Argument { get; set; }
        /// <summary>
        /// The bitrate in kb/s for this stream.
        /// </summary>
        public string Bitrate { get; set; }
        ///<inheritdoc cref="IStreamCommand.Index"/>
        public int Index { get; set; }
        ///<inheritdoc cref="IStreamCommand.Path"/>
        public string Path { get; set; }
        /// <summary>
        /// This is a video stream.
        /// </summary>
        public StreamType Type => StreamType.Video;
    }
}