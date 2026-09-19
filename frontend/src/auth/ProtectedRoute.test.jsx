import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import ProtectedRoute from "./ProtectedRoute.jsx";

const useAuthMock = vi.fn();
vi.mock("./AuthContext.jsx", () => ({ useAuth: () => useAuthMock() }));

function renderComRota(caminhoInicial) {
  return render(
    <MemoryRouter initialEntries={[caminhoInicial]}>
      <Routes>
        <Route path="/login" element={<div>Tela de login</div>} />
        <Route
          path="/painel"
          element={
            <ProtectedRoute>
              <div>Painel protegido</div>
            </ProtectedRoute>
          }
        />
      </Routes>
    </MemoryRouter>,
  );
}

describe("ProtectedRoute", () => {
  it("mostra o conteúdo protegido quando autenticado", () => {
    useAuthMock.mockReturnValue({ autenticado: true });

    renderComRota("/painel");

    expect(screen.getByText("Painel protegido")).toBeInTheDocument();
  });

  it("redireciona pro login quando não autenticado", () => {
    useAuthMock.mockReturnValue({ autenticado: false });

    renderComRota("/painel");

    expect(screen.getByText("Tela de login")).toBeInTheDocument();
    expect(screen.queryByText("Painel protegido")).not.toBeInTheDocument();
  });
});
