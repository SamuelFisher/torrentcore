# TorrentCore

[![Build Status](https://travis-ci.org/SamuelFisher/torrentcore.svg?branch=master)](https://travis-ci.org/SamuelFisher/torrentcore)
[![Build status](https://ci.appveyor.com/api/projects/status/p1kf1e1upjsiw60n/branch/master?svg=true)](https://ci.appveyor.com/project/SamuelFisher/torrentcore/branch/master)
[![NuGet](https://img.shields.io/nuget/vpre/TorrentCore.svg)](https://www.nuget.org/packages/TorrentCore)

A BitTorrent library that runs on all platforms supporting .NET Standard 2.0.

## Feature Progress

This project is currently work-in-progress and there are likely to be bugs and
missing features.

- [x] Open .torrent files
- [x] Upload/download torrents
- [x] Contact HTTP trackers
- [x] Compact peer lists [BEP 23](http://www.bittorrent.org/beps/bep_0023.html)
- [x] UDP trackers [BEP 15](http://www.bittorrent.org/beps/bep_0015.html)
- [x] Peer ID conventions [BEP 20](http://bittorrent.org/beps/bep_0020.html)
- [x] Multitracker metadata extension [BEP 12](http://bittorrent.org/beps/bep_0012.html)
- [x] Extension protocol [BEP 10](http://www.bittorrent.org/beps/bep_0010.html)
- [ ] Peer exchange [BEP 11](http://www.bittorrent.org/beps/bep_0011.html)
- [ ] Extension for Peers to Send Metadata Files [BEP 9](http://bittorrent.org/beps/bep_0009.html)
- [ ] UPnP port forwarding
- [ ] IPv6 trackers [BEP 7](http://www.bittorrent.org/beps/bep_0007.html)
- [ ] DHT for trackerless torrents [BEP 5](http://www.bittorrent.org/beps/bep_0005.html)
- [ ] uTorrent Transport Protocol [BEP 29](http://www.bittorrent.org/beps/bep_0029.html)

## Usage

TorrentCore is designed to be easy to use, while supporting more advanced
features if required.

There are no stable releases yet, but you can reference `TorrentCore` from
[NuGet](https://www.nuget.org/packages/TorrentCore/), or directly from AppVeyor. For more
information, see [getting started](https://torrentcore.org/getting-started/).

```csharp
var client = TorrentClient.Create();
var download = client.Add("sintel.torrent",
                          @"C:\Downloads\sintel");
download.Start();
await download.WaitForDownloadCompletionAsync();
```

For more customisation of the `TorrentClient`, use `TorrentClientBuilder`:

```csharp
var client = TorrentClientBuilder.CreateDefaultBuilder()
    .UsePort(8000)
    .Build();
```

See the [examples](examples) directory for more in-depth examples.

## Extensible and Modular

TorrentCore is designed to allow custom extensions to be added and parts of the
built-in functionality to be swapped out. TorrentCore uses the same dependency
injection framework as ASP.NET Core to make this possible. Below are some examples of
the ways in which functionality can be added or changed.

The public interface for extensions is unstable and subject to change while
TorrentCore is under pre-release development.

### Custom Transport Protocol

TorrentCore includes built-in support for communicating with peers over TCP. You
can add support for any custom communication protocol that is able to expose
connections to peers as a `System.IO.Stream`. (Of course, protocols other than
TCP and uTP are incompatible with other BitTorrent clients.)

For more information, see [custom transport protocols](https://torrentcore.org/extend/transport-protocol/).

The [CustomTransportProtocol example](examples/CustomTransportProtocol) demonstrates a
custom transport protocol by sending data as files on disk without any use of TCP or
sockets.

### BitTorrent Extension Protocol

Custom [BEP 10](http://www.bittorrent.org/beps/bep_0010.html) message handlers
can be provided by implementing an `IExtensionProtocolMessageHandler`. You can
then register your custom extension message handler to handle custom message
types.

For more information, see [custom extension protocol messages](https://torrentcore.org/extend/extension-protocol/).

### Modules

Modules are a general-purpose low-level mechanism to add functionality by hooking into
events. Examples of things that modules can do include:

- Modify the connection handshake sent to peers
- Add a new type of message
- Send raw BitTorrent messages to peers
- Override the behaviour of messages built into the BitTorrent protocol itself

Some of the core functionality is implemented through modules, including
the core protocol messages and the BEP 10 extension protocol. For more
information, see [custom modules](https://torrentcore.org/extend/modules/).

### Pipeline Stages

When a torrent is started, it is managed by a number of sequential stages in a
pipeline that take it from checking the existing downloaded data to seeding to
other peers. New stages can be added to the pipeline and built-in stages can
be swapped for custom implementations.

For more information, see [pipeline stages](https://torrentcore.org/extend/pipeline/).

### Data Storage

The file data for torrents is usually stored on disk. TorrentCore
includes mechanisms to store the data on disk and in-memory, but you can provide
custom storage mechanisms by implementing an `IFileHandler`.

For more information, see [data storage](https://torrentcore.org/extend/data-storage/).

### Piece Picking Algorithms

A custom algorithm for deciding which pieces to request from peers can be used
by implementing an `IPiecePicker`.

For more information, see [piece picking](https://torrentcore.org/extend/piece-picking/).

## Command-Line Client

In addition to a library, TorrentCore provides a basic command-line client for
downloading torrents. Usage is as follows:

```
torrentcorecli --help

usage: torrentcorecli [-p <arg>] [-o <arg>] [-v] [--ui [arg]] [--]
                      <input>

    -p, --port <arg>      Port to listen for incoming connections on.
    -o, --output <arg>    Path to save downloaded files to.
    -v, --verbose         Show detailed logging information.
    <input>               Path of torrent file to download.
```

## Contributing

Contributions are welcome! Please submit a pull request.
