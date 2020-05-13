using DEnc;
using DEnc.Models;
using NaiveProgress;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;

namespace DEncTests
{
    public class EncodingTests : IDisposable
    {
        const string ffmpegPath = @"C:\Program Files\ffmpeg\bin\ffmpeg.exe";
        const string ffprobePath = @"C:\Program Files\ffmpeg\bin\ffprobe.exe";
        const string mp4boxPath = @"C:\Program Files\GPAC\mp4box.exe";

        const string testFileName = "testfile.ogg";
        const string subtitleTestFileName = "test5.mkv";
        const string multiLanguageTestFileName = "testlang.mp4";

        List<DashEncodeResult> encodeResults;
        DashEncodeResult encodeResult;

        public EncodingTests()
        {
            encodeResults = new List<DashEncodeResult>();
        }

        string RunPath => Environment.CurrentDirectory;

        List<Quality> Qualities => new List<Quality>()
        {
            new Quality(1920, 1080, 4000, H264Preset.fast),
            new Quality(1280, 720, 1280, H264Preset.fast),
            new Quality(640, 480, 768, H264Preset.fast)
        };

        List<Quality> SubtitleQualities => new List<Quality>()
        {
            new Quality(1280, 720, 9000, H264Preset.ultrafast),
            new Quality(640, 480, 768, H264Preset.ultrafast)
        };

        List<Quality> MultiLanguageQualities => new List<Quality>() { new Quality(640, 480, 768, H264Preset.ultrafast) };

        [Fact]
        public void GenerateDash_NormalEncode_ProducesCorrectDashEncodeResult()
        {
            DEnc.Encoder encoder = new DEnc.Encoder(ffmpegPath, ffprobePath, mp4boxPath);
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
        public void GenerateDash_WithManySubtitles_ProducesSubtitleFiles()
        {
            DEnc.Encoder encoder = new DEnc.Encoder(ffmpegPath, ffprobePath, mp4boxPath);
            encoder.EnableStreamCopying = true;
            DashConfig config = new DashConfig(subtitleTestFileName, RunPath, SubtitleQualities, "outputmulti");

            encodeResult = encoder.GenerateDash(config);

            Assert.NotNull(encodeResult.DashFilePath);
            Assert.NotNull(encodeResult.DashFileContent);
            Assert.NotNull(encodeResult.MediaFiles);
            Assert.Equal(15, encodeResult.MediaFiles.Count());
            Assert.Contains("outputmulti_audio_default_1_dashinit.mp4", encodeResult.MediaFiles);
            Assert.Contains("outputmulti_subtitle_eng_2.vtt", encodeResult.MediaFiles);
            Assert.Contains("outputmulti_subtitle_und_10.vtt", encodeResult.MediaFiles);
            Assert.Contains("outputmulti_subtitle_eng_12.vtt", encodeResult.MediaFiles);
        }

        [Fact]
        public void GenerateDash_WithManySubtitleLanguages_ProducesSubtitleFiles()
        {
            DEnc.Encoder encoder = new DEnc.Encoder(ffmpegPath, ffprobePath, mp4boxPath);
            encoder.EnableStreamCopying = true;
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

            DEnc.Encoder encoder = new DEnc.Encoder(ffmpegPath, ffprobePath, mp4boxPath);
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

        public void Dispose()
        {
            if(encodeResult != null)
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

                    foreach (var file in result.MediaFiles)
                    {
                        try
                        {
                            File.Delete(Path.Combine(basePath, file));
                        }
                        catch (Exception) { }
                    }
                }
            }
        }
    }
}
