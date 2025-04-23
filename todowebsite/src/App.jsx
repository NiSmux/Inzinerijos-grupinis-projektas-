import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';

import Header from './Header.jsx';
import Footer from './Footer.jsx';
import TaskBoard from './TaskBoard.jsx';
import LoginForm from './LoginForm.jsx';
import SignupForm from './SignupForm.jsx';

const ProtectedRoute = ({ isAuthenticated, children }) => {
  if (!isAuthenticated) {
    return <Navigate to="/" replace />;
  }
  return children;
};

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem('authToken');
    setIsAuthenticated(!!token);
  }, []);

  return (
    <Router>
      <Header />
      <div className="App">
        <Routes>
          <Route
            path="/"
            element={<LoginForm setIsAuthenticated={setIsAuthenticated} />}
          />
          <Route path="/signup" element={<SignupForm />} />
          <Route
            path="/taskboard"
            element={
              <ProtectedRoute isAuthenticated={isAuthenticated}>
                <TaskBoard />
              </ProtectedRoute>
            }
          />
        </Routes>
      </div>
      <Footer />
    </Router>
  );
}

export default App;