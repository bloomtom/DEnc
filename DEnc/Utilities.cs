using DEnc.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DEnc
{
    public static class Utilities
    {
        public static bool IsStreamValid(this MediaStream stream)
        {
            if (stream == null) { return false; }

            string taggedMimetype = null;
            string taggedFilename = null;
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
                        case "FILENAME":
                            taggedFilename = tag.value;
                            break;
                    }
                }
            }
            if (taggedMimetype != null && taggedMimetype.ToUpper().StartsWith("IMAGE/")) { return false; }
            if ((stream.bit_rate == 0 || (!string.IsNullOrWhiteSpace(taggedBitsPerSecond) && taggedBitsPerSecond != "0")) && stream.avg_frame_rate == "0/0") { return false; }

            return true;
        }

        /// <summary>
        /// Cleans a filename of invalid characters
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string CleanFileName(string name)
        {
            StringBuilder sb = new StringBuilder(name);
            foreach(string s in Constants.IllegalFilesystemChars)
            {
                sb.Replace(s, String.Empty);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets all the BaseURL file names from the MPD file
        /// </summary>
        /// <param name="mpdFile"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetFileNames(this MPD mpdFile)
        {
            if(mpdFile is null)
            {
                return new List<string>();
            }

            List<string> names = new List<string>();

            foreach(var period in mpdFile.Period)
            {
                foreach(var set in period.AdaptationSet)
                {
                    foreach(var representation in set.Representation)
                    {
                        names.AddRange(representation.BaseURL);
                    }
                }
            }

            return names;
        }
    }
}
