using System;
using System.Collections.Generic;
using System.IO;

namespace Qkmaxware;

/// <summary>
/// Iterate over a sequence of bytes in a certain endianness order 
/// </summary>
public class ByteIterator {

    private ByteOrder endianess;
    private IEnumerator<byte>? byteList;
    private BinaryReader? byteReader;

    public ByteIterator(ByteOrder endianess, IEnumerable<byte> bytes) {
        this.byteList = bytes.GetEnumerator();
        this.endianess = endianess;
    }

    public ByteIterator (ByteOrder endianess, BinaryReader bytes) {
        this.endianess = endianess;
        this.byteReader = bytes;
    }

    private bool advance() {
        if (byteList != null) {
            return byteList.MoveNext();
        } else if (byteReader != null) {
            return byteReader.BaseStream.Position < byteReader.BaseStream.Length;
        } else {
            return false;
        }
    }

    private byte current() {
        if (byteList != null) {
            return byteList.Current;
        } else if (byteReader != null) {
            return byteReader.ReadByte();
        } else {
            return default(byte);
        }
    }

    private byte[] read(int bytes) {
        // Read bytes (given in specific endian-order)
        byte[] array = new byte[bytes];
        for (var i = 0; i < bytes; i++) {
            if (advance()) {
                array[i] = current();
            } else {
                array[i] = default(byte);
            }
        }
        // Convert bytes to system endian order
        if (bytes > 1) {
            if (BitConverter.IsLittleEndian && endianess == ByteOrder.BigEndian) {
                Array.Reverse(array);
            } else if (!BitConverter.IsLittleEndian && endianess == ByteOrder.LittleEndian) {
                Array.Reverse(array);
            }
        }
        // Return array
        return array;
    }

    public byte ReadU8() {
        return read(1)[0];
    }

    public char ReadAscii() {
        return System.Text.Encoding.ASCII.GetString(read(1))[0];
    }

    public ushort ReadU16() {
        return BitConverter.ToUInt16(read(2), 0);
    }
    
    public uint ReadU32() {
        return BitConverter.ToUInt32(read(4), 0);
    }

    public ulong ReadU64() {
        return BitConverter.ToUInt32(read(8), 0);
    }

    public sbyte ReadS8() {
        return Convert.ToSByte(read(1)[0]);
    }

    public short ReadS16() {
        return BitConverter.ToInt16(read(2), 0);
    }
    
    public int ReadS32() {
        return BitConverter.ToInt32(read(4), 0);
    }

    public long ReadS64() {
        return BitConverter.ToInt32(read(8), 0);
    }

    public float ReadIEEEFloat() {
        return BitConverter.ToSingle(read(4), 0);
    }
    public double ReadIEEEDouble() {
        return BitConverter.ToDouble(read(8), 0);
    }
}