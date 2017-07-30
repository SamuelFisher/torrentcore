import * as React from 'react';
import './App.css';
import Torrents from './components/Torrents';
import TorrentDetails from './components/TorrentDetails';
import Peers from './components/Peers';
import {
  BrowserRouter as Router,
  Route,
  NavLink,
  Redirect
} from 'react-router-dom';

class App extends React.Component<{}, {}> {
  render() {
    return (
      <Router>
      <div className="App">
        <nav className="navbar navbar-toggleable-md navbar-inverse bg-inverse">
          <button className="navbar-toggler navbar-toggler-right"
                  type="button" data-toggle="collapse"
                  data-target="#navbarsExampleDefault" aria-controls="navbarsExampleDefault"
                  aria-expanded="false" aria-label="Toggle navigation">
            <span className="navbar-toggler-icon" />
          </button>
          <a className="navbar-brand" href="#">TorrentCore</a>
          <ul className="navbar-nav mr-auto">
            <li className="nav-item">
              <NavLink to="/torrents" className="nav-link">Torrents</NavLink>
            </li>
            <li className="nav-item">
              <NavLink to="/peers" className="nav-link">Peers</NavLink>
            </li>
          </ul>
        </nav>

        <div className="container">
          <Route
            exact={true}
            path="/"
            render={() => (
              <Redirect to="/torrents"/>
            )}
          />
          <Route exact={true} path="/torrents" component={Torrents} />
          <Route path="/torrents/:infoHash" component={TorrentDetails} />
          <Route path="/peers" component={Peers} />
        </div>
      </div>
      </Router>
    );
  }
}

export default App;
