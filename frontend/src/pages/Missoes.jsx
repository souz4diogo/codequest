import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import Nav from "../components/Nav.jsx";

// Missões do dia (RF09–RF13): lista, criação manual com validação da faixa de XP (RN04) e conclusão.
export default function Missoes() {
  const [missoes, setMissoes] = useState(null);
  const [faixas, setFaixas] = useState([]);
  const [erro, setErro] = useState(null);
  const [feedback, setFeedback] = useState(null);

  async function carregar() {
    try {
      const [lista, fx] = await Promise.all([api.missoesDoDia(), api.faixasEsforco()]);
      setMissoes(lista);
      setFaixas(fx);
    } catch (err) {
      setErro(err.message);
    }
  }

  useEffect(() => {
    carregar();
  }, []);

  async function concluir(id) {
    setErro(null);
    setFeedback(null);
    try {
      const r = await api.concluirMissao(id);
      setFeedback(
        `+${r.xpCreditado} XP (base ${r.xpBase} × ${r.multiplicador.toFixed(1)}), +${r.goldGanho} gold.` +
          (r.subiuNivel ? ` Subiu para o nível ${r.nivelAtual}! 🎉` : "")
      );
      await carregar();
    } catch (err) {
      setErro(err.message);
    }
  }

  if (erro && !missoes)
    return (
      <div className="content">
        <Nav />
        <p className="erro">{erro}</p>
      </div>
    );

  return (
    <div className="content">
      <Nav />

      <FormSugerirMissao
        onCriada={() => {
          setFeedback("A IA sugeriu uma missão!");
          carregar();
        }}
        onErro={setErro}
      />

      <FormNovaMissao
        faixas={faixas}
        onCriada={() => {
          setFeedback("Missão criada!");
          carregar();
        }}
        onErro={setErro}
      />

      {feedback && <p className="feedback">{feedback}</p>}
      {erro && <p className="erro">{erro}</p>}

      <div className="section-title">Missões de hoje</div>
      {!missoes ? (
        <p className="muted">Carregando…</p>
      ) : missoes.length === 0 ? (
        <p className="muted">Nenhuma missão para hoje. Crie uma acima. 👆</p>
      ) : (
        <div className="grid">
          {missoes.map((m) => (
            <CardMissao key={m.id} missao={m} onConcluir={() => concluir(m.id)} />
          ))}
        </div>
      )}
    </div>
  );
}

const ROTULOS_TIPO = {
  Diaria: "🎯 Diária",
  SugeridaIA: "✨ Sugerida pela IA",
};

function CardMissao({ missao, onConcluir }) {
  const encerrada = missao.status === "Concluida" || missao.status === "Expirada";
  return (
    <div className="card">
      <div style={{ display: "flex", justifyContent: "space-between", gap: 12 }}>
        <strong>{missao.titulo}</strong>
        <span className="muted">{rotuloStatus(missao.status)}</span>
      </div>
      {ROTULOS_TIPO[missao.tipo] && (
        <span className="muted" style={{ fontSize: 12 }}>
          {ROTULOS_TIPO[missao.tipo]}
        </span>
      )}
      {missao.descricao && <p className="muted">{missao.descricao}</p>}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginTop: 8 }}>
        <span className="muted">
          {missao.xpRecompensa} XP · <span style={{ color: "var(--gold)" }}>{missao.goldRecompensa} gold</span>
        </span>
        {!encerrada && (
          <button className="btn btn-primary" onClick={onConcluir}>
            Concluir
          </button>
        )}
      </div>
    </div>
  );
}

function rotuloStatus(status) {
  const mapa = {
    Pendente: "⏳ Pendente",
    EmAndamento: "▶️ Em andamento",
    Concluida: "✅ Concluída",
    Expirada: "⌛ Expirada",
  };
  return mapa[status] ?? status;
}

