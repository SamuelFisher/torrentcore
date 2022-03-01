// This file is part of TorrentCore.
//   https://torrentcore.org
// Copyright (c) Samuel Fisher.
//
// Licensed under the GNU Lesser General Public License, version 3. See the
// LICENSE file in the project root for full license information.

using TorrentCore.Transport;

namespace TorrentCore.Test.Extension;

public class FakeTransportStream : ITransportStream
{
    public FakeTransportStream(string displayAddress)
    {
        DisplayAddress = displayAddress;
    }

    public bool IsConnected => throw new NotImplementedException();

    public string DisplayAddress { get; }

    public object Address => throw new NotImplementedException();

    public Stream Stream => new MemoryStream();

    public Task ConnectAsync()
    {
        throw new NotImplementedException();
    }

    public void Disconnect()
    {
        throw new NotImplementedException();
    }
}
