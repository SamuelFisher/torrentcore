// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System.Collections.Concurrent;

namespace TorrentCore.Engine;

public class MainLoop : IMainLoop
{
    private readonly AutoResetEvent _handle = new AutoResetEvent(false);
    private readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();
    private readonly HashSet<RegularTask> _regularTasks = new HashSet<RegularTask>();

    private Thread? _mainLoop;
    private CancellationTokenSource? _cancelToken;
    private Timer? _regularTaskTimer;

    /// <summary>
    /// Gets a value indicating whether the loop is running.
    /// </summary>
    public bool IsRunning { get; private set; }

    public void Start()
    {
        if (!IsRunning)
        {
            IsRunning = true;

            // Start main loop
            _cancelToken = new CancellationTokenSource();
            _mainLoop = new Thread(() =>
            {
                Thread.CurrentThread.Name = "MainLoop";
                Loop(_cancelToken.Token);
            });
            _mainLoop.Start();

            var scheduleRegularTasks = new Action(() =>
            {
                AddTask(() =>
                {
                    foreach (var task in _regularTasks)
                        task.Execute();
                    _regularTaskTimer!.Change(100, Timeout.Infinite);
                });
            });
            _regularTaskTimer = new Timer(state => scheduleRegularTasks(), null, -1, Timeout.Infinite);
            scheduleRegularTasks();
        }
    }

    public void Stop()
    {
        if (IsRunning)
        {
            IsRunning = false;

            // Stop main loop
            _cancelToken?.Cancel();
            AddTask(() => { });
        }
    }

    public void AddTask(Action t)
    {
        _queue.Enqueue(t);
        _handle.Set();
    }

    public IRegularTask AddRegularTask(Action t)
    {
        var rt = new RegularTask(t, RemoveRegularTask);
        _regularTasks.Add(rt);
        return rt;
    }

    void Loop(CancellationToken ct)
    {
        while (true)
        {
            Action? task = null;
            if (_queue.Count > 0)
                _queue.TryDequeue(out task);

            if (task == null)
                _handle.WaitOne();
            else
                task();

            if (ct.IsCancellationRequested)
                break;
        }
    }

    void RemoveRegularTask(RegularTask task)
    {
        _regularTasks.Remove(task);
    }

    private class RegularTask : IRegularTask
    {
        private readonly Action _execute;
        private readonly Action<RegularTask> _cancel;

        public RegularTask(Action execute, Action<RegularTask> cancel)
        {
            _execute = execute;
            _cancel = cancel;
        }

        public void Execute() => _execute();

        public void Dispose() => _cancel(this);
    }
}
