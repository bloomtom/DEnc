using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using DEnc.Serialization;
using System.Threading;

namespace DEnc
{
    /// <summary>
    /// A construct for performing encode functions.
    /// </summary>
    public class Encoder
    {
        /// <summary>
        /// The path to ffmpeg.
        /// </summary>
        public string FFmpegPath { get; private set; }
        /// <summary>
        /// The path to ffprobe.
        /// </summary>
        public string FFprobePath { get; private set; }
        /// <summary>
        /// The path to MP4Box.
        /// </summary>
        public string BoxPath { get; private set; }
        /// <summary>
        /// The temp path to store encodes in progress.
        /// </summary>
        public string WorkingDirectory { get; private set; }

        /// <summary>
        /// If set to true, quality crushing is not performed.
        /// You may end up with files larger then your input depending on your quality set.
        /// </summary>
        public bool DisableQualityCrushing { get; set; } = false;
        /// <summary>
        /// If set to true, the 'copy' quality will actually copy the media streams under some circumstances instead of running them through the encoder.
        /// Copying is only performed if the input video streams match the desired quality pixel format, and if the desired level and profile are superior to the input video streams.
        /// </summary>
        public bool EnableStreamCopying { get; set; } = false;

        private readonly Action<string> stdoutLog;
        private readonly Action<string> stderrLog;

