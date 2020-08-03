using DEnc.Models;
using DEnc.Models.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace DEnc.Commands
{
    internal class FFmpegVideoCommandBuilder
    {
        private readonly int bitrate;
        private readonly List<string> commands;
        private readonly int index;
        private readonly string path;
        private FFmpegVideoCommandBuilder(int index, int bitrate, string path, ICollection<string> additionalFlags)
        {
            this.index = index;
            this.bitrate = bitrate;
            this.path = path;
            commands = new List<string>
            {
                $"-map 0:{index}"
            };
            if (additionalFlags != null && additionalFlags.Any())
            {
                commands.AddRange(additionalFlags);
            }
        }

        public static FFmpegVideoCommandBuilder Initilize(int index, int bitrate, string path, ICollection<string> additionalFlags)
        {
            FFmpegVideoCommandBuilder builder = new FFmpegVideoCommandBuilder(index, bitrate, path, additionalFlags);
            return builder;
        }
        public StreamVideoFile Build()
        {
            commands.Add($"\"{ path}\"");

            return new StreamVideoFile
            {
                Type = StreamType.Video,
                Index = index,
                Bitrate = bitrate.ToString(),
                Path = path,
                Argument = string.Join(" ", commands)
            };
        }

        public FFmpegVideoCommandBuilder WithBitrate(int bitrate)
        {
            if (bitrate == 0)
            {
                return this;
            }
            commands.Add($"-b:v {bitrate}k");
            return this;
        }

        public FFmpegVideoCommandBuilder WithBitrate(int bitrate, int defaultBitrate)
        {
            if (bitrate == 0)
            {
                commands.Add($"-b:v {defaultBitrate}k");
                return this;
            }
            commands.Add($"-b:v {bitrate}k");
            return this;
        }

        public FFmpegVideoCommandBuilder WithFramerate(int framerate)
        {
            if (framerate == 0)
            {
                return this;
            }
            commands.Add($"-r {framerate}");
            return this;
        }

        public FFmpegVideoCommandBuilder WithPixelFormat(string format)
        {
            TryAddSimpleCommand($"-pix_fmt {format}", format);
            return this;
        }

        public FFmpegVideoCommandBuilder WithPreset(H264Preset preset)
        {
            if (preset == H264Preset.none)
            {
                return this;
            }

            TryAddSimpleCommand($"-preset {preset}", preset.ToString());
            return this;
        }

        public FFmpegVideoCommandBuilder WithProfile(H264Profile profile)
        {
            if (profile == H264Profile.none)
            {
                return this;
            }
            TryAddSimpleCommand($"-profile:v {profile}", profile.ToString());
            return this;
        }

        public FFmpegVideoCommandBuilder WithProfileLevel(string level)
        {
            TryAddSimpleCommand($"-level {level}", level);
            return this;
        }

        public FFmpegVideoCommandBuilder WithSize(IQuality quality)
        {
            if (quality.Width == 0 || quality.Height == 0)
            {
                return this;
            }
            commands.Add($"-s {quality.Width}x{quality.Height}");
            return this;
        }
        public FFmpegVideoCommandBuilder WithVideoCodec(string sourceCodec, int keyframeInterval, bool enableCopy)
        {
            string defaultCoding = $"-x264-params keyint={keyframeInterval}:scenecut=0";

            //TODO: Remove inline ternary and squash the switch statement
            switch (sourceCodec)
            {
                case "h264":
                    commands.Add($"-vcodec {(enableCopy ? "copy" : "libx264")} {defaultCoding}");
                    break;

                default:
                    commands.Add($"-vcodec libx264 {defaultCoding}");
                    break;
            }
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