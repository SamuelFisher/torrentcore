import * as React from 'react';
import axios from 'axios';
import {Link} from 'react-router-dom';

export interface Props {
}

interface State {
  downloads: any[];
}

export default class Torrents extends React.Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = {
      downloads: []
    };
  }

  render() {
    const { downloads } = this.state;
    
    return (
      <div>
        <h3>Active Torrents ({downloads.length})</h3>
        <table className="table table-hover table-bordered">
          <thead className="thead-default">
            <tr>
              <th>Name</th>
              <th>Peers</th>
              <th>State</th>
              <th>Progress</th>
            </tr>
          </thead>
          <tbody>
            {downloads.map(function(t: any, i: number){
                return (
                  <tr>
                    <td><Link to={'/torrents/' + t.infoHash} className="nav-link">{t.name}</Link></td>
                    <td>{t.peers}</td>
                    <td>{t.state}</td>
                    <td>
                      <div className="progress">
                        <div
                          className="progress-bar"
                          role="progressbar"
                          style={{width : t.progress * 100 + '%'}}
                          aria-valuenow={t.progress * 100}
                          aria-valuemin="0"
                          aria-valuemax="100"
                        />
                      </div>
                    </td>
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
    axios.get(process.env.REACT_APP_API_BASE + `/api/torrents`)
      .then(res => {
        const downloads = res.data;
        this.setState({ downloads });
      });
  }
}
