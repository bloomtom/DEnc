using System;
using Xunit;
using DEnc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NaiveProgress;
using System.Threading;
using DEnc.Models.Interfaces;
using DEnc.Models;

namespace DEncTests
{
    public class Tests
    {
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
                new Quality(0, 0, 500, default(H264Preset)),
                new Quality(0, 0, 1200, default(H264Preset)),
                new Quality(0, 0, 2000, default(H264Preset))
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
    }
}
