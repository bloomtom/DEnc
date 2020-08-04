namespace DEnc.Models.Interfaces
{
    /// <summary>
    /// A generic stream for ffmpeg command rendering
    /// </summary>
    public interface IStreamFile
    {
        /// <summary>
        /// The argument used to generate this stream.
        /// </summary>
        string Argument { get; set; }
        /// <summary>
        /// The DASH index for this stream.
        /// </summary>
        int Index { get; set; }
        /// <summary>
        /// The path to the output for this stream.
        /// </summary>
        string Path { get; set; }
        /// <summary>
        /// The type of this stream.
        /// </summary>
        StreamType Type { get; }
    }
}