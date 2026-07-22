const express = require('express');
const mongoose = require('mongoose');
const cors = require('cors');
const Candidate = require('./models/Candidate'); // Ensure this file exists in /models/Candidate.js
require('dotenv').config();

const app = express();
const PORT = 5000;

app.use(cors());
app.use(express.json());

// Registration API
app.post('/api/register/candidate', async (req, res) => {
  try {
    const newCandidate = new Candidate(req.body);
    await newCandidate.save();
    res.status(201).json({ message: "Registration successful" });
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

mongoose.connect('mongodb://127.0.0.1:27017/talentx')
  .then(() => console.log('📡 Server running on http://localhost:5000'))
  .catch((err) => console.error(err));

app.listen(PORT);