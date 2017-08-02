import * as React from 'react';
import axios from 'axios';

export interface Props {
  match: any;
}

interface State {
  peer: {
    address: string;
    host: string;
    client: string;
    clientVersion: string;
    supportedExtensions: string[];
  }
}

export default class PeerDetails extends React.Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = {
      peer: {
        address: '',
        host: '',
        client: '',
        clientVersion: '',
        supportedExtensions: []
      }
    };
  }

  render() {
    const { peer } = this.state;
    
    return (
      <div>
        <h3>{peer.address}</h3>

        <p>
          <strong>Hostname:</strong> {peer.host}<br />
          <strong>Client:</strong> {peer.client}<br />
          <strong>Client version:</strong> {peer.clientVersion}<br />
          <strong>Supported extensions:</strong>
          <ul>
            {peer.supportedExtensions.map(function(p: string, i: number){
                return (
                  <li>{p}</li>
                );
            })}
          </ul>
          <br />
        </p>
      </div>
    );
  }

  componentDidMount() {
    this.getPeer();
  }

  private getPeer() {
    axios.get(process.env.REACT_APP_API_BASE + `/api/peers/` + this.props.match.params.peerId)
      .then(res => {
        const peer = res.data;
        this.setState({ peer });
      });
  }
}
