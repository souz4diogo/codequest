import { createContext, useContext, useState, useCallback } from "react";
import { api, getToken, setToken, clearToken } from "../api/client.js";

const AuthContext = createContext(null);
const LOGIN_KEY = "codequest.login";

// Sessão da SPA: guarda o JWT (via client) e expõe entrar/registrar/sair.
// Substitui o cookie + AuthenticationStateProvider do Blazor.
export function AuthProvider({ children }) {
  const [sessao, setSessao] = useState(() => {
    const token = getToken();
    return token ? { token, login: localStorage.getItem(LOGIN_KEY) } : null;
  });

  const aplicar = useCallback((resp) => {
    setToken(resp.token);
    localStorage.setItem(LOGIN_KEY, resp.login);
    setSessao({ token: resp.token, login: resp.login, playerId: resp.playerId });
  }, []);

  const entrar = useCallback(
    async (login, senha) => aplicar(await api.login(login, senha)),
    [aplicar],
  );

  const registrar = useCallback(
    async (login, senha) => aplicar(await api.registrar(login, senha)),
    [aplicar],
  );

  const sair = useCallback(() => {
    clearToken();
    localStorage.removeItem(LOGIN_KEY);
    setSessao(null);
  }, []);

  return (
    <AuthContext.Provider value={{ sessao, autenticado: !!sessao, entrar, registrar, sair }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth deve ser usado dentro de <AuthProvider>.");
  return ctx;
}
