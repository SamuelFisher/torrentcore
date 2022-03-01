// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace TorrentCore.Engine;

public class MainLoop : IMainLoop
{
    private readonly ILogger<MainLoop> _logger;
    private readonly AutoResetEvent _handle = new AutoResetEvent(false);
    private readonly ConcurrentQueue<Func<Task>> _queue = new ConcurrentQueue<Func<Task>>();
    private readonly HashSet<RegularTask> _regularTasks = new HashSet<RegularTask>();

    private Thread? _mainLoop;
    private CancellationTokenSource? _cancelToken;
    private Timer? _regularTaskTimer;

    public MainLoop(ILogger<MainLoop> logger)
    {
        _logger = logger;
    }

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
            _mainLoop = new Thread(async () =>
            {
                Thread.CurrentThread.Name = "MainLoop";
                await Loop(_cancelToken.Token);
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
        _queue.Enqueue(() =>
        {
            t();
            return Task.CompletedTask;
        });
        _handle.Set();
    }

    public IRegularTask AddRegularTask(Action t)
    {
        var rt = new RegularTask(
            () =>
            {
                t();
                return Task.CompletedTask;
            },
            RemoveRegularTask);
        _regularTasks.Add(rt);
        return rt;
    }

    public IRegularTask AddRegularTask(Func<Task> t)
    {
        var rt = new RegularTask(t, RemoveRegularTask);
        _regularTasks.Add(rt);
        return rt;
    }

    async Task Loop(CancellationToken ct)
    {
        while (true)
        {
            Func<Task>? task = null;
            if (_queue.Count > 0)
                _queue.TryDequeue(out task);

            if (task == null)
            {
                _handle.WaitOne();
            }
            else
            {
                try
                {
                    await task();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled task exception.");
                }
            }

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
        private readonly Func<Task> _execute;
        private readonly Action<RegularTask> _cancel;

        public RegularTask(Func<Task> execute, Action<RegularTask> cancel)
        {
            _execute = execute;
            _cancel = cancel;
        }

        public void Execute() => _execute();

        public void Dispose() => _cancel(this);
    }
}
