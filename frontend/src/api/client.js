// Cliente HTTP central da API .NET. Injeta o Bearer, trata erros e centraliza o storage do token.
// Decisão da fase 2: token em localStorage (simples; casa com o token-no-corpo da API, sem refresh
// token ainda). Dá pra endurecer depois (memória + refresh cookie HttpOnly) sem mexer nas páginas.
const BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5289";
const TOKEN_KEY = "codequest.token";

export function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}
export function setToken(token) {
  localStorage.setItem(TOKEN_KEY, token);
}
export function clearToken() {
  localStorage.removeItem(TOKEN_KEY);
}

/** Erro de API com o status HTTP e a mensagem amigável vinda do backend. */
export class ApiError extends Error {
  constructor(status, mensagem) {
    super(mensagem);
    this.status = status;
  }
}

async function request(path, { method = "GET", body, auth = true } = {}) {
  const headers = { "Content-Type": "application/json" };
  const token = getToken();
  if (auth && token) headers.Authorization = `Bearer ${token}`;

  const resp = await fetch(`${BASE_URL}${path}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  });

  // Token inválido/expirado: limpa a sessão para o app voltar ao login.
  if (resp.status === 401) {
    clearToken();
    throw new ApiError(401, "Sessão expirada. Entre novamente.");
  }

  const texto = await resp.text();
  const dados = texto ? JSON.parse(texto) : null;

  if (!resp.ok) {
    throw new ApiError(resp.status, dados?.erro ?? "Erro inesperado na API.");
  }
  return dados;
}

export const api = {
  registrar: (login, senha) =>
    request("/api/auth/registrar", { method: "POST", body: { login, senha }, auth: false }),
  login: (login, senha) =>
    request("/api/auth/login", { method: "POST", body: { login, senha }, auth: false }),
  obterPlayer: () => request("/api/player"),
  adicionarXp: (xpBase) => request("/api/player/xp", { method: "POST", body: { xpBase } }),
};
