# DEnc
>Easy dotnet [DASH](https://en.wikipedia.org/wiki/Dynamic_Adaptive_Streaming_over_HTTP).

This library acts as a simplification interface wrapping around ffmpeg and mp4box. Simply pass in a file and the desired qualities, and the complicated commands and output processing is handled for you. The result is a set of media files an an mpd file


## Nuget Packages

Package Name | Target Framework | Version
---|---|---
[DEnc](https://www.nuget.org/packages/bloomtom.DEnc) | .NET Standard 2.0 | ![NuGet](https://img.shields.io/nuget/v/bloomtom.DEnc.svg)


## Usage
Usage is fairly straightforward. Create a new instance of `DEnc.Encoder` then call `encoder.GenerateDash`. Everything has a docstring, but the more esoteric bits are covered below.

## FAQ
##### What do I give for `ffmpegPath`, `ffprobePath` and `boxPath`?

You'll need an ffmpeg, ffprobe and mp4box executable on the system you're running from. If you put these in your path variable then the defaults can be left.

You can get executables for your platform here:
 - [ffmpeg](https://ffmpeg.org/)
 - [mp4box](https://gpac.wp.imt.fr/downloads/)

##### What are `stdoutLog` and `stderrLog`?

These parameters are optional on construction of `DEnc.Encoder`, but it's highly recommended you provide an action for them. Both callbacks are called to reflect the std output streams for ffmpeg and mp4box. Don't be fooled by the name "stderr", that's actually the logging output for most messages, not just errors. DEnc also writes its own info log entries to this callback.

##### What are qualities?

Qualities are sets of parameters which direct an ffmpeg output stream. You can have several qualities to support DASH [adaptive streaming](https://en.wikipedia.org/wiki/Adaptive_bitrate_streaming)

When you call `encoder.GenerateDash` a collection of qualities is required. It's important that the collection you provide is distinct on the bitrate, because the bitrate is used in output filenames to ensure they have unique names. Also, it doesn't make much sense to make multiple files at the same bitrate for a DASH encode anyway.

##### I asked for quality x,y,z but instead I got x,y,q or even just x,q instead. What gives?

The qualities you give may not be what's actually output. The desired Qualities list is _crushed_ against the input file. Quality crushing happens when you ask for output bitrates higher than the input file bitrate by more than 5%. All bitrates that meet this criteria are removed from the qualities list, and they're replaced by a single _copy_ quality that will mirror the input media exactly, or at least as close as ffmpeg can get it to be. This feature is definitely recommended to save disk space and reduce encode time, but it is optional. Set `DisableQualityCrushing` to true to disable crushing.