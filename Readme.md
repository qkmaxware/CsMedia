# Qkmaxware.Media
This library contains importers and exporters for various media types (image, video, audio) that I found myself using a lot for personal projects.

- [Qkmaxware.Media](#qkmaxwaremedia)
  - [Build Status](#build-status)
  - [Getting Started](#getting-started)
  - [Supported Formats](#supported-formats)
    - [Audio](#audio)
    - [Image](#image)
    - [Video](#video)
  - [Example(s)](#examples)
    - [Change File Format](#change-file-format)
    - [Modifying Image Pixels](#modifying-image-pixels)
    - [Changing Image Bit Depth](#changing-image-bit-depth)
    - [Convert Image Sequence to Video](#convert-image-sequence-to-video)

## Build Status
![](https://github.com/qkmaxware/CsMedia/workflows/Build/badge.svg)

## Getting Started
The library is available as a NuGet package for any .Net implementation that supports the .Net Standard 2.0. Visit the [Packages](https://github.com/qkmaxware/CsMedia/packages) page for downloads.

## Supported Formats
### Audio
Generation of audio waveforms and serializing to WAV formatted files
| Format | Extension | Import | Export |
|--------|-----------|--------|--------|
| Wavefront | .wav   |        | &check;|

### Image
Serialization of pixel data to image file formats. Pixels are internally saved as samples of 32bit unsigned integers, the image's SampleBitDepth determines the "max" value of the pixel sample values.
| Format | Extension | Import | Export |
|--------|-----------|--------|--------|
| Portable Pixel Map | .ppm      | &check; | &check;|
| Targa Graphics | .tga | | &check; |
| Tag Image Format | .tif, .tiff | &check; | &check; |

### Video
Serialization of frames of images to video file formats.
| Format | Extension | Import | Export |
|--------|-----------|--------|--------|
| YUV4MPEG2 | .y4m   |        | &check;|

## Example(s)
Below are some example uses of this library, for other examples check out the **tests** in the **Media.Test** directory. 

### Change File Format
```cs
using Qkmaxware.Media.Image;

...

var from = new TagImageFileFormat();
var to = new TargaGraphicFormat();
var image = from.Load("path/to/my/image.tif");
to.Save("path/to/my/image.tga", image);
```

### Modifying Image Pixels
```cs
using Qkmaxware.Media.Image;

...

var format = new TagImageFileFormat();
var image = format.Load("path/to/my/image.tif");
for (var row = 0; row < image.Height; row++) {
    for (var col = 0; col < image.Width; col++) {
        // Do some transformation to the pixels
        image.Pixels[row, col] = transform(image.Pixels[row, col]);
    }
}
format.Save("path/to/my/image.tif", image);
```

### Changing Image Bit Depth
```cs
using Qkmaxware.Media.Image;
using Qkmaxware.Media.Image.Filters;

...

var format = new TagImageFileFormat();
var image = format.Load("path/to/my/image.8bit.tif");
var converted = image.ChangeBitDepth(SampleDepth.Bit16);
format.Save("path/to/my/image.16bit.tif", converted);
```

### Convert Image Sequence to Video
```cs
using Qkmaxware.Media.Image;
using Qkmaxware.Media.Video;

...

var format = new TagImageFileFormat();
var video = new MemoryVideo();
foreach (var file in Directory.GetFiles(".", "*.tif")) {
    video.AddFrame(format.Load(file));
}
var videoFormat = new Yuv4Mpeg2Format();
videoFormat.Save("path/to/my/video.y4m", video);
```