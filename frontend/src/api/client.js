// Cliente HTTP central da API .NET. Injeta o Bearer, trata erros e centraliza o storage dos
// tokens. Decisão da fase 2 mantida: tokens em localStorage (simples; casa com token-no-corpo
// da API). O refresh token permite renovar sem novo login quando o access token expira/401.
const BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5289";
const TOKEN_KEY = "codequest.token";
const REFRESH_KEY = "codequest.refreshToken";

export function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}
export function setToken(token) {
  localStorage.setItem(TOKEN_KEY, token);
}
export function getRefreshToken() {
  return localStorage.getItem(REFRESH_KEY);
}
export function setSessionTokens(token, refreshToken) {
  setToken(token);
  localStorage.setItem(REFRESH_KEY, refreshToken);
}
export function clearToken() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(REFRESH_KEY);
}

/** Erro de API com o status HTTP e a mensagem amigável vinda do backend. */
export class ApiError extends Error {
  constructor(status, mensagem) {
    super(mensagem);
    this.status = status;
  }
}

async function fazerFetch(path, { method, headers, body }) {
  try {
    return await fetch(`${BASE_URL}${path}`, {
      method,
      headers,
      body: body ? JSON.stringify(body) : undefined,
    });
  } catch {
    // fetch rejeita antes de qualquer resposta (rede caiu, CORS bloqueou, API fora do ar).
    throw new ApiError(0, "Não foi possível falar com o servidor. Verifique sua conexão e tente de novo.");
  }
}

async function extrairCorpo(resp) {
  const texto = await resp.text();
  try {
    return texto ? JSON.parse(texto) : null;
  } catch {
    return null; // Corpo não é JSON (ex.: erro 5xx cru do servidor).
  }
}

// Evita renovar em paralelo quando vários requests em voo tomam 401 ao mesmo tempo: todos
// esperam a mesma promessa de renovação em vez de gastar o refresh token (uso único) várias vezes.
let renovacaoEmVoo = null;

async function renovarSessao() {
  const refreshToken = getRefreshToken();
  if (!refreshToken) return false;

  renovacaoEmVoo ??= (async () => {
    const resp = await fazerFetch("/api/auth/refresh", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: { refreshToken },
    });
    if (!resp.ok) return false;

    const dados = await extrairCorpo(resp);
    setSessionTokens(dados.token, dados.refreshToken);
    return true;
  })().finally(() => {
    renovacaoEmVoo = null;
  });

  return renovacaoEmVoo;
}

async function request(path, { method = "GET", body, auth = true, tentandoDeNovo = false } = {}) {
  const headers = { "Content-Type": "application/json" };
  const token = getToken();
  if (auth && token) headers.Authorization = `Bearer ${token}`;

  const resp = await fazerFetch(path, { method, headers, body });

  // Access token inválido/expirado: tenta renovar uma vez com o refresh token antes de deslogar.
  if (resp.status === 401 && auth && !tentandoDeNovo && (await renovarSessao())) {
    return request(path, { method, body, auth, tentandoDeNovo: true });
  }
  if (resp.status === 401) {
    clearToken();
    throw new ApiError(401, "Sessão expirada. Entre novamente.");
  }

  const dados = await extrairCorpo(resp);
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
  logout: (refreshToken) =>
    request("/api/auth/logout", { method: "POST", body: { refreshToken }, auth: false }),
  obterPlayer: () => request("/api/player"),

  // Missões (RF09–RF13)
  missoesDoDia: () => request("/api/missoes/dia"),
  faixasEsforco: () => request("/api/missoes/faixas"),
  criarMissao: (dados) => request("/api/missoes", { method: "POST", body: dados }),
  sugerirMissao: (texto) => request("/api/missoes/sugerir", { method: "POST", body: { texto } }),
  concluirMissao: (id) => request(`/api/missoes/${id}/concluir`, { method: "POST" }),

  // Loja (RF20)
  itensLoja: () => request("/api/loja/itens"),
  comprarItem: (itemId) => request(`/api/loja/comprar/${itemId}`, { method: "POST" }),

  // Exercícios gerados por IA (RF14/RF15)
  topicosDisponiveis: () => request("/api/exercicios/topicos"),
  gerarExercicio: (topicoId, dificuldade, formato) =>
    request("/api/exercicios/gerar", { method: "POST", body: { topicoId, dificuldade, formato } }),
  responderExercicio: (id, resposta) =>
    request(`/api/exercicios/${id}/responder`, { method: "POST", body: { resposta } }),

  // Foco / Pomodoro (RF19)
  registrarFoco: (topicoId, minutosPlanejados, minutosReais) =>
    request("/api/foco", { method: "POST", body: { topicoId, minutosPlanejados, minutosReais } }),

  // Árvore de habilidades (RF06)
  arvore: () => request("/api/arvore"),

  // Mentor / dúvidas (RF22–RF23)
  duvidas: () => request("/api/duvidas"),
  perguntarMentor: (topicoId, pergunta) =>
    request("/api/duvidas", { method: "POST", body: { topicoId, pergunta } }),
  marcarRevisaoDuvida: (id, marcada) =>
    request(`/api/duvidas/${id}/revisao`, { method: "POST", body: { marcada } }),

  // Testes de avaliação (RF17)
  iniciarTeste: (topicoId, dificuldade) =>
    request("/api/testes", { method: "POST", body: { topicoId, dificuldade } }),
  responderTeste: (id, respostas) =>
    request(`/api/testes/${id}/responder`, { method: "POST", body: { respostas } }),

  // Boss fight (RF12/RN08)
  iniciarBoss: (moduloId) => request(`/api/arvore/modulos/${moduloId}/boss`, { method: "POST" }),
  responderBoss: (testeId, respostas) =>
    request(`/api/arvore/boss/${testeId}/responder`, { method: "POST", body: { respostas } }),

  // Painel: atividade recente e radar de habilidades (RF08/RF24)
  atividade: () => request("/api/dashboard/atividade"),
  radar: () => request("/api/dashboard/radar"),

  // Gestão de tópicos (RF05)
  listarTopicosModulo: (moduloId) => request(`/api/modulos/${moduloId}/topicos`),
  criarTopico: (moduloId, nome, prioridade) =>
    request(`/api/modulos/${moduloId}/topicos`, { method: "POST", body: { nome, prioridade } }),
  editarTopico: (moduloId, topicoId, nome, prioridade) =>
    request(`/api/modulos/${moduloId}/topicos/${topicoId}`, { method: "PUT", body: { nome, prioridade } }),
  arquivarTopico: (moduloId, topicoId, arquivado) =>
    request(`/api/modulos/${moduloId}/topicos/${topicoId}/arquivar`, { method: "POST", body: { arquivado } }),
};
