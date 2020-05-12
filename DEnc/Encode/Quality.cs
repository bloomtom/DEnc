using System;
using System.Collections.Generic;
using System.Text;

namespace DEnc
{
    /// <summary>
    /// An interface for a quality. Qualities are used to specify the outputs for DASHification.
    /// </summary>
    public interface IQuality
    {
        /// <summary>
        /// Width of frame in pixels.
        /// </summary>
        int Width { get; set; }
        /// <summary>
        /// Height of frame in pixels.
        /// </summary>
        int Height { get; set; }
        /// <summary>
        /// Bitrate of media in kb/s.
        /// </summary>
        int Bitrate { get; set; }
        /// <summary>
        /// ffmpeg preset (veryfast, fast, medium, slow, veryslow).
        /// </summary>
        string Preset { get; set; }
        /// <summary>
        /// ffmpeg h264 encoding profile (Baseline, Main, High,)
        /// </summary>
        string Profile { get; set; }
        /// <summary>
        /// ffmpeg h264 encoding profile level (3.0, 4.0, 4.1...)
        /// </summary>
        string Level { get; set; }
        /// <summary>
        /// ffmpeg pixel format or pix_fmt.
        /// </summary>
        string PixelFormat { get; set; }
    }

    /// <summary>
    /// A Quality implementation that uses the Bitrate for Equals and GetHashCode, and displays a friendly output for ToString.
    /// </summary>
    public class Quality : IQuality
    {
        /// <summary>
        /// Width of frame in pixels.
        /// </summary>
        public int Width { get; set; } = 0;
        /// <summary>
        /// Height of frame in pixels.
        /// </summary>
        public int Height { get; set; } = 0;
        /// <summary>
        /// Bitrate of media in kb/s.
        /// </summary>
        public int Bitrate { get; set; } = 0;
        /// <summary>
        /// ffmpeg preset (veryfast, fast, medium, slow, veryslow).
        /// </summary>
        public string Preset { get; set; } = "medium";
        /// <summary>
        /// ffmpeg h264 encoding profile (Baseline, Main, High,)
        /// </summary>
        public string Profile { get; set; } = "high";
        /// <summary>
        /// ffmpeg h264 encoding profile level (3.0, 4.0, 4.1...)
        /// </summary>
        public string Level { get; set; } = "4.0";
        /// <summary>
        /// ffmpeg pixel format or pix_fmt.
        /// </summary>
        public string PixelFormat { get; set; } = "yuv420p";

        /// <summary>
        /// 
        /// </summary>
        public Quality()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width">Width of frame in pixels</param>
        /// <param name="height">Height of frame in pixels</param>
        /// <param name="bitrate">The bitrate in kb/s</param>
        /// <param name="preset">h264 preset</param>
        public Quality(int width, int height, int bitrate, string preset)
        {
            Width = width;
            Height = height;
            Bitrate = bitrate;
            Preset = preset;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width">Width of frame in pixels</param>
        /// <param name="height">Height of frame in pixels</param>
        /// <param name="bitrate">The bitrate in kb/s</param>
        /// <param name="preset">h264 preset</param>
        /// <param name="profile">h264 profile</param>
        public Quality(int width, int height, int bitrate, string preset, string profile)
            :this(width, height, bitrate, preset)
        {
            Profile = profile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Width}x{Height} @ {Bitrate} kb/s - {Preset}";
        }

        /// <summary>
        /// Generates a set of qualities from a given DefaultQuality level.
        /// </summary>
        /// <param name="q"></param>
        /// <param name="preset"></param>
        /// <returns></returns>
        public static IEnumerable<IQuality> GenerateDefaultQualities(DefaultQuality q, string preset)
        {
            switch (q)
            {
                case DefaultQuality.potato:
                    return new List<Quality>()
                    {
                        new Quality(1280, 720, 1600, preset),
                        new Quality(854, 480, 800, preset),
                        new Quality(640, 360, 500, preset)
                    };
                case DefaultQuality.low:
                    return new List<Quality>()
                    {
                        new Quality(1280, 720, 2400, preset),
                        new Quality(1280, 720, 1600, preset),
                        new Quality(640, 360, 700, preset),
                    };
                case DefaultQuality.high:
                    return new List<Quality>()
                    {
                        new Quality(1920, 1080, 6000, preset),
                        new Quality(1920, 1080, 4000, preset),
                        new Quality(1280, 720, 2000, preset),
                    };
                case DefaultQuality.ultra:
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
            return new Quality(0, 0, 0, "");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return base.Equals(Bitrate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Bitrate.GetHashCode();
        }
    }
}
