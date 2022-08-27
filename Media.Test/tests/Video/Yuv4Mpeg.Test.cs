using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qkmaxware.Media.Image;
using Qkmaxware.Media.Video;

namespace Qkmaxware.Media.Test.Video {

[TestClass]
public class Yuv4MpegSerializerTest {
    [TestMethod]
    public void TestSerialization() {
        // Create animation
        List<IImage> frames = new List<IImage>();  
        var length = 48; 
        for (var i = 0; i <= length; i++) {
            var percent = ((i)/(double)length);
            var image = new Color[480,640];
            for (var k = 0; k < image.GetLength(0); k++) {
                for (var L = 0; L < image.GetLength(1); L++) {
                    image[k,L] = Color.Black;
                }
            }
            // 3 pixel wide line
            for (var j = 0; j < (percent * 640); j++) {
                // Move across the screen (height is first dimension, width the second)
                image[480/2,        j] = Color.Red;
                image[480/2 - 1,    j] = Color.Red;
                image[480/2 + 1,    j] = Color.Red;
            }
            frames.Add(new MemoryImage(image));
        }
        var video = new MemoryVideo(frames);

        // Serialize
        var serializer = new Yuv4Mpeg2Format();
        Directory.CreateDirectory("processed");
        using (var writer = new BinaryWriter(File.Open(Path.Combine("processed", "movie.y4m"), FileMode.Create))) {

            serializer.SaveTo(writer, video);
        }
    }   
}

}