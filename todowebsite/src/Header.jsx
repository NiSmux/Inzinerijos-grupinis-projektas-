import './Header.css';
import { useNavigate } from 'react-router-dom';

function Header() {
  const navigate = useNavigate();

  function handleLogout() {
    localStorage.removeItem("token"); // or sessionStorage.removeItem("token");
    navigate("/");
  }

  return (
    <header>
      <h1>Checkmark Champions</h1>
      <nav>
        <ul>
          <li><a href="#">Home</a></li>
          <li><a href="#">Tasks</a></li>
          <li>
            <button onClick={handleLogout}>Log Out</button>
          </li>
        </ul>
      </nav>
      <hr />
    </header>
  );
}

export default Header;
