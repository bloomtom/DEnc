using DEnc.Models;
using System.Collections.Generic;
using System.Linq;

namespace DEnc.Commands
{
    /// <summary>
    /// Provides a set of methods for building an ffmpeg command for a single video output stream using H264 encoding.
    /// </summary>
    public class FFmpegH264VideoCommandBuilder
    {
        private readonly int bitrate;
        private readonly List<string> commands;
        private readonly int index;
        private readonly string path;

        /// <inheritdoc cref="FFmpegH264VideoCommandBuilder"/>
        /// <param name="index">The DASH index. See: <see cref="IStreamCommand.Index"></see></param>
        /// <param name="bitrate">The bitrate to stamp on the DASH stream. This can be different than the actual bitrate used for the encode.</param>
        /// <param name="copySourceStream">If true then vcodec will be set to copy. Only enable this if you're sure the source is h264 and your target playback device supports the pix_format and profile.</param>
        /// <param name="path">The output path on disk for this stream.</param>
        /// <param name="additionalFlags">Any additional arbitrary flags to pass to ffmpeg for this stream. Can be used to partially bypass the fluent API.</param>
        public FFmpegH264VideoCommandBuilder(int index, int bitrate, bool copySourceStream, string path, ICollection<string> additionalFlags = null)
        {
            this.index = index;
            this.bitrate = bitrate;
            this.path = path;
            commands = new List<string>
            {
                $"-map 0:{index}",
                $"-vcodec {(copySourceStream ? "copy" : "libx264")}"
            };
            if (additionalFlags != null && additionalFlags.Any())
            {
                commands.AddRange(additionalFlags);
            }
        }

        /// <summary>
        /// Builds a video stream object from all the parameters and arguments specified so far.
        /// The output path is added to the end for ffmpeg. This is an idempotent operation.
        /// </summary>
        public VideoStreamCommand Build()
        {
            return new VideoStreamCommand
            {
                Index = index,
                Bitrate = bitrate.ToString(),
                Path = path,
                Argument = string.Join(" ", commands.Union(new List<string>(){ $"\"{ path}\""}))
            };
        }

        /// <summary>
        /// Applies a constrained bitrate using the video buffer verifier if the target bitrate is greater than zero.
        /// </summary>
        /// <param name="targetKBitrate">The average bitrate to target.</param>
        /// <param name="maxBitrateFactor">A multiplier of the target bitrate to use as the cap. This cap is hard per average bitrate within a buffer period.</param>
        /// <param name="bufferFactor">The ffmpeg bufsize is set to (targetKBitrate * maxBitrateFactor * bufferFactor).<para/>
        /// Larger values helps avoid crushing small bursts of complexity in a scene. Smaller values makes the streaming bitrate more smooth.<br/>
        /// For VOD streaming, values from 1-3 are typically sane.</param>
        public FFmpegH264VideoCommandBuilder WithAverageBitrate(int targetKBitrate, float maxBitrateFactor = 1.5f, float bufferFactor = 2f)
        {
            if (targetKBitrate <= 0)
            {
                return this;
            }

            int maxBitrate = (int)(targetKBitrate * maxBitrateFactor);
            int bufferSize = (int)(maxBitrate * bufferFactor);
            commands.Add($"-b:v {targetKBitrate}k -maxrate {maxBitrate}k -bufsize {bufferSize}k");
            return this;
        }

        /// <summary>
        /// Applies a parameter for the framerate if the given value is greater than zero.
        /// </summary>
        /// <param name="framerate">The framerate in frames per second.</param>
        public FFmpegH264VideoCommandBuilder WithFramerate(decimal framerate)
        {
            if (framerate <= 0)
            {
                return this;
            }
            commands.Add($"-r {framerate}");
            return this;
        }

        /// <summary>
        /// Applies a parameter for the keyint if the given value is greater than zero.
        /// </summary>
        /// <param name="keyframeInterval">The keyframe interval in frames. Generally this is 3x the framerate.</param>
        /// <param name="scenecut">Leave at zero to force a keyframe on every interval only. Higher values allow keyframes between intervals.</param>
        public FFmpegH264VideoCommandBuilder WithKeyframeInteval(decimal keyframeInterval, int scenecut = 0)
        {
            if (keyframeInterval <= 0)
            {
                return this;
            }
            commands.Add($"-x264-params keyint={keyframeInterval}:scenecut={scenecut}");
            return this;
        }

        /// <summary>
        /// Applies a parameter to specify the pix_format.
        /// </summary>
        /// <param name="format">The format to use. yuv420p is widely supported.</param>
        /// <returns></returns>
        public FFmpegH264VideoCommandBuilder WithPixelFormat(string format)
        {
            TryAddSimpleCommand($"-pix_fmt {format}", format);
            return this;
        }

        /// <summary>
        /// Applies an ffmpeg preset to set encoding performance.
        /// </summary>
        public FFmpegH264VideoCommandBuilder WithPreset(H264Preset preset)
        {
            if (preset == H264Preset.None)
            {
                return this;
            }

            TryAddSimpleCommand($"-preset {preset.ToString().ToLowerInvariant()}", preset.ToString());
            return this;
        }

        /// <summary>
        /// Applies an H264 profile.
        /// </summary>
        public FFmpegH264VideoCommandBuilder WithProfile(H264Profile profile)
        {
            if (profile == H264Profile.None)
            {
                return this;
            }
            TryAddSimpleCommand($"-profile:v {profile.ToString().ToLowerInvariant()}", profile.ToString());
            return this;
        }

        /// <summary>
        /// Applies an H264 profile level.
        /// </summary>
        /// <param name="level">The level to use. Typically 4, 4.1, 4.2 or 5.</param>
        public FFmpegH264VideoCommandBuilder WithProfileLevel(string level)
        {
            TryAddSimpleCommand($"-level {level}", level);
            return this;
        }

        /// <summary>
        /// Applies a resolution constraint from a quality's width and height if both values are greater than zero
        /// </summary>
        /// <param name="quality">The quality to derive the width and height from.</param>
        public FFmpegH264VideoCommandBuilder WithSize(IQuality quality)
        {
            if (quality.Width <= 0 || quality.Height <= 0)
            {
                return this;
            }
            commands.Add($"-s {quality.Width}x{quality.Height}");
            return this;
        }

        /// <summary>
        /// Adds a simple command if the <paramref name="value"/> is not empty
        /// </summary>
        /// <remarks>
        /// The point of this method is to reduce duplicated string.IsNullOrEmpty checks for the same command types
        /// </remarks>
        /// <param name="command">The fully qualified command assuming value is not empty</param>
        /// <param name="value">The value of the command, if this is empty the provided command is discarded</param>
        private void TryAddSimpleCommand(string command, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            commands.Add(command);
        }
    }
}