import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, useLocation } from 'react-router-dom';

import Header from './Header.jsx';
import Footer from './Footer.jsx';
import TaskBoard from './TaskBoard.jsx';
import LoginForm from './LoginForm.jsx';
import SignupForm from './SignupForm.jsx';
import Settings from './Settings.jsx'; // <-- Importuojame Settings komponentą


// Wrapper component to handle layout logic
const AppContent = ({ isAuthenticated, setIsAuthenticated }) => {
  const location = useLocation();
  // Paslepiame Header ir Footer puslapiuose /, /signup.
  // Jei settings irgi norite paslėpti, pridėkite || location.pathname === '/settings'
  const hideHeaderFooter = location.pathname === '/' || location.pathname === '/signup';

  return (
    <>
      {!hideHeaderFooter && <Header />}
      <div className="App">
        <Routes>
          {/* Login puslapis */}
          <Route
            path="/"
            element={<LoginForm setIsAuthenticated={setIsAuthenticated} />}
          />
          {/* Registracijos puslapis */}
          <Route path="/signup" element={<SignupForm />} />

          {/* Apsaugotas TaskBoard puslapis - pasiekiamas tik prisijungus */}
          <Route
            path="/taskboard"
            element={
              <ProtectedRoute isAuthenticated={isAuthenticated}>
                <TaskBoard />
              </ProtectedRoute>
            }
          />

          {/* <-- NAUJAS APSAUGOTAS MARŠRUTAS NUSTATYMŲ PUSLAPIUI --> */}
          {/* Settings puslapis - pasiekiamas tik prisijungus */}
          <Route
            path="/settings"
            element={
              <ProtectedRoute isAuthenticated={isAuthenticated}>
                <Settings /> {/* Atvaizduojame Settings komponentą */}
              </ProtectedRoute>
            }
          />
          {/* <-- NAUJO MARŠRUTO PABAIGA --> */}

          {/* Jei turite kitų maršrutų, pridėkite juos čia */}
           {/* <Route path="*" element={<NotFoundPage />} />  // Pvz. 404 Not Found puslapis */}

        </Routes>
      </div>
      {!hideHeaderFooter && <Footer />}
    </>
  );
};

// Komponentas maršrutų apsaugai
const ProtectedRoute = ({ isAuthenticated, children }) => {
  if (!isAuthenticated) {
    // Jei vartotojas neprisijungęs, nukreipiame jį į Login puslapį
    return <Navigate to="/" replace />;
  }
  // Jei vartotojas prisijungęs, atvaizduojame prašomą komponentą (children)
  return children;
};

function App() {
  // Būsena, nurodanti, ar vartotojas yra prisijungęs
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  // Paveikiame efektą prisijungus prie komponento (Mount)
  // Patikriname localStorage, ar yra 'authToken' (JWT tokenas)
  useEffect(() => {
    const token = localStorage.getItem('authToken');
    // Nustatome isAuthenticated būseną. !!token paverčia token'o reikšmę į boolean: true jei tokenas yra, false jei null/undefined.
    setIsAuthenticated(!!token);
  }, []); // [] užtikrina, kad šis efektas bus paleistas tik vieną kartą, komponentui užsikrovus

  return (
    // Pagrindinis Router komponentas, kuris apgaubia visą aplikaciją
    <Router>
      {/* Atvaizduojame AppContent, kuris tvarko Header/Footer rodymą ir Routes */}
      <AppContent isAuthenticated={isAuthenticated} setIsAuthenticated={setIsAuthenticated} />
    </Router>
  );
}

export default App;
