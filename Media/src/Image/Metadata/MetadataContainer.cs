using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace Qkmaxware.Media.Image.Metadata;

/// <summary>
/// Container for storing metadata
/// </summary>
public class MetadataContainer : IEnumerable<Field> {
    private List<Field> _fields = new List<Field>();
    
    /// <summary>
    /// Find the first metadata field with the given name
    /// </summary>
    /// <param name="name">field name</param>
    /// <returns>field value</returns>
    public Field FindField(string name) {
        return _fields.Where(field => field.Name == name).FirstOrDefault();
    }

    /// <summary>
    /// Find the first metadata field with the given tag
    /// </summary>
    /// <param name="tag">exif tag</param>
    /// <returns>field value</returns>
    public Field FindField(ExifTag tag) {
        return _fields.Where(field => field.ExifTag.HasValue && field.ExifTag.Value == tag).FirstOrDefault();
    }

    /// <summary>
    /// Find the first metadata field of the given type
    /// </summary>
    /// <typeparam name="T">field type</typeparam>
    /// <returns>field if it exists</returns>
    public Field<T>? FindField<T>() {
        return FindFields<T>().FirstOrDefault();
    }

    /// <summary>
    /// Find the first metadata field of the given type with the given field name
    /// </summary>
    /// <param name="name">field name</param>
    /// <typeparam name="T">field type</typeparam>
    /// <returns>field if it exists</returns>
    public Field<T>? FindField<T>(string name) {
        return (Field<T>?)_fields.Where(field => field is Field<T> && field.Name == name).FirstOrDefault();
    }

    /// <summary>
    /// Find the first metadata field of the given type with the given tag
    /// </summary>
    /// <param name="tag">exif tag</param>
    /// <typeparam name="T">field type</typeparam>
    /// <returns>field value</returns>
    public Field<T>? FindField<T>(ExifTag tag) {
        return (Field<T>?)_fields.Where(field => field is Field<T> && field.ExifTag.HasValue && field.ExifTag.Value == tag).FirstOrDefault();
    }

    /// <summary>
    /// Find all metadata fields of the given type
    /// </summary>
    /// <typeparam name="T">field type</typeparam>
    /// <returns>all fields of the given type</returns>
    public IEnumerable<Field<T>> FindFields<T>() {
        return _fields.OfType<Field<T>>();
    }

    /// <summary>
    /// Add a new metadata field
    /// </summary>
    /// <param name="field">field to add</param>
    /// <typeparam name="T">field type</typeparam>
    public void AddField(Field field) {
        _fields.Add(field);
    }

    /// <summary>
    /// Remove a specific metadata field
    /// </summary>
    /// <param name="field">field type</param>
    public void RemoveField(Field field) {
        _fields.Remove(field);
    }

    /// <summary>
    /// Remove all metadata fields of the given type
    /// </summary>
    /// <typeparam name="T">field type</typeparam>
    public void RemoveFields<T>() {
        _fields.RemoveAll(field => field is Field<T>);
    }

    public IEnumerator<Field> GetEnumerator() => _fields.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _fields.GetEnumerator();
}