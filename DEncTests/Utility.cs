using DEnc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DEncTests
{
    public static class Utility
    {
        public static void RunAndCleanupTest(Action<List<DashEncodeResult>> test)
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
