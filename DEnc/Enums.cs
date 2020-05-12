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
        /// Excellend quality HD
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
    /// H264 Presets
    /// </summary>
    public enum H264Preset
    {
        ultrafast, 
        superfast, 
        veryfast, 
        fast, 
        medium, 
        slow, 
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
