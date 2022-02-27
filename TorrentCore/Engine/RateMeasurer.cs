// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

namespace TorrentCore.Engine;

/// <summary>
/// Provides statistics on data transfer speeds.
/// </summary>
public class RateMeasurer
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    private readonly LinkedList<RateMeasurement> _measurements = new LinkedList<RateMeasurement>();

    /// <summary>
    /// Initializes a new instance of the <see cref="RateMeasurer"/> class.
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
        lock (_measurements)
        {
            _measurements.AddLast(new RateMeasurement(DateTime.UtcNow, value));

            Clean();
        }
    }

    /// <summary>
    /// Resets all rate measurements to zero.
    /// </summary>
    public void Reset()
    {
        lock (_measurements)
        {
            _measurements.Clear();
        }
    }

    /// <summary>
    /// Returns the average rate in units per second.
    /// </summary>
    /// <returns>Average number of units per second.</returns>
    public long AverageRate()
    {
        lock (_measurements)
        {
            if (_measurements.Count < 2)
                return 0;

            Clean();

            long sum = 0;
            foreach (var measurement in _measurements)
                sum += measurement.Measurement;

            long bytesPerMillisecond = (long)(sum / Interval.TotalMilliseconds);
            return bytesPerMillisecond * 1000;
        }
    }

    private void Clean()
    {
        var minTime = DateTime.UtcNow - Interval;
        while (_measurements != null && _measurements.First().Time < minTime)
            _measurements.RemoveFirst();
    }

    class RateMeasurement
    {
        public RateMeasurement(DateTime time, long measurement)
        {
            Time = time;
            Measurement = measurement;
        }

        public DateTime Time { get; }

        public long Measurement { get; }
    }
}
