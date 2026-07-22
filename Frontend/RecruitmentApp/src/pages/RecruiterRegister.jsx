export default function RecruiterRegister() {
  return (
    <div style={{ padding: '3rem', maxWidth: '700px', margin: 'auto' }}>
      <div className="dashboard-card">
        <h2 style={{ marginBottom: '1.5rem' }}>Business Registration</h2>

        {/* Account Credentials Section */}
        <div style={{ marginBottom: '2rem', paddingBottom: '1rem', borderBottom: '1px solid #E2E8F0' }}>
          <h4 style={{ marginBottom: '1rem' }}>Account Information</h4>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
            <input type="email" placeholder="Business Email" className="form-input" />
            <input type="password" placeholder="Create Password" className="form-input" />
          </div>
        </div>

        {/* Business Details Section */}
        <form style={{ display: 'grid', gap: '1rem' }}>
          <input type="text" placeholder="Company Legal Name" className="form-input" />
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
            <input type="text" placeholder="Registration Number" className="form-input" />
            <input type="text" placeholder="Industry" className="form-input" />
          </div>
          <input type="text" placeholder="Company Website URL" className="form-input" />
          <textarea placeholder="Brief Company Overview" className="form-input" rows="4"></textarea>
          <button className="btn-primary" style={{ marginTop: '1rem' }}>Register Business Account</button>
        </form>
      </div>
    </div>
  );
}