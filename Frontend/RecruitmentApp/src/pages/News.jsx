import React from 'react';

export default function News() {
  const newsItems = [
    {
      title: "TalentX Secures Series A Funding",
      date: "July 12, 2026",
      summary: "We are thrilled to announce our recent funding to further accelerate AI-driven recruitment solutions."
    },
    {
      title: "New AI Features Released",
      date: "July 05, 2026",
      summary: "Our latest update includes improved skill extraction and predictive hiring analytics for all premium users."
    },
    {
      title: "Future of HR Technology",
      date: "June 28, 2026",
      summary: "Industry leaders discuss how AI is shaping the modern workplace and the role of automated screening."
    }
  ];

  return (
    <div style={{ padding: '3rem', maxWidth: '1000px', margin: '0 auto' }}>
      <h1 style={{ marginBottom: '2rem' }}>TalentX Industry News</h1>
      
      <div style={{ display: 'grid', gap: '1.5rem' }}>
        {newsItems.map((item, index) => (
          <div key={index} className="dashboard-card">
            <span style={{ color: 'var(--blue-primary)', fontWeight: '600', fontSize: '0.85rem' }}>{item.date}</span>
            <h2 style={{ margin: '0.5rem 0' }}>{item.title}</h2>
            <p style={{ color: 'var(--text-secondary)' }}>{item.summary}</p>
            <button className="btn-outline" style={{ marginTop: '1rem' }}>Read More</button>
          </div>
        ))}
      </div>
    </div>
  );
}