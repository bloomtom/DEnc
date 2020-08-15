namespace DEnc.Models
{
    /// <summary>
    /// Defines an audio downmixing algorithm type to use if more than two audio channels are encountered within a single stream.
    /// </summary>
    public enum DownmixMode
    {
        /// <summary>
        /// Does not downmix.
        /// </summary>
        None,

        /// <summary>
        /// Uses the default ffmpeg algorithm with the -ac 2 flag.<br/>
        /// See: https://trac.ffmpeg.org/wiki/AudioChannelManipulation#a5.1stereo
        /// </summary>
        Default,

        /// <summary>
        /// Uses an audio filter which focuses more on dialog than background effects.<br/>
        /// See: https://forum.doom9.org/showthread.php?t=168267
        /// </summary>
        Nightmode
    }

    /// <summary>
    /// Contains audio stream specific configuration for generating transcoding commands.
    /// </summary>
    public class AudioConfig
    {
        /// <inheritdoc cref="DownmixMode"/>
        public DownmixMode DownmixMode { get; set; } = DownmixMode.None;

        /// <summary>
        /// The maximum per-channel bitrate to allow before transcoding the stream.
        /// </summary>
        public int MaxPerChannelBitrate { get; set; } = 1024 * 96;
    }
}