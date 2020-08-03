using DEnc;
using DEnc.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace DEncTests
{
    public class ConfigTests
    {
        private const string testFileName = "testfile.ogg";

        private List<Quality> Qualities => new List<Quality>()
        {
            new Quality(1920, 1080, 4000, H264Preset.fast),
            new Quality(1280, 720, 1280, H264Preset.fast),
            new Quality(640, 480, 768, H264Preset.fast)
        };

        [Fact]
        public void Constructor_WithDuplicateQualities_ThrowsArgumentException()
        {
            List<Quality> qualities = new List<Quality>()
            {
                new Quality(1280, 720, 9000, H264Preset.ultrafast),
                new Quality(1234, 754, 9000, H264Preset.ultrafast)
            };

            var exception = Assert.Throws<ArgumentException>("qualities", () => new DashConfig(testFileName, Environment.CurrentDirectory, qualities));
            Assert.Equal("Duplicate quality bitrates found. Bitrates must be distinct.\r\nParameter name: qualities", exception.Message);
        }

        [Fact]
        public void Constructor_WithEmptyQualities_ThrowsArgumentOutOfRangeException()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>("qualities", () => new DashConfig(testFileName, Environment.CurrentDirectory, new List<Quality>()));
        }

        [Fact]
        public void Constructor_WithInvalidInputPath_ThrowsFileNotFoundException()
        {
            string testFile = "nonexistantFile.doesntexist";
            var exception = Assert.Throws<FileNotFoundException>(() => new DashConfig(testFile, Environment.CurrentDirectory, Qualities));
            Assert.Equal("Input path does not exist.", exception.Message);
        }

        [Fact]
        public void Constructor_WithInvalidOutputCharacters_CleansCharacters()
        {
            string outputName = "testfile*&:\\";
            DashConfig config = new DashConfig(testFileName, Environment.CurrentDirectory, Qualities, outputName);

            Assert.Equal("testfile", config.OutputFileName);
        }

        [Fact]
        public void Constructor_WithInvalidOutputPath_ThrowsDirectoryNotFoundException()
        {
            string testDir = @"D:\nodir\this\does\not\exist";
            var exception = Assert.Throws<DirectoryNotFoundException>(() => new DashConfig(testFileName, testDir, Qualities));
            Assert.Equal("Output directory does not exist.", exception.Message);
        }

        [Fact]
        public void Constructor_WithNullOutputFileName_UsesInputName()
        {
            DashConfig config = new DashConfig(testFileName, Environment.CurrentDirectory, Qualities);

            Assert.Equal("testfile", config.OutputFileName);
        }

        [Fact]
        public void Constructor_WithNullQualities_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>("qualities", () => new DashConfig(testFileName, Environment.CurrentDirectory, null));
        }
    }
}