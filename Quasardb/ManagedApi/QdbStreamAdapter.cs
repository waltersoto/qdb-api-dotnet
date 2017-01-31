﻿using System;
using System.IO;
using Quasardb.NativeApi;

namespace Quasardb.ManagedApi
{
    unsafe class QdbStreamAdapter : Stream
    {
        private readonly qdb_stream_handle _handle;
        private readonly bool _isWritable;

        public QdbStreamAdapter(qdb_stream_handle handle, bool isWritable)
        {
            _handle = handle;
            _isWritable = isWritable;
        }

        protected override void Dispose(bool disposing)
        {
            _handle.Dispose();
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            // no need to flush anything
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfClosed();

            long pos;
            bool checkUpperBound;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    pos = offset > 0 ? offset : 0;
                    checkUpperBound = offset > 0;
                    break;

                case SeekOrigin.Current:
                    pos = Position + offset;
                    checkUpperBound = offset > 0;
                    break;

                case SeekOrigin.End:
                    pos = offset >= 0 ? Length : Length + offset;
                    checkUpperBound = false;
                    break;

                default:
                    throw new NotSupportedException("SeekOrigin." + origin + " is not supported by stream");
            }

            pos = Math.Max(0, pos);
            if (checkUpperBound)
                pos = Math.Min(pos, Length);

            var upos = checked((ulong) pos);
            var error = qdb_api.qdb_stream_setpos(_handle, upos);
            QdbExceptionThrower.ThrowIfNeeded(error);
         
            return pos;
        }

        public override void SetLength(long value)
        {
            ThrowIfClosed();
            
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Stream length must be non-negative");

            var upos = unchecked((ulong)value);
            var error = qdb_api.qdb_stream_truncate(_handle, upos);

            if (error == qdb_error_t.qdb_e_out_of_bounds)
                throw new NotSupportedException("Size of stream cannot be increased by SetLength().");

            QdbExceptionThrower.ThrowIfNeeded(error);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfClosed();

            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Buffer cannot be null");

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "Non-negative number required");

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Non-negative number required");

            if (offset + count > buffer.Length)
                throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

            UIntPtr size = (UIntPtr)count;
            fixed (byte* ptr = buffer)
            {
                var error = qdb_api.qdb_stream_read(_handle, ptr + offset, ref size);
                QdbExceptionThrower.ThrowIfNeeded(error);
            }

            return (int)size;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowIfClosed();

            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Buffer cannot be null");

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "Non-negative number required");

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Non-negative number required");

            if (offset + count > buffer.Length)
                throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

            fixed (byte* ptr = buffer)
            {
                var error = qdb_api.qdb_stream_write(_handle, ptr+offset, (UIntPtr)count);
                QdbExceptionThrower.ThrowIfNeeded(error);
            }
        }

        public override bool CanRead
        {
            get { return !_handle.IsClosed; }
        }

        public override bool CanSeek
        {
            get { return !_handle.IsClosed; }
        }

        public override bool CanWrite
        {
            get { return _isWritable && !_handle.IsClosed; }
        }

        public override long Length
        {
            get
            {
                ThrowIfClosed();

                ulong size;
                var error = qdb_api.qdb_stream_size(_handle, out size);
                QdbExceptionThrower.ThrowIfNeeded(error);
                return checked((long) size);
            }
        }

        public override long Position
        {
            get
            {
                ThrowIfClosed();

                ulong position;
                var error = qdb_api.qdb_stream_getpos(_handle, out position);
                QdbExceptionThrower.ThrowIfNeeded(error);
                return checked((long)position);
            }
            set
            {
                ThrowIfClosed();

                var pos = Math.Max(0, Math.Min(value, Length));

                var upos = checked((ulong)pos);
                var error = qdb_api.qdb_stream_setpos(_handle, upos);
                QdbExceptionThrower.ThrowIfNeeded(error);
            }
        }

        private void ThrowIfClosed()
        {
            if (_handle.IsClosed)
                throw new ObjectDisposedException("Cannot access a closed Stream.");
        }
    }
}