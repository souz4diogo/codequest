import { createContext, useContext, useState, useCallback } from "react";
import { api, getToken, getRefreshToken, setSessionTokens, clearToken } from "../api/client.js";

const AuthContext = createContext(null);
const LOGIN_KEY = "codequest.login";

// Sessão da SPA: guarda o JWT + refresh token (via client) e expõe entrar/registrar/sair.
// Substitui o cookie + AuthenticationStateProvider do Blazor.
export function AuthProvider({ children }) {
  const [sessao, setSessao] = useState(() => {
    const token = getToken();
    return token ? { token, login: localStorage.getItem(LOGIN_KEY) } : null;
  });

  const aplicar = useCallback((resp) => {
    setSessionTokens(resp.token, resp.refreshToken);
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
    const refreshToken = getRefreshToken();
    clearToken();
    localStorage.removeItem(LOGIN_KEY);
    setSessao(null);
    // Revoga no servidor sem bloquear o logout local — não há nada a fazer se falhar
    // (o refresh token já saiu do storage, então não pode mais ser usado por aqui de qualquer forma).
    if (refreshToken) api.logout(refreshToken).catch(() => {});
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
