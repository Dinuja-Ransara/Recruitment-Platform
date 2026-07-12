import React from 'react';

export default function CandidatePortal() {
  return (
    <div style={{ padding: '3rem', maxWidth: '1200px', margin: '0 auto' }}>
      
      {/* Header Section */}
      <div style={{ marginBottom: '3rem' }}>
        <h1 style={{ color: 'var(--text-primary)', fontSize: '2.5rem', marginBottom: '0.5rem' }}>
          Welcome back, <span style={{ color: 'var(--blue-primary)' }}>Dinuja</span>
        </h1>
        <p style={{ color: 'var(--text-secondary)', fontSize: '1.1rem' }}>
          Your AI-powered job recommendations are ready.
        </p>
      </div>

      {/* Grid Layout for Features */}
      <div style={{ 
        display: 'grid', 
        gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', 
        gap: '2rem' 
      }}>
        
        {/* CV Upload Card */}
        <div className="dashboard-card">
          <h2 style={{ fontSize: '1.25rem', marginBottom: '1rem' }}>CV/Resume Management</h2>
          <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
            Upload your latest resume. Our AI will automatically parse your skills and experience.
          </p>
          <div style={{ 
            border: '2px dashed var(--blue-accent)', 
            borderRadius: '8px', 
            padding: '2rem', 
            textAlign: 'center',
            backgroundColor: 'var(--blue-light)',
            marginBottom: '1rem'
          }}>
            <p style={{ color: 'var(--blue-primary)', fontWeight: '600' }}>Drop your PDF here</p>
          </div>
          <button className="btn-primary" style={{ width: '100%' }}>Upload New Resume</button>
        </div>

        {/* AI Job Recommendations Card */}
        <div className="dashboard-card">
          <h2 style={{ fontSize: '1.25rem', marginBottom: '1rem' }}>Top AI Job Matches</h2>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            
            {/* Mock Job 1 */}
            <div style={{ padding: '1rem', borderBottom: '1px solid #E2E8F0' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <h3 style={{ margin: 0, fontSize: '1.1rem' }}>Senior Frontend Engineer</h3>
                <span style={{ color: 'var(--blue-accent)', fontWeight: 'bold' }}>98% Match</span>
              </div>
              <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', marginTop: '0.5rem' }}>React, JavaScript, UI/UX</p>
            </div>

            {/* Mock Job 2 */}
            <div style={{ padding: '1rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <h3 style={{ margin: 0, fontSize: '1.1rem' }}>Software Architect</h3>
                <span style={{ color: 'var(--blue-accent)', fontWeight: 'bold' }}>92% Match</span>
              </div>
              <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', marginTop: '0.5rem' }}>System Design, C#, .NET</p>
            </div>

          </div>
          <button className="btn-primary" style={{ width: '100%', marginTop: '1rem', backgroundColor: 'transparent', color: 'var(--blue-primary)', border: '1px solid var(--blue-primary)' }}>
            View All Jobs
          </button>
        </div>

        {/* Application Tracking Card */}
        <div className="dashboard-card">
          <h2 style={{ fontSize: '1.25rem', marginBottom: '1rem' }}>Application Status</h2>
          <div style={{ 
            padding: '1rem', 
            backgroundColor: '#F8FAFC', 
            borderRadius: '8px',
            borderLeft: '4px solid var(--blue-primary)'
          }}>
            <h3 style={{ margin: 0, fontSize: '1rem' }}>Full Stack Developer</h3>
            <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', margin: '0.5rem 0' }}>Applied: July 10, 2026</p>
            <span style={{ 
              display: 'inline-block',
              padding: '0.25rem 0.75rem', 
              backgroundColor: 'var(--blue-light)', 
              color: 'var(--blue-primary)',
              borderRadius: '99px',
              fontSize: '0.8rem',
              fontWeight: '600'
            }}>Interview Scheduled</span>
          </div>
        </div>

      </div>
    </div>
  );
}