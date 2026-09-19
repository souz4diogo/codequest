import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import Carregando from "../components/Carregando.jsx";
import EstadoVazio from "../components/EstadoVazio.jsx";
import {
  IconCode,
  IconLoader,
  IconInbox,
  IconCheckCircle,
  IconXCircle,
  IconChevronRight,
} from "../components/icons.jsx";

const DIFICULDADES = ["Easy", "Medium", "Hard", "Expert"];
const FORMATOS = [
  { valor: "MultiplaEscolha", rotulo: "Múltipla escolha" },
  { valor: "Codigo", rotulo: "Código" },
  { valor: "Debug", rotulo: "Debug" },
  { valor: "Refactor", rotulo: "Refactor" },
  { valor: "CodeReview", rotulo: "Code review" },
  { valor: "LeituraDeCodigo", rotulo: "Leitura de código" },
];

// Exercícios gerados e corrigidos pela IA (RF14/RF15). O front nunca vê o gabarito: só o
// enunciado público até enviar a resposta, e aí a API devolve o parecer completo.
export default function Exercicio() {
  const [topicos, setTopicos] = useState(null);
  const [erro, setErro] = useState(null);

  useEffect(() => {
    api.topicosDisponiveis().then(setTopicos).catch((err) => setErro(err.message));
  }, []);

  if (erro && !topicos)
    return (
      <div className="page">
        <p className="erro">{erro}</p>
      </div>
    );

  return (
    <div className="page">
      <div className="page-header">
        <h1>Exercício</h1>
        <p>Gerado pela IA na hora, corrigido pela IA quando você responde.</p>
      </div>
      {!topicos ? (
        <Carregando />
      ) : topicos.length === 0 ? (
        <EstadoVazio icon={IconInbox}>
          <p>Nenhum tópico liberado ainda.</p>
        </EstadoVazio>
      ) : (
        <PainelExercicio topicos={topicos} />
      )}
    </div>
  );
}

