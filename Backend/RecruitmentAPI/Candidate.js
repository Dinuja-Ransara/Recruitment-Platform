const mongoose = require('mongoose');
const candidateSchema = new mongoose.Schema({
  name: String,
  email: { type: String, unique: true },
  skills: [String],
  createdAt: { type: Date, default: Date.now }
});
module.exports = mongoose.model('Candidate', candidateSchema);