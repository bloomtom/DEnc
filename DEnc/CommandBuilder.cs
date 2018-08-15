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

        private static IEnumerable<StreamFile> BuildVideoCommands(IEnumerable<MediaStream> streams, IEnumerable<IQuality> qualities, int defaultBitrate, int framerate, int keyInterval, bool enableStreamCopying, string outDirectory, string outFilename)
        {
            var getSize = new Func<IQuality, string>(x => { return (x.Width == 0 || x.Height == 0) ? "" : $"-s {x.Width}x{x.Height}"; });
            var getBitrate = new Func<int, string>(x => { return (x == 0) ? "" : $"-b:v {x}k"; });
            var getPreset = new Func<IQuality, string>(x => { return (string.IsNullOrEmpty(x.Preset)) ? "" : $"-preset {x.Preset}"; });
            var getFramerate = new Func<int, string>(x => { return (x == 0) ? "" : $"-r {x}"; });
            var getFilename = new Func<string, string, int, string>((path, filename, bitrate) => { return Path.Combine(path, $"{filename}_{(bitrate == 0 ? "original" : bitrate.ToString())}.mp4"); });

            var getVideoCodec = new Func<string, bool, int, string>((sourceCodec, enableCopy, keyframeInterval) =>
            {
                switch (sourceCodec)
                {
                    case "h264":
                        return $"-vcodec {(enableCopy ? "copy" : "libx264")} -x264-params keyint={keyframeInterval}:scenecut=0";
                    default:
                        return $"-vcodec libx264 -x264-params keyint={keyframeInterval}:scenecut=0";
                }
            });

            var output = new List<StreamFile>();
            foreach (var stream in streams)
            {
                foreach (var quality in qualities)
                {
                    string path = getFilename(outDirectory, outFilename, quality.Bitrate);

                    var command = new StreamFile
                    {
                        Type = StreamType.Video,
                        Origin = stream.index,
                        Name = quality.Bitrate.ToString(),
                        Path = path,
                        Argument = string.Join(" ", new string[]
                        {
                            $"-map 0:{stream.index}",
                            "-sn",
                            "-map_metadata -1",
                            getSize(quality),
                            getBitrate(quality.Bitrate == 0 ? defaultBitrate : quality.Bitrate),
                            getPreset(quality),
                            getFramerate(framerate),
                            getVideoCodec(stream.codec_name, quality.Bitrate == 0 && enableStreamCopying, keyInterval),
                            '"' + path + '"'
                        })
                    };

                    output.Add(command);
                }
            }
            return output;
        }

        private static IEnumerable<StreamFile> BuildAudioCommands(IEnumerable<MediaStream> streams, string outDirectory, string outFilename)
        {
            var output = new List<StreamFile>();
            foreach (var stream in streams)
            {
                bool codecSupported = SupportedCodecs.ContainsKey(stream.codec_name);
                string language = stream.tag.Where(x => x.key == "language").Select(x => x.value).FirstOrDefault() ?? ((stream.disposition.@default > 0) ? "Default" : "Unknown");
                string path = Path.Combine(outDirectory, $"{outFilename}_audio_{language}_{stream.index}.mp4");
                string codec = codecSupported ? "" : $"-c:a aac -b:a {stream.bit_rate * 1.1}";

                var command = new StreamFile
                {
                    Type = StreamType.Audio,
                    Origin = stream.index,
                    Name = language,
                    Path = path,
                    Argument = string.Join(" ", new string[]
                    {
                            $"-map 0:{stream.index}",
                             "-sn",
                             "-map_metadata -1",
                            codec,
                            '"' + path + '"'
                    })
                };

                output.Add(command);
            }
            return output;
        }

        private static IEnumerable<StreamFile> BuildSubtitleCommands(IEnumerable<MediaStream> streams, string outDirectory, string outFilename)
        {
            var output = new List<StreamFile>();
            foreach (var stream in streams)
            {
                string language = stream.tag.Where(x => x.key == "language").Select(x => x.value).FirstOrDefault() ?? "Unknown";
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
        /// <param name="framerate">The output framerate.</param>
        /// <param name="keyInterval">The output key interval. For best results should be a multiple of framerate.</param>
        /// <param name="qualities">A collection of qualities to encode to. Entries in the collection must have a distinct bitrate, otherwise behavior is undefined.</param>
        /// <param name="inputAudioCodec">The input file audio codec.</param>
        /// <param name="inputVideoCodec">The input file video codec</param>
        /// <param name="defaultBitrate">The bitrate to use for the copy quality. Typically the input file bitrate.</param>
        /// <param name="enableStreamCopying">Set true to enable -vcodec copy for the copy quality.</param>
        internal static CommandBuildResult BuildFfmpegCommand(
            string inPath,
            string outDirectory,
            string outFilename,
            int framerate,
            int keyInterval,
            IEnumerable<IQuality> qualities,
            MediaMetadata metadata,
            int defaultBitrate,
            bool enableStreamCopying)
        {

            var videoCommand = BuildVideoCommands(metadata.VideoStreams, qualities, defaultBitrate, framerate, keyInterval, enableStreamCopying, outDirectory, outFilename);
            var audioCommand = BuildAudioCommands(metadata.AudioStreams, outDirectory, outFilename);
            var subtitleCommand = BuildSubtitleCommands(metadata.SubtitleStreams, outDirectory, outFilename);

            var allCommands = videoCommand.Concat(audioCommand).Concat(subtitleCommand);

            string parameters = $"-i \"{inPath}\" -y -hide_banner {string.Join(" ", allCommands.Select(x => x.Argument))}";

            return new CommandBuildResult(parameters, allCommands);
        }

        /// <summary>
        /// Builds the arguments for the CLI version of Mp4Box.
        /// </summary>
        /// <param name="inFiles">A collection of full paths to input files to encode from.</param>
        /// <param name="outFilePath">The full path to write the mpd file to.</param>
        /// <param name="keyInterval">The key interval in milliseconds. This can be derived from: (keyframeInterval / framerate) * 1000</param>
        internal static CommandBuildResult BuildMp4boxMpdCommand(IEnumerable<string> inFiles, string outFilePath, int keyInterval)
        {
            string parameters = $"-dash {keyInterval} -quiet -rap -frag-rap -bs-switching no -subsegs-per-sidx 0 -sample-groups-traf" +
                $" -profile dashavc264:onDemand -out \"{outFilePath}\" -- {string.Join(" ", inFiles.Select(x => '"' + x + '"'))}";
            return new CommandBuildResult(parameters, new List<StreamFile>() { new StreamFile() { Path = outFilePath } });
        }
    }
}
