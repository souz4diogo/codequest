import { beforeEach, describe, expect, it, vi } from "vitest";
import { api, getToken, getRefreshToken, setToken, setSessionTokens, clearToken, ApiError } from "./client.js";

function jsonResponse(status, body) {
  return {
    ok: status >= 200 && status < 300,
    status,
    text: async () => JSON.stringify(body ?? {}),
  };
}

/** Chama a promessa e devolve o erro lançado, pra checar status/mensagem sem depender de
 * como toMatchObject trata propriedades não-enumeráveis de Error. */
async function capturarErro(promessa) {
  try {
    await promessa;
  } catch (erro) {
    return erro;
  }
  throw new Error("Esperava que a promessa rejeitasse, mas ela resolveu.");
}

beforeEach(() => {
  localStorage.clear();
  vi.restoreAllMocks();
});

describe("storage de tokens", () => {
  it("setSessionTokens grava os dois tokens e clearToken apaga os dois", () => {
    setSessionTokens("acesso1", "refresh1");

    expect(getToken()).toBe("acesso1");
    expect(getRefreshToken()).toBe("refresh1");

    clearToken();

    expect(getToken()).toBeNull();
    expect(getRefreshToken()).toBeNull();
  });
});

describe("request — casos básicos", () => {
  it("envia Authorization quando há token e devolve o corpo da resposta", async () => {
    setSessionTokens("meu-token", "meu-refresh");
    const fetchMock = vi.fn().mockResolvedValue(jsonResponse(200, { nivel: 3 }));
    global.fetch = fetchMock;

    const player = await api.obterPlayer();

    expect(player).toEqual({ nivel: 3 });
    const [, opcoes] = fetchMock.mock.calls[0];
    expect(opcoes.headers.Authorization).toBe("Bearer meu-token");
  });

  it("login não envia Authorization mesmo com token salvo (auth: false)", async () => {
    setSessionTokens("token-antigo", "refresh-antigo");
    const fetchMock = vi.fn().mockResolvedValue(
      jsonResponse(200, { token: "t", refreshToken: "r", login: "diogo", playerId: 1 })
    );
    global.fetch = fetchMock;

    await api.login("diogo", "senha123");

    const [, opcoes] = fetchMock.mock.calls[0];
    expect(opcoes.headers.Authorization).toBeUndefined();
    expect(JSON.parse(opcoes.body)).toEqual({ login: "diogo", senha: "senha123" });
  });

  it("erro HTTP (não-401) joga ApiError com a mensagem do backend", async () => {
    global.fetch = vi.fn().mockResolvedValue(jsonResponse(400, { erro: "XP fora da faixa." }));

    const erro = await capturarErro(api.criarMissao({ titulo: "x" }));

    expect(erro).toBeInstanceOf(ApiError);
    expect(erro.status).toBe(400);
    expect(erro.message).toBe("XP fora da faixa.");
  });

  it("falha de rede (fetch rejeita) joga ApiError de status 0", async () => {
    global.fetch = vi.fn().mockRejectedValue(new TypeError("Failed to fetch"));

    const erro = await capturarErro(api.obterPlayer());

    expect(erro.status).toBe(0);
  });
});

describe("request — renovação automática no 401", () => {
  it("renova a sessão sozinho e repete a chamada original com o token novo", async () => {
    setSessionTokens("token-expirado", "refresh-valido");
    let chamadasPlayer = 0;
    global.fetch = vi.fn().mockImplementation(async (url) => {
      if (url.endsWith("/api/auth/refresh")) {
        return jsonResponse(200, { token: "token-novo", refreshToken: "refresh-novo" });
      }
      chamadasPlayer += 1;
      return chamadasPlayer === 1 ? jsonResponse(401, { erro: "expirado" }) : jsonResponse(200, { nivel: 5 });
    });

    const player = await api.obterPlayer();

    expect(player).toEqual({ nivel: 5 });
    expect(getToken()).toBe("token-novo");
    expect(getRefreshToken()).toBe("refresh-novo");
  });

  it("sem refresh token salvo, 401 desloga direto (sem tentar renovar)", async () => {
    setToken("token-expirado");
    global.fetch = vi.fn().mockResolvedValue(jsonResponse(401, { erro: "expirado" }));

    const erro = await capturarErro(api.obterPlayer());

    expect(erro.status).toBe(401);
    expect(getToken()).toBeNull();
  });

  it("refresh token inválido: limpa a sessão e joga 401 sem tentar de novo", async () => {
    setSessionTokens("token-expirado", "refresh-invalido");
    global.fetch = vi.fn().mockImplementation(async (url) => {
      if (url.endsWith("/api/auth/refresh")) return jsonResponse(401, { erro: "Refresh token inválido." });
      return jsonResponse(401, { erro: "expirado" });
    });

    const erro = await capturarErro(api.obterPlayer());

    expect(erro.status).toBe(401);
    expect(getToken()).toBeNull();
    expect(getRefreshToken()).toBeNull();
  });

  it("duas chamadas simultâneas que tomam 401 renovam a sessão uma única vez (sem gastar o refresh token 2x)", async () => {
    setSessionTokens("token-expirado", "refresh-valido");
    let chamadasPlayer = 0;
    let chamadasRefresh = 0;
    global.fetch = vi.fn().mockImplementation(async (url) => {
      if (url.endsWith("/api/auth/refresh")) {
        chamadasRefresh += 1;
        return jsonResponse(200, { token: "token-novo", refreshToken: "refresh-novo" });
      }
      chamadasPlayer += 1;
      // As duas primeiras chamadas (uma de cada request concorrente) tomam 401;
      // as duas seguintes (retry de cada uma) já acham o token novo.
      return chamadasPlayer <= 2 ? jsonResponse(401, { erro: "expirado" }) : jsonResponse(200, { nivel: 7 });
    });

    const [a, b] = await Promise.all([api.obterPlayer(), api.obterPlayer()]);

    expect(a).toEqual({ nivel: 7 });
    expect(b).toEqual({ nivel: 7 });
    expect(chamadasRefresh).toBe(1);
  });
});
