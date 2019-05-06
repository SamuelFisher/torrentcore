// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TorrentCore.Transport
{
    /// <summary>
    /// Provides a stream with a specified maximum upload and download rate.
    ///
    /// Provides asynchronous writing and synchronous reading.
    /// </summary>
    internal partial class RateLimitedStream
    {
        private readonly Queue<byte[]> _writeQueue = new Queue<byte[]>();
        private readonly ManualResetEvent _writeFinished = new ManualResetEvent(true);

        bool _isWriting = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="RateLimitedStream"/> class.
        /// </summary>
        /// <param name="baseStream">Stream to use for data.</param>
        public RateLimitedStream(Stream baseStream)
            : this(baseStream, 0, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateLimitedStream"/> class.
        /// </summary>
        /// <param name="baseStream">Stream to use for data.</param>
        /// <param name="maxUploadRate">Maximum upload rate in bytes per second.</param>
        /// <param name="maxDownloadRate">Maximum download rate in bytes per second.</param>
        public RateLimitedStream(Stream baseStream, uint maxUploadRate, uint maxDownloadRate)
        {
            BaseStream = baseStream;
            Limiter = new RateLimiter();
            Limiter.MaxUploadRate = maxUploadRate;
            Limiter.MaxDownloadRate = maxDownloadRate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateLimitedStream"/> class.
        /// </summary>
        /// <param name="baseStream">Stream to use for data.</param>
        /// <param name="rateLimiter">RateLimiter to use.</param>
        public RateLimitedStream(Stream baseStream, RateLimiter rateLimiter)
        {
            BaseStream = baseStream;
            Limiter = rateLimiter;
        }

        /// <summary>
        /// Gets the underlying stream.
        /// </summary>
        public Stream BaseStream { get; }

        /// <summary>
        /// Gets the RateLimited for this stream.
        /// </summary>
        public RateLimiter Limiter { get; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int wait = Limiter.TimeUntilCanReceive(count);
            if (wait > 0)
                Task.Delay(wait).Wait();
            return BaseStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] data = new byte[count];
            Array.Copy(buffer, offset, data, 0, count);

            bool startWriting = false;
            lock (_writeQueue)
            {
                if (!_isWriting)
                {
                    startWriting = true;
                    _isWriting = true;
                }
            }

            lock (_writeQueue)
            {
                _writeQueue.Enqueue(data);
                _writeFinished.Reset();
            }

            if (startWriting)
            {
                Task.Factory.StartNew(() =>
                {
                    while (_writeQueue.Count > 0)
                    {
                        byte[] d;
                        lock (_writeQueue)
                            d = _writeQueue.Dequeue();
                        int wait = Limiter.TimeUntilCanSend(d.Length);
                        if (wait > 0)
                            Task.Delay(wait).Wait();
                        BaseStream.Write(d, 0, d.Length);
                    }

                    lock (_writeQueue)
                    {
                        _isWriting = false;
                        _writeFinished.Set();
                    }
                });
            }
        }
    }

    internal partial class RateLimitedStream : Stream
    {
        public override bool CanRead => BaseStream.CanRead;

        public override bool CanSeek => BaseStream.CanSeek;

        public override bool CanWrite => BaseStream.CanWrite;

        public override long Length => BaseStream.Length;

        public override long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        public override void Flush()
        {
            // Wait for data to be written
            if (_writeQueue.Count > 0)
                _writeFinished.WaitOne();

            BaseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            BaseStream.SetLength(value);
        }
    }
}
