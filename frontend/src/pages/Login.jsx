import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext.jsx";
import { IconLoader } from "../components/icons.jsx";

export default function Login() {
  const { entrar } = useAuth();
  const navigate = useNavigate();
  const [login, setLogin] = useState("");
  const [senha, setSenha] = useState("");
  const [erro, setErro] = useState(null);
  const [carregando, setCarregando] = useState(false);

  async function enviar(e) {
    e.preventDefault();
    setErro(null);
    setCarregando(true);
    try {
      await entrar(login, senha);
      navigate("/", { replace: true });
    } catch (err) {
      setErro(err.message);
    } finally {
      setCarregando(false);
    }
  }

  return (
    <div className="auth-shell">
      <form className="card auth-card" onSubmit={enviar}>
        <div className="auth-logo">
          <span className="mark">CQ</span>
          <span className="display">CodeQuest</span>
        </div>
        <h1 className="display">Entrar</h1>

        {erro && <p className="erro">{erro}</p>}

        <label>
          Login
          <input
            value={login}
            onChange={(e) => setLogin(e.target.value)}
            autoComplete="username"
            autoFocus
          />
        </label>

        <label>
          Senha
          <input
            type="password"
            value={senha}
            onChange={(e) => setSenha(e.target.value)}
            autoComplete="current-password"
          />
        </label>

        <button className="btn btn-primary btn-block" disabled={carregando}>
          {carregando && <IconLoader size={16} />}
          {carregando ? "Entrando…" : "Entrar"}
        </button>

        <p className="muted">
          Não tem conta? <Link to="/registrar">Criar conta</Link>
        </p>
      </form>
    </div>
  );
}