        /// <summary>
        /// Creates a new encoder with the given paths for ffmpeg and MP4Box, as well as the working directory.
        /// The given pointers to ffmpeg and MP4Box are tested by executing them with no parameters upon construction. An exception is thrown if the execution fails.
        /// </summary>
        /// <param name="ffmpegPath">A full path or environmental variable for ffmpeg.</param>
        /// <param name="ffprobePath">A full path or environmental variable for ffprobe.</param>
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
        /// <param name="inFile">The video file to convert. Pass a full path if possible. Otherwise it will be rooted to the application working directory (not the working directory given to this instance).</param>
        /// <param name="outFilename">The base filename to use for the output files. Files will be overwritten if they exist.</param>
        /// <param name="framerate">Output video stream framerate. Pass zero to make this automatic based on the input file.</param>
        /// <param name="keyframeInterval">Output video keyframe interval. Pass zero to make this automatically 3x the framerate.</param>
        /// <param name="qualities">Parameters to pass to ffmpeg when performing the preparation encoding. Bitrates must be distinct, an exception will be thrown if they are not.</param>
        /// <param name="options">Options for the ffmpeg encode.</param>
        /// <param name="outDirectory">The directory to place output files and intermediary files in.</param>
        /// <param name="progress">A callback for progress events. The collection will contain values with the Name property of "Encode", "DASHify", "Post Process"</param>
        /// <param name="cancel">Allows cancellation of the operation.</param>
        /// <returns>An object containing a representation of the generated MPD file, it's path, and the associated filenames, or null if no file generated.</returns>
        public DashEncodeResult GenerateDash(string inFile, string outFilename, int framerate, int keyframeInterval,
            IEnumerable<IQuality> qualities, IEncodeOptions options = null, string outDirectory = null, IProgress<IEnumerable<EncodeStageProgress>> progress = null, CancellationToken cancel = default(CancellationToken))
        {
            cancel.ThrowIfCancellationRequested();

            options = options ?? new H264EncodeOptions();
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

            // Map input file to a full path if it's relative.
            inFile = Path.GetFullPath(inFile);

            // Check for invalid characters and remove them.
            outFilename = RemoveSymbols(outFilename, '#', '&', '*', '<', '>', '/', '?', ':', '"');
            // Another check to ensure we didn't remove all the characters.
            if (outFilename.Length == 0)
            {
                throw new ArgumentNullException("Output filename is null or empty.");
            }

            // Check bitrate distinction.
            if (qualities.GroupBy(x => x.Bitrate).Count() != qualities.Count())
            {
                throw new ArgumentOutOfRangeException("Duplicate bitrates found. Bitrates must be distinct.");
            }

            var inputStats = ProbeFile(inFile);
            if (inputStats == null) { throw new NullReferenceException("ffprobe query returned a null result."); }
            int inputBitrate = (int)(inputStats.Bitrate / 1024);
            if (!DisableQualityCrushing)
            {
                qualities = QualityCrusher.CrushQualities(qualities, inputBitrate);
            }
            var compareQuality = qualities.First();
            bool enableStreamCopy = EnableStreamCopying && compareQuality.Bitrate == 0 &&
                Copyable264Infer.DetermineCopyCanBeDone(compareQuality.PixelFormat, compareQuality.Level, compareQuality.Profile, inputStats.VideoStreams);

            var progressList = new List<EncodeStageProgress>()
            {
                new EncodeStageProgress("Encode", 0),
                new EncodeStageProgress("DASHify", 0),
                new EncodeStageProgress("Post Process", 0)
            };
            const int encodeStage = 0;
            const int dashStage = 1;
            const int postStage = 2;

            var stdErrShim = stderrLog;
            if (progress != null)
            {
                stdErrShim = new Action<string>(x =>
                {
                    stderrLog(x);
                    if (x != null)
                    {
                        var match = Encode.Regexes.ParseProgress.Match(x);
                        if (match.Success && TimeSpan.TryParse(match.Value, out TimeSpan p))
                        {
                            ReportProgress(progress, progressList, encodeStage, Math.Min(1, (float)(p.TotalMilliseconds / 1000) / inputStats.Duration));
                        }
                    }
                });
            }

            framerate = framerate <= 0 ? (int)Math.Round(inputStats.Framerate) : framerate;
            keyframeInterval = keyframeInterval <= 0 ? framerate * 3 : keyframeInterval;

            // Build task definitions.
            var ffmpegCommand = CommandBuilder.BuildFfmpegCommand(
                inPath: inFile,
                outDirectory: WorkingDirectory,
                outFilename: outFilename,
                options: options,
                framerate: framerate,
                keyframeInterval: keyframeInterval,
                qualities: qualities.OrderByDescending(x => x.Bitrate),
                metadata: inputStats,
                defaultBitrate: inputBitrate,
                enableStreamCopying: enableStreamCopy);

            cancel.ThrowIfCancellationRequested();

            // Generate intermediates
            try
            {
                ExecutionResult ffResult;
                stderrLog.Invoke($"Running ffmpeg with arguments: {ffmpegCommand.RenderedCommand}");
                ffResult = ManagedExecution.Start(FFmpegPath, ffmpegCommand.RenderedCommand, stdoutLog, stdErrShim, cancel);

                // Detect error in ffmpeg process and cleanup, then return null.
                if (ffResult.ExitCode != 0)
                {
                    stderrLog.Invoke($"ERROR: ffmpeg returned code {ffResult.ExitCode}. File: {inFile}");
                    CleanOutputFiles(ffmpegCommand.CommandPieces.Select(x => x.Path));
                    return null;
                }
            }
            catch (Exception ex)
            {
                CleanOutputFiles(ffmpegCommand.CommandPieces.Select(x => x.Path));

                if (ex is OperationCanceledException)
                {
                    throw new OperationCanceledException($"Exception running ffmpeg on {inFile}", ex);
                }
                else
                {
                    throw new Exception($"Exception running ffmpeg on {inFile}", ex);
                }
            }


            var audioVideoFiles = ffmpegCommand.CommandPieces.Where(x => x.Type == StreamType.Video || x.Type == StreamType.Audio);

            var mp4boxCommand = CommandBuilder.BuildMp4boxMpdCommand(
                inFiles: audioVideoFiles,
                outFilePath: Path.Combine(outDirectory, outFilename) + ".mpd",
                keyInterval: (keyframeInterval / framerate) * 1000,
                flags: options.AdditionalMP4BoxFlags);

            // Generate DASH files.
            ExecutionResult mpdResult;
            stderrLog.Invoke($"Running MP4Box with arguments: {mp4boxCommand.RenderedCommand}");
            try
            {
                mpdResult = ManagedExecution.Start(BoxPath, mp4boxCommand.RenderedCommand, stdoutLog, stderrLog, cancel);
            }
            catch (Exception ex)
            {
                CleanOutputFiles(audioVideoFiles.Select(x => x.Path));

                if (ex is OperationCanceledException)
                {
                    throw new OperationCanceledException($"Exception running MP4box on {inFile}", ex);
                }
                else
                {
                    throw new Exception($"Exception running MP4box on {inFile}", ex);
                }
            }

            // Report DASH complete.
            if (mpdResult.ExitCode == 0)
            {
                ReportProgress(progress, progressList, dashStage, 1);
            }

            // Cleanup intermediates.
            CleanOutputFiles(audioVideoFiles.Select(x => x.Path));

            ReportProgress(progress, progressList, postStage, 0.33);

            // Move subtitles found in media
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
            // Add external subtitles
            int originIndex = ffmpegCommand.CommandPieces.Max(x => x.Origin) + 1;
            string baseFilename = Path.GetFileNameWithoutExtension(inFile);
            foreach (var vttFile in Directory.EnumerateFiles(Path.GetDirectoryName(inFile), baseFilename + "*", SearchOption.TopDirectoryOnly))
            {
                if (vttFile.EndsWith(".vtt"))
                {
                    string vttFilename = Path.GetFileName(vttFile);
                    string vttName = GetSubtitleName(vttFilename);
                    string vttOutputPath = Path.Combine(outDirectory, $"{outFilename}_subtitle_{vttName}_{originIndex}.vtt");

                    var subFile = new StreamFile()
                    {
                        Type = StreamType.Subtitle,
                        Origin = originIndex,
                        Path = vttOutputPath,
                        Name = $"{vttName}_{originIndex}"
                    };
                    originIndex++;
                    File.Copy(vttFile, vttOutputPath, true);
                    subtitles.Add(subFile);
                }
            }

            ReportProgress(progress, progressList, postStage, 0.66);

            try
            {
                string mpdFilepath = mp4boxCommand.CommandPieces.FirstOrDefault().Path;
                if (File.Exists(mpdFilepath))
                {
                    MPD mpd = PostProcessMpdFile(mpdFilepath, subtitles);

                    var result = new DashEncodeResult(mpd, inputStats.Metadata, TimeSpan.FromMilliseconds((inputStats.VideoStreams.FirstOrDefault()?.duration ?? 0) * 1000), mpdFilepath);

                    // Detect error in MP4Box process and cleanup, then return null.
                    if (mpdResult.ExitCode != 0)
                    {
                        stderrLog.Invoke($"ERROR: MP4Box returned code {mpdResult.ExitCode}. File: {inFile}");
                        CleanOutputFiles(result.MediaFiles.Select(x => Path.Combine(outDirectory, x)));
                        CleanOutputFiles(mpdResult.Output);

                        return null;
                    }

                    // Success.
                    return result;
                }

                stderrLog.Invoke($"ERROR: MP4Box did not produce the expected mpd file at path {mpdFilepath}. File: {inFile}");
                return null;
            }
            finally
            {
                ReportProgress(progress, progressList, postStage, 1);
            }
        }

