import { NavLink } from "react-router-dom";
import { useAuth } from "../auth/AuthContext.jsx";
import {
  IconDashboard,
  IconTree,
  IconTarget,
  IconCode,
  IconClipboardCheck,
  IconTimer,
  IconShoppingBag,
  IconMessageCircle,
  IconLogOut,
  IconActivity,
} from "./icons.jsx";

const ITENS = [
  { to: "/", end: true, label: "Painel", Icon: IconDashboard },
  { to: "/arvore", label: "Árvore", Icon: IconTree },
  { to: "/missoes", label: "Missões", Icon: IconTarget },
  { to: "/exercicio", label: "Exercício", Icon: IconCode },
  { to: "/testes", label: "Testes", Icon: IconClipboardCheck },
  { to: "/foco", label: "Foco", Icon: IconTimer },
  { to: "/loja", label: "Loja", Icon: IconShoppingBag },
  { to: "/mentor", label: "Mentor", Icon: IconMessageCircle },
  { to: "/relatorio", label: "Relatório", Icon: IconActivity },
];

function Brand() {
  return (
    <div className="brand">
      <span className="mark">CQ</span>
      <span className="display">CodeQuest</span>
    </div>
  );
}

function ItemNav({ to, end, label, Icon }) {
  return (
    <NavLink to={to} end={end} className={({ isActive }) => `nav-item${isActive ? " ativo" : ""}`}>
      <Icon size={19} />
      <span>{label}</span>
    </NavLink>
  );
}

// Navegação persistente das telas protegidas: sidebar fixa em desktop, topbar + tira de
// ícones rolável em mobile (regra de navegação adaptativa por breakpoint).
export default function Layout({ children }) {
  const { sessao, sair } = useAuth();

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <Brand />
        <nav className="sidebar-nav">
          {ITENS.map((item) => (
            <ItemNav key={item.to} {...item} />
          ))}
        </nav>
        <div className="sidebar-footer">
          <div className="sidebar-user">
            <span className="avatar">{sessao?.login?.slice(0, 2).toUpperCase()}</span>
            <div style={{ minWidth: 0 }}>
              <div className="login">{sessao?.login}</div>
              <div className="rotulo">Jogador</div>
            </div>
          </div>
          <button className="btn btn-ghost btn-block" onClick={sair}>
            <IconLogOut size={17} />
            Sair
          </button>
        </div>
      </aside>

      <div className="main">
        <header className="topbar-mobile">
          <Brand />
          <button className="btn btn-icon btn-ghost" onClick={sair} aria-label="Sair">
            <IconLogOut size={18} />
          </button>
        </header>
        <nav className="topbar-nav">
          {ITENS.map((item) => (
            <ItemNav key={item.to} {...item} />
          ))}
        </nav>

        {children}
      </div>
    </div>
  );
}
