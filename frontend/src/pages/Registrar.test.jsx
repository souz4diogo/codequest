import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import Registrar from "./Registrar.jsx";

const registrarMock = vi.fn();
const navigateMock = vi.fn();

vi.mock("../auth/AuthContext.jsx", () => ({ useAuth: () => ({ registrar: registrarMock }) }));
vi.mock("react-router-dom", async (importOriginal) => {
  const real = await importOriginal();
  return { ...real, useNavigate: () => navigateMock };
});

beforeEach(() => {
  vi.clearAllMocks();
});

function renderRegistrar() {
  return render(
    <MemoryRouter>
      <Registrar />
    </MemoryRouter>,
  );
}

describe("Registrar", () => {
  it("cria a conta e navega pro painel — registro já autentica", async () => {
    const usuario = userEvent.setup();
    registrarMock.mockResolvedValue(undefined);
    renderRegistrar();

    await usuario.type(screen.getByLabelText(/login/i), "novo_user");
    await usuario.type(screen.getByLabelText(/senha/i), "segredo123");
    await usuario.click(screen.getByRole("button", { name: /criar conta/i }));

    expect(registrarMock).toHaveBeenCalledWith("novo_user", "segredo123");
    expect(navigateMock).toHaveBeenCalledWith("/", { replace: true });
  });

  it("mostra a mensagem de erro quando o login já está em uso", async () => {
    const usuario = userEvent.setup();
    registrarMock.mockRejectedValue(new Error("Este login já está em uso."));
    renderRegistrar();

    await usuario.type(screen.getByLabelText(/login/i), "existente");
    await usuario.type(screen.getByLabelText(/senha/i), "segredo123");
    await usuario.click(screen.getByRole("button", { name: /criar conta/i }));

    expect(await screen.findByText("Este login já está em uso.")).toBeInTheDocument();
    expect(navigateMock).not.toHaveBeenCalled();
  });

  it("mostra a dica de tamanho mínimo da senha", () => {
    renderRegistrar();

    expect(screen.getByText("Mínimo de 6 caracteres.")).toBeInTheDocument();
  });
});
