import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Header from './Header.jsx';
import Footer from './Footer.jsx';
import TaskBoard from './TaskBoard';
import LoginForm from './LoginForm.jsx';
import SignupForm from './SignupForm.jsx';

function App() {
  return (
    <Router>
      <Header />
      <div className="App">
        <Routes>
          <Route path="/" element={<LoginForm />} />
          <Route path="/signup" element={<SignupForm />} />
          <Route path="/taskboard" element={<TaskBoard />} />
        </Routes>
      </div>
      <Footer />
    </Router>
  );
}

export default App;
