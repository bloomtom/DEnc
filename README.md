# DEnc
>Easy dotnet [DASH](https://en.wikipedia.org/wiki/Dynamic_Adaptive_Streaming_over_HTTP "https://en.wikipedia.org").

This library acts as a simplification interface wrapping around ffmpeg and mp4box. Simply pass in a file and the desired qualities, and the complicated commands and output processing is handled for you. The result is a set of media files and an mpd file.

## Nuget Packages
Package Name | Target Framework | Version
---|---|---
[DEnc](https://www.nuget.org/packages/bloomtom.DEnc "https://www.nuget.org") | .NET Standard 2.0 | ![NuGet](https://img.shields.io/nuget/v/bloomtom.DEnc.svg)


## Usage

### v1.0 API (legacy)
Usage is fairly straightforward. Create a new instance of `DEnc.Encoder` then call `encoder.GenerateDash`. This version has been deprecated. The rest of this readme refers to the 2.0 API.

### v2.0 API
This API version provides much more extensibility and insight for the library user. Standard usage remains simple though. Create your encoder instance, then call `encoder.ProbeFile` to get some `MediaMetadata` for the content you're converting. This probe step was broken out so you can inspect the media metadata before deciding on the encoding approach to use. Then create an instance of `DashConfig`, and pass that along with your media metadata to `encoder.GenerateDash`.

#### Usage Example
```csharp
string inputPath = "/home/bloomtom/videos/ubuntu.iso"; // Your input video file.

var probeData = encoder.ProbeFile(inputPath, out _);
var qualities = Quality.GenerateDefaultQualities(DefaultQuality.Medium, H264Preset.Medium);
var dashConfig = new DashConfig(inputPath, "/home/bloomtom/encoded/", qualities);

var dashResult = encoder.GenerateDash(dashConfig, probeData);
```

##### DashConfig
The bulk of parameters for `GenerateDash` are contained in the data POCO `DashConfig`. The constructor for this performs several pre-check functions. When creating an instance of `DashConfig`, an exception will be thrown if the input path doesn't exist, the output directory doesn't exist, you gave a null or empty qualities parameter, or the set of qualities is not distinct on the bitrate. It also performs the function of either generating an output filename, or ensuring the filename given doesn't contain any illegal characters. The `OutputFileName` property has a setter, but it's recommended that you don't use it unless illegal character stripping of the output filename is undesirable.

##### Progress
Progress is now reported as one double precision value as the other stages turned out to take negligible time in practice.

#### Exceptions

 - **`ProbeFile`**
   - `Exception`

     Thrown if ffprobe cannot be run, or it throws a hard error.

   - `InvalidOperationException`

     May be thrown during deserialization of the ffprobe output. Deserialization is provided by [XmlSerializer](https://docs.microsoft.com/en-us/dotnet/api/system.xml.serialization.xmlserializer.deserialize?view=netstandard-2.0 "https://docs.microsoft.com").

 - **`DashConfig`**
   - `FileNotFoundException`

     The input path does not exist.

   - `DirectoryNotFoundException`

     The output directory does not exist.

   - `ArgumentNullException`

     The qualities parameter is null.

   - `ArgumentOutOfRangeException`

     The qualities parameter is an empty set.

   - `ArgumentException`

     The set of qualities contains two or more qualities with the same bitrate.

 - **`GenerateDash`**
   - `ArgumentNullException`

     The given `probedInputData` value is null.

   - `OperationCanceledException`

     The given cancel token is triggered and a token checkpoint is reached.

   - `FFMpegFailedException`

     The base class for complex exceptions. Thrown when the ffmpeg encode process failed. The exception value contains detailed information about the running state, including the parameters given to ffmpeg, and StringBuilder containing the running log received from ffmpeg while it was running. This log can often contain insight into why the file could not be encoded.

     - `Mp4boxFailedException`

       Derives from `FFMpegFailedException`. Thrown when the MP4Box fails, and contains both the parameters given to ffmpeg and MP4Box, as well as the complete log from both processes.

       - `DashManifestNotCreatedException`

         Derives from `Mp4boxFailedException`. Thrown when after all processing, the expected .mpd dash manifest doesn't exist on disk. This exception really shouldn't be thrown, and exists more as a guarantee that when `GenerateDash` returns a value, the mpd file _really_ exists, for sure. This exception contains everything you'd expect in an `Mp4boxFailedException`, as well as the expected path for the mpd file.

## FAQ

##### What do I give for `ffmpegPath`, `ffprobePath` and `boxPath`?
You'll need an ffmpeg, ffprobe and mp4box executable on the system you're running from. If you put these in your path variable then the defaults can be left.

You can get executables for your platform here:
 - [ffmpeg](https://ffmpeg.org/ "https://ffmpeg.org/")
 - [mp4box](https://gpac.wp.imt.fr/downloads/ "https://gpac.wp.imt.fr/downloads/")


##### What are `stdoutLog` and `stderrLog`?
These parameters are optional on construction of `DEnc.Encoder` and provide a log stream from ffmpeg and MP4Box. For the v1.0 API it was recommended that you provide an action for them. On the v2.0 API these logs are recorded internally, and exceptions from `GenerateDash` include them, which means you probably need to collect them all the time yourself unless you really want to.


##### What are qualities?
Qualities are sets of parameters which direct an ffmpeg output stream. You can have several qualities to support DASH [adaptive streaming](https://en.wikipedia.org/wiki/Adaptive_bitrate_streaming "https://en.wikipedia.org")

When you call `encoder.GenerateDash` a collection of qualities is required. It's important that the collection you provide is distinct on the bitrate, because the bitrate is used in output filenames to ensure they have unique names. Also, it doesn't make much sense to make multiple files at the same bitrate for a DASH encode anyway.


##### I asked for quality x,y,z but instead I got x,y,q or even just x,q instead. What gives?
The qualities you give may not be what's actually output. The desired Qualities list is _crushed_ against the input file. Quality crushing happens when you ask for output bitrates within 10% of or greater than input file bitrate (by default). All bitrates which meet this criteria are removed from the qualities list, and they're replaced by a single _copy_ quality that will mirror the input media exactly, or at least as close as ffmpeg can get it to be. This feature is definitely recommended to save disk space and reduce encode time, but it is optional. Set your `DashConfig` instance's `QualityCrushTolerance` property to zero to disable crushing for an encode.


##### I have extra subtitle files. How do I get them into the output mpd?
This is supported natively. Simply put vtt files alongside the source file with the same name, and they will be picked up. You can have multiple vtt files by using the naming format `file.X.vtt`. where X is some string or language code.

For the input file "myvideo.mp4", the following table shows what will be picked up as an external subtitle file.

Filename | Language | Imported?
---|---|---
myvideo.mp4 |  | No, this is the source.
myvideo.srt |  | No, only vtt is supported.
myvideo.vtt | unk | Yes
myvideo.2.vtt | unk | Yes
myvideo.subtitleasdf.vtt | unk | Yes
myvideo.en.vtt | eng | Yes
myvideo.jpn.vtt | jpn | Yes