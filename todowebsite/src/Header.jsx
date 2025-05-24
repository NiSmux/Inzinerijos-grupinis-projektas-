import './Header.css';
import { useNavigate, Link } from 'react-router-dom'; // Importuojame Link komponentą

function Header() {
  const navigate = useNavigate();

  function handleLogout() {
    // Naudojame "authToken" kaip nurodyta App.jsx faile
    localStorage.removeItem("authToken");
    // Jei naudojate api.js utility su interceptoriais, galite nustatyti, kad tokenas nebūtų siunčiamas po atsijungimo.
    // Pvz., api.defaults.headers.common['Authorization'] = null; ar pan.
    navigate("/"); // Nukreipiame į login puslapį po atsijungimo
  }

  return (
    <header>
      <h1>Checkmark Champions</h1>
      <nav>
        <ul>
          {/* Pakeičiame <a> į Link ir href į to. Nukreipiame į /taskboard po prisijungimo */}
          <li><Link to="/taskboard">Home</Link></li>
          {/* Pakeičiame <a> į Link ir href į to. */}
          <li><Link to="/taskboard">Tasks</Link></li>
          {/* <-- PRIDEDAME NAUJĄ NUORODĄ Į NUSTATYMŲ PUSLAPĮ --> */}
          {/* Naudojame Link komponentą, nukreipiame į /settings */}
          <li><Link to="/settings">Settings</Link></li>
          {/* <-- NUORODOS PABAIGA --> */}
          <li>
            {/* Log Out mygtukas su esama navigacijos logika */}
            <button onClick={handleLogout}>Log Out</button>
          </li>
        </ul>
      </nav>
      <hr /> {/* Horizontali linija, kaip jūsų pavyzdyje */}
    </header>
  );
}

export default Header;
