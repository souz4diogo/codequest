import { useEffect, useRef, useState } from "react";
import { api } from "../api/client.js";
import Carregando from "../components/Carregando.jsx";
import { IconTimer, IconCheckCircle, IconXCircle, IconZap } from "../components/icons.jsx";

const DURACOES = [25, 50];
const RAIO = 96;
const CIRCUNFERENCIA = 2 * Math.PI * RAIO;

// Timer Pomodoro (RF19): XP só é creditado se o timer completar — desistir não rende nada.
export default function Foco() {
  const [topicos, setTopicos] = useState(null);
  const [erro, setErro] = useState(null);

  useEffect(() => {
    api.topicosDisponiveis().then(setTopicos).catch((err) => setErro(err.message));
  }, []);

  return (
    <div className="page">
      <div className="page-header">
        <h1>Foco</h1>
        <p>Sessão Pomodoro — só rende XP se o timer completar.</p>
      </div>
      {erro && !topicos && <p className="erro">{erro}</p>}
      {!topicos ? (
        <Carregando />
      ) : (
        <Pomodoro topicos={topicos} />
      )}
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
    const totalSegundos = duracao * 60;
    const mm = String(Math.floor(segundosRestantes / 60)).padStart(2, "0");
    const ss = String(segundosRestantes % 60).padStart(2, "0");
    const pct = segundosRestantes / totalSegundos;
    const offset = CIRCUNFERENCIA * (1 - pct);

    return (
      <div className="card timer-shell">
        <div className="timer-ring">
          <svg width={220} height={220} viewBox="0 0 220 220">
            <circle cx="110" cy="110" r={RAIO} fill="none" stroke="var(--bg-base-alt)" strokeWidth={12} />
            <circle
              cx="110"
              cy="110"
              r={RAIO}
              fill="none"
              stroke="var(--streak)"
              strokeWidth={12}
              strokeLinecap="round"
              strokeDasharray={CIRCUNFERENCIA}
              strokeDashoffset={offset}
              style={{ transition: "stroke-dashoffset 900ms linear" }}
            />
          </svg>
          <span className="valor mono">
            {mm}:{ss}
          </span>
        </div>
        <p className="muted">{topicos.find((t) => t.id === topicoId)?.nome}</p>
        <button className="btn btn-ghost" onClick={desistir}>
          Desistir
        </button>
      </div>
    );
  }

  return (
    <>
      <div className="card">
        <div className="form-row">
          <select className="campo" value={topicoId ?? ""} onChange={(e) => setTopicoId(Number(e.target.value))}>
            {topicos.map((t) => (
              <option key={t.id} value={t.id}>
                {t.moduloNome} · {t.nome}
              </option>
            ))}
          </select>
          <div className="duracao-toggle" style={{ flex: "0 0 auto" }}>
            {DURACOES.map((d) => (
              <button key={d} type="button" className={duracao === d ? "ativo" : ""} onClick={() => setDuracao(d)}>
                {d} min
              </button>
            ))}
          </div>
          <button className="btn btn-primary" onClick={iniciar} disabled={!topicoId} style={{ flex: "0 0 auto" }}>
            <IconTimer size={16} /> Iniciar
          </button>
        </div>
      </div>

      {erro && <p className="erro">{erro}</p>}

      {resultado && (
        <div className="card" style={{ display: "flex", flexDirection: "column", gap: 8 }}>
          <div className="rotulo-e-icone" style={{ color: resultado.completou ? "var(--success)" : "var(--danger)" }}>
            {resultado.completou ? <IconCheckCircle size={20} /> : <IconXCircle size={20} />}
            <strong>{resultado.completou ? "Sessão completa!" : "Sessão interrompida"}</strong>
          </div>
          <p className="muted">
            {resultado.minutosReais} de {resultado.minutosPlanejados} min.
          </p>
          {resultado.completou ? (
            <p className="feedback">
              <IconZap /> +{resultado.xpGanho} XP.
            </p>
          ) : (
            <p className="muted">Sem XP — o timer não completou.</p>
          )}
        </div>
      )}
    </>
  );
}
