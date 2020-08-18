namespace DEnc.Models
{
    /// <summary>
    /// Some example qualities.
    /// </summary>
    public enum DefaultQuality
    {
        /// <summary>
        /// Very low quality. Low grade SD to low grade HD.
        /// </summary>
        Potato,

        /// <summary>
        /// Low quality HD.
        /// </summary>
        Low,

        /// <summary>
        /// Decent quality HD.
        /// </summary>
        Medium,

        /// <summary>
        /// Good quality HD.
        /// </summary>
        High,

        /// <summary>
        /// Excellent quality HD.
        /// </summary>
        Ultra
    }

    /// <summary>
    /// H264 Presets.
    /// The slower the preset the smaller the file size and the better the quality, but the longer the encoding time.
    /// Has diminishing returns near the end of each spectrum
    /// Recommended range is medium -> faster
    /// </summary>
    public enum H264Preset
    {
        /// <summary>
        /// Reserved for copy quality.
        /// </summary>
        None = 0,

        /// <summary>
        /// ~55% faster than medium. Extremely low quality.
        /// </summary>
        Ultrafast,

        /// <summary>
        /// ~50% faster than medium. Very low quality.
        /// </summary>
        Superfast,

        /// <summary>
        /// ~45% faster than medium. Quality begins dropping significantly here.
        /// </summary>
        Veryfast,

        /// <summary>
        /// ~25% faster than medium.
        /// </summary>
        Faster,

        /// <summary>
        /// ~10% faster than medium.
        /// </summary>
        Fast,

        /// <summary>
        /// Default preset, quality begins to peak here.
        /// </summary>
        Medium,

        /// <summary>
        /// ~40% slower than medium. Marginally better quality than medium.
        /// </summary>
        Slow,

        /// <summary>
        /// ~100% slower than medium. Near identical quality to slow.
        /// </summary>
        Slower,

        /// <summary>
        /// ~280% slower than medium. Near identical quality to slower.
        /// </summary>
        Veryslow
    }

    /// <summary>
    /// H264 Profiles
    /// </summary>
    public enum H264Profile
    {
        /// <summary>
        /// Reserved for copy profile.
        /// </summary>
        None = 0,

        /// <summary>
        /// Ideal for older mobile devices.
        /// </summary>
        Baseline,

        /// <summary>
        /// Ideal for web streaming and the majority of devices.
        /// </summary>
        Main,

        /// <summary>
        /// Ideal for high-end mobile device, or direct playback on a computer.
        /// </summary>
        High
    }

    /// <summary>
    /// Indicates the type of stream in an <see cref="IStreamCommand"/>.
    /// </summary>
    public enum StreamType
    {
        /// <summary>
        /// Video stream
        /// </summary>
        Video,

        /// <summary>
        /// Audio stream
        /// </summary>
        Audio,

        /// <summary>
        /// Subtitle stream
        /// </summary>
        Subtitle,

        /// <summary>
        /// DASH manifest
        /// </summary>
        MPD
    }
}