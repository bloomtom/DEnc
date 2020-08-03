﻿using DEnc.Models.Interfaces;
using System.Collections.Generic;

namespace DEnc.Models
{
    /// <summary>
    /// A set of encoder options for h.264 encoding in ffmpeg using sane defaults.
    /// Default values may change over time, and will only be considered to be a minor change on versioning.
    /// </summary>
    public class H264EncodeOptions : IEncodeOptions
    {
        ///<inheritdoc cref="H264EncodeOptions"/>
        public H264EncodeOptions()
        {
        }

        /// <summary>
        /// Flags to be applied to every audio stream. You can add flags, but the defaults set may be important to the generation of a valid DASH file, so avoid changing them unless you know what they do.
        /// </summary>
        public ICollection<string> AdditionalAudioFlags { get; set; } = new List<string>() { "-sn", "-ignore_unknown", "-map_chapters -1" };

        /// <summary>
        /// Flags for ffmpeg itself, not individual encode streams.
        /// </summary>
        public ICollection<string> AdditionalFlags { get; set; } = new List<string>();
        /// <summary>
        /// Flags passed to MP4Box for final massaging of the video file and generation of the dash manifest.
        /// </summary>
        public ICollection<string> AdditionalMP4BoxFlags { get; set; } = new List<string>()
        {
            "-profile dashavc264:onDemand",
            "-sample-groups-traf",
            "-subsegs-per-sidx 0",
            "-bs-switching no",
            "-rap",
            "-frag-rap",
            "-quiet",
        };

        /// <summary>
        /// Flags to be applied to every video stream. You can add flags, but the defaults set may be important to the generation of a valid DASH file, so avoid changing them unless you know what they do.
        /// </summary>
        public ICollection<string> AdditionalVideoFlags { get; set; } = new List<string>() { "-sn", "-ignore_unknown", "-map_chapters -1" };
    }
}