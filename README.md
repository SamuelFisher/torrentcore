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

## Usage:

TorrentCore is designed to be easy to use, while supporting more advanced features if required.

```csharp
var client = new TorrentClient();
var download = client.Add("sintel.torrent",
                          @"C:\Downloads\sintel");
download.Start();
await download.WaitForCompletion();
```

## Contributing:

Contributions welcome! Please submit a pull request.
