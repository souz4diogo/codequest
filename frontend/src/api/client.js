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

  let resp;
  try {
    resp = await fetch(`${BASE_URL}${path}`, {
      method,
      headers,
      body: body ? JSON.stringify(body) : undefined,
    });
  } catch {
    // fetch rejeita antes de qualquer resposta (rede caiu, CORS bloqueou, API fora do ar).
    throw new ApiError(0, "Não foi possível falar com o servidor. Verifique sua conexão e tente de novo.");
  }

  // Token inválido/expirado: limpa a sessão para o app voltar ao login.
  if (resp.status === 401) {
    clearToken();
    throw new ApiError(401, "Sessão expirada. Entre novamente.");
  }

  const texto = await resp.text();
  let dados = null;
  try {
    dados = texto ? JSON.parse(texto) : null;
  } catch {
    // Corpo não é JSON (ex.: erro 5xx cru do servidor) — segue com dados nulo.
  }

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
