using DEnc.Models;
using DEnc.Models.Interfaces;
using DEnc.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DEnc.Commands
{
    internal class FFmpegCommandBuilder
    {
        private readonly List<StreamAudioFile> audioFiles;
        private readonly bool enableStreamCopying;
        private readonly string inputPath;
        private readonly IEncodeOptions options;
        private readonly string outputBaseFilename;
        private readonly string outputDirectory;
        private readonly List<StreamSubtitleFile> subtitleFiles;
        private readonly List<StreamVideoFile> videoFiles;
        private FFmpegCommandBuilder(string inPath, string outDirectory, string outBaseFilename, IEncodeOptions options, bool enableStreamCopying)
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

        private ICollection<string> AdditionalAudioFlags => options?.AdditionalAudioFlags;
        private ICollection<string> AdditionalFlags => options?.AdditionalFlags;
        private ICollection<string> AdditionalVideoFlags => options?.AdditionalVideoFlags;
        public FFmpegCommand Build()
        {
            var additionalFlags = AdditionalFlags ?? new List<string>();
            string initialArgs = $"-i \"{inputPath}\" -y -hide_banner";

            List<string> allCommands = new List<string>
            {
                initialArgs
            };
            allCommands.AddRange(AdditionalFlags);
            allCommands.AddRange(videoFiles.Select(x => x.Argument));
            allCommands.AddRange(audioFiles.Select(x => x.Argument));
            allCommands.AddRange(subtitleFiles.Select(x => x.Argument));

            string parameters = String.Join("\t", allCommands);

            return new FFmpegCommand(parameters, videoFiles, audioFiles, subtitleFiles);
        }

        public FFmpegCommandBuilder WithAudioCommands(IEnumerable<MediaStream> streams)
        {
            if (!streams.Any())
            {
                return this;
            }

            foreach (MediaStream audioStream in streams)
            {
                FFmpegAudioCommandBuilder builder = FFmpegAudioCommandBuilder.Initilize(audioStream, outputDirectory, outputBaseFilename, AdditionalAudioFlags);
                StreamAudioFile streamFile = builder
                    .WithLanguage()
                    .WithTitle()
                    .WithCodec()
                    .Build();

                audioFiles.Add(streamFile);
            }
            return this;
        }

        public FFmpegCommandBuilder WithSubtitleCommands(IEnumerable<MediaStream> streams)
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
                if (language is null) language = "und";

                string path = Path.Combine(outputDirectory, $"{outputBaseFilename}_subtitle_{language}_{subtitleStream.index}.vtt");

                StreamSubtitleFile command = new StreamSubtitleFile()
                {
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

        public FFmpegCommandBuilder WithVideoCommands(IEnumerable<MediaStream> videoStreams, IEnumerable<IQuality> qualities, int framerate, int keyframeInterval, int defaultBitrate)
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

                    FFmpegVideoCommandBuilder videoBuilder = FFmpegVideoCommandBuilder.Initilize(video.index, quality.Bitrate, path, AdditionalVideoFlags);

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

        internal static FFmpegCommandBuilder Initilize(string inPath, string outDirectory, string outBaseFilename, IEncodeOptions options, bool enableStreamCopying)
        {
            FFmpegCommandBuilder builder = new FFmpegCommandBuilder(inPath, outDirectory, outBaseFilename, options, enableStreamCopying);
            return builder;
        }
    }
}