import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

export default function Login({ setUserRole }) {
  const navigate = useNavigate();
  // Default to candidate for testing
  const [role, setRole] = useState('candidate');

  const handleLogin = (e) => {
    e.preventDefault();
    // In the final version, this will call your C# JWT API
    // For now, we mock the login and set the state
    setUserRole(role);
    navigate(`/${role}`);
  };

  return (
    <main className="hero-section" style={{ padding: '4rem 2rem' }}>
      {/* Background Effects */}
      <div className="orb orb-blue"></div>
      <div className="orb orb-cyan"></div>

      <div className="hero-content" style={{ width: '100%', maxWidth: '420px', zIndex: 10 }}>
        <div className="dashboard-card" style={{ 
          textAlign: 'left', 
          background: 'rgba(255, 255, 255, 0.85)', 
          backdropFilter: 'blur(16px)',
          WebkitBackdropFilter: 'blur(16px)'
        }}>
          
          <h2 style={{ fontSize: '1.75rem', marginBottom: '1.5rem', color: 'var(--text-primary)', textAlign: 'center' }}>
            Sign In to TalentX
          </h2>
          
          <form onSubmit={handleLogin} style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
            <div>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: 'var(--text-secondary)' }}>Email</label>
              <input type="email" placeholder="name@company.com" required className="form-input" defaultValue="demo@talentx.com" />
            </div>
            
            <div>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: 'var(--text-secondary)' }}>Password</label>
              <input type="password" placeholder="••••••••" required className="form-input" defaultValue="password123" />
            </div>

            {/* Prototype Mock Dropdown */}
            <div>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: 'var(--blue-primary)' }}>Demo Role (For Prototype)</label>
              <select value={role} onChange={(e) => setRole(e.target.value)} className="form-input" style={{ borderColor: 'var(--blue-primary)' }}>
                <option value="candidate">Candidate</option>
                <option value="recruiter">Recruiter</option>
                <option value="manager">Hiring Manager</option>
                <option value="admin">Administrator</option>
              </select>
            </div>

            <button type="submit" className="btn-primary" style={{ width: '100%', marginTop: '0.5rem' }}>
              Authenticate
            </button>
          </form>

        </div>
      </div>
    </main>
  );
}