using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using DEnc.Serialization;
using System.Threading;
using DEnc.Commands;
using DEnc.Models.Interfaces;
using DEnc.Models;

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

        public Dictionary<EncodingStage, double> Progress { get; private set; }

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
            Progress = new Dictionary<EncodingStage, double>()
            {
                [EncodingStage.Encode] = 0,
                [EncodingStage.DASHify] = 0,
                [EncodingStage.PostProcess] = 0,
            };

            this.stdoutLog = stdoutLog ?? new Action<string>((s) => { });
            this.stderrLog = stderrLog ?? new Action<string>((s) => { });

            if (!Directory.Exists(WorkingDirectory))
            {
                throw new DirectoryNotFoundException("The given path for the working directory doesn't exist.");
            }
        }

        [Obsolete("This method has been replaced by a new API", true)]
        public DashEncodeResult GenerateDash(string inFile, string outFilename, int framerate, int keyframeInterval,
            IEnumerable<IQuality> qualities, IEncodeOptions options = null, string outDirectory = null, IProgress<IEnumerable<EncodeStageProgress>> progress = null, CancellationToken cancel = default(CancellationToken))
        {
            throw new NotSupportedException();
        }

        public DashEncodeResult GenerateDash(DashConfig config, IProgress<Dictionary<EncodingStage, double>> progress = null, CancellationToken cancel = default(CancellationToken))
        {
            cancel.ThrowIfCancellationRequested();
            if (!Directory.Exists(WorkingDirectory))
            {
                throw new DirectoryNotFoundException("The given path for the working directory doesn't exist.");
            }

            //Field declarations
            MediaMetadata inputStats;
            IQuality compareQuality;
            int inputBitrate;
            bool enableStreamCopy = false;

            inputStats = ProbeFile(config.InputFilePath);
            if (inputStats == null) { throw new NullReferenceException("ffprobe query returned a null result."); }

            inputBitrate = (int)(inputStats.Bitrate / 1024);
            if (!DisableQualityCrushing)
            {
                config.Qualities = QualityCrusher.CrushQualities(config.Qualities, inputBitrate);
            }
            compareQuality = config.Qualities.First();


            if (EnableStreamCopying && compareQuality.Bitrate == 0)
            {
                enableStreamCopy = Copyable264Infer.DetermineCopyCanBeDone(compareQuality.PixelFormat, compareQuality.Level, compareQuality.Profile.ToString(), inputStats.VideoStreams);
            }

            // Set the framerate interval to match input if user has not already set
            if (config.Framerate <= 0)
            {
                config.Framerate = (int)Math.Round(inputStats.Framerate);
            }

            // Set the keyframe interval to match input if user has not already set
            if (config.KeyframeInterval <= 0)
            {
                config.KeyframeInterval = config.Framerate * 3;
            }

            //This is not really the proper place to have this
            // Logging shim for ffmpeg to get progress info
            var ffmpegLogShim = new Action<string>(x =>
            {
                if (x != null)
                {
                    var match = Encode.Regexes.ParseProgress.Match(x);
                    if (match.Success && TimeSpan.TryParse(match.Value, out TimeSpan p))
                    {
                        stdoutLog(x);
                        float progressFloat = Math.Min(1, (float)(p.TotalMilliseconds / 1000) / inputStats.Duration);
                        if (progress != null)
                        {
                            ReportProgress(progress, EncodingStage.Encode, progressFloat);
                        }
                            
                    }
                    else
                    {
                        stderrLog(x);
                    }
                }
                else
                {
                    stderrLog(x);
                }
            });

            cancel.ThrowIfCancellationRequested();
            FfmpegRenderedCommand ffmpgCommand = EncodeVideo(config, inputStats, inputBitrate, enableStreamCopy, ffmpegLogShim, cancel);
            if (ffmpgCommand is null)
            {
                return null;
            }

            Mp4BoxRenderedCommand mp4BoxCommand = GenerateDashManifest(config, ffmpgCommand.VideoPieces, ffmpgCommand.AudioPieces, cancel);
            if (mp4BoxCommand is null)
            {
                return null;
            }

            ReportProgress(progress, EncodingStage.DASHify, 1);
            ReportProgress(progress, EncodingStage.PostProcess, 0.3);

            int maxFileIndex = ffmpgCommand.AllPieces.Max(x => x.Index);
            List<StreamSubtitleFile> allSubtitles = ProcessSubtitles(config, ffmpgCommand.SubtitlePieces, maxFileIndex + 1);

            ReportProgress(progress, EncodingStage.PostProcess, 0.66);

            try
            {
                string mpdFilepath = mp4BoxCommand.MpdPath;
                if (File.Exists(mpdFilepath))
                {
                    MPD mpd = PostProcessMpdFile(mpdFilepath, allSubtitles);

                    var result = new DashEncodeResult(mpd, inputStats.Metadata, TimeSpan.FromMilliseconds((inputStats.VideoStreams.FirstOrDefault()?.duration ?? 0) * 1000), mpdFilepath);

                    // Success.
                    return result;
                }

                stderrLog.Invoke($"ERROR: MP4Box did not produce the expected mpd file at path {mpdFilepath}. File: {config.InputFilePath}");
                return null;
            }
            finally
            {
                ReportProgress(progress, EncodingStage.PostProcess, 1);
            }
        }

        private FfmpegRenderedCommand EncodeVideo(DashConfig config, MediaMetadata inputStats, int inputBitrate, bool enableStreamCopying, Action<string> progressCallback, CancellationToken cancel)
        {
            FfmpegRenderedCommand ffmpegCommand = FFmpegCommandBuilder 
                .Initilize(
                    inPath: config.InputFilePath,
                    outDirectory: config.OutputDirectory,
                    outBaseFilename: config.OutputFileName,
                    options: config.Options,
                    enableStreamCopying: enableStreamCopying
                 )
                .WithVideoCommands(inputStats.VideoStreams, config.Qualities, config.Framerate, config.KeyframeInterval, inputBitrate)
                .WithAudioCommands(inputStats.AudioStreams)
                .WithSubtitleCommands(inputStats.SubtitleStreams)
                .Build();

            // Generate intermediates
            try
            {
                ExecutionResult ffResult;
                stderrLog.Invoke($"Running ffmpeg with arguments: {ffmpegCommand.RenderedCommand}");
                ffResult = ManagedExecution.Start(FFmpegPath, ffmpegCommand.RenderedCommand, stdoutLog, progressCallback, cancel); //TODO: Use a better log/error callback mechanism? Also use a better progress mechanism

                // Detect error in ffmpeg process and cleanup, then return null.
                if (ffResult.ExitCode != 0)
                {
                    stderrLog.Invoke($"ERROR: ffmpeg returned code {ffResult.ExitCode}. File: {config.InputFilePath}");
                    CleanOutputFiles(ffmpegCommand.AllPieces.Select(x => x.Path));
                    return null;
                }
            }
            catch (Exception ex)
            {
                CleanOutputFiles(ffmpegCommand.AllPieces.Select(x => x.Path));

                if (ex is OperationCanceledException)
                {
                    throw new OperationCanceledException($"Exception running ffmpeg on {config.InputFilePath}", ex);
                }
                else
                {
                    throw new Exception($"Exception running ffmpeg on {config.InputFilePath}", ex);
                }
            }

            return ffmpegCommand;
        }

        private Mp4BoxRenderedCommand GenerateDashManifest(DashConfig config, IEnumerable<StreamVideoFile> videoFiles, IEnumerable<StreamAudioFile> audioFiles, CancellationToken cancel)
        {
            string mpdOutputPath = Path.Combine(config.OutputDirectory, config.OutputFileName) + ".mpd";
            var mp4boxCommand = Mp4BoxCommandBuilder.BuildMp4boxMpdCommand(
                videoFiles: videoFiles,
                audioFiles: audioFiles,
                mpdOutputPath: mpdOutputPath,
                keyInterval: (config.KeyframeInterval / config.Framerate) * 1000,
                additionalFlags: config.Options.AdditionalMP4BoxFlags);

            // Generate DASH files.
            ExecutionResult mpdResult;
            stderrLog.Invoke($"Running MP4Box with arguments: {mp4boxCommand.RenderedCommand}");
            try
            {
                mpdResult = ManagedExecution.Start(BoxPath, mp4boxCommand.RenderedCommand, stdoutLog, stderrLog, cancel);
                
                // Dash Failed TODO: Add in Progress report behavior that was excluded from this
                // Detect error in MP4Box process and cleanup, then return null.
                if (mpdResult.ExitCode != 0)
                {
                    MPD mpdFile = MPD.LoadFromFile(mpdOutputPath);
                    var filePaths = mpdFile.GetFileNames().Select(x => Path.Combine(config.OutputDirectory, x));

                    stderrLog.Invoke($"ERROR: MP4Box returned code {mpdResult.ExitCode}. File: {config.InputFilePath}");
                    CleanOutputFiles(filePaths);
                    CleanOutputFiles(mpdResult.Output);

                    return null;
                }
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    throw new OperationCanceledException($"Exception running MP4box on {config.InputFilePath}", ex);
                }
                else
                {
                    throw new Exception($"Exception running MP4box on {config.InputFilePath}", ex);
                }
            }
            finally
            {
                CleanOutputFiles(videoFiles.Select(x => x.Path));
                CleanOutputFiles(audioFiles.Select(x => x.Path));
            }

            return mp4boxCommand;
        }

        /// <summary>
        /// Processes the media subtitles and finds and handles external subtitle files
        /// </summary>
        /// <param name="config">The <see cref="DashConfig"/></param>
        /// <param name="subtitleFiles">The subtitle stream files</param>
        /// <param name="startFileIndex">The index additional subtitles need to start at. This should be the max index of the ffmpeg pieces +1</param>
        private List<StreamSubtitleFile> ProcessSubtitles(DashConfig config, IEnumerable<StreamSubtitleFile> subtitleFiles, int startFileIndex)
        {
            // Move subtitles found in media
            List<StreamSubtitleFile> subtitles = new List<StreamSubtitleFile>();
            foreach (var subFile in subtitleFiles)
            {
                string oldPath = subFile.Path;
                subFile.Path = Path.Combine(config.OutputDirectory, Path.GetFileName(subFile.Path));
                subtitles.Add(subFile);
                if (oldPath != subFile.Path)
                {
                    if (File.Exists(subFile.Path)) 
                    { 
                        File.Delete(subFile.Path); 
                    }
                    File.Move(oldPath, subFile.Path);
                }
            }

            // Add external subtitles
            string baseFilename = Path.GetFileNameWithoutExtension(config.InputFilePath);
            foreach (var vttFile in Directory.EnumerateFiles(Path.GetDirectoryName(config.InputFilePath), baseFilename + "*", SearchOption.TopDirectoryOnly))
            {
                if (vttFile.EndsWith(".vtt"))
                {
                    string vttFilename = Path.GetFileName(vttFile);
                    string vttName = GetSubtitleName(vttFilename);
                    string vttOutputPath = Path.Combine(config.OutputDirectory, $"{config.OutputFileName}_subtitle_{vttName}_{startFileIndex}.vtt");

                    var subFile = new StreamSubtitleFile()
                    {
                        Type = StreamType.Subtitle,
                        Index = startFileIndex,
                        Path = vttOutputPath,
                        Language = $"{vttName}_{startFileIndex}"
                    };
                    startFileIndex++;
                    File.Copy(vttFile, vttOutputPath, true);
                    subtitles.Add(subFile);
                }
            }

            return subtitles;
        }
        private void ReportProgress(IProgress<Dictionary<EncodingStage, double>> reporter, EncodingStage stage, double value)
        {
            Progress[stage] = value;
            reporter?.Report(Progress);
        }

        private static string GetSubtitleName(string vttFilename)
        {
            if (vttFilename.Contains("."))
            {
                var dotComponents = vttFilename.Split('.');
                foreach (var component in dotComponents)
                {
                    if (Constants.Languages.TryGetValue(component, out string languageName))
                    {
                        return languageName;
                    }
                }
            }
            return "und";
        }

        /// <summary>
        /// Performs on-disk post processing of the generated MPD file.
        /// Subtitles are added, useless tags removed, etc.
        /// </summary>
        private static MPD PostProcessMpdFile(string filepath, List<StreamSubtitleFile> subtitles)
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
                        Lang = sub.Language,
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
                            Value = $"{sub.Language} {representationId.ToString()}"
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

                var firstVideoStream = videoStreams.FirstOrDefault(x => Constants.SupportedCodecs.ContainsKey(x.codec_name));
                var firstAudioStream = audioStreams.FirstOrDefault(x => Constants.SupportedCodecs.ContainsKey(x.codec_name));

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
