using DEnc.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DEnc.Commands
{
    internal class CommandBuilder2 : ICommandBuilder
    {
        string inputPath;
        string outputDirectory;
        string outputBaseFilename;
        bool enableStreamCopying;
        IEncodeOptions options;

        List<StreamVideoFile> videoFiles;
        List<StreamAudioFile> audioFiles;
        List<StreamSubtitleFile> subtitleFiles;

        private CommandBuilder2(string inPath, string outDirectory, string outBaseFilename, IEncodeOptions options, bool enableStreamCopying)
        {
            inputPath = inPath;
            outputDirectory = outDirectory;
            outputBaseFilename = outBaseFilename;
            this.options = options;
            this.enableStreamCopying = enableStreamCopying;

            videoFiles = new List<StreamVideoFile>();
            audioFiles = new List<StreamAudioFile>();
            subtitleFiles = new List<StreamSubtitleFile>();
        }
        private ICollection<string> AdditionalVideoFlags => options?.AdditionalVideoFlags;
        private ICollection<string> AdditionalAudioFlags => options?.AdditionalAudioFlags;
        private ICollection<string> AdditionalFlags => options?.AdditionalFlags;

        internal static ICommandBuilder Initilize(string inPath, string outDirectory, string outBaseFilename, IEncodeOptions options, bool enableStreamCopying)
        {
            ICommandBuilder builder = new CommandBuilder2(inPath, outDirectory, outBaseFilename, options, enableStreamCopying);
            return builder;
        }

        public CommandBuildResult2 Build()
        {

            var additionalFlags = AdditionalFlags ?? new List<string>();
            string initialArgs = $"-i \"{inputPath}\" -y -hide_banner";

            List<string> allCommands = new List<string>();
            allCommands.Add(initialArgs);
            allCommands.AddRange(AdditionalFlags);
            allCommands.AddRange(videoFiles.Select(x => x.Argument));
            allCommands.AddRange(audioFiles.Select(x => x.Argument));
            allCommands.AddRange(subtitleFiles.Select(x => x.Argument));

            string parameters = String.Join("\t", allCommands);

            return new CommandBuildResult2(parameters, videoFiles, audioFiles, subtitleFiles);
        }

        public ICommandBuilder WithVideoCommands(IEnumerable<MediaStream> videoStreams, IEnumerable<IQuality> qualities, int framerate, int keyframeInterval, int defaultBitrate)
        {
            // TODO: TEMP(ish) for now, ideally the caller could call all the appropriate builder methods
            foreach (MediaStream video in videoStreams)
            {
                if (!video.IsStreamValid())
                {
                    continue;
                }

                foreach (IQuality quality in qualities)
                {
                    bool copyThisStream = enableStreamCopying && quality.Bitrate == 0;
                    string path = Path.Combine(outputDirectory, $"{outputBaseFilename}_{(quality.Bitrate == 0 ? "original" : quality.Bitrate.ToString())}.mp4");

                    IVideoCommandBuilder videoBuilder = VideoCommandBuilder.Initilize(video.index, quality.Bitrate, path, AdditionalVideoFlags);

                    if (!copyThisStream)
                    {
                        videoBuilder
                            .WithSize(quality)
                            .WithBitrate(quality.Bitrate, defaultBitrate)
                            .WithPreset(quality.Preset)
                            .WithProfile(quality.Profile)
                            .WithProfileLevel(quality.Level)
                            .WithPixelFormat(quality.PixelFormat);
                    }

                    StreamVideoFile videoStream = videoBuilder
                        .WithFramerate(framerate)
                        .WithVideoCodec(video.codec_name, keyframeInterval, copyThisStream)
                        .Build();

                    videoFiles.Add(videoStream);
                }
            }

            return this;
        } 

        public ICommandBuilder WithAudioCommands(IEnumerable<MediaStream> streams)
        {
            if (!streams.Any())
            {
                return this;
            }

            foreach (MediaStream audioStream in streams)
            {
                IAudioCommandBuilder builder = AudioCommandBuilder.Initilize(audioStream, outputDirectory, outputBaseFilename, AdditionalAudioFlags);
                StreamAudioFile streamFile = builder
                    .WithLanguage()
                    .WithTitle()
                    .WithCodec()
                    .Build();

                audioFiles.Add(streamFile);
            }
            return this;
        }

        public ICommandBuilder WithSubtitleCommands(IEnumerable<MediaStream> streams)
        {
            if (!streams.Any())
            {
                return this;
            }

            foreach (MediaStream subtitleStream in streams)
            {
                if (!Constants.SupportedSubtitleCodecs.Contains(subtitleStream.codec_name))
                {
                    continue;
                }

                string language = subtitleStream.tag
                    .Where(x => x.key == "language")
                    .Select(x => x.value)
                    .FirstOrDefault();
                if(language is null) language = "und";

                string path = Path.Combine(outputDirectory, $"{outputBaseFilename}_subtitle_{language}_{subtitleStream.index}.vtt");

                StreamSubtitleFile command = new StreamSubtitleFile()
                {
                    Type = StreamType.Subtitle,
                    Index = subtitleStream.index,
                    Language = language,
                    Path = path,
                    Argument = string.Join(" ", new string[]
                    {
                            $"-map 0:{subtitleStream.index}",
                            '"' + path + '"'
                    })
                };
                subtitleFiles.Add(command);
            }
            return this;
        }
    }

    internal class VideoCommandBuilder : IVideoCommandBuilder
    {
        int index;
        int bitrate;
        string path;
        List<string> commands;

        public static IVideoCommandBuilder Initilize(int index, int bitrate, string path, ICollection<string> additionalFlags)
        {
            IVideoCommandBuilder builder = new VideoCommandBuilder(index, bitrate, path, additionalFlags);
            return builder;
        }

        private VideoCommandBuilder(int index, int bitrate, string path, ICollection<string> additionalFlags)
        {
            this.index = index;
            this.bitrate = bitrate;
            this.path = path;
            commands = new List<string>();

            commands.Add($"-map 0:{index}");
            if(additionalFlags != null && additionalFlags.Any())
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

        public IVideoCommandBuilder WithSize(IQuality quality)
        {
            if (quality.Width == 0 || quality.Height == 0)
            {
                return this;
            }
            commands.Add($"-s {quality.Width}x{quality.Height}");
            return this;
        }

        public IVideoCommandBuilder WithBitrate(int bitrate)
        {
            if (bitrate == 0)
            {
                return this;
            }
            commands.Add($"-b:v {bitrate}k");
            return this;
        }

        public IVideoCommandBuilder WithBitrate(int bitrate, int defaultBitrate)
        {
            if (bitrate == 0)
            {
                commands.Add($"-b:v {defaultBitrate}k");
                return this;
            }
            commands.Add($"-b:v {bitrate}k");
            return this;
        }

        public IVideoCommandBuilder WithPreset(string preset)
        {
            TryAddSimpleCommand($"-preset {preset}", preset);
            return this;
        }

        public IVideoCommandBuilder WithProfile(string profile)
        {
            TryAddSimpleCommand($"-profile:v {profile}", profile);
            return this;
        }

        public IVideoCommandBuilder WithProfileLevel(string level)
        {
            TryAddSimpleCommand($"-level {level}", level);
            return this;
        }

        public IVideoCommandBuilder WithPixelFormat(string format)
        {
            TryAddSimpleCommand($"-pix_fmt {format}", format);
            return this;
        }

        public IVideoCommandBuilder WithFramerate(int framerate)
        {
            if(framerate == 0)
            {
                return this;
            }
            commands.Add($"-r {framerate}");
            return this;
        }

        public IVideoCommandBuilder WithVideoCodec(string sourceCodec, int keyframeInterval, bool enableCopy)
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

    internal class AudioCommandBuilder : IAudioCommandBuilder
    {
        List<string> commands;

        MediaStream audioStream;
        string outputDirectory;
        string outputBaseFilename;
        bool codecSupported;

        string path;
        string language;
        string title;

        public static IAudioCommandBuilder Initilize(MediaStream audioStream, string outputDirectory, string outputBaseFilename, ICollection<string> additionalFlags)
        {
            IAudioCommandBuilder builder = new AudioCommandBuilder(audioStream, outputDirectory, outputBaseFilename, additionalFlags);
            return builder;
        }

        private AudioCommandBuilder(MediaStream audioStream, string outputDirectory, string outputBaseFilename, ICollection<string> additionalFlags)
        {
            this.audioStream = audioStream;
            this.outputDirectory = outputDirectory;
            this.outputBaseFilename = outputBaseFilename;
            commands = new List<string>();

            codecSupported = Constants.SupportedCodecs.ContainsKey(audioStream.codec_name);

            commands.Add($"-map 0:{audioStream.index}");
            if (additionalFlags != null && additionalFlags.Any())
            {
                commands.AddRange(additionalFlags);
            }
        }

        public StreamAudioFile Build()
        {
            path = Path.Combine(outputDirectory, $"{outputBaseFilename}_audio_{language}_{audioStream.index}.mp4");
            commands.Add($"\"{path}\"");

            return new StreamAudioFile
            {
                Type = StreamType.Audio,
                Index = audioStream.index,
                Name = $"{language} {title}",
                Path = path,
                Argument = string.Join(" ", commands)
            };
        }

        public IAudioCommandBuilder WithLanguage()
        {
            string language = audioStream.tag
                .Where(x => x.key == "language")
                .Select(x => x.value)
                .FirstOrDefault();

            if(language is null)
            {
                language = audioStream.disposition.@default > 0 ? "default" : "und";
            }
            this.language = language;
            return this;
        }

        public IAudioCommandBuilder WithTitle()
        {
            string title = audioStream.tag
                .Where(x => x.key == "title")
                .Select(x => x.value)
                .FirstOrDefault();

            if(title is null)
            {
                title = audioStream.index.ToString();
            }
            this.title = title;
            return this;
        }

        public IAudioCommandBuilder WithCodec()
        {
            if (codecSupported)
            {
                return this;
            }
            commands.Add($"-c:a aac -b:a {audioStream.bit_rate * 1.1}");
            return this;
        }
    }

    internal interface ICommandBuilder
    {
        CommandBuildResult2 Build();
        ICommandBuilder WithVideoCommands(IEnumerable<MediaStream> videoStreams, IEnumerable<IQuality> qualities, int framerate, int keyframeInterval, int defaultBitrate);
        ICommandBuilder WithAudioCommands(IEnumerable<MediaStream> streams);
        ICommandBuilder WithSubtitleCommands(IEnumerable<MediaStream> streams);
    }

    internal interface IVideoCommandBuilder
    {
        StreamVideoFile Build();
        IVideoCommandBuilder WithSize(IQuality quality);
        IVideoCommandBuilder WithBitrate(int bitrate);
        IVideoCommandBuilder WithBitrate(int bitrate, int defaultBitrate);
        IVideoCommandBuilder WithPreset(string preset);
        IVideoCommandBuilder WithProfile(string profile);
        IVideoCommandBuilder WithProfileLevel(string level);
        IVideoCommandBuilder WithPixelFormat(string format);
        IVideoCommandBuilder WithFramerate(int framerate);
        IVideoCommandBuilder WithVideoCodec(string sourceCodec, int keyInterval, bool enableCopy);
    }

    internal interface IAudioCommandBuilder
    {
        StreamAudioFile Build();
        IAudioCommandBuilder WithLanguage();
        IAudioCommandBuilder WithTitle();
        IAudioCommandBuilder WithCodec();
    }
}
