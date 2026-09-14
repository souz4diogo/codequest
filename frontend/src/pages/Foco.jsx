import { useEffect, useRef, useState } from "react";
import { api } from "../api/client.js";
import Nav from "../components/Nav.jsx";

const DURACOES = [25, 50];

// Timer Pomodoro (RF19): XP só é creditado se o timer completar — desistir não rende nada.
export default function Foco() {
  const [topicos, setTopicos] = useState(null);
  const [erro, setErro] = useState(null);

  useEffect(() => {
    api.topicosDisponiveis().then(setTopicos).catch((err) => setErro(err.message));
  }, []);

  return (
    <div className="content">
      <Nav />
      <div className="section-title" style={{ marginTop: 0 }}>
        Foco
      </div>
      {erro && !topicos && <p className="erro">{erro}</p>}
      {!topicos ? <p className="muted">Carregando…</p> : <Pomodoro topicos={topicos} />}
    </div>
  );
}

function Pomodoro({ topicos }) {
  const [topicoId, setTopicoId] = useState(topicos[0]?.id ?? null);
  const [duracao, setDuracao] = useState(DURACOES[0]);
  const [segundosRestantes, setSegundosRestantes] = useState(null);
  const [rodando, setRodando] = useState(false);
  const [resultado, setResultado] = useState(null);
  const [erro, setErro] = useState(null);
  const inicioRef = useRef(null);

  useEffect(() => {
    if (!rodando) return;
    const id = setInterval(() => {
      setSegundosRestantes((s) => {
        if (s <= 1) {
          clearInterval(id);
          finalizar(duracao);
          return 0;
        }
        return s - 1;
      });
    }, 1000);
    return () => clearInterval(id);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [rodando]);

  function iniciar() {
    setErro(null);
    setResultado(null);
    inicioRef.current = Date.now();
    setSegundosRestantes(duracao * 60);
    setRodando(true);
  }

  async function finalizar(minutosReais) {
    setRodando(false);
    try {
      const sessao = await api.registrarFoco(topicoId, duracao, minutosReais);
      setResultado(sessao);
    } catch (err) {
      setErro(err.message);
    }
  }

  function desistir() {
    const minutosCorridos = Math.floor((Date.now() - inicioRef.current) / 60000);
    setRodando(false);
    finalizar(minutosCorridos);
  }

  if (rodando) {
    const mm = String(Math.floor(segundosRestantes / 60)).padStart(2, "0");
    const ss = String(segundosRestantes % 60).padStart(2, "0");
    return (
      <div className="card" style={{ textAlign: "center", padding: 48 }}>
        <div className="display" style={{ fontSize: 72 }}>
          {mm}:{ss}
        </div>
        <p className="muted" style={{ marginTop: 8 }}>
          {topicos.find((t) => t.id === topicoId)?.nome}
        </p>
        <button className="btn btn-ghost" style={{ marginTop: 24 }} onClick={desistir}>
          Desistir
        </button>
      </div>
    );
  }

  return (
    <>
      <div className="card">
        <div className="acoes" style={{ flexWrap: "wrap" }}>
          <select className="campo" value={topicoId ?? ""} onChange={(e) => setTopicoId(Number(e.target.value))}>
            {topicos.map((t) => (
              <option key={t.id} value={t.id}>
                {t.moduloNome} · {t.nome}
              </option>
            ))}
          </select>
          <div style={{ display: "flex", gap: 8 }}>
            {DURACOES.map((d) => (
              <button
                key={d}
                type="button"
                className="campo"
                onClick={() => setDuracao(d)}
                style={{
                  cursor: "pointer",
                  border: duracao === d ? "1px solid var(--info)" : undefined,
                  color: duracao === d ? "var(--info)" : undefined,
                }}
              >
                {d} min
              </button>
            ))}
          </div>
          <button className="btn btn-primary" onClick={iniciar} disabled={!topicoId}>
            Iniciar
          </button>
        </div>
      </div>

      {erro && <p className="erro">{erro}</p>}

      {resultado && (
        <div className="card" style={{ marginTop: 16 }}>
          <strong style={{ color: resultado.completou ? "var(--success)" : "var(--medium)" }}>
            {resultado.completou ? "Sessão completa!" : "Sessão interrompida"}
          </strong>
          <p className="muted">
            {resultado.minutosReais} de {resultado.minutosPlanejados} min.
            {resultado.completou ? ` +${resultado.xpGanho} XP.` : " Sem XP — o timer não completou."}
          </p>
        </div>
      )}
    </>
  );
}

