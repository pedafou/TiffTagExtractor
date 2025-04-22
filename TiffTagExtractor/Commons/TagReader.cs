using System;
using System.IO;
using System.Text;

namespace TiffTagExtractor.Commons
{
    class TagReader : BinaryReader
    {
        public TagReader(System.IO.Stream stream) : base(stream) { }

        public bool Seek(object offset)
        {
            ulong pos = 0;
            try
            {
                pos = Convert.ToUInt64(offset);
            }
            catch
            {
                return false;
            }

            if (pos > Int64.MaxValue)
            {
                base.BaseStream.Seek((Int64)(Int64.MaxValue), SeekOrigin.Begin);
                base.BaseStream.Seek((Int64)(pos - Int64.MaxValue), SeekOrigin.Current);
            }
            else
            {
                base.BaseStream.Seek((Int64)pos, SeekOrigin.Begin);
            }

            return true;
        }

        public Array Read(int dataType, int count, bool byteOrder)
        {
            switch(dataType)
            {
                case 1:
                    return ReadByte(count, byteOrder);
                case 2:
                    return ReadAscii(count, byteOrder);
                case 3:
                    return ReadShort(count, byteOrder);
                case 4:
                    return ReadLong(count, byteOrder);
                case 5:
                    return ReadRational(count, byteOrder);
                case 6:
                    return ReadSByte(count, byteOrder);
                case 7:
                    return ReadUndefined(count, byteOrder);
                case 8:
                    return ReadSShort(count, byteOrder);
                case 9:
                    return ReadSLong(count, byteOrder);
                case 10:
                    return ReadSRational(count, byteOrder);
                case 11:
                    return ReadFloat(count, byteOrder);
                case 12:
                    return ReadDouble(count, byteOrder);
                default:
                    return null;
            }
        }

        // 1 BYTE
        public byte ReadByte(bool byteOrder)
        {
            var data = base.ReadByte();
            return data;
        }
        public byte[] ReadByte(int count, bool byteOrder)
        {
            var data = base.ReadBytes(count);
            return data;
        }

        // 2 ASCII
        public char[] ReadAscii(int count, bool byteOrder)
        {
            if (count == 0)
                return new char[] { };

            var data = base.ReadBytes(count);
            if (byteOrder)
                Array.Reverse(data);

            return Encoding.ASCII.GetString(data, 0, count).ToCharArray();
        }

        // 3 Unsigned Short
        public ushort ReadShort(bool byteOrder)
        {
            var data = base.ReadBytes(2);
            if (byteOrder)
                Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }
        public ushort[] ReadShort(int count, bool byteOrder)
        {
            var result = new ushort[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadShort(byteOrder);
            return result;
        }

        // 4 Unsigned LONG
        public UInt32 ReadLong(bool byteOrder)
        {
            var data = base.ReadBytes(4);
            if (byteOrder)
                Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }
        public UInt32[] ReadLong(int count, bool byteOrder)
        {
            var result = new UInt32[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadLong(byteOrder);
            return result;
        }

        // 5 Unsigned Rational
        public double ReadRational(bool byteOrder)
        {
            var data1 = base.ReadBytes(4);
            var data2 = base.ReadBytes(4);
            if (byteOrder)
            {
                Array.Reverse(data1);
                Array.Reverse(data2);
            }

            return Convert.ToDouble(BitConverter.ToUInt32(data1, 0))
                / Convert.ToDouble(BitConverter.ToUInt32(data2, 0));
        }
        public double[] ReadRational(int count, bool byteOrder)
        {
            var result = new double[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadRational(byteOrder);
            return result;
        }

        // 6 Signed BYTE
        public sbyte ReadSByte(bool byteOrder)
        {
            var data = base.ReadByte();
            return Convert.ToSByte(data);
        }
        public sbyte[] ReadSByte(int count, bool byteOrder)
        {
            var result = new sbyte[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadSByte(byteOrder);
            return result;
        }

        // 7 UNDEFINED
        public byte[] ReadUndefined(int count, bool byteOrder)
        {
            var data = base.ReadBytes(count);
            if (byteOrder)
                Array.Reverse(data);
            return data;
        }

        // 8 Signed SHORT
        public short ReadSShort(bool byteOrder)
        {
            var data = base.ReadBytes(2);
            if (byteOrder)
                Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }
        public short[] ReadSShort(int count, bool byteOrder)
        {
            var result = new short[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadSShort(byteOrder);
            return result;
        }

        // 9 Signed LONG
        public Int32 ReadSLong(bool byteOrder)
        {
            var data = base.ReadBytes(4);
            if (byteOrder)
                Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }
        public Int32[] ReadSLong(int count, bool byteOrder)
        {
            var result = new Int32[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadSLong(byteOrder);
            return result;
        }

        // 10 Signed Rational
        public double ReadSRational(bool byteOrder)
        {
            var data1 = base.ReadBytes(4);
            var data2 = base.ReadBytes(4);
            if (byteOrder)
            {
                Array.Reverse(data1);
                Array.Reverse(data2);
            }

            return Convert.ToDouble(BitConverter.ToInt32(data1, 0))
                / Convert.ToDouble(BitConverter.ToInt32(data2, 0));
        }
        public double[] ReadSRational(int count, bool byteOrder)
        {
            var result = new double[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadSRational(byteOrder);
            return result;
        }

        // 11 Single Float
        public float ReadFloat(bool byteOrder)
        {
            var data = base.ReadBytes(4);
            if (byteOrder)
                Array.Reverse(data);
            return BitConverter.ToSingle(data, 0);
        }
        public float[] ReadFloat(int count, bool byteOrder)
        {
            var result = new float[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadFloat(byteOrder);
            return result;
        }

        // 12 Double Float
        public double ReadDouble(bool byteOrder)
        {
            var data = base.ReadBytes(8);
            if (byteOrder)
                Array.Reverse(data);
            return BitConverter.ToDouble(data, 0);
        }
        public double[] ReadDouble(int count, bool byteOrder)
        {
            var result = new double[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadDouble(byteOrder);
            return result;
        }

        // ** Unsigned unsigned 64 Long
        public UInt64 ReadLong64(bool byteOrder)
        {
            var data = base.ReadBytes(8);
            if (byteOrder)
                Array.Reverse(data);
            return BitConverter.ToUInt64(data, 0);
        }

        public static int GetBytes(int dataType)
        {
            switch (dataType)
            {
                case 1:
                case 2:
                case 6:
                case 7:
                    return 1;
                case 3:
                case 8:
                    return 2;
                case 4:
                case 9:
                case 11:
                    return 4;
                case 5:
                case 10:
                case 12:
                    return 8;
                default:
                    return 0;
            }
        }
    }
}
