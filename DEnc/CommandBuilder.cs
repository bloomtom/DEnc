using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DEnc
{
    internal class CommandBuildResult
    {
        public string CommandArguments { get; private set; }
        public IEnumerable<string> OutputFiles { get; private set; }

        internal CommandBuildResult(string commandArguments, IEnumerable<string> files)
        {
            CommandArguments = commandArguments;
            OutputFiles = files;
        }
    }

    internal static class CommandBuilder
    {
        internal static List<string> SupportedCodecs { get; } = new List<string>() { "opus", "aac", "mp3", "h264", "vp8" };

        /// <summary>
        /// Builds the arguments for the CLI version of ffmpeg.
        /// </summary>
        /// <param name="ffPath">The path to ffmpeg. Just 'ffmpeg' if the program is in your path.</param>
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
            string ffPath,
            string inPath,
            string outDirectory,
            string outFilename,
            int framerate,
            int keyInterval,
            IEnumerable<IQuality> qualities,
            string inputAudioCodec,
            string inputVideoCodec,
            int defaultBitrate,
            bool enableStreamCopying)
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

            var getAudioCodec = new Func<string, string>((sourceCodec) => { return SupportedCodecs.Contains(sourceCodec) ? "" : "-c:a aac -b:a 160k"; });

            var qStrings = qualities.Select(x =>
            {
                return string.Join(" ", new string[]
                {
                    getSize(x),
                    getBitrate(x.Bitrate == 0 ? defaultBitrate : x.Bitrate),
                    getPreset(x),
                    getFramerate(framerate),
                    getVideoCodec(inputVideoCodec, x.Bitrate == 0 && enableStreamCopying, keyInterval),
                    getAudioCodec(inputAudioCodec),
                    '"' + getFilename(outDirectory, outFilename, x.Bitrate) + '"'
                });
            });

            string parameters = $"-i \"{inPath}\" -y -hide_banner {string.Join(" ", qStrings)}";
            return new CommandBuildResult(parameters, qualities.Select(x => { return getFilename(outDirectory, outFilename, x.Bitrate); }));
        }

        /// <summary>
        /// Builds the arguments for the CLI version of Mp4Box.
        /// </summary>
        /// <param name="mp4boxPath">The path to mp4box. Just 'mp4box' if the program is in your path.</param>
        /// <param name="inFiles">A collection of full paths to input files to encode from.</param>
        /// <param name="outFilePath">The full path to write the mpd file to.</param>
        /// <param name="keyInterval">The key interval in milliseconds. This can be derived from: (keyframeInterval / framerate) * 1000</param>
        internal static CommandBuildResult BuildMp4boxCommand(string mp4boxPath, IEnumerable<string> inFiles, string outFilePath, int keyInterval)
        {
            string parameters = $"-dash {keyInterval} -quiet -rap -frag-rap -profile dashavc264:onDemand -out \"{outFilePath}\" -- {string.Join(" ", inFiles.Select(x => '"' + x + '"'))}";
            return new CommandBuildResult(parameters, new List<string>() { outFilePath });
        }

        internal static List<string> GetSupportedCodecs()
        {
            return new List<string>(SupportedCodecs);
        }
    }
}
