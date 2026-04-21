using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;

namespace FindFileDuplicates
{
    /// <summary>
    /// Taken from CoreFx on GitHub 
    /// https://github.com/dotnet/corefx/blob/fd7aa74afeb64ee90693eb337bf5733edc1459e6/src/System.IO.Compression/src/System/IO/Compression/ZipCustomStreams.cs
    /// </summary>
    internal class SubReadStream : Stream
    {
        #region fields

        private readonly long _startInSuperStream;
        private long _positionInSuperStream;
        private readonly long _endInSuperStream;
        private readonly Stream _superStream;
        private Boolean _canRead;
        private Boolean _isDisposed;

        #endregion

        #region constructors

        public SubReadStream(Stream superStream, long startPosition, long maxLength)
        {
            _startInSuperStream = startPosition;
            _positionInSuperStream = startPosition;
            _endInSuperStream = startPosition + maxLength;
            _superStream = superStream;
            _canRead = true;
            _isDisposed = false;
        }

        #endregion

        #region properties

        public override long Length
        {
            get
            {
                Contract.Ensures(Contract.Result<Int64>() >= 0);

                ThrowIfDisposed();

                return _endInSuperStream - _startInSuperStream;
            }
        }

        public override long Position
        {
            get
            {
                Contract.Ensures(Contract.Result<Int64>() >= 0);

                ThrowIfDisposed();

                return _positionInSuperStream - _startInSuperStream;
            }
            set
            {
                ThrowIfDisposed();

                throw new NotSupportedException("SR.SeekingNotSupported");
            }
        }

        public override bool CanRead { get { return _superStream.CanRead && _canRead; } }

        public override bool CanSeek { get { return false; } }

        public override bool CanWrite { get { return false; } }

        #endregion

        #region methods

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(this.GetType().ToString(), "SR.HiddenStreamName");
        }
        private void ThrowIfCantRead()
        {
            if (!CanRead)
                throw new NotSupportedException("SR.ReadingNotSupported");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            //parameter validation sent to _superStream.Read
            int origCount = count;

            ThrowIfDisposed();
            ThrowIfCantRead();

            if (_superStream.Position != _positionInSuperStream)
                _superStream.Seek(_positionInSuperStream, SeekOrigin.Begin);
            if (_positionInSuperStream + count > _endInSuperStream)
                count = (int)(_endInSuperStream - _positionInSuperStream);

            Debug.Assert(count >= 0);
            Debug.Assert(count <= origCount);

            int ret = _superStream.Read(buffer, offset, count);

            _positionInSuperStream += ret;
            return ret;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfDisposed();
            throw new NotSupportedException("SR.SeekingNotSupported");
        }

        public override void SetLength(long value)
        {
            ThrowIfDisposed();
            throw new NotSupportedException("SR.SetLengthRequiresSeekingAndWriting");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            throw new NotSupportedException("SR.WritingNotSupported");
        }

        public override void Flush()
        {
            ThrowIfDisposed();
            throw new NotSupportedException("SR.WritingNotSupported");
        }

        // Close the stream for reading.  Note that this does NOT close the superStream (since 
        // the substream is just 'a chunk' of the super-stream 
        protected override void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                _canRead = false;
                _isDisposed = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }

}
