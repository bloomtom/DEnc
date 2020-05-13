using System.Collections.Generic;

namespace DEnc.Models.Interfaces
{
    /// <summary>
    /// An interface for encoder options.
    /// </summary>
    public interface IEncodeOptions
    {
        /// <summary>
        /// Additional flags to pass to the encoder for each audio stream.
        /// </summary>
        ICollection<string> AdditionalAudioFlags { get; set; }
        /// <summary>
        /// Additional general flags to pass to the encoder
        /// </summary>
        ICollection<string> AdditionalFlags { get; set; }
        /// <summary>
        /// Additional flags to pass to the encoder for each video stream.
        /// </summary>
        ICollection<string> AdditionalVideoFlags { get; set; }
        /// <summary>
        /// Additional flags to pass to MP4Box to dashify the video file and generate a manifest.
        /// </summary>
        ICollection<string> AdditionalMP4BoxFlags { get; set; }
    }
}