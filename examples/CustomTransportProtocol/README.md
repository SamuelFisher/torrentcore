Example: Custom Transport Prococol
==================================

This example demonstrates how to create a custom Transport protocol.

## Summary

The custom `FileTransportProtocol` sends data between peers by writing each
packet as a file in a specified directory. The remote peer watches the directory
for new files.

The custom transport protocol is used by registering it in the DI container:

```csharp
// Simplified for brevity. See Program.cs for the full code.

var client = new TorrentClientBuilder()
    .ConfigureServices(services =>
    {
        services.AddSingleton<ITransportProtocol>(s => new FileTransportProtocol(...));
    })
    .AddBitTorrentApplicationProtocol()
    .AddDefaultPipeline()
    .Build();
```

The program sets up two peers - one seeder and one downloader with a torrent 
consisting of the single file [`TorrentContent.txt`](TorrentContent.txt). The program
completes once the downloader has downloaded the torrent.

A file-based tracker is used, where peers announce by placing a file containing their peer ID into a known directory.

## File Transport Structure

The directory structure is as follows (all relative to the working directory):

- `seeder` - contains the torrent data to be sent to the downloader by the seeder
- `downloader` - the downloader will download the torrent into this directory
- `transport` - directory used by the `FileTransportProtocol`
  - `announce` - peers announce to the file-based tracker by placing the file `{PeerId}.txt` containing their peer ID into this directory
  - `{PeerA}/{PeerB}` - contains the data sent from PeerA to PeerB

After running the example you should see that `downloader/TorrentContent.txt` contains the data downloaded from the seeder:

```
Torrent content here!
```

You can also see each packet transmitted between the seeder and downloader by looking at the files on disk in the `transport` directory. The directory names are padded
with trailing `X`s to make them 20 characters long (the length of a BitTorrent Peer
ID).

```bash
transport/DOWNLOADERXXXXXXXXXX/SEEDERXXXXXXXXXXXXXX$ ll 

# Files sent from Seeder -> Downloader

File_000.bin # BitTorrent connection header
File_001.bin # Bitfield message
File_002.bin # Unchoke message
File_003.bin # Piece message conntaining the torrent data
```
