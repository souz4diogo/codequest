import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./auth/AuthContext.jsx";
import ProtectedRoute from "./auth/ProtectedRoute.jsx";
import Login from "./pages/Login.jsx";
import Registrar from "./pages/Registrar.jsx";
import Dashboard from "./pages/Dashboard.jsx";
import Missoes from "./pages/Missoes.jsx";
import Loja from "./pages/Loja.jsx";

// Rotas da SPA. Login/registrar são públicos; painel, missões e loja exigem sessão.
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
              <ProtectedRoute>
                <Dashboard />
              </ProtectedRoute>
            }
          />
          <Route
            path="/missoes"
            element={
              <ProtectedRoute>
                <Missoes />
              </ProtectedRoute>
            }
          />
          <Route
            path="/loja"
            element={
              <ProtectedRoute>
                <Loja />
              </ProtectedRoute>
            }
          />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
