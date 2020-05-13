using DEnc.Commands.Interfaces;
using DEnc.Models;
using DEnc.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DEnc.Commands
{
    internal class FFmpegVideoCommandBuilder : IFFmpegVideoCommandBuilder
    {
        int index;
        int bitrate;
        string path;
        List<string> commands;

        public static IFFmpegVideoCommandBuilder Initilize(int index, int bitrate, string path, ICollection<string> additionalFlags)
        {
            IFFmpegVideoCommandBuilder builder = new FFmpegVideoCommandBuilder(index, bitrate, path, additionalFlags);
            return builder;
        }

        private FFmpegVideoCommandBuilder(int index, int bitrate, string path, ICollection<string> additionalFlags)
        {
            this.index = index;
            this.bitrate = bitrate;
            this.path = path;
            commands = new List<string>();

            commands.Add($"-map 0:{index}");
            if (additionalFlags != null && additionalFlags.Any())
            {
                commands.AddRange(additionalFlags);
            }
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

        public IFFmpegVideoCommandBuilder WithSize(IQuality quality)
        {
            if (quality.Width == 0 || quality.Height == 0)
            {
                return this;
            }
            commands.Add($"-s {quality.Width}x{quality.Height}");
            return this;
        }

        public IFFmpegVideoCommandBuilder WithBitrate(int bitrate)
        {
            if (bitrate == 0)
            {
                return this;
            }
            commands.Add($"-b:v {bitrate}k");
            return this;
        }

        public IFFmpegVideoCommandBuilder WithBitrate(int bitrate, int defaultBitrate)
        {
            if (bitrate == 0)
            {
                commands.Add($"-b:v {defaultBitrate}k");
                return this;
            }
            commands.Add($"-b:v {bitrate}k");
            return this;
        }

        public IFFmpegVideoCommandBuilder WithPreset(H264Preset preset)
        {
            TryAddSimpleCommand($"-preset {preset}", preset.ToString());
            return this;
        }

        public IFFmpegVideoCommandBuilder WithProfile(H264Profile profile)
        {
            TryAddSimpleCommand($"-profile:v {profile}", profile.ToString());
            return this;
        }

        public IFFmpegVideoCommandBuilder WithProfileLevel(string level)
        {
            TryAddSimpleCommand($"-level {level}", level);
            return this;
        }

        public IFFmpegVideoCommandBuilder WithPixelFormat(string format)
        {
            TryAddSimpleCommand($"-pix_fmt {format}", format);
            return this;
        }

        public IFFmpegVideoCommandBuilder WithFramerate(int framerate)
        {
            if (framerate == 0)
            {
                return this;
            }
            commands.Add($"-r {framerate}");
            return this;
        }

        public IFFmpegVideoCommandBuilder WithVideoCodec(string sourceCodec, int keyframeInterval, bool enableCopy)
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
