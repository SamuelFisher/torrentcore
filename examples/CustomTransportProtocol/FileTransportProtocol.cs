// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Application.BitTorrent;
using TorrentCore.Transport;

namespace CustomTransportProtocol;

class FileTransportProtocol : ITransportProtocol
{
    private readonly DirectoryInfo _rootDir;
    private readonly PeerId _localPeerId;
    private readonly List<ITransportStream> _streams;

    private FileSystemWatcher? _fileWatcher;

    public FileTransportProtocol(DirectoryInfo rootDir, PeerId localPeerId)
    {
        _rootDir = rootDir;
        _localPeerId = localPeerId;
        _streams = new List<ITransportStream>();
    }

    public event Action<AcceptConnectionEventArgs>? AcceptConnectionHandler;

    public IEnumerable<ITransportStream> Streams => _streams;

    public void Start()
    {
        var inDir = Path.Combine(_rootDir.FullName, _localPeerId.ToString());
        Directory.CreateDirectory(inDir);
        _fileWatcher = new FileSystemWatcher(inDir);
        _fileWatcher.Created += FileWatcher_Created;
    }

    private void FileWatcher_Created(object sender, FileSystemEventArgs e)
    {
        var transportStream = new FileTransportStream(
            new DirectoryInfo(e.FullPath),
            new DirectoryInfo(Path.Combine(_rootDir.FullName, e.Name!, _localPeerId.ToString())));

        AcceptConnectionHandler?.Invoke(new AcceptConnectionEventArgs(transportStream, () =>
        {
            _streams.Add(transportStream);
        }));
    }

    public void Stop()
    {
        foreach (var stream in Streams)
        {
            stream.Disconnect();
        }
    }
}
