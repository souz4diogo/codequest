import { useEffect, useState } from "react";
import Markdown from "react-markdown";
import { api } from "../api/client.js";
import Carregando from "../components/Carregando.jsx";
import EstadoVazio from "../components/EstadoVazio.jsx";
import { IconMessageCircle, IconLoader, IconInbox, IconStar, IconSparkles } from "../components/icons.jsx";

// Chat com o mentor IA (RF22/RF23): contexto automático de tópico, histórico persistido e
// marcação para revisão.
export default function Mentor() {
  const [topicos, setTopicos] = useState([]);
  const [duvidas, setDuvidas] = useState(null);
  const [topicoId, setTopicoId] = useState("");
  const [pergunta, setPergunta] = useState("");
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState(null);

  async function carregar() {
    try {
      const lista = await api.duvidas();
      setDuvidas(lista);
    } catch (err) {
      setErro(err.message);
    }
  }

  useEffect(() => {
    api.topicosDisponiveis().then(setTopicos).catch(() => {});
    carregar();
  }, []);

  async function enviar(e) {
    e.preventDefault();
    if (!pergunta.trim()) return;
    setErro(null);
    setEnviando(true);
    try {
      await api.perguntarMentor(topicoId ? Number(topicoId) : null, pergunta.trim());
      setPergunta("");
      await carregar();
    } catch (err) {
      setErro(err.message);
    } finally {
      setEnviando(false);
    }
  }

  async function alternarRevisao(duvida) {
    try {
      await api.marcarRevisaoDuvida(duvida.id, !duvida.marcadaParaRevisao);
      await carregar();
    } catch (err) {
      setErro(err.message);
    }
  }

  return (
    <div className="page">
      <div className="page-header">
        <h1>Mentor</h1>
        <p>Pergunte qualquer coisa — o contexto do tópico atual entra automaticamente.</p>
      </div>

      <form className="card" onSubmit={enviar}>
        <select className="campo" value={topicoId} onChange={(e) => setTopicoId(e.target.value)}>
          <option value="">Sem tópico específico</option>
          {topicos.map((t) => (
            <option key={t.id} value={t.id}>
              {t.moduloNome} · {t.nome}
            </option>
          ))}
        </select>
        <textarea
          className="campo"
          value={pergunta}
          onChange={(e) => setPergunta(e.target.value)}
          rows={3}
          placeholder="Qual é a sua dúvida?"
          style={{ width: "100%", marginTop: 10, resize: "vertical" }}
        />
        <button className="btn btn-primary" style={{ marginTop: 10 }} disabled={enviando || !pergunta.trim()}>
          {enviando ? <IconLoader size={16} /> : <IconMessageCircle size={16} />}
          {enviando ? "Perguntando…" : "Perguntar"}
        </button>
      </form>

      {erro && <p className="erro">{erro}</p>}

      <div className="section-title">Histórico</div>
      {!duvidas ? (
        <Carregando />
      ) : duvidas.length === 0 ? (
        <EstadoVazio icon={IconInbox}>
          <p>Nenhuma dúvida ainda.</p>
          <p className="muted">Pergunte algo acima.</p>
        </EstadoVazio>
      ) : (
        <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
          {duvidas.map((d) => (
            <div className="card" key={d.id} style={{ display: "flex", flexDirection: "column", gap: 8 }}>
              <div style={{ display: "flex", justifyContent: "space-between", gap: 12, alignItems: "flex-start" }}>
                <strong>{d.pergunta}</strong>
                <button
                  className={`btn btn-sm ${d.marcadaParaRevisao ? "btn-gold" : "btn-ghost"}`}
                  onClick={() => alternarRevisao(d)}
                >
                  <IconStar size={14} filled={d.marcadaParaRevisao} />
                  {d.marcadaParaRevisao ? "Marcada" : "Marcar p/ revisão"}
                </button>
              </div>
              {d.topicoNome && (
                <span className="badge badge-primary" style={{ alignSelf: "flex-start" }}>
                  <IconSparkles /> {d.topicoNome}
                </span>
              )}
              <div className="markdown">
                <Markdown>{d.resposta}</Markdown>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
