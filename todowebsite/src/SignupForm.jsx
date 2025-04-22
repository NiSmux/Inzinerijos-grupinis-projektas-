import { useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import './SignupForm.css';

const SignupForm = () => {
  const [email, setEmail] = useState('');
  const [name, setName] = useState('');
  const [password, setPassword] = useState('');
  const navigate = useNavigate();

  const handleSignup = async (e) => {
    e.preventDefault();
    
    // Debug: Show what we're sending
    const payload = {
      Name: name,
      Email: email,
      Password: password
    };
    console.log("Sending:", payload);
  
    try {
      const response = await axios.post('http://localhost:5293/api/auth/register', payload, {
        headers: {
          'Content-Type': 'application/json'
        }
      });
      console.log("Response:", response.data);
      

      alert('Account created!');
      navigate('/'); // Go to login page
    } catch (err) {
      console.error(err);
      alert(err.response?.data?.message || 'Signup failed.');
    }
  };

  return (
    <div className="signup-container">
      <div className="signup-box">
        <h2>Sign Up</h2>
        <form onSubmit={handleSignup}>
          <input type="text" placeholder="Name" onChange={(e) => setName(e.target.value)} required />
          <input type="email" placeholder="Email" onChange={(e) => setEmail(e.target.value)} required />
          <input type="password" placeholder="Password" onChange={(e) => setPassword(e.target.value)} required />
          <button type="submit">Sign Up</button>
        </form>
        <p>Already have an account? <a href="/">Log in</a></p>
      </div>
    </div>
  );
};

export default SignupForm;
