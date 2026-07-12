import React from 'react';

export default function Jobs() {
  // Mock data for the prototype
  const jobList = [
    { title: "Senior Frontend Engineer", company: "TechNova", location: "Remote", type: "Full-time" },
    { title: "Software Architect", company: "CloudScale", location: "Colombo", type: "Full-time" },
    { title: "AI Research Lead", company: "DataMind", location: "Hybrid", type: "Contract" },
  ];

  return (
    <div style={{ padding: '3rem', maxWidth: '1200px', margin: '0 auto' }}>
      <h1 style={{ marginBottom: '2rem' }}>Find Your Next Opportunity</h1>
      
      {/* Search Bar */}
      <div className="dashboard-card" style={{ marginBottom: '2rem', display: 'flex', gap: '1rem' }}>
        <input type="text" placeholder="Job title, keywords, or company" className="form-input" />
        <button className="btn-primary">Search</button>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '250px 1fr', gap: '2rem' }}>
        {/* Sidebar Filters */}
        <aside>
          <div className="dashboard-card">
            <h3 style={{ fontSize: '1rem', marginBottom: '1rem' }}>Filters</h3>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
              <label><input type="checkbox" /> Remote</label>
              <label><input type="checkbox" /> Full-time</label>
              <label><input type="checkbox" /> Contract</label>
            </div>
          </div>
        </aside>

        {/* Job List */}
        <main style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          {jobList.map((job, index) => (
            <div key={index} className="dashboard-card" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <h3 style={{ margin: '0 0 0.5rem 0' }}>{job.title}</h3>
                <p style={{ margin: 0, color: 'var(--text-secondary)' }}>{job.company} • {job.location}</p>
              </div>
              <button className="btn-outline">Apply Now</button>
            </div>
          ))}
        </main>
      </div>
    </div>
  );
}