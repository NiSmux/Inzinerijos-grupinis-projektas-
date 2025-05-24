import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, useLocation } from 'react-router-dom';

import Header from './Header.jsx';
import Footer from './Footer.jsx';
import TaskBoard from './TaskBoard.jsx';
import LoginForm from './LoginForm.jsx';
import SignupForm from './SignupForm.jsx';
import BoardsList from './BoardsList.jsx';

// Wrapper component to handle layout logic
const AppContent = ({ isAuthenticated, setIsAuthenticated }) => {
  const location = useLocation();
  const hideHeaderFooter = location.pathname === '/' || location.pathname === '/signup';

  return (
    <>
      {!hideHeaderFooter && <Header />}
      <div className="App">
        <Routes>
          <Route
            path="/"
            element={<LoginForm setIsAuthenticated={setIsAuthenticated} />}
          />
          <Route path="/signup" element={<SignupForm />} />
          <Route
            path="/boards"
            element={
              <ProtectedRoute isAuthenticated={isAuthenticated}>
                <BoardsList />
              </ProtectedRoute>
            }
          />
          <Route
            path="/board/:boardId"
            element={
              <ProtectedRoute isAuthenticated={isAuthenticated}>
                <TaskBoard />
              </ProtectedRoute>
            }
          />
          {/* Legacy route - redirect to boards list */}
          <Route
            path="/taskboard"
            element={
              <ProtectedRoute isAuthenticated={isAuthenticated}>
                <Navigate to="/boards" replace />
              </ProtectedRoute>
            }
          />
        </Routes>
      </div>
      {!hideHeaderFooter && <Footer />}
    </>
  );
};

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
      <AppContent isAuthenticated={isAuthenticated} setIsAuthenticated={setIsAuthenticated} />
    </Router>
  );
}

export default App;
