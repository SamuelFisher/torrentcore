// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Transport;

namespace CustomTransportProtocol;

/// <summary>
/// Transports data by writing it to a file on disk in a specified folder.
/// </summary>
class FileTransportStream : ITransportStream
{
    private readonly DirectoryInfo _inDir;
    private readonly DirectoryInfo _outDir;

    private Stream? _stream;
    private bool _isConnected;

    public FileTransportStream(DirectoryInfo inDir, DirectoryInfo outDir)
    {
        _inDir = inDir;
        _outDir = outDir;
    }

    public bool IsConnected => _isConnected;

    public string DisplayAddress => "File based tracker";

    public object Address => "File based tracker";

    public Stream Stream => _stream ?? throw new InvalidOperationException();

    public Task Connect()
    {
        // Wrap in a BufferedStream so that each file represents one message
        _stream = new BufferedStream(new FileChunkStream(_inDir, _outDir));
        _isConnected = true;
        return Task.CompletedTask;
    }

    public void Disconnect()
    {
        Stream.Dispose();
        _isConnected = false;
    }

    private class FileChunkStream : Stream
    {
        private readonly DirectoryInfo _inDir;
        private readonly FileSystemWatcher _inDirWatcher;
        private readonly DirectoryInfo _outDir;

        private int _inFileId;
        private int _outFileId;

        private int _remainingOffset;
        private byte[]? _remaining;

        public FileChunkStream(DirectoryInfo inDir, DirectoryInfo outDir)
        {
            if (!inDir.Exists)
                inDir.Create();
            if (!outDir.Exists)
                outDir.Create();

            _inDir = inDir;
            _inDirWatcher = new FileSystemWatcher(inDir.FullName);
            _outDir = outDir;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {
            // Do nothing
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;
            if (_remaining != null && _remainingOffset < _remaining.Length)
            {
                int toCopy = Math.Min(_remaining.Length - _remainingOffset, count);
                Array.Copy(_remaining, _remainingOffset, buffer, offset, toCopy);
                _remainingOffset += toCopy;

                read = toCopy;
                if (read == count)
                    return read;
            }

            var expectedPath = Path.Combine(_inDir.FullName, $"File_{_inFileId++:000}.bin");
            while (!File.Exists(expectedPath))
            {
                _inDirWatcher.WaitForChanged(WatcherChangeTypes.Created, 300);
            }

            _remainingOffset = 0;
            _remaining = File.ReadAllBytes(expectedPath);
            {
                int toCopy = Math.Min(_remaining.Length - _remainingOffset, count - read);
                Array.Copy(_remaining, _remainingOffset, buffer, offset, toCopy);
                _remainingOffset += toCopy;

                read = toCopy;
            }

            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var tmpPath = Path.Combine(_outDir.FullName, $"File_{_outFileId:000}.bin.tmp");
            var path = Path.Combine(_outDir.FullName, $"File_{_outFileId++:000}.bin");

            using (var fs = File.Create(tmpPath))
                fs.Write(buffer, offset, count);

            File.Move(tmpPath, path);
        }
    }
}
