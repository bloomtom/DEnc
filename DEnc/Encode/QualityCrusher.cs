using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DEnc
{
    /// <summary>
    /// Provides a method for crushing qualities.
    /// </summary>
    public static class QualityCrusher
    {
        private const double minCrushTolerance = 0.90;

        /// <summary>
        /// Removes qualities higher than the given bitrate and substitutes removed qualities with a copy quality.
        /// </summary>
        /// <param name="qualities">The quality collection to crush.</param>
        /// <param name="bitrateKbs">Bitrate in kb/s.</param>
        /// <returns></returns>
        public static IEnumerable<IQuality> CrushQualities(IEnumerable<IQuality> qualities, long bitrateKbs)
        {
            if (qualities == null || !qualities.Any()) { return qualities; }

            IQuality defaultQuality = qualities.First();

            // Crush
            var crushed = qualities.Where(x => x.Bitrate < bitrateKbs * minCrushTolerance).Distinct();
            if (crushed.Count() < qualities.Count())
            {
                if (crushed.Where(x => x.Bitrate == 0).FirstOrDefault() == null)
                {
                    var copyQuality = Quality.GetCopyQuality();
                    copyQuality.Level = defaultQuality.Level;
                    copyQuality.PixelFormat = defaultQuality.PixelFormat;
                    copyQuality.Profile = defaultQuality.Profile;
                    var newQualities = new List<IQuality>() { copyQuality }; // Add a copy quality to replace removed qualities.
                    newQualities.AddRange(crushed);
                    return newQualities;
                }

                return crushed;
            }

            return qualities;
        }
    }
}
