import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import Nav from "../components/Nav.jsx";
import AtividadeChart from "../components/AtividadeChart.jsx";
import RadarChart from "../components/RadarChart.jsx";

// Porta do Dashboard.razor: cards de nível/XP/gold/streak, barra de XP, gráfico de atividade
// (RF24) e radar de habilidades por módulo (RF08).
export default function Dashboard() {
  const [player, setPlayer] = useState(null);
  const [atividade, setAtividade] = useState(null);
  const [radar, setRadar] = useState(null);
  const [ultimo, setUltimo] = useState(null);
  const [erro, setErro] = useState(null);

  async function carregar() {
    try {
      const [p, a, r] = await Promise.all([api.obterPlayer(), api.atividade(), api.radar()]);
      setPlayer(p);
      setAtividade(a);
      setRadar(r);
    } catch (err) {
      setErro(err.message);
    }
  }

  useEffect(() => {
    carregar();
  }, []);

  async function adicionarXp(xp) {
    setErro(null);
    try {
      setUltimo(await api.adicionarXp(xp));
      await carregar();
    } catch (err) {
      setErro(err.message);
    }
  }

  if (erro && !player)
    return (
      <div className="content">
        <p className="erro">{erro}</p>
      </div>
    );
  if (!player)
    return (
      <div className="content">
        <p className="muted">Carregando…</p>
      </div>
    );

  const totalProximo = player.xpTotal + player.xpParaProximoNivel;
  const pct = totalProximo > 0 ? Math.round((player.xpTotal / totalProximo) * 100) : 100;

  return (
    <div className="content">
      <Nav />

      <section className="card hero">
        <span className="avatar avatar-lg">{player.nome.slice(0, 2).toUpperCase()}</span>
        <div style={{ flex: 1, minWidth: 220 }}>
          <div className="muted">Bem-vindo de volta</div>
          <div className="display" style={{ fontSize: 24, color: "var(--gold)" }}>
            {player.nome} — Nível {player.nivel}
          </div>
          <div className="xp-bar">
            <span className="fill" style={{ width: `${pct}%` }} />
          </div>
          <div className="muted" style={{ marginTop: 6 }}>
            {player.xpTotal} XP · faltam {player.xpParaProximoNivel} pro nível {player.nivel + 1}
          </div>
        </div>
      </section>

      <div className="grid grid-4">
        <Stat rotulo="Nível" valor={player.nivel} />
        <Stat rotulo="XP" valor={player.xpTotal} />
        <Stat rotulo="Gold" valor={player.gold} cor="var(--gold)" />
        <Stat rotulo="Streak" valor={`${player.streakDias} 🔥`} cor="var(--focus-timer)" />
      </div>

      {player.pocaoStreakAtiva && <p className="muted">🛡️ Poção de streak ativa</p>}

      <div className="section-title">Testar recompensa</div>
      <div className="acoes">
        <button className="btn btn-ghost" onClick={() => adicionarXp(25)}>
          +25 XP
        </button>
        <button className="btn btn-ghost" onClick={() => adicionarXp(60)}>
          +60 XP
        </button>
      </div>

      {ultimo && (
        <p className="feedback">
          +{ultimo.xpCreditado} XP (base {ultimo.xpBase} × {ultimo.multiplicador.toFixed(1)}), +
          {ultimo.goldGanho} gold.
          {ultimo.subiuNivel && <strong> Subiu para o nível {ultimo.nivelAtual}! 🎉</strong>}
        </p>
      )}

      <div className="grid grid-2">
        <div className="card">
          <div className="section-title" style={{ marginTop: 0 }}>
            Atividade (30 dias)
          </div>
          {atividade ? <AtividadeChart dias={atividade} /> : <p className="muted">Carregando…</p>}
        </div>
        <div className="card" style={{ display: "flex", flexDirection: "column", alignItems: "center" }}>
          <div className="section-title" style={{ marginTop: 0, alignSelf: "flex-start" }}>
            Radar de habilidades
          </div>
          {radar ? <RadarChart eixos={radar} /> : <p className="muted">Carregando…</p>}
        </div>
      </div>

      {erro && <p className="erro">{erro}</p>}
    </div>
  );
}

function Stat({ rotulo, valor, cor }) {
  return (
    <div className="card stat">
      <span className="muted">{rotulo}</span>
      <strong style={{ color: cor }}>{valor}</strong>
    </div>
  );
}
