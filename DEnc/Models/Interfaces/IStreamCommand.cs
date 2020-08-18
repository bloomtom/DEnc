namespace DEnc.Models
{
    /// <summary>
    /// A generic stream for ffmpeg command rendering
    /// </summary>
    public interface IStreamCommand
    {
        /// <summary>
        /// The ffmpeg argument used to generate this stream.
        /// </summary>
        string Argument { get; set; }

        /// <summary>
        /// The DASH index for this stream. Indexes must be unique across all streams in a DASH manifest.
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