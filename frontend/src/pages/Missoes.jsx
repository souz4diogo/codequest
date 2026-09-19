import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import Carregando from "../components/Carregando.jsx";
import EstadoVazio from "../components/EstadoVazio.jsx";
import {
  IconTarget,
  IconSparkles,
  IconWand,
  IconPlus,
  IconLoader,
  IconInbox,
  IconClock,
  IconPlayCircle,
  IconCheckCircle,
  IconXCircle,
  IconCoins,
} from "../components/icons.jsx";

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
          (r.subiuNivel ? ` Subiu para o nível ${r.nivelAtual}!` : "")
      );
      await carregar();
    } catch (err) {
      setErro(err.message);
    }
  }

  if (erro && !missoes)
    return (
      <div className="page">
        <p className="erro">{erro}</p>
      </div>
    );

  return (
    <div className="page">
      <div className="page-header">
        <h1>Missões</h1>
        <p>Complete o que der pra hoje — cada uma paga XP e gold ao concluir.</p>
      </div>

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

      <div className="section-title">
        <IconTarget size={16} /> Missões de hoje
      </div>
      {!missoes ? (
        <Carregando />
      ) : missoes.length === 0 ? (
        <EstadoVazio icon={IconInbox}>
          <p>Nenhuma missão para hoje.</p>
          <p className="muted">Crie uma acima ou peça uma sugestão à IA.</p>
        </EstadoVazio>
      ) : (
        <div className="grid grid-2">
          {missoes.map((m) => (
            <CardMissao key={m.id} missao={m} onConcluir={() => concluir(m.id)} />
          ))}
        </div>
      )}
    </div>
  );
}

const TIPO_INFO = {
  Diaria: { texto: "Diária", Icon: IconTarget },
  SugeridaIA: { texto: "Sugerida pela IA", Icon: IconSparkles },
};

const STATUS_INFO = {
  Pendente: { texto: "Pendente", Icon: IconClock, badge: "badge-neutro" },
  EmAndamento: { texto: "Em andamento", Icon: IconPlayCircle, badge: "badge-info" },
  Concluida: { texto: "Concluída", Icon: IconCheckCircle, badge: "badge-success" },
  Expirada: { texto: "Expirada", Icon: IconXCircle, badge: "badge-danger" },
};

function CardMissao({ missao, onConcluir }) {
  const encerrada = missao.status === "Concluida" || missao.status === "Expirada";
  const status = STATUS_INFO[missao.status] ?? STATUS_INFO.Pendente;
  const tipo = TIPO_INFO[missao.tipo];

  return (
    <div className="card card-interactive" style={{ display: "flex", flexDirection: "column", gap: 10 }}>
      <div style={{ display: "flex", justifyContent: "space-between", gap: 12, alignItems: "flex-start" }}>
        <strong>{missao.titulo}</strong>
        <span className={`badge ${status.badge}`}>
          <status.Icon /> {status.texto}
        </span>
      </div>
      {tipo && (
        <span className="badge badge-primary" style={{ alignSelf: "flex-start" }}>
          <tipo.Icon /> {tipo.texto}
        </span>
      )}
      {missao.descricao && <p className="muted">{missao.descricao}</p>}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginTop: 4 }}>
        <span className="rotulo-e-icone muted">
          {missao.xpRecompensa} XP ·{" "}
          <span className="rotulo-e-icone" style={{ color: "var(--gold)" }}>
            <IconCoins size={14} /> {missao.goldRecompensa}
          </span>
        </span>
        {!encerrada && (
          <button className="btn btn-primary btn-sm" onClick={onConcluir}>
            Concluir
          </button>
        )}
      </div>
    </div>
  );
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
        <IconWand size={16} /> Pedir à IA
      </div>
      <div className="form-row" style={{ marginTop: 10 }}>
        <input
          className="campo"
          placeholder='Ex.: "quero praticar herança"'
          value={texto}
          onChange={(e) => setTexto(e.target.value)}
        />
        <button className="btn btn-primary" disabled={enviando || !texto.trim()}>
          {enviando ? <IconLoader size={16} /> : <IconSparkles size={16} />}
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
        <IconPlus size={16} /> Nova missão
      </div>
      <div style={{ display: "flex", flexDirection: "column", gap: 10, marginTop: 10 }}>
        <input className="campo" placeholder="Título" value={titulo} onChange={(e) => setTitulo(e.target.value)} />
        <input
          className="campo"
          placeholder="Descrição (opcional)"
          value={descricao}
          onChange={(e) => setDescricao(e.target.value)}
        />
        <div className="form-row">
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
            style={{ maxWidth: 110 }}
            min={faixa?.xpMinimo}
            max={faixa?.xpMaximo}
          />
          <button className="btn btn-primary" disabled={enviando || !titulo.trim() || !xpValido}>
            Criar
          </button>
        </div>
        {faixa && !xpValido && (
          <p className="muted">
            XP precisa ficar entre {faixa.xpMinimo} e {faixa.xpMaximo} para esforço {faixa.esforco}.
          </p>
        )}
      </div>
    </form>
  );
}
