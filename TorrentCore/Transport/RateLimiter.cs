// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentCore.Transport
{
    /// <summary>
    /// Enforces a maximum transfer rate for upload and download speeds.
    /// </summary>
    public class RateLimiter
    {
        long _lastUpload;
        long _startedDownload;
        long _toUpload = 0;
        long _downloadedBytes;

        /// <summary>
        /// Initializes a new instance of the <see cref="RateLimiter"/> class with upload and download set to unlimited.
        /// </summary>
        public RateLimiter()
        {
            MaxUploadRate = 0;
            MaxDownloadRate = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateLimiter"/> class with the specified maximum upload and download rates.
        /// </summary>
        /// <param name="maxUploadRate">The maximum upload rate in bytes per second.</param>
        /// <param name="maxDownloadRate">The maximum download rate in bytes per second.</param>
        public RateLimiter(uint maxUploadRate, uint maxDownloadRate)
            : this()
        {
            MaxUploadRate = maxUploadRate;
            MaxDownloadRate = maxDownloadRate;
        }

        /// <summary>
        /// Gets the number of milliseconds since the system startup.
        /// </summary>
        protected long CurrentMilliseconds => Environment.TickCount;

        /// <summary>
        /// Gets or sets the maximum upload rate in bytes per second.
        /// </summary>
        public uint MaxUploadRate { get; set; }

        /// <summary>
        /// Gets or sets the maximum download rate in bytes per second.
        /// </summary>
        public uint MaxDownloadRate { get; set; }

        /// <summary>
        /// Gets a value indicating whether the upload rate is limited.
        /// </summary>
        public bool IsUploadLimited => MaxUploadRate != 0;

        /// <summary>
        /// Gets a value indicating whether the download rate is limited.
        /// </summary>
        public bool IsDownloadLimited => MaxDownloadRate != 0;

        /// <summary>
        /// Determines the amount of time that must pass until the specified number of bytes can be sent.
        /// </summary>
        /// <param name="length">Number of bytes to send.</param>
        /// <returns>Amount of time in milliseconds.</returns>
        public int TimeUntilCanSend(long length)
        {
            // Don't limit unlimited connections
            if (MaxUploadRate == 0)
                return 0;

            lock (this)
            {
                _toUpload += length;

                // If this is the first transmission, send without delay
                if (_lastUpload == 0)
                {
                    _lastUpload = CurrentMilliseconds;
                    return 0;
                }

                // How long should it take to send 'toUpload' bytes?
                double timeInSeconds = (double)_toUpload / (double)MaxUploadRate;
                Debug.WriteLine("It should take {0} seconds to write {1} bytes.", timeInSeconds, _toUpload);
                double timeInMilliseconds = timeInSeconds * 1000d;

                // How long has it been since the last data was sent?
                double interval = CurrentMilliseconds - _lastUpload;
                Debug.WriteLine(string.Format("It has actually been {0} seconds. (Last at {1}).", interval / 1000d, _lastUpload));

                // Wait the difference
                int difference = (int)Math.Round(timeInMilliseconds - interval);

                int wait = Math.Max(0, difference);
                if (wait > 0)
                {
                    Debug.WriteLine(string.Format("So let's wait {0} seconds.", difference / 1000d));
                    _lastUpload = CurrentMilliseconds + wait; // Last upload happened at the target time
                    _toUpload = 0;
                }
                else
                {
                    Debug.WriteLine("So let's not wait.");
                }

                return wait;
            }
        }

        /// <summary>
        /// Determines the amount of time that must pass until the specified number of bytes can be received.
        /// </summary>
        /// <param name="length">Number of bytes to receive.</param>
        /// <returns>Amount of time in milliseconds.</returns>
        public int TimeUntilCanReceive(long length)
        {
            if (_startedDownload == 0)
                _startedDownload = CurrentMilliseconds;

            if (MaxDownloadRate == 0)
                return 0; // Unlimited

            if (_downloadedBytes == 0)
            {
                _downloadedBytes += length;
                return 0;
            }

            _downloadedBytes += length;
            long elapsedMilliseconds = CurrentMilliseconds - _startedDownload;

            // Prevent rate going too high if data hasn't been sent for a while
            elapsedMilliseconds = Math.Min(elapsedMilliseconds, 1000);

            if (elapsedMilliseconds > 0)
            {
                // Calculate the current bps.
                long bps = _downloadedBytes * 1000L / elapsedMilliseconds;

                // If the bps are more then the maximum bps, try to throttle.
                if (bps > MaxDownloadRate)
                {
                    // Calculate the time to sleep.
                    long wakeElapsed = _downloadedBytes * 1000L / MaxDownloadRate;
                    int toSleep = (int)(wakeElapsed - elapsedMilliseconds);

                    if (toSleep > 1)
                    {
                        ResetDownload();
                        return toSleep;
                    }
                }
            }

            return 0;
        }

        void ResetDownload()
        {
            long difference = CurrentMilliseconds - _startedDownload;

            if (difference > 1000)
            {
                _downloadedBytes = 0;
                _startedDownload = CurrentMilliseconds;
            }
        }
    }
}
