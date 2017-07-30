import * as React from 'react';
import axios from 'axios';

export interface Props {
  match: any;
}

interface State {
  torrent: {
    name: string,
    infoHash: string,
    size: number,
    downloaded: number,
    downloadRate: number,
    uploadRate: number,
    peers: any[],
    pieces: any[],
    blockRequests: any[]
  };
}

export default class TorrentDetails extends React.Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = {
      torrent: {
        name: '',
        infoHash: '',
        size: 0,
        downloaded: 0,
        downloadRate: 0,
        uploadRate: 0,
        peers: [],
        pieces: [],
        blockRequests: []
      }
    };
  }

  render() {

    const torrent = this.state.torrent;
    const rawPieces = this.state.torrent.pieces;
    const spacing = 100;
    const pieces = this.splitarray(rawPieces, spacing);

    return (
      <div>
        <h3>{this.state.torrent.name}</h3>

        <p>
          <strong>Downloaded:</strong> {this.formatBytes(torrent.downloaded, false)} / {this.formatBytes(torrent.size, false)}<br />
          <strong>DL Rate:</strong> {this.formatBytes(torrent.downloadRate, true)}/s<br />
          <strong>UL Rate:</strong> {this.formatBytes(torrent.uploadRate, true)}/s<br />
        </p>

        <h3>Peers ({this.state.torrent.peers.length})</h3>
        <table className="table table-hover table-bordered">
          <thead className="thead-default">
            <tr>
              <th>Address</th>
            </tr>
          </thead>
          <tbody>
            {this.state.torrent.peers.map(function(p: any, i: number){
                return (
                  <tr>
                    <td>{p.address}</td>
                  </tr>
                );
            })}
          </tbody>
        </table>

        <h3>Pieces ({rawPieces.length})</h3>
        <div>
          {
            pieces.map((pieceGroup: any, index: number) => {
              return (
                <div className="piece-indicator">
                {
                  pieceGroup.map((piece: any, index1: number) => {
                    return (
                      <span
                        style={{
                          position: 'absolute',
                          left: index1 * 100 / spacing + '%',
                          width: 100 / spacing + '%'
                        }}
                        className={piece.completed ? 'piece complete' : 'piece incomplete'}
                      />
                    );
                  })
                }
                </div>
              );
            })
          }
        </div>

        <h3>Oustanding Block Requests ({this.state.torrent.blockRequests.length})</h3>
        <table className="table table-hover table-bordered">
          <thead className="thead-default">
            <tr>
              <th>Piece</th>
              <th>Offset (B)</th>
              <th>Length (B)</th>
            </tr>
          </thead>
          <tbody>
            {this.state.torrent.blockRequests.map(function(p: any, i: number){
                return (
                  <tr>
                    <td>{p.pieceIndex}</td>
                    <td>{p.offset}</td>
                    <td>{p.length}</td>
                  </tr>
                );
            })}
          </tbody>
        </table>
      </div>
    );
  }

  componentDidMount() {
    this.getPeers();
  }

  private getPeers() {
    axios.get(process.env.REACT_APP_API_BASE + `/api/torrents/` + this.props.match.params.infoHash)
      .then(res => {
        this.setState({ torrent: res.data });
      });
  }

  private splitarray(input: any, spacing: any) {
      var output = [];

      for (var i = 0; i < input.length; i += spacing)
      {
          output[output.length] = input.slice(i, i + spacing);
      }

      return output;
  }

  private formatBytes(bytes: number, si: boolean) {
    let th = si ? 1000 : 1024;
    if (Math.abs(bytes) < th) {
        return bytes + ' B';
    }

    var units = si
        ? ['kB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB']
        : ['KiB', 'MiB', 'GiB', 'TiB', 'PiB', 'EiB', 'ZiB', 'YiB'];

    var u = -1;
    do {
        bytes /= th;
        ++u;
    } while(Math.abs(bytes) >= th && u < units.length - 1);

    return bytes.toFixed(1) + ' ' + units[u];
  }
}
