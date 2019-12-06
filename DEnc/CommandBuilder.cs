using DEnc.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

    internal class CommandBuildResult
    {
        public string RenderedCommand { get; private set; }
        public IEnumerable<StreamFile> CommandPieces { get; private set; }

        internal CommandBuildResult(string commandArguments, IEnumerable<StreamFile> commands)
        {
            RenderedCommand = commandArguments;
            CommandPieces = commands;
        }
    }

    internal class StreamFile
    {
        public StreamType Type { get; set; }
        public int Origin { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Argument { get; set; }
    }

    internal class Codec
    {
        public string Name { get; private set; }
        public string Container { get; private set; }
        public string Extension { get; private set; }

        public Codec(string name, string container, string extension)
        {
            Name = name;
            Container = container;
            Extension = extension;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    internal static class CommandBuilder
    {
        public static IReadOnlyDictionary<string, Codec> SupportedCodecs { get; } = new List<Codec>()
        {
            new Codec("opus", "ogg", "ogg"),
            new Codec("aac", "mp4", "aac"),
            new Codec("mp3", "mp3", "mp3"),
            new Codec("h264", "mp4", "mp4"),
            new Codec("vp8", "webm", "webm")
        }.ToDictionary(x => x.Name);

        private static bool CheckStreamValid(MediaStream stream)
        {
            if (stream == null) { return false; }

            string taggedMimetype = null;
            string taggedFilename = null;
            string taggedBitsPerSecond = null;

            if (stream.tag != null)
            {
                foreach (var tag in stream.tag)
                {
                    switch (tag.key.ToUpper())
                    {
                        case "BPS":
                            taggedBitsPerSecond = tag.value;
                            break;
                        case "MIMETYPE":
                            taggedMimetype = tag.value;
                            break;
                        case "FILENAME":
                            taggedFilename = tag.value;
                            break;
                    }
                }
            }
            if (taggedMimetype != null && taggedMimetype.ToUpper().StartsWith("IMAGE/")) { return false; }
            if ((stream.bit_rate == 0 || (!string.IsNullOrWhiteSpace(taggedBitsPerSecond) && taggedBitsPerSecond != "0")) && stream.avg_frame_rate == "0/0") { return false; }

            return true;
        }

        private static IEnumerable<StreamFile> BuildVideoCommands(IEnumerable<MediaStream> streams, IEnumerable<IQuality> qualities, ICollection<string> additionalFlags, int framerate, int keyframeInterval, int defaultBitrate, bool enableStreamCopying, string outDirectory, string outFilename)
        {
            additionalFlags = additionalFlags ?? new List<string>();

            var getSize = new Func<IQuality, string>(x => { return (x.Width == 0 || x.Height == 0) ? "" : $"-s {x.Width}x{x.Height}"; });
            var getBitrate = new Func<int, string>(x => { return (x == 0) ? "" : $"-b:v {x}k"; });
            var getPreset = new Func<string, string>(x => { return (string.IsNullOrEmpty(x)) ? "" : $"-preset {x}"; });
            var getProfile = new Func<string, string>(x => { return (string.IsNullOrEmpty(x)) ? "" : $"-profile:v {x}"; });
            var getProfileLevel = new Func<string, string>(x => { return (string.IsNullOrEmpty(x)) ? "" : $"-level {x}"; });
            var getPixelFormat = new Func<string, string>(x => { return (string.IsNullOrEmpty(x)) ? "" : $"-pix_fmt {x}"; });
            var getFramerate = new Func<int, string>(x => { return (x == 0) ? "" : $"-r {x}"; });
            var getFilename = new Func<string, string, int, string>((path, filename, bitrate) => { return Path.Combine(path, $"{filename}_{(bitrate == 0 ? "original" : bitrate.ToString())}.mp4"); });

            var getVideoCodec = new Func<string, bool, int, string>((sourceCodec, enableCopy, keyInterval) =>
            {
                string defaultCoding = $"-x264-params keyint={keyframeInterval}:scenecut=0";
                switch (sourceCodec)
                {
                    case "h264":
                        return $"-vcodec {(enableCopy ? "copy" : "libx264")} {defaultCoding}";
                    default:
                        return $"-vcodec libx264 {defaultCoding}";
                }
            });

            var output = new List<StreamFile>();
            foreach (var stream in streams)
            {
                if (!CheckStreamValid(stream)) { continue; }

                foreach (var quality in qualities)
                {
                    string path = getFilename(outDirectory, outFilename, quality.Bitrate);
                    bool copyThisStream = enableStreamCopying && quality.Bitrate == 0;
                    var command = new StreamFile
                    {
                        Type = StreamType.Video,
                        Origin = stream.index,
                        Name = quality.Bitrate.ToString(),
                        Path = path,
                        Argument = $"-map 0:{stream.index} " + string.Join(" ", additionalFlags.Concat(new string[]
                        {
                            copyThisStream ? "" : getSize(quality),
                            copyThisStream ? "" : getBitrate(quality.Bitrate == 0 ? defaultBitrate : quality.Bitrate),
                            copyThisStream ? "" : getPreset(quality.Preset),
                            copyThisStream ? "" : getProfile(quality.Profile),
                            copyThisStream ? "" : getProfileLevel(quality.Level),
                            copyThisStream ? "" : getPixelFormat(quality.PixelFormat),
                            getFramerate(framerate),
                            getVideoCodec(stream.codec_name, copyThisStream, keyframeInterval),
                            '"' + path + '"'
                        }))
                    };

                    output.Add(command);
                }
            }
            return output;
        }

        private static IEnumerable<StreamFile> BuildAudioCommands(IEnumerable<MediaStream> streams, ICollection<string> additionalFlags, string outDirectory, string outFilename)
        {
            additionalFlags = additionalFlags ?? new List<string>();

            var output = new List<StreamFile>();
            foreach (var stream in streams)
            {
                bool codecSupported = SupportedCodecs.ContainsKey(stream.codec_name);
                string language = stream.tag.Where(x => x.key == "language").Select(x => x.value).FirstOrDefault() ?? ((stream.disposition.@default > 0) ? "default" : "und");
                string path = Path.Combine(outDirectory, $"{outFilename}_audio_{language}_{stream.index}.mp4");
                string codec = codecSupported ? "" : $"-c:a aac -b:a {stream.bit_rate * 1.1}";

                var command = new StreamFile
                {
                    Type = StreamType.Audio,
                    Origin = stream.index,
                    Name = language,
                    Path = path,
                    Argument = $"-map 0:{stream.index} " + string.Join(" ", additionalFlags.Concat(new string[]
                    {
                            codec,
                            '"' + path + '"'
                    }))
                };

                output.Add(command);
            }
            return output;
        }

        private static IEnumerable<StreamFile> BuildSubtitleCommands(IEnumerable<MediaStream> streams, string outDirectory, string outFilename)
        {
            var supportedCodecs = new List<string>()
            {
                "webvtt",
                "ass",
                "mov_text",
                "subrip",
                "text"
            };

            var output = new List<StreamFile>();
            foreach (var stream in streams)
            {
                if (!supportedCodecs.Contains(stream.codec_name)) { continue; }
                string language = stream.tag.Where(x => x.key == "language").Select(x => x.value).FirstOrDefault() ?? "und";
                string path = Path.Combine(outDirectory, $"{outFilename}_subtitle_{language}_{stream.index}.vtt");

                var command = new StreamFile
                {
                    Type = StreamType.Subtitle,
                    Origin = stream.index,
                    Name = language,
                    Path = path,
                    Argument = string.Join(" ", new string[]
                    {
                            $"-map 0:{stream.index}",
                            '"' + path + '"'
                    })
                };

                output.Add(command);
            }
            return output;
        }

        /// <summary>
        /// Builds the arguments for the CLI version of ffmpeg.
        /// </summary>
        /// <param name="inPath">The source file to encode.</param>
        /// <param name="outDirectory">The directory to place output file in.</param>
        /// <param name="outFilename">The base filename to use when naming output files (format is [outFilename]_[qualityBitrate].mp4).</param>
        /// <param name="options">Flags to pass to the encoder.</param>
        /// <param name="framerate">The output framerate.</param>
        /// <param name="keyframeInterval">The output key interval. For best results should be a multiple of framerate.</param>
        /// <param name="metadata">Metadata on the encoded stream.</param>
        /// <param name="qualities">A collection of qualities to encode to. Entries in the collection must have a distinct bitrate, otherwise behavior is undefined.</param>
        /// <param name="defaultBitrate">The bitrate to use for the copy quality. Typically the input file bitrate.</param>
        /// <param name="enableStreamCopying">Set true to enable -vcodec copy for the copy quality.</param>
        internal static CommandBuildResult BuildFfmpegCommand(
            string inPath,
            string outDirectory,
            string outFilename,
            IEncodeOptions options,
            IEnumerable<IQuality> qualities,
            int framerate,
            int keyframeInterval,
            MediaMetadata metadata,
            int defaultBitrate,
            bool enableStreamCopying)
        {

            var videoCommand = BuildVideoCommands(metadata.VideoStreams, qualities, options.AdditionalVideoFlags, framerate, keyframeInterval, defaultBitrate, enableStreamCopying, outDirectory, outFilename);
            var audioCommand = BuildAudioCommands(metadata.AudioStreams, options.AdditionalAudioFlags, outDirectory, outFilename);
            var subtitleCommand = BuildSubtitleCommands(metadata.SubtitleStreams, outDirectory, outFilename);

            var allCommands = videoCommand.Concat(audioCommand).Concat(subtitleCommand);

            var additionalFlags = options.AdditionalFlags ?? new List<string>();
            string parameters = $"-i \"{inPath}\" -y -hide_banner\t{string.Join("\t", additionalFlags.Concat(allCommands.Select(x => x.Argument)))}";

            return new CommandBuildResult(parameters, allCommands);
        }

        /// <summary>
        /// Builds the arguments for the CLI version of Mp4Box.
        /// </summary>
        /// <param name="inFiles">A collection of full paths to input files to encode from.</param>
        /// <param name="outFilePath">The full path to write the mpd file to.</param>
        /// <param name="keyInterval">The key interval in milliseconds. This can be derived from: (keyframeInterval / framerate) * 1000</param>
        /// <param name="flags">Additional flags to pass to MP4Box. Should include a profile</param>
        internal static CommandBuildResult BuildMp4boxMpdCommand(IEnumerable<string> inFiles, string outFilePath, int keyInterval, ICollection<string> flags)
        {
            flags = flags ?? new List<string>();

            flags.Add($"-dash {keyInterval}");
            flags.Add($"-out \"{outFilePath}\"");

            string parameters = $"{string.Join("\t", flags)}\t--\t{string.Join("\t", inFiles.Select(x => '"' + x + '"'))}";
            return new CommandBuildResult(parameters, new List<StreamFile>() { new StreamFile() { Path = outFilePath } });
        }
    }
}