        private string RemoveSymbols(string outFilename, params char[] symbols)
        {
            bool charsRemoved = false;
            var sb = new System.Text.StringBuilder();
            foreach (char c in outFilename)
            {
                if (symbols.Contains(c))
                {
                    charsRemoved = true;
                    continue;
                }
                sb.Append(c);
            }
            if (charsRemoved)
            {
                stderrLog($"outfilename contained one or more invalid characters, which were removed.");
            }

            return sb.ToString();
        }

        private static string GetSubtitleName(string vttFilename)
        {
            if (vttFilename.Contains("."))
            {
                var dotComponents = vttFilename.Split('.');
                foreach (var component in dotComponents)
                {
                    if (LanguageCodes.Languages.TryGetValue(component, out string languageName))
                    {
                        return languageName;
                    }
                }
            }
            return "und";
        }

        private static void ReportProgress(IProgress<IEnumerable<EncodeStageProgress>> reporter, List<EncodeStageProgress> progresses, int index, double value)
        {
            progresses[index] = new EncodeStageProgress(progresses[index].Name, value);
            reporter?.Report(progresses);
        }

        /// <summary>
        /// Performs on-disk post processing of the generated MPD file.
        /// Subtitles are added, useless tags removed, etc.
        /// </summary>
        private static MPD PostProcessMpdFile(string filepath, List<StreamFile> subtitles)
        {
            MPD.LoadFromFile(filepath, out MPD mpd, out Exception ex);
            mpd.ProgramInformation = null;

            // Get the highest used representation ID so we can increment it for new IDs.
            int.TryParse(mpd.Period.Max(x => x.AdaptationSet.Max(y => y.Representation.Max(z => z.Id))), out int representationId);
            representationId++;

            foreach (var period in mpd.Period)
            {
                // Add subtitles to this period.
                foreach (var sub in subtitles)
                {
                    period.AdaptationSet.Add(new AdaptationSet()
                    {
                        MimeType = "text/vtt",
                        Lang = sub.Name,
                        ContentType = "text",
                        Representation = new List<Representation>()
                        {
                            new Representation()
                            {
                                Id = representationId.ToString(),
                                Bandwidth = 256,
                                BaseURL = new List<string>()
                                {
                                    Path.GetFileName(sub.Path)
                                }
                            }
                        },
                        Role = new DescriptorType()
                        {
                            SchemeIdUri = "urn:gpac:dash:role:2013",
                            Value = $"{sub.Name} {representationId.ToString()}"
                        }
                    });
                    representationId++;
                }
            }

            mpd.SaveToFile(filepath);
            return mpd;
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

                var metadata = new Dictionary<string, string>();
                if (t.format.tag != null)
                {
                    foreach (var item in t.format.tag)
                    {
                        if (!metadata.ContainsKey(item.key))
                        {
                            metadata.Add(item.key.ToLower(System.Globalization.CultureInfo.InvariantCulture), item.value);
                        }
                    }
                }

                var firstVideoStream = videoStreams.FirstOrDefault(x => CommandBuilder.SupportedCodecs.ContainsKey(x.codec_name));
                var firstAudioStream = audioStreams.FirstOrDefault(x => CommandBuilder.SupportedCodecs.ContainsKey(x.codec_name));

                if (!decimal.TryParse(firstVideoStream?.r_frame_rate, out decimal framerate)) { framerate = 24; }

                float duration = t.format != null ? t.format.duration : 0;

                var meta = new MediaMetadata(videoStreams, audioStreams, subtitleStreams, metadata, firstVideoStream?.bit_rate ?? t.format.bit_rate, framerate, duration);
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
                    int attempts = 0;
                    while (File.Exists(file))
                    {
                        attempts++;
                        try
                        {
                            File.Delete(file);
                        }
                        catch (IOException)
                        {
                            if (attempts < 5)
                            {
                                Thread.Sleep(200);
                                continue;
                            }
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    stderrLog.Invoke(ex.ToString());
                }
            }
        }
    }
}
