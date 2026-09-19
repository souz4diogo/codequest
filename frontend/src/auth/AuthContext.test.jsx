import { render, screen, act } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { AuthProvider, useAuth } from "./AuthContext.jsx";

vi.mock("../api/client.js", async (importOriginal) => {
  const real = await importOriginal();
  return { ...real, api: { login: vi.fn(), registrar: vi.fn(), logout: vi.fn() } };
});

import { api, getToken, getRefreshToken } from "../api/client.js";

function Consumidor() {
  const { sessao, autenticado, entrar, registrar, sair } = useAuth();
  return (
    <div>
      <span data-testid="autenticado">{String(autenticado)}</span>
      <span data-testid="login">{sessao?.login ?? ""}</span>
      <button onClick={() => entrar("diogo", "senha123")}>Entrar</button>
      <button onClick={() => registrar("diogo", "senha123")}>Registrar</button>
      <button onClick={() => sair()}>Sair</button>
    </div>
  );
}

beforeEach(() => {
  localStorage.clear();
  vi.clearAllMocks();
});

describe("AuthProvider", () => {
  it("começa sem sessão quando não há token salvo", () => {
    render(
      <AuthProvider>
        <Consumidor />
      </AuthProvider>,
    );

    expect(screen.getByTestId("autenticado")).toHaveTextContent("false");
  });

  it("hidrata a sessão a partir do token já salvo no localStorage", () => {
    localStorage.setItem("codequest.token", "token-existente");
    localStorage.setItem("codequest.login", "diogo");

    render(
      <AuthProvider>
        <Consumidor />
      </AuthProvider>,
    );

    expect(screen.getByTestId("autenticado")).toHaveTextContent("true");
    expect(screen.getByTestId("login")).toHaveTextContent("diogo");
  });

  it("entrar() guarda os tokens e autentica a sessão", async () => {
    const usuario = userEvent.setup();
    api.login.mockResolvedValue({ token: "t1", refreshToken: "r1", login: "diogo", playerId: 5 });

    render(
      <AuthProvider>
        <Consumidor />
      </AuthProvider>,
    );
    await usuario.click(screen.getByText("Entrar"));

    expect(screen.getByTestId("autenticado")).toHaveTextContent("true");
    expect(getToken()).toBe("t1");
    expect(getRefreshToken()).toBe("r1");
  });

  it("registrar() também autentica a sessão (registro já loga)", async () => {
    const usuario = userEvent.setup();
    api.registrar.mockResolvedValue({ token: "t2", refreshToken: "r2", login: "novo", playerId: 9 });

    render(
      <AuthProvider>
        <Consumidor />
      </AuthProvider>,
    );
    await usuario.click(screen.getByText("Registrar"));

    expect(screen.getByTestId("login")).toHaveTextContent("novo");
  });

  it("sair() limpa a sessão local e revoga o refresh token no servidor", async () => {
    const usuario = userEvent.setup();
    api.login.mockResolvedValue({ token: "t1", refreshToken: "r1", login: "diogo", playerId: 5 });
    api.logout.mockResolvedValue(undefined);

    render(
      <AuthProvider>
        <Consumidor />
      </AuthProvider>,
    );
    await usuario.click(screen.getByText("Entrar"));
    await usuario.click(screen.getByText("Sair"));

    expect(screen.getByTestId("autenticado")).toHaveTextContent("false");
    expect(getToken()).toBeNull();
    expect(api.logout).toHaveBeenCalledWith("r1");
  });

  it("sair() sem sessão ativa não chama a API (não tem refresh token pra revogar)", async () => {
    const usuario = userEvent.setup();
    render(
      <AuthProvider>
        <Consumidor />
      </AuthProvider>,
    );

    await act(async () => {
      await usuario.click(screen.getByText("Sair"));
    });

    expect(api.logout).not.toHaveBeenCalled();
  });

  it("useAuth fora do AuthProvider joga erro claro", () => {
    const SemProvider = () => {
      useAuth();
      return null;
    };
    const consoleErro = vi.spyOn(console, "error").mockImplementation(() => {});

    expect(() => render(<SemProvider />)).toThrow("useAuth deve ser usado dentro de <AuthProvider>.");

    consoleErro.mockRestore();
  });
});
