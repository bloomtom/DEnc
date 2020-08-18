using System.Text.RegularExpressions;

namespace DEnc
{
    internal static class Regexes
    {
        internal static Regex ParseProgress { get; } = new Regex(@"(?<=frame=.+time=)\d\S+", RegexOptions.Compiled);
    }
}