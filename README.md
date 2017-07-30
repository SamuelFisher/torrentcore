# TorrentCore

[![Build Status](https://travis-ci.org/SamuelFisher/torrentcore.svg?branch=master)](https://travis-ci.org/SamuelFisher/torrentcore)

A BitTorrent library that runs on all platforms supporting the .NET Platform Standard 1.4.

## Feature progress:

- [x] Open .torrent files
- [x] Upload/download torrents
- [x] Contact HTTP trackers
- [x] Compact peer lists [BEP 23](http://www.bittorrent.org/beps/bep_0023.html)
- [ ] UDP trackers [BEP 15](http://www.bittorrent.org/beps/bep_0015.html)
- [ ] UPnP port forwarding
- [ ] IPv6 trackers [BEP 7](http://www.bittorrent.org/beps/bep_0007.html)
- [ ] DHT for trackerless torrents [BEP 5](http://www.bittorrent.org/beps/bep_0005.html) _(in progress)_
- [ ] uTorrent Transport Protocol [BEP 29](http://www.bittorrent.org/beps/bep_0029.html)

## Usage

TorrentCore is designed to be easy to use, while supporting more advanced features if required.

```csharp
var client = new TorrentClient();
var download = client.Add("sintel.torrent",
                          @"C:\Downloads\sintel");
download.Start();
await download.WaitForCompletionAsync();
```

## Web UI

TorrentCore includes an optional web UI that can be used for detailed monitoring of Torrent downloads. It does not provide any functionality to control downloads.

It can be enabled by referencing `TorrentCore.Web` and calling:

```csharp
client.EnableWebUI();
```

This starts a web interface on `http://localhost:5001/`.

![/webui-screenshot.png](/webui-screenshot.png)

The web interface requires .NET Platform Standard 1.6.

## Command-Line Client

In addition to a library, TorrentCore provides a basic command-line client for downloading torrents. Usage is as follows:

```
torrentcorecli --help

usage: torrentcorecli [-p <arg>] [-o <arg>] [-v] [--ui [arg]] [--]
                      <input>

    -p, --port <arg>      Port to listen for incoming connections on.
    -o, --output <arg>    Path to save downloaded files to.
    -v, --verbose         Show detailed logging information.
    --ui [arg]            Run a web UI, optionally specifying the port
                          to listen on (default: 5001).
    <input>               Path of torrent file to download.
```

## Contributing

Contributions are welcome! Please submit a pull request.
