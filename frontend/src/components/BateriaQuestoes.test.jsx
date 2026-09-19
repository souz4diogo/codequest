import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import BateriaQuestoes from "./BateriaQuestoes.jsx";

const questoes = [
  { enunciado: "2 + 2 = ?", alternativas: ["3", "4"] },
  { enunciado: "Capital do Brasil?", alternativas: ["Brasília", "Rio"] },
];

describe("BateriaQuestoes", () => {
  it("começa com o botão de enviar desabilitado (nenhuma questão respondida)", () => {
    render(<BateriaQuestoes questoes={questoes} onEnviar={vi.fn()} enviando={false} />);

    expect(screen.getByRole("button", { name: /enviar respostas/i })).toBeDisabled();
  });

  it("só habilita enviar depois de responder TODAS as questões", async () => {
    const usuario = userEvent.setup();
    render(<BateriaQuestoes questoes={questoes} onEnviar={vi.fn()} enviando={false} />);

    await usuario.click(screen.getByRole("button", { name: /^B\s*4$/ }));
    expect(screen.getByRole("button", { name: /enviar respostas/i })).toBeDisabled();

    await usuario.click(screen.getByRole("button", { name: /^B\s*Rio$/ }));
    expect(screen.getByRole("button", { name: /enviar respostas/i })).toBeEnabled();
  });

  it("envia os índices das alternativas escolhidas, uma por questão", async () => {
    const usuario = userEvent.setup();
    const onEnviar = vi.fn();
    render(<BateriaQuestoes questoes={questoes} onEnviar={onEnviar} enviando={false} />);

    await usuario.click(screen.getByRole("button", { name: /^B\s*4$/ })); // questão 1, alternativa índice 1
    await usuario.click(screen.getByRole("button", { name: /^A\s*Brasília$/ })); // questão 2, índice 0
    await usuario.click(screen.getByRole("button", { name: /enviar respostas/i }));

    expect(onEnviar).toHaveBeenCalledWith([1, 0]);
  });

  it("trocar de alternativa na mesma questão substitui a resposta anterior", async () => {
    const usuario = userEvent.setup();
    const onEnviar = vi.fn();
    render(<BateriaQuestoes questoes={questoes} onEnviar={onEnviar} enviando={false} />);

    await usuario.click(screen.getByRole("button", { name: /^A\s*3$/ }));
    await usuario.click(screen.getByRole("button", { name: /^B\s*4$/ })); // muda de ideia na questão 1
    await usuario.click(screen.getByRole("button", { name: /^A\s*Brasília$/ }));
    await usuario.click(screen.getByRole("button", { name: /enviar respostas/i }));

    expect(onEnviar).toHaveBeenCalledWith([1, 0]);
  });

  it("enviando=true desabilita o botão e troca o texto", () => {
    render(<BateriaQuestoes questoes={questoes} onEnviar={vi.fn()} enviando={true} />);

    const botao = screen.getByRole("button", { name: /enviando/i });
    expect(botao).toBeDisabled();
  });
});
