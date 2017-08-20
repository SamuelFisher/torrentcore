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
        long lastUpload;
        long startedDownload;
        long toUpload = 0;
        long downloadedBytes;
        
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
                toUpload += length;

                // If this is the first transmission, send without delay
                if (lastUpload == 0)
                {
                    lastUpload = CurrentMilliseconds;
                    return 0;
                }

                // How long should it take to send 'toUpload' bytes?
                double timeInSeconds = (double)toUpload / (double)MaxUploadRate;
                Debug.WriteLine("It should take {0} seconds to write {1} bytes.", timeInSeconds, toUpload);
                double timeInMilliseconds = timeInSeconds * 1000d;

                // How long has it been since the last data was sent?
                double interval = CurrentMilliseconds - lastUpload;
                Debug.WriteLine(string.Format("It has actually been {0} seconds. (Last at {1}).", interval / 1000d, lastUpload));

                // Wait the difference
                int difference = (int)Math.Round(timeInMilliseconds - interval);

                int wait = Math.Max(0, difference);
                if (wait > 0)
                {
                    Debug.WriteLine(string.Format("So let's wait {0} seconds.", difference / 1000d));
                    lastUpload = CurrentMilliseconds + wait; // Last upload happened at the target time
                    toUpload = 0;
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
            if (startedDownload == 0)
                startedDownload = CurrentMilliseconds;

            if (MaxDownloadRate == 0)
                return 0; // Unlimited

            if (downloadedBytes == 0)
            {
                downloadedBytes += length;
                return 0;
            }

            downloadedBytes += length;
            long elapsedMilliseconds = CurrentMilliseconds - startedDownload;

            // Prevent rate going too high if data hasn't been sent for a while
            elapsedMilliseconds = Math.Min(elapsedMilliseconds, 1000);

            if (elapsedMilliseconds > 0)
            {
                // Calculate the current bps.
                long bps = downloadedBytes * 1000L / elapsedMilliseconds;

                // If the bps are more then the maximum bps, try to throttle.
                if (bps > MaxDownloadRate)
                {
                    // Calculate the time to sleep.
                    long wakeElapsed = downloadedBytes * 1000L / MaxDownloadRate;
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
            long difference = CurrentMilliseconds - startedDownload;

            if (difference > 1000)
            {
                downloadedBytes = 0;
                startedDownload = CurrentMilliseconds;
            }
        }
    }
}
