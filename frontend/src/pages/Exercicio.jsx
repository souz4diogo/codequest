import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import Nav from "../components/Nav.jsx";

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
      <div className="content">
        <Nav />
        <p className="erro">{erro}</p>
      </div>
    );

  return (
    <div className="content">
      <Nav />
      <div className="section-title" style={{ marginTop: 0 }}>
        Exercício
      </div>
      {!topicos ? (
        <p className="muted">Carregando…</p>
      ) : topicos.length === 0 ? (
        <p className="muted">Nenhum tópico liberado ainda.</p>
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
        <div className="acoes" style={{ flexWrap: "wrap" }}>
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
            {gerando ? "Gerando…" : exercicio ? "Gerar outro" : "Gerar exercício"}
          </button>
        </div>
      </form>

      {erro && <p className="erro">{erro}</p>}

      {enunciado && (
        <div className="card" style={{ marginTop: 16 }}>
          <strong>{enunciado.titulo}</strong>
          <p style={{ whiteSpace: "pre-wrap" }}>{enunciado.enunciado}</p>

          {(enunciado.codigoPartida || enunciado.codigoApresentado) && (
            <pre className="card" style={{ background: "var(--bg-surface)", overflowX: "auto" }}>
              <code>{enunciado.codigoPartida ?? enunciado.codigoApresentado}</code>
            </pre>
          )}

          {enunciado.comportamentoEsperado && (
            <p className="muted">{enunciado.comportamentoEsperado}</p>
          )}

          {enunciado.casosDeTeste?.length > 0 && (
            <ul className="muted">
              {enunciado.casosDeTeste.map((c, i) => (
                <li key={i}>
                  {c.entrada} → {c.saidaEsperada}
                </li>
              ))}
            </ul>
          )}

          {enunciado.dicas?.length > 0 && (
            <details style={{ marginTop: 8 }}>
              <summary className="muted">Dicas</summary>
              <ul className="muted">
                {enunciado.dicas.map((d, i) => (
                  <li key={i}>{d}</li>
                ))}
              </ul>
            </details>
          )}

          {!correcao && (
            <form onSubmit={enviar} style={{ marginTop: 16 }}>
              {formato === "MultiplaEscolha" ? (
                <div className="grid">
                  {enunciado.alternativas.map((alt, i) => {
                    const letra = String.fromCharCode(65 + i);
                    const selecionada = resposta === alt.texto;
                    return (
                      <button
                        type="button"
                        key={i}
                        className="opt"
                        onClick={() => setResposta(alt.texto)}
                        style={{
                          display: "flex",
                          gap: 10,
                          alignItems: "center",
                          textAlign: "left",
                          border: selecionada ? "1px solid var(--info)" : "1px solid var(--border)",
                          borderRadius: "var(--radius)",
                          padding: "12px 14px",
                          background: "var(--bg-surface)",
                        }}
                      >
                        <span className="badge badge-tipo">{letra}</span>
                        <span>{alt.texto}</span>
                      </button>
                    );
                  })}
                </div>
              ) : (
                <textarea
                  className="campo"
                  value={resposta}
                  onChange={(e) => setResposta(e.target.value)}
                  rows={10}
                  placeholder="Digite sua resposta…"
                  style={{ width: "100%", fontFamily: "var(--font-code)", resize: "vertical" }}
                />
              )}
              <button className="btn btn-primary" style={{ marginTop: 12 }} disabled={enviando || !resposta.trim()}>
                {enviando ? "Enviando…" : "Enviar resposta"}
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
  return (
    <div className={`card ${correcao.aprovado ? "" : "erro-card"}`} style={{ marginTop: 16 }}>
      <strong style={{ color: correcao.aprovado ? "var(--success)" : "var(--medium)" }}>
        {correcao.aprovado ? "Aprovado!" : "Quase — vamos revisar"} · Nota {correcao.nota}
      </strong>

      {correcao.problemas?.length > 0 && (
        <ul style={{ marginTop: 8 }}>
          {correcao.problemas.map((p, i) => (
            <li key={i} style={{ marginBottom: 6 }}>
              <code style={{ fontFamily: "var(--font-code)" }}>{p.trecho}</code>: {p.explicacao}
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
          {correcao.recompensa.subiuNivel ? ` Subiu para o nível ${correcao.recompensa.nivelAtual}! 🎉` : ""}
        </p>
      )}

      {correcao.revisaoAgendadaPara && (
        <p className="muted">Revisão agendada para {correcao.revisaoAgendadaPara}.</p>
      )}
    </div>
  );
}

