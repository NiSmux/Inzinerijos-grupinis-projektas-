import { useLocation } from "react-router-dom";

const Footer = () => {
  const location = useLocation();
  const isAuthPage = ["/login", "/register"].includes(location.pathname);

  return (
    <footer className="footer" style={isAuthPage ? { color: '#fff' } : {}}>
      © Checkmark Champions
    </footer>
  );
};

export default Footer;
