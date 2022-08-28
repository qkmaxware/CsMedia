using System.Text.RegularExpressions;

namespace Qkmaxware.Media.Image.Metadata;

/// <summary>
/// Simple metadata field
/// </summary>
public class Field {
    /// <summary>
    /// Metadata field name
    /// </summary>
    /// <value>name</value>
    public string? Name {get; protected set;}
    /// <summary>
    /// Metadata Exchangeable Image File Format tag if applicable
    /// </summary>
    /// <value>EXIF tag</value>
    public ExifTag? ExifTag {get; protected set;}

    public Field(string name, ExifTag? tag) {
        this.Name = name;
        this.ExifTag = tag;
    }

    public Field(ExifTag tag) {
        this.ExifTag = tag;
        this.Name = tag.ToStringWithSpaces();
    }

    public override string ToString() => $"{Name}({ExifTag})";
}

/// <summary>
/// Metadata field storing a value of a particular type
/// </summary>
/// <typeparam name="T">stored value type</typeparam>
public class Field<T> : Field {
    /// <summary>
    /// Stored metadata field value
    /// </summary>
    /// <value>stored field value</value>
    public T? Value {get; set;}

    public Field(string name, ExifTag? tag) : base(name, tag) {}
    public Field(string name, ExifTag? tag, T? value) : base(name, tag) {
        this.Value = value;
    }
    public Field(ExifTag tag) : base(tag) {}
    public Field(ExifTag tag, T? value) : base(tag) {
        this.Value = value;
    }

    public override string ToString() => $"{Name}({ExifTag}) = {Value}({typeof(T)})";
}