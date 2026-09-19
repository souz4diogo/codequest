import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./auth/AuthContext.jsx";
import ProtectedRoute from "./auth/ProtectedRoute.jsx";
import Layout from "./components/Nav.jsx";
import Login from "./pages/Login.jsx";
import Registrar from "./pages/Registrar.jsx";
import Dashboard from "./pages/Dashboard.jsx";
import Missoes from "./pages/Missoes.jsx";
import Loja from "./pages/Loja.jsx";
import Exercicio from "./pages/Exercicio.jsx";
import Foco from "./pages/Foco.jsx";
import Arvore from "./pages/Arvore.jsx";
import Mentor from "./pages/Mentor.jsx";
import Testes from "./pages/Testes.jsx";
import Relatorio from "./pages/Relatorio.jsx";

// Envolve uma página protegida na sidebar/topbar persistente (Layout) — só um lugar define
// a navegação, em vez de cada página renderizar a própria <Nav/> (regra: navigation-consistency).
function Protegida({ children }) {
  return (
    <ProtectedRoute>
      <Layout>{children}</Layout>
    </ProtectedRoute>
  );
}

// Rotas da SPA. Login/registrar são públicos; o resto exige sessão.
export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/registrar" element={<Registrar />} />
          <Route
            path="/"
            element={
              <Protegida>
                <Dashboard />
              </Protegida>
            }
          />
          <Route
            path="/missoes"
            element={
              <Protegida>
                <Missoes />
              </Protegida>
            }
          />
          <Route
            path="/loja"
            element={
              <Protegida>
                <Loja />
              </Protegida>
            }
          />
          <Route
            path="/exercicio"
            element={
              <Protegida>
                <Exercicio />
              </Protegida>
            }
          />
          <Route
            path="/foco"
            element={
              <Protegida>
                <Foco />
              </Protegida>
            }
          />
          <Route
            path="/arvore"
            element={
              <Protegida>
                <Arvore />
              </Protegida>
            }
          />
          <Route
            path="/mentor"
            element={
              <Protegida>
                <Mentor />
              </Protegida>
            }
          />
          <Route
            path="/testes"
            element={
              <Protegida>
                <Testes />
              </Protegida>
            }
          />
          <Route
            path="/relatorio"
            element={
              <Protegida>
                <Relatorio />
              </Protegida>
            }
          />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
