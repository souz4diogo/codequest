import { useEffect, useState } from "react";
import Markdown from "react-markdown";
import { api } from "../api/client.js";
import Nav from "../components/Nav.jsx";

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
    <div className="content">
      <Nav />
      <div className="section-title" style={{ marginTop: 0 }}>
        Mentor
      </div>

      <form className="card" onSubmit={enviar}>
        <div className="acoes" style={{ flexWrap: "wrap" }}>
          <select className="campo" value={topicoId} onChange={(e) => setTopicoId(e.target.value)}>
            <option value="">Sem tópico específico</option>
            {topicos.map((t) => (
              <option key={t.id} value={t.id}>
                {t.moduloNome} · {t.nome}
              </option>
            ))}
          </select>
        </div>
        <textarea
          className="campo"
          value={pergunta}
          onChange={(e) => setPergunta(e.target.value)}
          rows={3}
          placeholder="Qual é a sua dúvida?"
          style={{ width: "100%", marginTop: 8, resize: "vertical" }}
        />
        <button className="btn btn-primary" style={{ marginTop: 8 }} disabled={enviando || !pergunta.trim()}>
          {enviando ? "Perguntando…" : "Perguntar"}
        </button>
      </form>

      {erro && <p className="erro">{erro}</p>}

      <div className="section-title">Histórico</div>
      {!duvidas ? (
        <p className="muted">Carregando…</p>
      ) : duvidas.length === 0 ? (
        <p className="muted">Nenhuma dúvida ainda. Pergunte algo acima. 👆</p>
      ) : (
        duvidas.map((d) => (
          <div className="card" key={d.id} style={{ marginBottom: 12 }}>
            <div style={{ display: "flex", justifyContent: "space-between", gap: 12 }}>
              <strong>{d.pergunta}</strong>
              <button className="btn btn-ghost" onClick={() => alternarRevisao(d)}>
                {d.marcadaParaRevisao ? "★ Marcada" : "☆ Marcar p/ revisão"}
              </button>
            </div>
            {d.topicoNome && <span className="muted">{d.topicoNome}</span>}
            <div className="markdown" style={{ marginTop: 8 }}>
              <Markdown>{d.resposta}</Markdown>
            </div>
          </div>
        ))
      )}
    </div>
  );
}

