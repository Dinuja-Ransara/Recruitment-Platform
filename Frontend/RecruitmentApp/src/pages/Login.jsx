import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';

export default function Login({ setUserRole }) {
  const navigate = useNavigate();
  const [role, setRole] = useState('candidate'); // Default to candidate

  const handleLogin = (e) => {
    e.preventDefault();
    // Logic for admin vs user...
    setUserRole(role);
    navigate(`/${role}`);
  };

  return (
    <main className="hero-section" style={{ padding: '4rem 2rem' }}>
      <div className="dashboard-card" style={{ maxWidth: '400px', margin: 'auto' }}>
        
        {/* Toggle Switch */}
        <div style={{ display: 'flex', marginBottom: '2rem', background: '#F1F5F9', padding: '0.25rem', borderRadius: '8px' }}>
          <button onClick={() => setRole('candidate')} className={role === 'candidate' ? 'btn-primary' : ''} style={{ flex: 1, border: 'none', padding: '0.75rem', borderRadius: '6px' }}>Candidate</button>
          <button onClick={() => setRole('recruiter')} className={role === 'recruiter' ? 'btn-primary' : ''} style={{ flex: 1, border: 'none', padding: '0.75rem', borderRadius: '6px' }}>Recruiter</button>
        </div>

        <h2 style={{ textAlign: 'center', marginBottom: '1.5rem' }}>Sign In</h2>
        <form onSubmit={handleLogin} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          <input type="email" placeholder="Email" className="form-input" required />
          <input type="password" placeholder="Password" className="form-input" required />
          <button type="submit" className="btn-primary">Authenticate</button>
        </form>

        <p style={{ textAlign: 'center', marginTop: '1.5rem', fontSize: '0.9rem' }}>
          Don't have an account? <Link to={`/register/${role}`} style={{ color: 'var(--blue-primary)' }}>Register here</Link>
        </p>
      </div>
    </main>
  );
}