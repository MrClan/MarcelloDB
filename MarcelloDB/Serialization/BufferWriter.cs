﻿using System;

namespace MarcelloDB.Serialization
{
    internal class BufferWriter
    {
        internal byte[] Buffer { get; set; }

        internal bool IsLittleEndian { get; set; }

        internal int Position { get; set; }

        internal BufferWriter(byte[] buffer)
        {
            Initialize(buffer, BitConverter.IsLittleEndian);
        }

        internal BufferWriter(byte[] buffer, bool isLittleEndian)
        {
            Initialize(buffer, isLittleEndian);
        }

        internal BufferWriter WriteByte(byte value)
        {
            WriteBytes(new byte[]{ value });
            return this;
        }

        public BufferWriter WriteInt16(Int16 value)
        {
            WriteBytes(LittleEndian(BitConverter.GetBytes(value)));
            return this;
        }

        internal BufferWriter WriteInt32(Int32 value)
        {
            WriteBytes(LittleEndian(BitConverter.GetBytes(value)));
            return this;
        }

        internal BufferWriter WriteInt64(Int64 value)
        {
            WriteBytes(LittleEndian(BitConverter.GetBytes(value)));
            return this;
        }

        internal BufferWriter WriteBytes(byte[] bytes)
        {
            if (this.Position + bytes.Length > Buffer.Length)
            {
                //Double buffer
                var doubleSize = (this.Position + bytes.Length) * 2;
                var newBuffer = new byte[(int)doubleSize];
                this.Buffer.CopyTo(newBuffer, 0);
                this.Buffer = newBuffer;
            }
            bytes.CopyTo(Buffer, this.Position);
            this.Position += bytes.Length;
            return this;
        }

        internal byte[] GetTrimmedBuffer()
        {
            if (this.Buffer.Length == this.Position)
            {
                return this.Buffer;
            }
            else
            {
                var trimmed = new byte[this.Position];
                Array.Copy(this.Buffer, trimmed, this.Position);
                return trimmed;
            }
        }

        void Initialize(byte[] buffer, bool isLittleEndian)
        {
            this.Buffer = buffer;
            this.Position = 0;
            this.IsLittleEndian = isLittleEndian;
        }

        byte[] LittleEndian(byte[] bytes)
        {
            if (!this.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }
    }
}


