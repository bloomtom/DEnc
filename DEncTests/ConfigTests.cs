using DEnc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace DEncTests
{
    public class ConfigTests
    {
        const string testFileName = "testfile.ogg";

        List<Quality> Qualities => new List<Quality>()
        {
            new Quality(1920, 1080, 4000, "fast"),
            new Quality(1280, 720, 1280, "fast"),
            new Quality(640, 480, 768, "fast")
        };

        [Fact]
        public void Constructor_WithDuplicateQualities_ThrowsArgumentException()
        {
            List<Quality> qualities = new List<Quality>()
            {
                new Quality(1280, 720, 9000, "ultrafast"),
                new Quality(1234, 754, 9000, "ultrafast")
            };

            var exception = Assert.Throws<ArgumentException>("qualities", () => new DashConfig(testFileName, Environment.CurrentDirectory, qualities));
            Assert.Equal("Duplicate quality bitrates found. Bitrates must be distinct.", exception.Message);
        }

        [Fact]
        public void Constructor_WithInvalidInputPath_ThrowsFileNotFoundException()
        {
            string testFile = "nonexistantFile.doesntexist";
            var exception = Assert.Throws<FileNotFoundException>(() => new DashConfig(testFile, Environment.CurrentDirectory, Qualities));
            Assert.Equal("Input path does not exist.", exception.Message);
        }
    }
}
