using System.Text.RegularExpressions;

namespace DEnc.Encode
{
    internal static class Regexes
    {
        internal static Regex ParseProgress { get; } = new Regex(@"(?<=frame=.+time=)\d\S+", RegexOptions.Compiled);
    }
}