import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import Nav from "../components/Nav.jsx";
import BateriaQuestoes from "../components/BateriaQuestoes.jsx";

const DIFICULDADES = ["Easy", "Medium", "Hard", "Expert"];

// Teste de avaliação por tópico (RF17): 5–10 questões geradas pela IA; a nota atualiza o
// nível do tópico (RN07) e os erros viram gaps para revisão futura.
export default function Testes() {
  const [topicos, setTopicos] = useState(null);
  const [erro, setErro] = useState(null);

  useEffect(() => {
    api.topicosDisponiveis().then(setTopicos).catch((err) => setErro(err.message));
  }, []);

  return (
    <div className="content">
      <Nav />
      <div className="section-title" style={{ marginTop: 0 }}>
        Teste de avaliação
      </div>
      {erro && !topicos && <p className="erro">{erro}</p>}
      {!topicos ? <p className="muted">Carregando…</p> : <PainelTeste topicos={topicos} />}
    </div>
  );
}

function PainelTeste({ topicos }) {
  const [topicoId, setTopicoId] = useState(topicos[0]?.id ?? null);
  const [dificuldade, setDificuldade] = useState(DIFICULDADES[0]);
  const [teste, setTeste] = useState(null);
  const [correcao, setCorrecao] = useState(null);
  const [gerando, setGerando] = useState(false);
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState(null);

  async function iniciar() {
    setErro(null);
    setCorrecao(null);
    setTeste(null);
    setGerando(true);
    try {
      setTeste(await api.iniciarTeste(topicoId, dificuldade));
    } catch (err) {
      setErro(err.message);
    } finally {
      setGerando(false);
    }
  }

  async function enviar(respostas) {
    setEnviando(true);
    setErro(null);
    try {
      setCorrecao(await api.responderTeste(teste.id, respostas));
    } catch (err) {
      setErro(err.message);
    } finally {
      setEnviando(false);
    }
  }

  return (
    <>
      <div className="card">
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
          <button className="btn btn-primary" onClick={iniciar} disabled={gerando}>
            {gerando ? "Gerando…" : teste ? "Gerar outro" : "Iniciar teste"}
          </button>
        </div>
      </div>

      {erro && <p className="erro">{erro}</p>}

      {teste && !correcao && (
        <div style={{ marginTop: 16 }}>
          <BateriaQuestoes questoes={teste.questoes} onEnviar={enviar} enviando={enviando} />
        </div>
      )}

      {correcao && (
        <div className="card" style={{ marginTop: 16 }}>
          <strong style={{ color: correcao.nota >= 70 ? "var(--success)" : "var(--medium)" }}>
            Nota {correcao.nota}
          </strong>
          <p className="muted">
            {correcao.acertos.filter(Boolean).length} de {correcao.acertos.length} corretas.
          </p>
          {correcao.gaps.length > 0 && (
            <p className="muted">Gaps identificados: {correcao.gaps.join(", ")}</p>
          )}
        </div>
      )}
    </>
  );
}
