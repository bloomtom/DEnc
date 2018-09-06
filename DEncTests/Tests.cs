using System;
using Xunit;
using DEnc;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DEncTests
{
    public class Tests
    {
        [Fact]
        public void TestGenerateMpd()
        {
            DashEncodeResult s = null;

            try
            {
                string runPath = Path.Combine(Environment.CurrentDirectory, @"..\..\..\");

                float progress = 0;
                Encoder c = new Encoder();
                s = c.GenerateDash(
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
                    progress: new Action<float>(x => { progress = x; }));

                Assert.NotNull(s.DashFilePath);
                Assert.NotNull(s.DashFileContent);
                Assert.NotNull(s.MediaFiles);
                Assert.Equal(4, s.MediaFiles.Count());
                Assert.Equal(1, progress);
            }
            finally
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

        [Fact]
        public void TestMultipleTrack()
        {
            DashEncodeResult s = null;

            try
            {
                string runPath = Path.Combine(Environment.CurrentDirectory, @"..\..\..\");

                Encoder c = new Encoder();
                s = c.GenerateDash(
                    inFile: Path.Combine(runPath, "test5.mkv"),
                    outFilename: "outputmulti",
                    framerate: 30,
                    keyframeInterval: 90,
                    qualities: new List<Quality>
                    {
                        new Quality(1280, 720, 900, "ultrafast"),
                        new Quality(640, 480, 768, "ultrafast"),
                    },
                    outDirectory: runPath);

                Assert.NotNull(s.DashFilePath);
                Assert.NotNull(s.DashFileContent);
                Assert.NotNull(s.MediaFiles);
                Assert.Equal(12, s.MediaFiles.Count());
            }
            finally
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

        [Fact]
        public void TestFailOnDupQuality()
        {
            Encoder c = new Encoder();

            string testfile = Path.GetTempPath() + "denctestfile.test";
            using (File.Create(testfile, 1, FileOptions.DeleteOnClose))
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    c.GenerateDash(
                        inFile: testfile,
                        outFilename: "outputdup",
                        framerate: 30,
                        keyframeInterval: 90,
                        qualities: new List<Quality>
                        {
                        new Quality(1920, 1080, 4096, "veryfast"),
                        new Quality(1280, 720, 768, "veryfast"),
                        new Quality(640, 480, 768, "veryfast"),
                        });
                });
            }
        }
    }
}
