using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using DEnc.Serialization;

namespace DEnc
{
    public class Encoder
    {
        public string FFmpegPath { get; private set; }
        public string FFprobePath { get; private set; }
        public string BoxPath { get; private set; }
        public string WorkingDirectory { get; private set; }

        public bool DisableQualityCrushing { get; set; } = false;
        public bool EnableStreamCopying { get; set; } = false;

        private const double bitrateCrushTolerance = 0.95;

        private readonly Action<string> stdoutLog;
        private readonly Action<string> stderrLog;

        /// <summary>
        /// Creates a new encoder with the given paths for ffmpeg and MP4Box, as well as the working directory.
        /// The given pointers to ffmpeg and MP4Box are tested by executing them with no parameters upon construction. An exception is thrown if the execution fails.
        /// </summary>
        /// <param name="ffmpegPath">A full path or environmental variable for ffmpeg.</param>
        /// <param name="boxPath">A full path or environmental variable for MP4Box.</param>
        ///<param name="stdoutLog">A callback which reflects stdout of ffmpeg/MP4Box. May be left null.</param>
        ///<param name="stderrLog">A callback used for logging, and for the stderr of ffmpeg/MP4Box. May be left null.</param>
        /// <param name="workingDirectory">A directory to generate output files in. If null, a temp path is used.</param>
        public Encoder(string ffmpegPath = "ffmpeg", string ffprobePath = "ffprobe", string boxPath = "MP4Box", Action<string> stdoutLog = null, Action<string> stderrLog = null, string workingDirectory = null)
        {
            FFmpegPath = ffmpegPath;
            FFprobePath = ffprobePath;
            BoxPath = boxPath;
            WorkingDirectory = workingDirectory ?? Path.GetTempPath();
            this.stdoutLog = stdoutLog ?? new Action<string>((s) => { });
            this.stderrLog = stderrLog ?? new Action<string>((s) => { });

            if (!Directory.Exists(WorkingDirectory))
            {
                throw new DirectoryNotFoundException("The given path for the working directory doesn't exist.");
            }
        }