function FormSugerirMissao({ onCriada, onErro }) {
  const [texto, setTexto] = useState("");
  const [enviando, setEnviando] = useState(false);

  async function enviar(e) {
    e.preventDefault();
    onErro(null);
    if (!texto.trim()) return;
    setEnviando(true);
    try {
      await api.sugerirMissao(texto.trim());
      setTexto("");
      onCriada();
    } catch (err) {
      onErro(err.message);
    } finally {
      setEnviando(false);
    }
  }

  return (
    <form className="card" onSubmit={enviar}>
      <div className="section-title" style={{ marginTop: 0 }}>
        Pedir à IA
      </div>
      <div className="acoes" style={{ flexWrap: "wrap" }}>
        <input
          className="campo"
          placeholder='Ex.: "quero praticar herança"'
          value={texto}
          onChange={(e) => setTexto(e.target.value)}
          style={{ flex: 1, minWidth: 220 }}
        />
        <button className="btn btn-primary" disabled={enviando || !texto.trim()}>
          {enviando ? "Pensando…" : "Sugerir missão"}
        </button>
      </div>
    </form>
  );
}

function FormNovaMissao({ faixas, onCriada, onErro }) {
  const [titulo, setTitulo] = useState("");
  const [descricao, setDescricao] = useState("");
  const [esforco, setEsforco] = useState("");
  const [xp, setXp] = useState("");
  const [enviando, setEnviando] = useState(false);

  const faixa = faixas.find((f) => f.esforco === esforco);

  // Seleciona o primeiro esforço assim que as faixas chegam e sugere o XP mínimo.
  useEffect(() => {
    if (!esforco && faixas.length > 0) {
      setEsforco(faixas[0].esforco);
      setXp(String(faixas[0].xpMinimo));
    }
  }, [faixas, esforco]);

  function trocarEsforco(valor) {
    setEsforco(valor);
    const f = faixas.find((x) => x.esforco === valor);
    if (f) setXp(String(f.xpMinimo));
  }

  const xpNum = Number(xp);
  const xpValido = faixa && xpNum >= faixa.xpMinimo && xpNum <= faixa.xpMaximo;

  async function enviar(e) {
    e.preventDefault();
    onErro(null);
    if (!titulo.trim() || !esforco || !xpValido) return;
    setEnviando(true);
    try {
      await api.criarMissao({ titulo: titulo.trim(), esforco, xp: xpNum, descricao: descricao.trim() || null });
      setTitulo("");
      setDescricao("");
      onCriada();
    } catch (err) {
      onErro(err.message);
    } finally {
      setEnviando(false);
    }
  }

  return (
    <form className="card" onSubmit={enviar}>
      <div className="section-title" style={{ marginTop: 0 }}>
        Nova missão
      </div>
      <input
        className="campo"
        placeholder="Título"
        value={titulo}
        onChange={(e) => setTitulo(e.target.value)}
        style={{ marginTop: 8 }}
      />
      <input
        className="campo"
        placeholder="Descrição (opcional)"
        value={descricao}
        onChange={(e) => setDescricao(e.target.value)}
        style={{ marginTop: 8 }}
      />
      <div className="acoes" style={{ marginTop: 8, flexWrap: "wrap" }}>
        <select className="campo" value={esforco} onChange={(e) => trocarEsforco(e.target.value)}>
          {faixas.map((f) => (
            <option key={f.esforco} value={f.esforco}>
              {f.esforco} ({f.xpMinimo}–{f.xpMaximo} XP)
            </option>
          ))}
        </select>
        <input
          className="campo"
          type="number"
          value={xp}
          onChange={(e) => setXp(e.target.value)}
          style={{ width: 100 }}
          min={faixa?.xpMinimo}
          max={faixa?.xpMaximo}
        />
        <button className="btn btn-primary" disabled={enviando || !titulo.trim() || !xpValido}>
          Criar
        </button>
      </div>
      {faixa && !xpValido && (
        <p className="muted">XP precisa ficar entre {faixa.xpMinimo} e {faixa.xpMaximo} para esforço {faixa.esforco}.</p>
      )}
    </form>
  );
}

