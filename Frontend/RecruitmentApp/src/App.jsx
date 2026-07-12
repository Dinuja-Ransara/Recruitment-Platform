import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import { useState } from 'react';
import LandingPage from './pages/LandingPage';
import CandidatePortal from './pages/CandidatePortal';
import Login from './pages/Login';
import Jobs from './pages/Jobs';
import News from './pages/News';
import About from './pages/About';
import './App.css';

function App() {
  const [userRole, setUserRole] = useState(null); 

  const handleLogout = () => {
    setUserRole(null);
  };

  return (
    <Router>
      <div className="app-container">
        
        {/* Dynamic Navigation */}
        <nav className="navbar">
          <Link to="/" className="navbar-brand">TalentX</Link>
          
          <div className="nav-links" style={{ alignItems: 'center' }}>
            {!userRole ? (
              // When NO user is logged in, show a full landing page menu
              <>
    {/* New expanded navigation */}
    <Link to="/jobs" className="nav-item">Jobs</Link>
    <Link to="/news" className="nav-item">News</Link>
    <Link to="/about" className="nav-item">About</Link>
    
    {/* Spacer to push Sign In to the right */}
    <div style={{ flexGrow: 1 }}></div>
     <Link to="/login" className="btn-primary" style={{ padding: '0.5rem 1.25rem' }}>Sign In</Link>
  </>
            ) : (
              // When a user IS logged in, show their specific dashboard controls
              <>
                <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
                  Role: <strong style={{ color: 'var(--blue-primary)', textTransform: 'capitalize' }}>{userRole}</strong>
                </span>
                <Link to={`/${userRole}`} className="nav-item">Dashboard</Link>
                <button onClick={handleLogout} className="btn-outline" style={{ padding: '0.5rem 1.25rem' }}>Logout</button>
              </>
            )}
          </div>
        </nav>

        {/* Page Routing */}
        <Routes>
          <Route path="/" element={<LandingPage />} />
          <Route path="/login" element={<Login setUserRole={setUserRole} />} />
          
          {/* Protected Portals */}
          <Route path="/candidate" element={<CandidatePortal />} />
          <Route path="/recruiter" element={<div style={{padding: '4rem', textAlign: 'center'}}><h2>Recruiter Portal UI goes here</h2></div>} />
          <Route path="/manager" element={<div style={{padding: '4rem', textAlign: 'center'}}><h2>Hiring Manager UI goes here</h2></div>} />
          <Route path="/admin" element={<div style={{padding: '4rem', textAlign: 'center'}}><h2>Admin UI goes here</h2></div>} />
          <Route path="/jobs" element={<Jobs />} />
          <Route path="/news" element={<News />} />
          <Route path="/about" element={<About />} />
        </Routes>
        
      </div>
    </Router>
  );
}

export default App;