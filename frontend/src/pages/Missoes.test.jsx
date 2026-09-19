import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import Missoes from "./Missoes.jsx";

vi.mock("../api/client.js", () => ({
  api: {
    missoesDoDia: vi.fn(),
    faixasEsforco: vi.fn(),
    concluirMissao: vi.fn(),
    criarMissao: vi.fn(),
    sugerirMissao: vi.fn(),
  },
}));

import { api } from "../api/client.js";

const FAIXAS = [
  { esforco: "Rapida", xpMinimo: 10, xpMaximo: 20 },
  { esforco: "Media", xpMinimo: 30, xpMaximo: 60 },
];

beforeEach(() => {
  vi.clearAllMocks();
  api.faixasEsforco.mockResolvedValue(FAIXAS);
});

describe("Missoes — lista", () => {
  it("mostra o estado vazio quando não há missões hoje", async () => {
    api.missoesDoDia.mockResolvedValue([]);
    render(<Missoes />);

    expect(await screen.findByText("Nenhuma missão para hoje.")).toBeInTheDocument();
  });

  it("lista as missões do dia com status e recompensas", async () => {
    api.missoesDoDia.mockResolvedValue([
      { id: 1, titulo: "Revisar herança", status: "Pendente", tipo: "Diaria", xpRecompensa: 15, goldRecompensa: 5 },
    ]);
    render(<Missoes />);

    expect(await screen.findByText("Revisar herança")).toBeInTheDocument();
    expect(screen.getByText(/15 XP/)).toBeInTheDocument();
  });

  it("concluir uma missão chama a API e mostra a recompensa creditada", async () => {
    const usuario = userEvent.setup();
    api.missoesDoDia.mockResolvedValue([
      { id: 7, titulo: "Praticar LINQ", status: "Pendente", tipo: "Diaria", xpRecompensa: 20, goldRecompensa: 6 },
    ]);
    api.concluirMissao.mockResolvedValue({ xpCreditado: 22, xpBase: 20, multiplicador: 1.1, goldGanho: 6, subiuNivel: false });

    render(<Missoes />);
    await usuario.click(await screen.findByRole("button", { name: /concluir/i }));

    expect(api.concluirMissao).toHaveBeenCalledWith(7);
    expect(await screen.findByText(/\+22 XP \(base 20 × 1\.1\), \+6 gold\./)).toBeInTheDocument();
  });
});

describe("Missoes — criar missão manual (validação da faixa de XP, RN04)", () => {
  it("pré-seleciona o primeiro esforço e sugere o XP mínimo dele", async () => {
    api.missoesDoDia.mockResolvedValue([]);
    render(<Missoes />);

    await waitFor(() => expect(screen.getByPlaceholderText("Título")).toBeInTheDocument());
    const campoXp = screen.getByDisplayValue("10");
    expect(campoXp).toBeInTheDocument();
  });

  it("desabilita Criar quando o XP digitado está fora da faixa do esforço", async () => {
    const usuario = userEvent.setup();
    api.missoesDoDia.mockResolvedValue([]);
    render(<Missoes />);

    await usuario.type(await screen.findByPlaceholderText("Título"), "Ler capítulo 3");
    const campoXp = screen.getByDisplayValue("10");
    await usuario.clear(campoXp);
    await usuario.type(campoXp, "999");

    expect(screen.getByRole("button", { name: "Criar" })).toBeDisabled();
    expect(screen.getByText(/XP precisa ficar entre 10 e 20/)).toBeInTheDocument();
  });

  it("cria a missão quando título preenchido e XP dentro da faixa", async () => {
    const usuario = userEvent.setup();
    api.missoesDoDia.mockResolvedValue([]);
    api.criarMissao.mockResolvedValue({});

    render(<Missoes />);
    await usuario.type(await screen.findByPlaceholderText("Título"), "Ler capítulo 3");
    await usuario.click(screen.getByRole("button", { name: "Criar" }));

    expect(api.criarMissao).toHaveBeenCalledWith({
      titulo: "Ler capítulo 3",
      esforco: "Rapida",
      xp: 10,
      descricao: null,
    });
    expect(await screen.findByText("Missão criada!")).toBeInTheDocument();
  });

  it("trocar de esforço atualiza a faixa de XP sugerida", async () => {
    const usuario = userEvent.setup();
    api.missoesDoDia.mockResolvedValue([]);
    render(<Missoes />);

    await waitFor(() => expect(screen.getByDisplayValue("10")).toBeInTheDocument());
    await usuario.selectOptions(screen.getByDisplayValue(/Rapida/), "Media");

    expect(screen.getByDisplayValue("30")).toBeInTheDocument();
  });
});
