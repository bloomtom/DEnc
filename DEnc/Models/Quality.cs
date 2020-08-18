using System.Collections.Generic;

namespace DEnc.Models
{
    /// <summary>
    /// A Quality implementation that uses the Bitrate for Equals and GetHashCode, and displays a friendly output for ToString.
    /// </summary>
    public class Quality : IQuality
    {
        ///<inheritdoc cref="Quality"/>
        public Quality()
        {
        }

        /// <inheritdoc cref="Quality"/>
        /// <param name="width">Width of frame in pixels</param>
        /// <param name="height">Height of frame in pixels</param>
        /// <param name="bitrate">The bitrate in kb/s</param>
        /// <param name="preset">h264 preset</param>
        public Quality(int width, int height, int bitrate, H264Preset preset)
        {
            Width = width;
            Height = height;
            Bitrate = bitrate;
            Preset = preset;
        }

        /// <inheritdoc cref="Quality"/>
        /// <param name="width">Width of frame in pixels</param>
        /// <param name="height">Height of frame in pixels</param>
        /// <param name="bitrate">The bitrate in kb/s</param>
        /// <param name="preset">h264 preset</param>
        /// <param name="profile">h264 profile</param>
        public Quality(int width, int height, int bitrate, H264Preset preset, H264Profile profile)
            : this(width, height, bitrate, preset)
        {
            Profile = profile;
        }

        /// <summary>
        /// Bitrate of media in kb/s.
        /// </summary>
        public int Bitrate { get; set; } = 0;

        /// <summary>
        /// Height of frame in pixels.
        /// </summary>
        public int Height { get; set; } = 0;

        /// <summary>
        /// ffmpeg h264 encoding profile level (3.0, 4.0, 4.1...)
        /// </summary>
        public string Level { get; set; } = "4.0";

        /// <summary>
        /// ffmpeg pixel format or pix_fmt.
        /// </summary>
        public string PixelFormat { get; set; } = "yuv420p";

        /// <summary>
        /// ffmpeg preset (veryfast, fast, medium, slow, veryslow).
        /// </summary>
        public H264Preset Preset { get; set; } = H264Preset.Medium;

        /// <summary>
        /// ffmpeg h264 encoding profile (Baseline, Main, High,)
        /// </summary>
        public H264Profile Profile { get; set; } = H264Profile.High;

        /// <summary>
        /// Width of frame in pixels.
        /// </summary>
        public int Width { get; set; } = 0;
        /// <summary>
        /// Generates a set of qualities from a given DefaultQuality level.
        /// </summary>
        public static IEnumerable<IQuality> GenerateDefaultQualities(DefaultQuality q, H264Preset preset)
        {
            switch (q)
            {
                case DefaultQuality.Potato:
                    return new List<Quality>()
                    {
                        new Quality(1280, 720, 1600, preset),
                        new Quality(854, 480, 800, preset),
                        new Quality(640, 360, 500, preset)
                    };

                case DefaultQuality.Low:
                    return new List<Quality>()
                    {
                        new Quality(1280, 720, 2400, preset),
                        new Quality(1280, 720, 1600, preset),
                        new Quality(640, 360, 700, preset),
                    };

                case DefaultQuality.High:
                    return new List<Quality>()
                    {
                        new Quality(1920, 1080, 6000, preset),
                        new Quality(1920, 1080, 4000, preset),
                        new Quality(1280, 720, 2000, preset),
                    };

                case DefaultQuality.Ultra:
                    return new List<Quality>()
                    {
                        new Quality(1920, 1080, 8000, preset),
                        new Quality(1920, 1080, 6000, preset),
                        new Quality(1280, 720, 2000, preset),
                    };

                default:
                    break;
            }

            // Medium/default
            return new List<Quality>()
            {
                new Quality(1920, 1080, 3400, preset),
                new Quality(1280, 720, 1800, preset),
                new Quality(640, 360, 800, preset),
            };
        }

        /// <summary>
        /// Returns a "copy" quality. All integer values are zero and the preset is "copy".
        /// Directs ffmpeg to keep the original stream parameters.
        /// </summary>
        public static Quality GetCopyQuality()
        {
            return new Quality(0, 0, 0, H264Preset.None);
        }

        /// <summary>
        /// Uses Bitrate for comparison.
        /// </summary>
        public override bool Equals(object obj)
        {
            return base.Equals(Bitrate);
        }

        /// <summary>
        /// Yields the hash code for the Bitrate.
        /// </summary>
        public override int GetHashCode()
        {
            return Bitrate.GetHashCode();
        }

        /// <summary>
        /// Yields a user friendly string representation of the width, height, bitrate and preset.
        /// </summary>
        public override string ToString()
        {
            return $"{Width}x{Height} @ {Bitrate} kb/s - {Preset}";
        }
    }
}