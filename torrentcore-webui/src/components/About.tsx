import * as React from 'react';

export interface Props {
}

interface State {
}

export default class Torrents extends React.Component<Props, State> {
  constructor(props: Props) {
    super(props);
  }

  render() {    
    return (
      <div>
        <h3>About TorrentCore Web UI</h3>
        
        <p>
          <strong>Environment: </strong>{process.env.NODE_ENV}<br />
          <strong>API URL base: </strong>{process.env.REACT_APP_API_BASE}<br />
        </p>
      </div>
    );
  }
}