function PainelExercicio({ topicos }) {
  const [topicoId, setTopicoId] = useState(topicos[0].id);
  const [dificuldade, setDificuldade] = useState("Easy");
  const [formato, setFormato] = useState(FORMATOS[0].valor);
  const [exercicio, setExercicio] = useState(null);
  const [enunciado, setEnunciado] = useState(null);
  const [resposta, setResposta] = useState("");
  const [correcao, setCorrecao] = useState(null);
  const [gerando, setGerando] = useState(false);
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState(null);

  async function gerar() {
    setErro(null);
    setCorrecao(null);
    setResposta("");
    setGerando(true);
    try {
      const ex = await api.gerarExercicio(topicoId, dificuldade, formato);
      setExercicio(ex);
      setEnunciado(JSON.parse(ex.enunciadoJson));
    } catch (err) {
      setErro(err.message);
    } finally {
      setGerando(false);
    }
  }

  async function enviar(e) {
    e.preventDefault();
    if (!resposta.trim()) return;
    setErro(null);
    setEnviando(true);
    try {
      const parecer = await api.responderExercicio(exercicio.id, resposta);
      setCorrecao(parecer);
    } catch (err) {
      setErro(err.message);
    } finally {
      setEnviando(false);
    }
  }

  return (
    <>
      <form className="card" onSubmit={(e) => e.preventDefault()}>
        <div className="form-row">
          <select className="campo" value={topicoId} onChange={(e) => setTopicoId(Number(e.target.value))}>
            {topicos.map((t) => (
              <option key={t.id} value={t.id}>
                {t.moduloNome} · {t.nome}
              </option>
            ))}
          </select>
          <select className="campo" value={dificuldade} onChange={(e) => setDificuldade(e.target.value)}>
            {DIFICULDADES.map((d) => (
              <option key={d} value={d}>
                {d}
              </option>
            ))}
          </select>
          <select className="campo" value={formato} onChange={(e) => setFormato(e.target.value)}>
            {FORMATOS.map((f) => (
              <option key={f.valor} value={f.valor}>
                {f.rotulo}
              </option>
            ))}
          </select>
          <button className="btn btn-primary" onClick={gerar} disabled={gerando}>
            {gerando ? <IconLoader size={16} /> : <IconCode size={16} />}
            {gerando ? "Gerando…" : exercicio ? "Gerar outro" : "Gerar exercício"}
          </button>
        </div>
      </form>

      {erro && <p className="erro">{erro}</p>}

      {enunciado && (
        <div className="card" style={{ display: "flex", flexDirection: "column", gap: 12 }}>
          <strong style={{ fontSize: 16 }}>{enunciado.titulo}</strong>
          <p style={{ whiteSpace: "pre-wrap" }}>{enunciado.enunciado}</p>

          {(enunciado.codigoPartida || enunciado.codigoApresentado) && (
            <pre style={{ background: "var(--bg-base-alt)", border: "1px solid var(--border)", borderRadius: "var(--radius-sm)", padding: 14, overflowX: "auto" }}>
              <code className="mono">{enunciado.codigoPartida ?? enunciado.codigoApresentado}</code>
            </pre>
          )}

          {enunciado.comportamentoEsperado && <p className="muted">{enunciado.comportamentoEsperado}</p>}

          {enunciado.casosDeTeste?.length > 0 && (
            <ul style={{ display: "flex", flexDirection: "column", gap: 4 }}>
              {enunciado.casosDeTeste.map((c, i) => (
                <li key={i} className="muted mono" style={{ fontSize: 13 }}>
                  {c.entrada} → {c.saidaEsperada}
                </li>
              ))}
            </ul>
          )}

          {enunciado.dicas?.length > 0 && (
            <details>
              <summary className="muted" style={{ cursor: "pointer" }}>
                Dicas
              </summary>
              <ul className="muted" style={{ marginTop: 8, paddingLeft: 18 }}>
                {enunciado.dicas.map((d, i) => (
                  <li key={i}>{d}</li>
                ))}
              </ul>
            </details>
          )}

          {!correcao && (
            <form onSubmit={enviar} style={{ display: "flex", flexDirection: "column", gap: 12 }}>
              {formato === "MultiplaEscolha" ? (
                <div className="grid">
                  {enunciado.alternativas.map((alt, i) => {
                    const letra = String.fromCharCode(65 + i);
                    const selecionada = resposta === alt.texto;
                    return (
                      <button
                        type="button"
                        key={i}
                        className={`opt${selecionada ? " selecionada" : ""}`}
                        onClick={() => setResposta(alt.texto)}
                      >
                        <span className="badge-letra">{letra}</span>
                        <span>{alt.texto}</span>
                      </button>
                    );
                  })}
                </div>
              ) : (
                <textarea
                  className="campo mono"
                  value={resposta}
                  onChange={(e) => setResposta(e.target.value)}
                  rows={10}
                  placeholder="Digite sua resposta…"
                  style={{ width: "100%", resize: "vertical" }}
                />
              )}
              <button className="btn btn-primary" disabled={enviando || !resposta.trim()} style={{ alignSelf: "flex-start" }}>
                {enviando && <IconLoader size={16} />}
                {enviando ? "Enviando…" : "Enviar resposta"}
                {!enviando && <IconChevronRight size={16} />}
              </button>
            </form>
          )}
        </div>
      )}

      {correcao && <ResultadoCorrecao correcao={correcao} />}
    </>
  );
}

function ResultadoCorrecao({ correcao }) {
  const cor = correcao.aprovado ? "var(--success)" : "var(--danger)";
  return (
    <div className="card" style={{ display: "flex", flexDirection: "column", gap: 10, borderColor: correcao.aprovado ? "#3fb95055" : "#f8514955" }}>
      <div className="rotulo-e-icone" style={{ color: cor }}>
        {correcao.aprovado ? <IconCheckCircle size={20} /> : <IconXCircle size={20} />}
        <strong>{correcao.aprovado ? "Aprovado!" : "Quase — vamos revisar"} · Nota {correcao.nota}</strong>
      </div>

      {correcao.problemas?.length > 0 && (
        <ul style={{ display: "flex", flexDirection: "column", gap: 8 }}>
          {correcao.problemas.map((p, i) => (
            <li key={i}>
              <code className="mono" style={{ background: "var(--bg-base-alt)", padding: "2px 6px", borderRadius: 4 }}>
                {p.trecho}
              </code>
              : {p.explicacao}
              {p.correcao && <div className="muted">Correção: {p.correcao}</div>}
            </li>
          ))}
        </ul>
      )}

      {correcao.elogio && <p className="muted">{correcao.elogio}</p>}

      {correcao.conceitosParaRevisar?.length > 0 && (
        <p className="muted">Revisar: {correcao.conceitosParaRevisar.join(", ")}</p>
      )}

      {correcao.recompensa && (
        <p className="feedback">
          +{correcao.recompensa.xpCreditado} XP, +{correcao.recompensa.goldGanho} gold.
          {correcao.recompensa.subiuNivel ? ` Subiu para o nível ${correcao.recompensa.nivelAtual}!` : ""}
        </p>
      )}

      {correcao.revisaoAgendadaPara && <p className="muted">Revisão agendada para {correcao.revisaoAgendadaPara}.</p>}
    </div>
  );
}
