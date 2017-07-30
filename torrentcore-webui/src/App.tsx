import * as React from 'react';
import './App.css';
import Torrents from './components/Torrents';
import TorrentDetails from './components/TorrentDetails';
import Peers from './components/Peers';
import About from './components/About';
import {
  BrowserRouter as Router,
  Route,
  NavLink,
  Redirect
} from 'react-router-dom';

class App extends React.Component<{}, {}> {
  render() {
    return (
      <div>
        <Router>
          <div className="App">
            <nav className="navbar navbar-toggleable-md navbar-inverse bg-inverse">
              <button className="navbar-toggler navbar-toggler-right"
                      type="button" data-toggle="collapse"
                      data-target="#navbarsExampleDefault" aria-controls="navbarsExampleDefault"
                      aria-expanded="false" aria-label="Toggle navigation">
                <span className="navbar-toggler-icon" />
              </button>
              <span className="navbar-brand">TorrentCore</span>
              <ul className="navbar-nav mr-auto">
                <li className="nav-item">
                  <NavLink to="/torrents" className="nav-link">Torrents</NavLink>
                </li>
                <li className="nav-item">
                  <NavLink to="/peers" className="nav-link">Peers</NavLink>
                </li>
                <li className="nav-item">
                  <NavLink to="/about" className="nav-link">About</NavLink>
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
              <Route path="/about" component={About} />
            </div>
          </div>
        </Router>
        <footer className="footer">
          <div className="container">
            <p className="text-muted">Web UI powered by <a href="https://torrentcore.org">TorrentCore</a>.</p>
          </div>
        </footer>
      </div>
    );
  }
}

export default App;
