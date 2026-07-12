import { NavLink } from "react-router-dom";
import { useAuth } from "../auth/AuthContext.jsx";

// Cabeçalho compartilhado das telas protegidas: marca + navegação + sair.
export default function Nav() {
  const { sair } = useAuth();
  const estilo = ({ isActive }) => ({
    color: isActive ? "var(--gold)" : "var(--text-muted)",
    textDecoration: "none",
    fontWeight: isActive ? 700 : 500,
  });

  return (
    <header className="topo">
      <span className="display" style={{ fontSize: 22 }}>
        CodeQuest
      </span>
      <nav className="acoes" style={{ alignItems: "center" }}>
        <NavLink to="/" end style={estilo}>
          Painel
        </NavLink>
        <NavLink to="/missoes" style={estilo}>
          Missões
        </NavLink>
        <NavLink to="/loja" style={estilo}>
          Loja
        </NavLink>
        <button className="btn btn-ghost" onClick={sair}>
          Sair
        </button>
      </nav>
    </header>
  );
}
