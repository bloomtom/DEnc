namespace DEnc.Models.Interfaces
{
    internal interface IStreamFile
    {
        string Argument { get; set; }
        int Index { get; set; }
        string Path { get; set; }
        StreamType Type { get; set; }
    }
}