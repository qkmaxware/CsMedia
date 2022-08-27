using System;
using System.Text;

namespace Qkmaxware.Media.Image;

public class Histogram {

    public class Bucket {
        public long StartAt {get; private set;}
        public long EndAt {get; private set;}
        public long Size => EndAt - StartAt;
        public double Value {get; set;}

        public Bucket(long start, long end) {
            this.StartAt = start;
            this.EndAt = end;
        }
    }

    private Bucket[] buckets;

    public Histogram(IImage image, Func<Pixel,uint> selector, params long[] boundaries) {
        var start = 0L;
        this.buckets = new Bucket[boundaries.Length];
        for (var i = 0; i < this.buckets.Length; i++) {
            this.buckets[i] = new Bucket(start, boundaries[i]);
            start = boundaries[i];
        }

        if (image.Pixels == null) {
            return;
        }

        foreach (var pixel in image.Pixels) {
            var value = selector(pixel);
            
            // Get first bucket that can store pixel
            foreach (var bucket in this.buckets) {
                if (value >= bucket.StartAt && value < bucket.EndAt) {
                    bucket.Value++;
                    break;
                }
            }
        }
    }

    public Histogram(IImage image, Func<Pixel,uint> selector) {
        // Go over each pixel, place each pixel into a bucket
        if (image.Pixels == null) {
            buckets = new Bucket[0];
            return;
        }

        // Statistics
        uint min = 0;
        uint max = 0;
        bool first = true;
        foreach (var pixel in image.Pixels) {
            var value = selector(pixel);
            if (first) {
                first = false;
                min = value;
                max = value;
            } else {
                min = value < min ? value : min;
                max = value > max ? value : max;
            }
        }

        // Compute bucket size
        var range = max - min;
        uint count = 50;            // A fixed 50 buckets
        uint size = range / count;
        if (size == 0)
            size = 1;
        this.buckets = new Bucket[count];
        for(var i = 0; i < count; i++) {
            buckets[i] = new Bucket(i * size, (i + 1) * size);
        }

        // Organize pixels into buckets
        foreach (var pixel in image.Pixels) {
            var value = selector(pixel);
            
            // Get first bucket that can store pixel
            foreach (var bucket in this.buckets) {
                if (value >= bucket.StartAt && value < bucket.EndAt) {
                    bucket.Value++;
                    break;
                }
            }
        }
    }

    private Histogram(Bucket[] buckets) {
        this.buckets = buckets;
    }

    public Histogram ScaleData(int maxAllowedHeight) {
        double max = 0;
        foreach (var bucket in buckets) {
            max = bucket.Value > max ? bucket.Value : max;
        }   
        Bucket[] next = new Bucket[this.buckets.Length];
        for(var i = 0; i < this.buckets.Length; i++) {
            var bucket = new Bucket(this.buckets[i].StartAt, this.buckets[i].EndAt);
            next[i] = bucket;
            // Normalize data
            bucket.Value = (this.buckets[i].Value / max) * maxAllowedHeight;
        }

        return new Histogram(next);
    }

    public override string ToString() {
        double max = 0;
        foreach (var bucket in buckets) {
            max = bucket.Value > max ? bucket.Value : max;
        }   
        
        StringBuilder sb = new StringBuilder();
        for (double i = max; i > 0; i-=1) {
            sb.Append('|');
            for (var j = 0; j < this.buckets.Length; j++) {
                if (buckets[j].Value >= i) {
                    sb.Append('■');
                } else {
                    sb.Append(' ');
                }
            }
            sb.AppendLine();
        }
        sb.Append('|');
        for (var j = 0; j < this.buckets.Length; j++) {
            sb.Append('-');
        }

        return sb.ToString();
    }

}