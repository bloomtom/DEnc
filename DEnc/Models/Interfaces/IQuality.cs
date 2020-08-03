namespace DEnc.Models.Interfaces
{
    /// <summary>
    /// An interface for a quality. Qualities are used to specify the outputs for DASHification.
    /// </summary>
    public interface IQuality
    {
        /// <summary>
        /// Bitrate of media in kb/s.
        /// </summary>
        int Bitrate { get; set; }

        /// <summary>
        /// Height of frame in pixels.
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// ffmpeg h264 encoding profile level (3.0, 4.0, 4.1...)
        /// </summary>
        string Level { get; set; }

        /// <summary>
        /// ffmpeg pixel format or pix_fmt.
        /// </summary>
        string PixelFormat { get; set; }

        /// <summary>
        /// ffmpeg preset (veryfast, fast, medium, slow, veryslow).
        /// </summary>
        H264Preset Preset { get; set; }

        /// <summary>
        /// ffmpeg h264 encoding profile (Baseline, Main, High,)
        /// </summary>
        H264Profile Profile { get; set; }

        /// <summary>
        /// Width of frame in pixels.
        /// </summary>
        int Width { get; set; }
    }
}