        /// <summary>
        /// Converts the input file into an MPEG DASH representation with multiple bitrates.
        /// </summary>
        /// <param name="inFile">The video file to convert.</param>
        /// <param name="outFilename">The base filename to use for the output files. Files will be overwritten if they exist.</param>
        /// <param name="framerate">The global framerate to use for the output encode.</param>
        /// <param name="keyframeInterval">The interval for keyframes. Typically set for 3-10x the framerate depending on keydrop tolerance.</param>
        /// <param name="qualities">Parameters to pass to ffmpeg when performing the preparation encoding. Bitrates must be distinct, an exception will be thrown if they are not.</param>
        /// <param name="outDirectory">The directory to place output files and intermediary files in.</param>
        /// <returns>An object containing a representation of the generated MPD file, it's path, and the associated filenames, or null if no file generated.</returns>
        public DashEncodeResult GenerateDash(string inFile, string outFilename, int framerate, int keyframeInterval, IEnumerable<IQuality> qualities, string outDirectory = null)
        {
            outDirectory = outDirectory ?? WorkingDirectory;

            // Input validation.
            if (inFile == null || !File.Exists(inFile))
            {
                throw new FileNotFoundException("Input path does not exist.");
            }
            if (!Directory.Exists(outDirectory))
            {
                throw new DirectoryNotFoundException("Output directory does not exist.");
            }
            if (string.IsNullOrEmpty(outFilename))
            {
                throw new ArgumentNullException("Output filename is null or empty.");
            }
            if (qualities == null || qualities.Count() == 0)
            {
                throw new ArgumentOutOfRangeException("No qualitied specified. At least one quality is required.");
            }

            // Check bitrate distinction.
            if (qualities.GroupBy(x => x.Bitrate).Count() != qualities.Count())
            {
                throw new ArgumentOutOfRangeException("Duplicate bitrates found. Bitrates must be distinct.");
            }

            var inputStats = ProbeFile(inFile);
            int inputBitrate = (int)(inputStats.Bitrate / 1024);
            if (!DisableQualityCrushing)
            {
                qualities = CrushQualities(qualities, inputBitrate);
            }

            framerate = framerate <= 0 ? (int)Math.Round(inputStats.Framerate) : framerate;
            keyframeInterval = keyframeInterval <= 0 ? framerate * 3 : keyframeInterval;

            // Build task definitions.
            var ffmpegCommand = CommandBuilder.BuildFfmpegCommand(
                inPath: inFile,
                outDirectory: WorkingDirectory,
                outFilename: outFilename,
                framerate: framerate,
                keyInterval: keyframeInterval,
                qualities: qualities.OrderByDescending(x => x.Bitrate),
                metadata: inputStats,
                defaultBitrate: inputBitrate,
                enableStreamCopying: EnableStreamCopying);

            // Generate intermediates
            ExecutionResult ffResult;
            stderrLog.Invoke($"Running ffmpeg with arguments: {ffmpegCommand.RenderedCommand}");
            ffResult = ManagedExecution.Start(FFmpegPath, ffmpegCommand.RenderedCommand, stdoutLog, stderrLog);

            // Detect error in ffmpeg process and cleanup, then return null.
            if (ffResult.ExitCode != 0)
            {
                stderrLog.Invoke($"ERROR: ffmpeg returned code {ffResult.ExitCode}.");
                CleanOutputFiles(ffmpegCommand.CommandPieces.Select(x => x.Path));
                return null;
            }

            var audioVideoFiles = ffmpegCommand.CommandPieces.Where(x => x.Type == StreamType.Video || x.Type == StreamType.Audio);

            var mp4boxCommand = CommandBuilder.BuildMp4boxMpdCommand(
                inFiles: audioVideoFiles.Select(x => x.Path),
                outFilePath: Path.Combine(outDirectory, outFilename) + ".mpd",
                keyInterval: (keyframeInterval / framerate) * 1000);

            // Generate DASH files.
            ExecutionResult mpdResult;
            stderrLog.Invoke($"Running MP4Box with arguments: {mp4boxCommand.RenderedCommand}");
            mpdResult = ManagedExecution.Start(BoxPath, mp4boxCommand.RenderedCommand, stdoutLog, stderrLog);

            // Cleanup intermediates.
            CleanOutputFiles(audioVideoFiles.Select(x => x.Path));

            // Move subtitles
            List<StreamFile> subtitles = new List<StreamFile>();
            foreach (var subFile in ffmpegCommand.CommandPieces.Where(x => x.Type == StreamType.Subtitle))
            {
                string oldPath = subFile.Path;
                subFile.Path = Path.Combine(outDirectory, Path.GetFileName(subFile.Path));
                subtitles.Add(subFile);
                if (oldPath != subFile.Path)
                {
                    if (File.Exists(subFile.Path)) { File.Delete(subFile.Path); }
                    File.Move(oldPath, subFile.Path);
                }
            }

            string output = mp4boxCommand.CommandPieces.FirstOrDefault().Path;
            if (File.Exists(output))
            {
                MPD.LoadFromFile(output, out MPD mpd, out Exception ex);
                mpd.ProgramInformation = null;

                // Add adaptation sets for subtitles.
                int.TryParse(mpd.Period.Max(x => x.AdaptationSet.Max(y => y.Representation.Max(z => z.Id))), out int subId);
                subId++;
                foreach (var sub in subtitles)
                {
                    mpd.Period[0].AdaptationSet.Add(new AdaptationSet()
                    {
                        MimeType = "text/vtt",
                        Lang = sub.Name,
                        ContentType = "text",
                        Representation = new List<Representation>()
                        {
                            new Representation()
                            {
                                Id = subId.ToString(),
                                Bandwidth = 256,
                                BaseURL = new List<string>()
                                {
                                    Path.GetFileName(sub.Path)
                                }
                            }
                        }
                    });
                    subId++;
                }
                mpd.SaveToFile(output);

                var result = new DashEncodeResult(mpd, TimeSpan.FromMilliseconds((inputStats.VideoStreams.FirstOrDefault()?.duration ?? 0) * 1000), output);

                // Detect error in MP4Box process and cleanup, then return null.
                if (mpdResult.ExitCode != 0)
                {
                    stderrLog.Invoke($"ERROR: MP4Box returned code {ffResult.ExitCode}.");
                    CleanOutputFiles(result.MediaFiles.Select(x => Path.Combine(outDirectory, x)));
                    CleanOutputFiles(mpdResult.Output);
                    return null;
                }

                // Success.
                return result;
            }

            stderrLog.Invoke($"ERROR: MP4Box did not produce the expected mpd file at path {output}.");
            return null;
        }

