using DEnc.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DEnc
{
    /// <summary>
    /// Common utilities
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Cleans a filename of invalid characters
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string CleanFileName(string name)
        {
            StringBuilder sb = new StringBuilder(name);
            foreach (string s in Constants.IllegalFilesystemChars)
            {
                sb.Replace(s, string.Empty);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Attempts to delete the given set of files and returns a collection of the failures.
        /// </summary>
        /// <param name="files">The file paths to delete.</param>
        /// <returns>A collection of failures.</returns>
        public static IEnumerable<(string Path, Exception Ex)> DeleteFilesFromDisk(IEnumerable<string> files)
        {
            if (files == null) { return Enumerable.Empty<(string, Exception)>(); }
            var failures = new List<(string, Exception)>();
            foreach (var file in files)
            {
                try
                {
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
                    failures.Add((file, ex));
                }
            }
            return failures;
        }

        /// <summary>
        /// Gets all the BaseURL file names from the MPD file
        /// </summary>
        /// <param name="mpdFile"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetFileNames(this MPD mpdFile)
        {
            if (mpdFile is null)
            {
                return new List<string>();
            }

            List<string> names = new List<string>();

            foreach (var period in mpdFile.Period)
            {
                foreach (var set in period.AdaptationSet)
                {
                    foreach (var representation in set.Representation)
                    {
                        names.AddRange(representation.BaseURL);
                    }
                }
            }

            return names;
        }

        /// <summary>
        /// MediaStream extension which returns false if the stream is detected as a non-media stream.
        /// </summary>
        public static bool IsStreamValid(this MediaStream stream)
        {
            if (stream == null) { return false; }

            string taggedMimetype = null;
            string taggedBitsPerSecond = null;

            if (stream.tag != null)
            {
                foreach (var tag in stream.tag)
                {
                    switch (tag.key.ToUpper())
                    {
                        case "BPS":
                            taggedBitsPerSecond = tag.value;
                            break;

                        case "MIMETYPE":
                            taggedMimetype = tag.value;
                            break;
                    }
                }
            }
            if (taggedMimetype != null && taggedMimetype.ToUpper().StartsWith("IMAGE/")) { return false; }
            if ((stream.bit_rate == 0 || (!string.IsNullOrWhiteSpace(taggedBitsPerSecond) && taggedBitsPerSecond != "0")) && stream.avg_frame_rate == "0/0") { return false; }

            return true;
        }
    }
}