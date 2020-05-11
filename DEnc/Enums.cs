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
}
