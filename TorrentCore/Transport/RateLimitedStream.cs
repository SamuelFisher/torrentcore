// This file is part of TorrentCore.
//     https://torrentcore.org
// Copyright (c) 2016 Sam Fisher.
// 
// TorrentCore is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, version 3.
// 
// TorrentCore is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with TorrentCore.  If not, see <http://www.gnu.org/licenses/>.

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
    class RateLimitedStream : Stream
    {
        Queue<byte[]> writeQueue = new Queue<byte[]>();
        bool isWriting = false;
        ManualResetEvent writeFinished = new ManualResetEvent(true);

        /// <summary>
        /// Gets the underlying stream.
        /// </summary>
        public Stream BaseStream { get; private set; }

        /// <summary>
        /// Gets the RateLimited for this stream.
        /// </summary>
        public RateLimiter Limiter { get; private set; }

        /// <summary>
        /// Creates a new RateLimitedStream using the specified underlying stream for data.
        /// </summary>
        /// <param name="baseStream">Stream to use for data.</param>
        public RateLimitedStream(Stream baseStream)
            : this(baseStream, 0, 0)
        {
        }

        /// <summary>
        /// Creates a new RateLimitedStream using the specified underlying stream for data.
        /// </summary>
        /// <param name="baseStream">Stream to use for data.</param>
        /// <param name="maxUploadRate">Maximum upload rate in bytes per second.</param>
        /// <param name="maxDownloadRate">Maximum upload rate in bytes per second.</param>
        public RateLimitedStream(Stream baseStream, uint maxUploadRate, uint maxDownloadRate)
        {
            BaseStream = baseStream;
            Limiter = new RateLimiter();
            Limiter.MaxUploadRate = maxUploadRate;
            Limiter.MaxDownloadRate = maxDownloadRate;
        }

        /// <summary>
        /// Creates a new RateLimitedStream limited by the specified RateLimiter.
        /// </summary>
        /// <param name="baseStream">Stream to use for data.</param>
        /// <param name="rateLimiter">RateLimiter to use.</param>
        public RateLimitedStream(Stream baseStream, RateLimiter rateLimiter)
        {
            BaseStream = baseStream;
            Limiter = rateLimiter;
        }

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
            lock (writeQueue)
            {
                if (!isWriting)
                {
                    startWriting = true;
                    isWriting = true;
                }
            }

            lock (writeQueue)
            {
                writeQueue.Enqueue(data);
                writeFinished.Reset();
            }

            if (startWriting)
            {
                Task.Factory.StartNew(() =>
                {
                    while (writeQueue.Count > 0)
                    {
                        byte[] d;
                        lock (writeQueue)
                            d = writeQueue.Dequeue();
                        int wait = Limiter.TimeUntilCanSend(d.Length);
                        if (wait > 0)
                            Task.Delay(wait).Wait();
                        BaseStream.Write(d, 0, d.Length);
                    }

                    lock (writeQueue)
                    {
                        isWriting = false;
                        writeFinished.Set();
                    }
                });
            }
        }

        #region Passthrough Methods

        public override bool CanRead
        {
            get { return BaseStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return BaseStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return BaseStream.CanWrite; }
        }

        public override void Flush()
        {
            // Wait for data to be written
            if (writeQueue.Count > 0)
                writeFinished.WaitOne();

            BaseStream.Flush();
        }

        public override long Length
        {
            get { return BaseStream.Length; }
        }

        public override long Position
        {
            get { return BaseStream.Position; }
            set { BaseStream.Position = value; }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            BaseStream.SetLength(value);
        }

        #endregion
    }
}
