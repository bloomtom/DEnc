using System;
using Xunit;
using DEnc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NaiveProgress;
using System.Threading;

namespace DEncTests
{
    public class Tests
    {
        [Fact]
        public void TestGenerateMpd()
        {
            TestCleanup((results) =>
            {
                string runPath = Path.Combine(Environment.CurrentDirectory, @"..\..\..\");

                IEnumerable<EncodeStageProgress> progress = null;
                Encoder c = new Encoder();
                DashEncodeResult s = c.GenerateDash(
                    inFile: Path.Combine(runPath, "testfile.ogg"),
                    outFilename: "output",
                    framerate: 30,
                    keyframeInterval: 90,
                    qualities: new List<Quality>
                    {
                        new Quality(1920, 1080, 4000, "fast"),
                        new Quality(1280, 720, 1280, "fast"),
                        new Quality(640, 480, 768, "fast"),
                    },
                    outDirectory: runPath,
                    progress: new NaiveProgress<IEnumerable<EncodeStageProgress>>(x => { progress = x; }));
                results.Add(s);

                Assert.NotNull(s.DashFilePath);
                Assert.NotNull(s.DashFileContent);
                Assert.NotNull(s.MediaFiles);
                Assert.Equal(4, s.MediaFiles.Count());
                Assert.Equal(1.0, progress.Where(x => x.Name == "Encode").Select(y => y.Progress).Single());
                Assert.Equal(1.0, progress.Where(x => x.Name == "DASHify").Select(y => y.Progress).Single());
                Assert.Equal(1.0, progress.Where(x => x.Name == "Post Process").Select(y => y.Progress).Single());
            });
        }

        [Fact]
        public void TestCancellation()
        {
            var ts = new CancellationTokenSource(500);
            TestCleanup((results) =>
            {
                string runPath = Path.Combine(Environment.CurrentDirectory, @"..\..\..\");

                Encoder c = new Encoder();
                Assert.Throws<OperationCanceledException>(() =>
                {
                    results.Add(c.GenerateDash(
                    inFile: Path.Combine(runPath, "testfile.ogg"),
                    outFilename: "output",
                    framerate: 30,
                    keyframeInterval: 90,
                    qualities: new List<Quality>
                    {
                        new Quality(1920, 1080, 4000, "fast"),
                        new Quality(1280, 720, 1280, "fast"),
                        new Quality(640, 480, 768, "fast"),
                    },
                    outDirectory: runPath,
                    cancel: ts.Token));
                });
            });
        }

        [Fact]
        public void TestMultipleTrack()
        {
            TestCleanup((results) =>
            {
                string runPath = Path.Combine(Environment.CurrentDirectory, @"..\..\..\");

                Encoder c = new Encoder();
                c.EnableStreamCopying = true;
                DashEncodeResult s = c.GenerateDash(
                    inFile: Path.Combine(runPath, "test5.mkv"),
                    outFilename: "outputmulti#1",
                    framerate: 30,
                    keyframeInterval: 90,
                    qualities: new List<Quality>
                    {
                        new Quality(1280, 720, 9000, "ultrafast"),
                        new Quality(640, 480, 768, "ultrafast"),
                    },
                    outDirectory: runPath);
                results.Add(s);

                Assert.NotNull(s.DashFilePath);
                Assert.NotNull(s.DashFileContent);
                Assert.NotNull(s.MediaFiles);
                Assert.Equal(16, s.MediaFiles.Count());
                Assert.Contains("outputmulti1_audio_default_1_dashinit.mp4", s.MediaFiles);
                Assert.Contains("outputmulti1_subtitle_eng_2.vtt", s.MediaFiles);
                Assert.Contains("outputmulti1_subtitle_und_10.vtt", s.MediaFiles);
                Assert.Contains("outputmulti1_subtitle_eng_12.vtt", s.MediaFiles);
            });
        }

        [Fact]
        public void TestMultipleLanguage()
        {
            TestCleanup((results) =>
            {
                string runPath = Path.Combine(Environment.CurrentDirectory, @"..\..\..\");

                Encoder c = new Encoder();
                c.EnableStreamCopying = true;
                DashEncodeResult s = c.GenerateDash(
                    inFile: Path.Combine(runPath, "testlang.mp4"),
                    outFilename: "outputlang",
                    framerate: 30,
                    keyframeInterval: 90,
                    qualities: new List<Quality>
                    {
                        new Quality(640, 480, 768, "ultrafast"),
                    },
                    outDirectory: runPath);
                results.Add(s);

                Assert.NotNull(s.DashFilePath);
                Assert.NotNull(s.DashFileContent);
                Assert.NotNull(s.MediaFiles);
                Assert.Equal("avc1.42C028", s.DashFileContent.Period[0].AdaptationSet[0].Representation[0].Codecs);
                Assert.True(s.DashFileContent.Period[0].AdaptationSet.Where(x => x.Lang == "jpn" && x.MaxFrameRate == null).SingleOrDefault().Representation.Count() == 1);
                Assert.True(s.DashFileContent.Period[0].AdaptationSet.Where(x => x.Lang == "eng" && x.MaxFrameRate == null).SingleOrDefault().Representation.Count() == 2);
            });
        }

        [Fact]
        public void TestFailOnDupQuality()
        {
            TestCleanup((results) =>
            {
                Encoder c = new Encoder();

                string testfile = Path.GetTempPath() + "denctestfile.test";
                using (File.Create(testfile, 1, FileOptions.DeleteOnClose))
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() =>
                    {
                        results.Add(c.GenerateDash(
                            inFile: testfile,
                            outFilename: "outputdup",
                            framerate: 30,
                            keyframeInterval: 90,
                            qualities: new List<Quality>
                            {
                                new Quality(1920, 1080, 4096, "veryfast"),
                                new Quality(1280, 720, 768, "veryfast"),
                                new Quality(640, 480, 768, "veryfast"),
                            }));
                    });
                }
            });
        }

        [Fact]
        public void TestCopyableInferLevels()
        {
            Assert.True(Copyable264Infer.CompareLevels("4.2", 42));
            Assert.False(Copyable264Infer.CompareLevels("4.1", 42));
            Assert.True(Copyable264Infer.CompareLevels("1.0", 10));
            Assert.False(Copyable264Infer.CompareLevels("1.0", 15));
        }

        [Fact]
        public void TestCopyableInferProfiles()
        {
            Assert.True(Copyable264Infer.CompareProfiles("Main", "baseline"));
            Assert.True(Copyable264Infer.CompareProfiles("main", "main"));
            Assert.True(Copyable264Infer.CompareProfiles("High 10", "High"));
            Assert.True(Copyable264Infer.CompareProfiles("High444", "High 10"));
            Assert.False(Copyable264Infer.CompareProfiles("High", "High 10"));
            Assert.False(Copyable264Infer.CompareProfiles("High", "High10"));
            Assert.False(Copyable264Infer.CompareProfiles("main", "High 422"));
        }

        [Fact]
        public void TestCopyableInfer()
        {
            Assert.True(Copyable264Infer.DetermineCopyCanBeDone("yuv420p", "4.0", "High",
                new List<DEnc.Serialization.MediaStream>()
                {
                    new DEnc.Serialization.MediaStream(){ codec_name = "h264", level = 40, pix_fmt = "yuv420p", profile = "High" }
                }));
            Assert.True(Copyable264Infer.DetermineCopyCanBeDone("yuv420p", "4.2", "High",
                new List<DEnc.Serialization.MediaStream>()
                {
                    new DEnc.Serialization.MediaStream(){ codec_name = "h264", level = 40, pix_fmt = "yuv420p", profile = "Main" }
                }));
            Assert.False(Copyable264Infer.DetermineCopyCanBeDone("yuv420p", "4.0", "High",
                new List<DEnc.Serialization.MediaStream>()
                {
                    new DEnc.Serialization.MediaStream(){ codec_name = "h264", level = 40, pix_fmt = "yuv420p10le", profile = "High" }
                }));
            Assert.False(Copyable264Infer.DetermineCopyCanBeDone("yuv420p", "4.0", "High",
                new List<DEnc.Serialization.MediaStream>()
                {
                    new DEnc.Serialization.MediaStream(){ codec_name = "h264", level = 40, pix_fmt = "yuv420p", profile = "High 10" }
                }));
        }

        [Fact]
        public void TestQualityCrushing()
        {
            var testQualities = new List<IQuality>
            {
                new Quality(0, 0, 500, ""),
                new Quality(0, 0, 1200, ""),
                new Quality(0, 0, 2000, "")
            };

            // Test crush down
            var crushed = QualityCrusher.CrushQualities(testQualities, 1000);
            Assert.True(crushed.Where(x => x.Bitrate == 0).SingleOrDefault() != null);
            Assert.True(crushed.Where(x => x.Bitrate == 500).SingleOrDefault() != null);
            Assert.Equal(2, crushed.Count());

            // Test crush against lower tolerance
            crushed = QualityCrusher.CrushQualities(testQualities, 1100);
            Assert.True(crushed.Where(x => x.Bitrate == 0).SingleOrDefault() != null);
            Assert.True(crushed.Where(x => x.Bitrate == 500).SingleOrDefault() != null);
            Assert.Equal(2, crushed.Count());

            // Test crush against upper tolerance
            crushed = QualityCrusher.CrushQualities(testQualities, 1300);
            Assert.True(crushed.Where(x => x.Bitrate == 0).SingleOrDefault() != null);
            Assert.True(crushed.Where(x => x.Bitrate == 500).SingleOrDefault() != null);
            Assert.Equal(2, crushed.Count());

            // Test crush above upper tolerance
            crushed = QualityCrusher.CrushQualities(testQualities, 1400);
            Assert.True(crushed.Where(x => x.Bitrate == 0).SingleOrDefault() != null);
            Assert.True(crushed.Where(x => x.Bitrate == 500).SingleOrDefault() != null);
            Assert.True(crushed.Where(x => x.Bitrate == 1200).SingleOrDefault() != null);
            Assert.Equal(3, crushed.Count());

            // Test no crushing
            crushed = QualityCrusher.CrushQualities(testQualities, 4000);
            Assert.True(crushed.Where(x => x.Bitrate == 500).SingleOrDefault() != null);
            Assert.True(crushed.Where(x => x.Bitrate == 1200).SingleOrDefault() != null);
            Assert.True(crushed.Where(x => x.Bitrate == 2000).SingleOrDefault() != null);
            Assert.Equal(3, crushed.Count());

            // Test crush to bottom
            crushed = QualityCrusher.CrushQualities(testQualities, 400);
            Assert.True(crushed.Where(x => x.Bitrate == 0).SingleOrDefault() != null);
            Assert.Single(crushed);
        }

        internal void TestCleanup(Action<List<DashEncodeResult>> test)
        {
            var results = new List<DashEncodeResult>();

            try
            {
                test.Invoke(results);
            }
            finally
            {
                foreach (var s in results)
                {
                    if (s?.DashFilePath != null)
                    {
                        string basePath = Path.GetDirectoryName(s.DashFilePath);
                        if (File.Exists(s.DashFilePath))
                        {
                            File.Delete(s.DashFilePath);
                        }

                        foreach (var file in s.MediaFiles)
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
}
