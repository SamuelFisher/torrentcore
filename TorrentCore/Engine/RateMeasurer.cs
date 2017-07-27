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

namespace TorrentCore.Engine
{
    /// <summary>
    /// Provides statistics on data transfer speeds.
    /// </summary>
    public class RateMeasurer
    {
        const int MeasurePoints = 40;

        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly LinkedList<RateMeasurement> measurements = new LinkedList<RateMeasurement>();

        /// <summary>
        /// Creates a new RateMeasurer.
        /// </summary>
        public RateMeasurer()
        {
        }

        /// <summary>
        /// Adds the specified measure to the collection of measurements.
        /// </summary>
        /// <param name="value">Change since last call to AddMeasure.</param>
        public void AddMeasure(long value)
        {
            if (!stopwatch.IsRunning)
                stopwatch.Start();

            lock (measurements)
            {
                measurements.AddLast(new RateMeasurement(stopwatch.ElapsedMilliseconds, value));

                if (measurements.Count > MeasurePoints)
                    measurements.RemoveFirst();
            }
        }

        /// <summary>
        /// Resets all rate measurements to zero.
        /// </summary>
        public void Reset()
        {
            if (stopwatch.IsRunning)
                stopwatch.Stop();

            lock (measurements)
            {
                measurements.Clear();
            }
        }

        /// <summary>
        /// Returns the average rate in units per second.
        /// </summary>
        /// <returns>Average number of units per second.</returns>
        public long AverageRate()
        {
            lock (measurements)
            {
                if (measurements.Count < 2)
                    return 0;

                RateMeasurement last = measurements.Last.Value;
                RateMeasurement first = measurements.First.Value;

                long duration = last.Milliseconds - first.Milliseconds;

                long sum = 0;
                foreach (var measurement in measurements)
                    sum += measurement.Measurement;

                long bytesPerMillisecond = sum / duration;
                return bytesPerMillisecond * 1000;
            }
        }

        class RateMeasurement
        {
            public long Milliseconds { get; private set; }
            public long Measurement { get; private set; }

            public RateMeasurement(long milliseconds, long measurement)
            {
                Milliseconds = milliseconds;
                Measurement = measurement;
            }
        }
    }
}
