import React from 'react';

export default function About() {
  return (
    <div style={{ padding: '4rem 2rem', maxWidth: '1000px', margin: '0 auto' }}>
      <div className="dashboard-card" style={{ padding: '4rem' }}>
        <h1 style={{ color: 'var(--blue-primary)', marginBottom: '1.5rem' }}>About TalentX</h1>
        <p style={{ fontSize: '1.25rem', lineHeight: '1.8', color: 'var(--text-primary)', marginBottom: '2rem' }}>
          TalentX is an AI-Powered Recruitment and Talent Management Platform designed to modernize the recruitment lifecycle. 
          We believe that hiring should be intelligent, unbiased, and efficient.
        </p>
        
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '3rem', marginTop: '3rem' }}>
          <div>
            <h3 style={{ color: 'var(--blue-primary)' }}>Our Mission</h3>
            <p style={{ color: 'var(--text-secondary)' }}>
              To bridge the gap between world-class talent and innovative companies through cutting-edge AI and data-driven insights.
            </p>
          </div>
          <div>
            <h3 style={{ color: 'var(--blue-primary)' }}>Why Choose Us?</h3>
            <p style={{ color: 'var(--text-secondary)' }}>
              We leverage advanced machine learning for resume parsing, candidate ranking, and predictive analytics to ensure the right fit every time.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}