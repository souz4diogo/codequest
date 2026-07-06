import { Navigate } from "react-router-dom";
import { useAuth } from "./AuthContext.jsx";

// Equivalente ao [Authorize] + RedirectToLogin do Blazor: sem sessão, manda pro login.
export default function ProtectedRoute({ children }) {
  const { autenticado } = useAuth();
  return autenticado ? children : <Navigate to="/login" replace />;
}
