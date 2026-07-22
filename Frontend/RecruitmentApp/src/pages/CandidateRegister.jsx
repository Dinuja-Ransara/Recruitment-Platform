import '../index.css';
import React, { useState } from 'react';


export default function CandidateRegister() {
  const [formData, setFormData] = useState({ name: '', email: '', skills: [] });

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const response = await fetch('http://localhost:5000/api/register/candidate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(formData),
      });

      if (response.ok) {
        alert('Registration successful!');
      } else {
        alert('Registration failed. Please try again.');
      }
    } catch (error) {
      console.error('Error connecting to backend:', error);
    }
  };

  return (
    <div className="flex justify-center items-center min-h-[85vh] bg-gray-50">
      <div className="w-full max-w-sm bg-white p-8 rounded-2xl shadow-sm border border-gray-100">
        
        <h2 className="text-2xl font-bold text-center italic text-gray-900 mb-6">
          Register
        </h2>

        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Name Input */}
          <input
            type="text"
            placeholder="Full Name"
            className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-lg text-gray-800 placeholder-gray-500 focus:outline-none focus:bg-white focus:ring-2 focus:ring-blue-600 transition-all"
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            required
          />

          {/* Email Input */}
          <input
            type="email"
            placeholder="Email"
            className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-lg text-gray-800 placeholder-gray-500 focus:outline-none focus:bg-white focus:ring-2 focus:ring-blue-600 transition-all"
            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            required
          />

          {/* Submit Button */}
          <button
            type="submit"
            className="w-full py-3 px-4 bg-[#2563eb] hover:bg-blue-700 text-white font-semibold rounded-lg transition-colors mt-2"
          >
            Register Profile
          </button>
        </form>

        <div className="text-center mt-6 text-sm text-gray-600">
          Already have an account? <a href="/login" className="text-[#2563eb] italic hover:underline">Sign in here</a>
        </div>
        
      </div>
    </div>
  );
}