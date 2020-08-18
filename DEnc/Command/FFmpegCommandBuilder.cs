using DEnc.Models;
using DEnc.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DEnc.Commands
{
    /// <summary>
    /// Provides a very high level mechanism for generating an ffmpeg command which extracts streams from an input media file and prepares them for DASH encoding.
    /// </summary>
    public class FFmpegCommandBuilder
    {
        /// <summary>
        /// Contains the generated set of audio stream commands.
        /// </summary>
        protected readonly List<AudioStreamCommand> audioFiles = new List<AudioStreamCommand>();

        /// <summary>
        /// Contains the generated set of subtitle commands.
        /// </summary>
        protected readonly List<SubtitleStreamCommand> subtitleFiles = new List<SubtitleStreamCommand>();

        /// <summary>
        /// Contains the generated set of video stream commands.
        /// </summary>
        protected readonly List<VideoStreamCommand> videoFiles = new List<VideoStreamCommand>();

        /// <inheritdoc cref="FFmpegCommandBuilder"/>
        /// <param name="inPath">Sets <see cref="InputPath">InputPath</see></param>
        /// <param name="outDirectory">Sets <see cref="OutputDirectory">OutputDirectory</see></param>
        /// <param name="outBaseFilename">Sets <see cref="OutputBaseFilename">OutputBaseFilename</see></param>
        /// <param name="options">Sets <see cref="Options">Options</see></param>
        /// <param name="enableStreamCopying">Sets <see cref="EnableStreamCopying">EnableStreamCopying</see></param>
        public FFmpegCommandBuilder(string inPath, string outDirectory, string outBaseFilename, IEncodeOptions options, bool enableStreamCopying)
        {
            InputPath = inPath;
            OutputDirectory = outDirectory;
            OutputBaseFilename = outBaseFilename;
            Options = options;
            EnableStreamCopying = enableStreamCopying;
        }

        /// <summary>
        /// Enables video stream copying on the zero quality.
        /// </summary>
        public bool EnableStreamCopying { get; set; } = true;

        /// <summary>
        /// The absolute file path to the input file.
        /// </summary>
        public string InputPath { get; protected set; }

        /// <summary>
        /// Extra flags which are passed to ffmpeg.
        /// </summary>
        public IEncodeOptions Options { get; protected set; }

        /// <summary>
        /// The base filename given to output stream files.
        /// </summary>
        public string OutputBaseFilename { get; protected set; }

        /// <summary>
        /// The file path output stream files are stored in.
        /// </summary>
        public string OutputDirectory { get; protected set; }

        /// <summary>
        /// Generates an ffmpeg command from the internally assembled substream commands. This operation is idempotent.
        /// </summary>
        public virtual FFmpegCommand Build()
        {
            var additionalFlags = Options.AdditionalFlags ?? new List<string>();
            List<string> initialArgs = new List<string>() { $"-i \"{InputPath}\" -y -hide_banner" };
            initialArgs.AddRange(additionalFlags);

            return new FFmpegCommand(initialArgs, videoFiles, audioFiles, subtitleFiles);
        }

        /// <summary>
        /// Generates and appends commands for the given audio streams to the internal command set.
        /// </summary>
        public virtual FFmpegCommandBuilder WithAudioCommands(IEnumerable<MediaStream> streams, AudioConfig config)
        {
            if (!streams.Any())
            {
                return this;
            }

            foreach (MediaStream audioStream in streams)
            {
                int maxBitrate = audioStream.channels * config.MaxPerChannelBitrate;
                FFmpegAudioCommandBuilder builder = new FFmpegAudioCommandBuilder(audioStream, OutputDirectory, OutputBaseFilename, Options.AdditionalAudioFlags);
                var streamFile = builder
                    .WithLanguage()
                    .WithTitle()
                    .WithCodec(maxBitrate: maxBitrate)
                    .WithDownmix(config.DownmixMode)
                    .Build();

                audioFiles.Add(streamFile);
            }
            return this;
        }

        /// <summary>
        /// Generates and appends commands for the given subtitle streams to the internal command set.
        /// </summary>
        public virtual FFmpegCommandBuilder WithSubtitleCommands(IEnumerable<MediaStream> streams)
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

                string path = Path.Combine(OutputDirectory, $"{OutputBaseFilename}_subtitle_{language}_{subtitleStream.index}.vtt");

                SubtitleStreamCommand command = new SubtitleStreamCommand()
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

        /// <summary>
        /// Generates and appends commands for the given video streams to the internal command set.
        /// </summary>
        public virtual FFmpegCommandBuilder WithVideoCommands(IEnumerable<MediaStream> videoStreams, IEnumerable<IQuality> qualities, int framerate, int keyframeInterval, int defaultBitrate)
        {
            foreach (MediaStream video in videoStreams)
            {
                if (!video.IsStreamValid())
                {
                    continue;
                }

                foreach (IQuality quality in qualities)
                {
                    bool copyThisStream = EnableStreamCopying && quality.Bitrate == 0 && video.codec_name.ToLowerInvariant() == "h264";
                    string path = Path.Combine(OutputDirectory, $"{OutputBaseFilename}_{(quality.Bitrate == 0 ? "original" : quality.Bitrate.ToString())}.mp4");

                    FFmpegH264VideoCommandBuilder videoBuilder = new FFmpegH264VideoCommandBuilder(video.index, quality.Bitrate, copyThisStream, path, Options.AdditionalVideoFlags);

                    if (!copyThisStream)
                    {
                        videoBuilder
                            .WithSize(quality)
                            .WithAverageBitrate(quality.Bitrate > 0 ? quality.Bitrate : defaultBitrate)
                            .WithPreset(quality.Preset)
                            .WithProfile(quality.Profile)
                            .WithProfileLevel(quality.Level)
                            .WithPixelFormat(quality.PixelFormat);
                    }

                    VideoStreamCommand videoStream = videoBuilder
                        .WithFramerate(framerate)
                        .WithKeyframeInteval(keyframeInterval)
                        .Build();

                    videoFiles.Add(videoStream);
                }
            }

            return this;
        }
    }
}