using DEnc.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace DEnc
{
    public static class Extensions
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
    }
}
