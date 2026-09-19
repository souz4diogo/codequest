import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import Login from "./Login.jsx";

const entrarMock = vi.fn();
const navigateMock = vi.fn();

vi.mock("../auth/AuthContext.jsx", () => ({ useAuth: () => ({ entrar: entrarMock }) }));
vi.mock("react-router-dom", async (importOriginal) => {
  const real = await importOriginal();
  return { ...real, useNavigate: () => navigateMock };
});

beforeEach(() => {
  vi.clearAllMocks();
});

function renderLogin() {
  return render(
    <MemoryRouter>
      <Login />
    </MemoryRouter>,
  );
}

describe("Login", () => {
  it("envia login e senha digitados e navega pro painel quando dá certo", async () => {
    const usuario = userEvent.setup();
    entrarMock.mockResolvedValue(undefined);
    renderLogin();

    await usuario.type(screen.getByLabelText("Login"), "diogo");
    await usuario.type(screen.getByLabelText("Senha"), "segredo123");
    await usuario.click(screen.getByRole("button", { name: /entrar/i }));

    expect(entrarMock).toHaveBeenCalledWith("diogo", "segredo123");
    expect(navigateMock).toHaveBeenCalledWith("/", { replace: true });
  });

  it("mostra a mensagem de erro do backend e não navega quando o login falha", async () => {
    const usuario = userEvent.setup();
    entrarMock.mockRejectedValue(new Error("Login ou senha inválidos."));
    renderLogin();

    await usuario.type(screen.getByLabelText("Login"), "diogo");
    await usuario.type(screen.getByLabelText("Senha"), "senhaerrada");
    await usuario.click(screen.getByRole("button", { name: /entrar/i }));

    expect(await screen.findByText("Login ou senha inválidos.")).toBeInTheDocument();
    expect(navigateMock).not.toHaveBeenCalled();
  });

  it("tem um link pra criar conta", () => {
    renderLogin();

    expect(screen.getByRole("link", { name: /criar conta/i })).toHaveAttribute("href", "/registrar");
  });
});
