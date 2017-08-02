import * as React from 'react';
import axios from 'axios';
import {Link} from 'react-router-dom';

export interface Props {
}

interface State {
  peers: {
    address: string;
    host: string;
  }[];
}

export default class Peers extends React.Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = {
      peers: []
    };
  }

  render() {
    const { peers } = this.state;
    
    return (
      <div>
        <h3>Connected Peers ({peers.length})</h3>

        <table className="table">
          <thead className="thead-default">
            <tr>
              <th>Address</th>
              <th>Hostname</th>
              <th>Client</th>
              <th>Version</th>
            </tr>
          </thead>
          <tbody>
            {peers.map(function(o: any, i: number){
                return (
                  <tr>
                    <td><Link to={'/peers/' + o.peerId}>{o.address}</Link></td>
                    <td>{o.host}</td>
                    <td>{o.client}</td>
                    <td>{o.clientVersion}</td>
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
    axios.get(process.env.REACT_APP_API_BASE + `/api/peers`)
      .then(res => {
        const peers = res.data;
        this.setState({ peers });
      });
  }
}
