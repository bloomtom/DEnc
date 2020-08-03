using DEnc.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DEnc
{
    /// <summary>
    /// Contains methods for determining if a set of streams is less advanced than a reference quality, and therefore is copyable.
    /// </summary>
    public static class Copyable264Infer
    {
        /// <summary>
        /// Compares a level in decimal form (4.2) to a level in integer form (42).
        /// </summary>
        /// <param name="maxLevel">The max level in decimal string form.</param>
        /// <param name="compare">The compare level in integer form.</param>
        /// <returns>True if the compare level is less than or equal to the max level.</returns>
        public static bool CompareLevels(string maxLevel, int compare) => decimal.TryParse(maxLevel, out decimal m) && (m * 10) >= compare;

        /// <summary>
        /// Compares two x264 profiles and returns true if the compare is less advanced than the max.
        /// </summary>
        /// <param name="maxProfile">The highest profile level to allow.</param>
        /// <param name="compare">The profile being checked.</param>
        /// <returns>True if the given compare is less advanced than the max. (main is less than high, high is less than high 10).</returns>
        public static bool CompareProfiles(string maxProfile, string compare)
        {
            var max = NormalizeProfile(maxProfile);
            var com = NormalizeProfile(compare);
            return max != -1 && com != -1 && com <= max;
        }

        /// <summary>
        /// Compares a set of streams to a reference quality, and determines if the set are all equal to or less advanced than the reference.
        /// </summary>
        /// <param name="pixelFormat">A pixel format like: yuv420p</param>
        /// <param name="level">A level like: 4.0</param>
        /// <param name="profile">A profile like: High</param>
        /// <param name="streams">A set of media streams to check for compatibility.</param>
        /// <returns>True if the input streams are copyable.</returns>
        public static bool DetermineCopyCanBeDone(string pixelFormat, string level, string profile, IEnumerable<MediaStream> streams)
        {
            bool enableStreamCopy =
                streams.All(x =>
                x.codec_name.Equals("h264", StringComparison.OrdinalIgnoreCase) &&
                x.pix_fmt.Equals(pixelFormat, StringComparison.OrdinalIgnoreCase) &&
                CompareLevels(level, x.level) &&
                CompareProfiles(profile, x.profile));
            return enableStreamCopy;
        }
        private static double NormalizeProfile(string profile)
        {
            switch (profile.ToLowerInvariant())
            {
                case "baseline": return 1;
                case "main": return 2.0;
                case "high": return 3.0;
                case "high10":
                case "high 10": return 3.1;
                case "high422":
                case "high 422": return 3.422;
                case "high444":
                case "high 444": return 3.444;
                default:
                    break;
            }
            return -1;
        }
    }
}