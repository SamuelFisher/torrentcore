// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TorrentCore.Engine
{
    class MainLoop : IMainLoop
    {
        private readonly AutoResetEvent handle = new AutoResetEvent(false);
        private readonly ConcurrentQueue<Action> queue = new ConcurrentQueue<Action>();
        private readonly HashSet<RegularTask> regularTasks = new HashSet<RegularTask>();

        private Thread mainLoop;
        private CancellationTokenSource cancelToken;
        private Timer regularTaskTimer;

        /// <summary>
        /// Gets or a value indicating whether the loop is running.
        /// </summary>
        public bool IsRunning { get; private set; }

        public void Start()
        {
            if (!IsRunning)
            {
                IsRunning = true;

                // Start main loop
                cancelToken = new CancellationTokenSource();
                mainLoop = new Thread(() =>
                {
                    Thread.CurrentThread.Name = "MainLoop";
                    Loop(cancelToken.Token);
                });
                mainLoop.Start();

                var scheduleRegularTasks = new Action(() =>
                {
                    AddTask(() =>
                    {
                        foreach (var task in regularTasks)
                            task.Execute();
                        regularTaskTimer.Change(100, Timeout.Infinite);
                    });
                });
                regularTaskTimer = new Timer(state => scheduleRegularTasks(), null, -1, Timeout.Infinite);
                scheduleRegularTasks();
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;

                // Stop main loop
                cancelToken.Cancel();
                AddTask(() => { });
            }
        }

        public void AddTask(Action t)
        {
            queue.Enqueue(t);
            handle.Set();
        }

        public IRegularTask AddRegularTask(Action t)
        {
            var rt = new RegularTask(t, RemoveRegularTask);
            regularTasks.Add(rt);
            return rt;
        }

        void Loop(CancellationToken ct)
        {
            while (true)
            {
                Action task = null;
                if (queue.Count > 0)
                    queue.TryDequeue(out task);

                if (task == null)
                    handle.WaitOne();
                else
                    task();

                if (ct.IsCancellationRequested)
                    break;
            }
        }

        void RemoveRegularTask(RegularTask task)
        {
            regularTasks.Remove(task);
        }

        private class RegularTask : IRegularTask
        {
            private readonly Action execute;
            private readonly Action<RegularTask> cancel;

            public RegularTask(Action execute, Action<RegularTask> cancel)
            {
                this.execute = execute;
                this.cancel = cancel;
            }

            public void Execute() => execute();

            public void Cancel() => cancel(this);
        }
    }
}
