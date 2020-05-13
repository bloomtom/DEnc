using System;
using System.Collections.Generic;
using System.Text;

namespace DEnc
{
    internal enum StreamType
    {
        Video,
        Audio,
        Subtitle,
        MPD
    }

    /// <summary>
    /// Some example qualities.
    /// </summary>
    public enum DefaultQuality
    {
        /// <summary>
        /// Very low quality. Low grade SD to low grade HD
        /// </summary>
        potato,
        /// <summary>
        /// Low quality HD.
        /// </summary>
        low,
        /// <summary>
        /// Decent quality HD.
        /// </summary>
        medium,
        /// <summary>
        /// Good quality HD.
        /// </summary>
        high,
        /// <summary>
        /// Excellent quality HD
        /// </summary>
        ultra
    }

    /// <summary>
    /// The stages of the encoding process
    /// </summary>
    public enum EncodingStage
    {
        /// <summary>
        /// Encoding the video
        /// </summary>
        Encode = 1,

        /// <summary>
        /// Generating DASH Manifest
        /// </summary>
        DASHify = 2,

        /// <summary>
        /// Processing subtitles
        /// </summary>
        PostProcess = 3
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
        /// ~55% faster than medium. Extremely low quality.
        /// </summary>
        ultrafast,

        /// <summary>
        /// ~50% faster than medium. Very low quality.
        /// </summary>
        superfast,

        /// <summary>
        /// ~45% faster than medium. Quality begins dropping significantly here.
        /// </summary>
        veryfast,

        /// <summary>
        /// ~25% faster than medium.
        /// </summary>
        faster,

        /// <summary>
        /// ~10% faster than medium.
        /// </summary>
        fast,

        /// <summary>
        /// Default preset, quality begins to peak here.
        /// </summary>
        medium,

        /// <summary>
        /// ~40% slower than medium. Marginally better quality than medium.
        /// </summary>
        slow,

        /// <summary>
        /// ~100% slower than medium. Near identical quality to slow.
        /// </summary>
        slower,

        /// <summary>
        /// ~280% slower than medium. Near identical quality to slower.
        /// </summary>
        veryslow
    }

    /// <summary>
    /// H264 Profiles
    /// </summary>
    public enum H264Profile
    {
        /// <summary>
        /// Ideal for older mobile devices
        /// </summary>
        baseline,

        /// <summary>
        /// Ideal for web streaming and the majority of devices
        /// </summary>
        main,

        /// <summary>
        /// Ideal for high-end mobile device, or direct playback on a computer
        /// </summary>
        high
    }
}
