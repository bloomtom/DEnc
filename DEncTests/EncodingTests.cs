using DEnc;
using DEnc.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace DEncTests
{
    public class EncodingTests : IDisposable
    {
        private const string ffmpegPath = @"C:\Program Files\ffmpeg\bin\ffmpeg.exe";
        private const string ffprobePath = @"C:\Program Files\ffmpeg\bin\ffprobe.exe";
        private const string mp4boxPath = @"C:\Program Files\GPAC\mp4box.exe";

        private const string multiLanguageTestFileName = "testlang.mp4";
        private const string subtitleTestFileName = "test5.mkv";
        private const string testFileName = "testfile.ogg";
        private readonly List<DashEncodeResult> encodeResults;
        private bool disposedValue;
        private DashEncodeResult encodeResult;
        public EncodingTests()
        {
            encodeResults = new List<DashEncodeResult>();
        }

        private List<Quality> MultiLanguageQualities => new List<Quality>() { new Quality(640, 480, 768, H264Preset.ultrafast) };
        private List<Quality> Qualities => new List<Quality>()
        {
            new Quality(1920, 1080, 4000, H264Preset.fast),
            new Quality(1280, 720, 1280, H264Preset.fast),
            new Quality(640, 480, 768, H264Preset.fast)
        };

        private string RunPath => Environment.CurrentDirectory;
        private List<Quality> SubtitleQualities => new List<Quality>()
        {
            new Quality(1280, 720, 9000, H264Preset.fast),
            new Quality(640, 480, 768, H264Preset.faster)
        };
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void GenerateDash_NormalEncode_ProducesCorrectDashEncodeResult()
        {
            Encoder encoder = new Encoder(ffmpegPath, ffprobePath, mp4boxPath);
            DashConfig config = new DashConfig(testFileName, RunPath, Qualities, "output");

            encodeResult = encoder.GenerateDash(config);

            Assert.NotNull(encodeResult.DashFilePath);
            Assert.NotNull(encodeResult.DashFileContent);
            Assert.NotNull(encodeResult.MediaFiles);
            Assert.Equal(4, encodeResult.MediaFiles.Count());
        }

        [Fact]
        public void GenerateDash_WithCancellationToken_ThrowsOperationCanceledException()
        {
            var tokenSource = new CancellationTokenSource(500);
            DEnc.Encoder encoder = new DEnc.Encoder(ffmpegPath, ffprobePath, mp4boxPath);
            DashConfig config = new DashConfig(testFileName, RunPath, Qualities, "output");

            Assert.Throws<OperationCanceledException>(() => encodeResult = encoder.GenerateDash(config, cancel: tokenSource.Token));
        }

        [Fact]
        public void GenerateDash_WithLiveStreamingProfile_ProducesCorrectDashEncodeResult()
        {
            string outputFilename = "outputlive";

            var options = new H264EncodeOptions
            {
                AdditionalMP4BoxFlags = new List<string>()
                        {
                            "-profile \"dashavc264:live\"",
                            "-bs-switching no"
                        }
            };

            Encoder encoder = new Encoder(ffmpegPath, ffprobePath, mp4boxPath);
            DashConfig config = new DashConfig("testfile.ogg", RunPath, SubtitleQualities, outputFilename)
            {
                Options = options
            };

            try
            {
                encodeResult = encoder.GenerateDash(config);

                Assert.NotNull(encodeResult.DashFilePath);
                Assert.NotNull(encodeResult.DashFileContent);
            }
            finally
            {
                foreach (var file in Directory.EnumerateFiles(RunPath, $"{outputFilename}_*"))
                {
                    File.Delete(Path.Combine(RunPath, file));
                }
            }
        }

        [Fact]
        public void GenerateDash_WithManySubtitleLanguages_ProducesSubtitleFiles()
        {
            Encoder encoder = new Encoder(ffmpegPath, ffprobePath, mp4boxPath)
            {
                EnableStreamCopying = true
            };
            DashConfig config = new DashConfig(multiLanguageTestFileName, RunPath, MultiLanguageQualities, "outputlang");

            encodeResult = encoder.GenerateDash(config);

            Assert.NotNull(encodeResult.DashFilePath);
            Assert.NotNull(encodeResult.DashFileContent);
            Assert.NotNull(encodeResult.MediaFiles);
            Assert.Equal("avc1.640028", encodeResult.DashFileContent.Period[0].AdaptationSet[0].Representation[0].Codecs);
            Assert.True(encodeResult.DashFileContent.Period[0].AdaptationSet.Where(x => x.Lang == "jpn" && x.MaxFrameRate == null).SingleOrDefault().Representation.Count() == 1);
            Assert.True(encodeResult.DashFileContent.Period[0].AdaptationSet.Where(x => x.Lang == "eng" && x.MaxFrameRate == null).Count() == 2);
        }

        [Fact]
        public void GenerateDash_WithManySubtitles_ProducesSubtitleFiles()
        {
            Encoder encoder = new Encoder(ffmpegPath, ffprobePath, mp4boxPath)
            {
                EnableStreamCopying = true
            };
            DashConfig config = new DashConfig(subtitleTestFileName, RunPath, SubtitleQualities, "outputmulti");

            encodeResult = encoder.GenerateDash(config);

            Assert.NotNull(encodeResult.DashFilePath);
            Assert.NotNull(encodeResult.DashFileContent);
            Assert.NotNull(encodeResult.MediaFiles);
            Assert.Equal(16, encodeResult.MediaFiles.Count());
            Assert.Contains("outputmulti_audio_default_1_dashinit.mp4", encodeResult.MediaFiles);
            Assert.Contains("outputmulti_subtitle_eng_2.vtt", encodeResult.MediaFiles);
            Assert.Contains("outputmulti_subtitle_und_10.vtt", encodeResult.MediaFiles);
            Assert.Contains("outputmulti_subtitle_eng_12.vtt", encodeResult.MediaFiles);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Exceptions are caught as a collection then rethrown later.")]
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (encodeResult != null)
                    {
                        encodeResults.Add(encodeResult);
                    }

                    foreach (var result in encodeResults)
                    {
                        if (result?.DashFilePath != null)
                        {
                            string basePath = Path.GetDirectoryName(result.DashFilePath);
                            if (File.Exists(result.DashFilePath))
                            {
                                File.Delete(result.DashFilePath);
                            }

                            var exList = new List<Exception>();
                            foreach (var file in result.MediaFiles)
                            {
                                try
                                {
                                    File.Delete(Path.Combine(basePath, file));
                                }
                                catch (Exception ex)
                                {
                                    exList.Add(ex);
                                }
                            }
                            if (exList.Count > 0)
                            {
                                throw new Exception("Exceptions thrown during cleanup: " + string.Join("\n", exList));
                            }
                        }
                    }
                }

                disposedValue = true;
            }
        }
    }
}