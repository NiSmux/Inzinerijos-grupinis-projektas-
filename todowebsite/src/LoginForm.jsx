import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import './LoginForm.css';

const LoginForm = ({ setIsAuthenticated }) => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const navigate = useNavigate();

  const handleLogin = async (email, password) => {
    try {
      const response = await fetch('http://localhost:5293/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password }),
      });

      if (response.ok) {
        const data = await response.json();
        localStorage.setItem('authToken', data.token);
        console.log('Received token:', data.token);
        setIsAuthenticated(true); // ✅ Let app know we are logged in
        return true;
      } else {
        alert("Invalid login credentials");
        return false;
      }
    } catch (error) {
      console.error('Login error:', error);
      alert("Something went wrong during login.");
      return false;
    }
  };

  return (
    <div className="login-container">
      <div className="login-box">
        <h2>Login</h2>
        <form onSubmit={async (e) => {
          e.preventDefault();
          const success = await handleLogin(email, password);
          if (success) {
            navigate('/taskboard'); // ✅ Redirect after login
          }
        }}>
          <input type="email" placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} required />
          <input type="password" placeholder="Password" value={password} onChange={(e) => setPassword(e.target.value)} required />
          <button type="submit">Login</button>
        </form>
        <p>Don’t have an account? <a href="/signup">Sign up</a></p>
      </div>
    </div>
  );
};

export default LoginForm;