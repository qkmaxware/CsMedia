namespace Qkmaxware.Media.Image;

// Some EXIF tags from https://exiftool.org/TagNames/EXIF.html
public enum ExifTag : uint {
    Unknown = 0,
    PhotometricInterpretation = 262,
    Compression = 259, // 1 = none, 2 1D modified huffman encoding, 32773 Pack bits compression
    ImageLength = 257,
    ImageWidth = 256,
    ResolutionUnit = 296, // 1 = NONE, 2 = Inch, 3 = CM
    ResolutionX = 282,
    ResolutionY = 283,
    RowsPerStrip = 278, // if ImageLength is 24, and RowsPerStrip is 10, then there are 3 strips, with 10 rows in the first strip, 10 rows in the second strip, and 4 rows in the third strip. (The data in the last strip is not padded with 6 extra rows of dummy data.)
    StripOffsets = 273, // For each strip, the byte offset of that strip
    StripByteCounts = 279, // For each strip, the number of bytes in that strip after any compression.
    Software = 305,
    DateTime = 306,
    BitsPerSample = 258,
    ColourMap = 320, // Size = 3 * 2^BitsPerSample (triplets saves as RGB)
    SamplesPerPixel = 277, // The number of samples (RGB) per pixel. This is 3 for RGB images, but may have more for extra samples
    SubIFD = 330,
    SampleFormat = 339,
    ImageDescription = 270,
    Orientation = 274,
    PlanarConfiguration = 284,
    DocumentName = 269,
    PageName = 285,
    PageNumber = 297,
    ExtraSamples = 338,
    Artist = 315,
    ApplicationNotes = 700,
    ExifOffset = 34665,
    IccProfile = 34675,
    Make = 271,
    Model = 272,
    ExposureTime = 33434,
    FNumber = 33437,
    ISO = 34855,
    Aperture = 37378,
    FocalLength = 37386,
    LensMake = 42035,
    WhiteBalance = 41987,
    LensModel = 42036,
    Copyright = 33432,
}