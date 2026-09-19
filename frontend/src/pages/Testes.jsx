import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import BateriaQuestoes from "../components/BateriaQuestoes.jsx";
import Carregando from "../components/Carregando.jsx";
import EstadoVazio from "../components/EstadoVazio.jsx";
import { IconClipboardCheck, IconLoader, IconInbox, IconCheckCircle, IconXCircle } from "../components/icons.jsx";

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
    <div className="page">
      <div className="page-header">
        <h1>Teste de avaliação</h1>
        <p>Mede seu nível real no tópico e alimenta as missões seguintes com os gaps.</p>
      </div>
      {erro && !topicos && <p className="erro">{erro}</p>}
      {!topicos ? (
        <Carregando />
      ) : topicos.length === 0 ? (
        <EstadoVazio icon={IconInbox}>
          <p>Nenhum tópico liberado ainda.</p>
        </EstadoVazio>
      ) : (
        <PainelTeste topicos={topicos} />
      )}
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
          <button className="btn btn-primary" onClick={iniciar} disabled={gerando}>
            {gerando ? <IconLoader size={16} /> : <IconClipboardCheck size={16} />}
            {gerando ? "Gerando…" : teste ? "Gerar outro" : "Iniciar teste"}
          </button>
        </div>
      </div>

      {erro && <p className="erro">{erro}</p>}

      {teste && !correcao && <BateriaQuestoes questoes={teste.questoes} onEnviar={enviar} enviando={enviando} />}

      {correcao && (
        <div className="card" style={{ display: "flex", flexDirection: "column", gap: 8 }}>
          <div className="rotulo-e-icone" style={{ color: correcao.nota >= 70 ? "var(--success)" : "var(--danger)" }}>
            {correcao.nota >= 70 ? <IconCheckCircle size={20} /> : <IconXCircle size={20} />}
            <strong>Nota {correcao.nota}</strong>
          </div>
          <p className="muted">
            {correcao.acertos.filter(Boolean).length} de {correcao.acertos.length} corretas.
          </p>
          {correcao.gaps.length > 0 && <p className="muted">Gaps identificados: {correcao.gaps.join(", ")}</p>}
        </div>
      )}
    </>
  );
}