        /// <summary>
        /// Removes qualities higher than the given bitrate and substitutes removed qualities with a copy quality.
        /// </summary>
        /// <param name="qualities">The quality collection to crush.</param>
        /// <param name="bitrateKbs">Bitrate in kb/s.</param>
        /// <returns></returns>
        private IEnumerable<IQuality> CrushQualities(IEnumerable<IQuality> qualities, long bitrateKbs)
        {
            if (qualities == null || !qualities.Any()) { return qualities; }

            // Crush
            var crushed = qualities.Where(x => x.Bitrate < bitrateKbs * bitrateCrushTolerance).Distinct();
            if (crushed.Any() && crushed.Count() < qualities.Count())
            {
                if (crushed.Where(x => x.Bitrate == 0).FirstOrDefault() == null)
                {
                    var newQualities = new List<IQuality>() { Quality.GetCopyQuality() }; // Add a copy quality to replace removed qualities.
                    newQualities.AddRange(crushed);
                    return newQualities;
                }

                return crushed;
            }

            return qualities;
        }

        private MediaMetadata ProbeFile(string inFile)
        {
            string args = $"-print_format xml=fully_qualified=1 -show_format -show_streams -- \"{inFile}\"";
            var exResult = ManagedExecution.Start(FFprobePath, args);

            string xmlData = string.Join("\n", exResult.Output);

            if (FFprobeData.Deserialize(xmlData, out FFprobeData t))
            {
                List<MediaStream> audioStreams = new List<MediaStream>();
                List<MediaStream> videoStreams = new List<MediaStream>();
                List<MediaStream> subtitleStreams = new List<MediaStream>();
                foreach (var s in t.streams)
                {
                    switch (s.codec_type)
                    {
                        case "audio":
                            audioStreams.Add(s);
                            break;
                        case "video":
                            videoStreams.Add(s);
                            break;
                        case "subtitle":
                            subtitleStreams.Add(s);
                            break;
                        default:
                            break;
                    }
                }

                var firstVideoStream = videoStreams.FirstOrDefault(x => CommandBuilder.SupportedCodecs.ContainsKey(x.codec_name));
                var firstAudioStream = audioStreams.FirstOrDefault(x => CommandBuilder.SupportedCodecs.ContainsKey(x.codec_name));

                if (!decimal.TryParse(firstVideoStream?.r_frame_rate, out decimal framerate)) { framerate = 24; }

                var meta = new MediaMetadata(videoStreams, audioStreams, subtitleStreams, t.format.bit_rate, framerate);
                return meta;
            }

            return null;
        }

        private void CleanOutputFiles(IEnumerable<string> files)
        {
            if (files == null) { return; }
            foreach (var file in files)
            {
                try
                {
                    stderrLog.Invoke("Deleting file " + file);
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    stderrLog.Invoke(ex.ToString());
                }
            }
        }
    }
}
