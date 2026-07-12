import React from 'react';
import { Link } from 'react-router-dom';

export default function LandingPage() {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', width: '100%' }}>
      
      {/* 1. Hero Section (Anti-Gravity) */}
      <main className="hero-section">
        <div className="orb orb-blue"></div>
        <div className="orb orb-cyan"></div>

        <div className="hero-content">
          <div className="badge">✨ Next-Generation AI Hiring</div>
          <h1 className="hero-title">
            The <span className="text-gradient">AI-Powered</span> Talent<br />Management Platform
          </h1>
          <p className="hero-subtitle">
            Automate candidate screening, match top talent with precision, and modernize your recruitment lifecycle through intelligent insights.
          </p>
          <div className="hero-buttons">
            {/* Updated to route to Login */}
            <Link to="/login" className="btn-primary">Find Your Next Job</Link>
            <Link to="/login" className="btn-outline">Hire Top Talent</Link>
          </div>
        </div>
      </main>

      {/* 2. Platform Capabilities (Bento Grid) */}
      <section className="features-section">
        <div className="section-header">
          <h2>Intelligent Infrastructure</h2>
          <p>Built with advanced machine learning algorithms to eliminate bias and accelerate your hiring pipeline.</p>
        </div>

        <div className="bento-grid">
          <div className="bento-card bento-large">
            <div className="bento-icon">🎯</div>
            <div>
              <h3>AI-Powered Candidate Matching</h3>
              <p>Algorithms score every applicant against role requirements and rank them by predicted fit, cutting manual screening time by 35%.</p>
            </div>
          </div>

          <div className="bento-card">
            <div className="bento-icon">📄</div>
            <h3>Intelligent Resume Parsing</h3>
            <p>Extract structured skill and experience data from hundreds of CVs instantly.</p>
          </div>

          <div className="bento-card">
            <div className="bento-icon">📅</div>
            <h3>Automated Scheduling</h3>
            <p>Seamlessly coordinate interviews with Microsoft Outlook and Google Calendar integrations.</p>
          </div>

          <div className="bento-card bento-large">
            <div className="bento-icon">📊</div>
            <div>
              <h3>Predictive Hiring Analytics</h3>
              <p>Forecast time-to-fill, candidate offer acceptance probability, and retention risk using historical hiring data.</p>
            </div>
          </div>
        </div>
      </section>

      {/* 3. Dual Audience Routing */}
      <section className="audience-section">
        <div className="audience-grid">
          <div className="audience-card">
            <h3>For Job Seekers</h3>
            <p>Upload your resume and let our AI match you with roles that perfectly fit your unique skill set.</p>
            {/* Updated to route to Login */}
            <Link to="/login" className="btn-primary">Access Candidate Portal</Link>
          </div>
          
          <div className="audience-card">
            <h3>For Employers</h3>
            <p>Manage job postings, review AI-ranked shortlists, and make data-driven hiring decisions.</p>
            {/* Updated to route to Login */}
            <Link to="/login" className="btn-outline">Access Recruiter Portal</Link>
          </div>
        </div>
      </section>

    </div>
  );
}