import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import AtividadeChart from "../components/AtividadeChart.jsx";
import RadarChart from "../components/RadarChart.jsx";
import Carregando from "../components/Carregando.jsx";
import { IconZap, IconCoins, IconFlame, IconShield, IconTrophy, IconActivity, IconRadar } from "../components/icons.jsx";

// Porta do Dashboard.razor: cards de nível/XP/gold/streak, barra de XP, gráfico de atividade
// (RF24) e radar de habilidades por módulo (RF08).
export default function Dashboard() {
  const [player, setPlayer] = useState(null);
  const [atividade, setAtividade] = useState(null);
  const [radar, setRadar] = useState(null);
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

  if (erro && !player)
    return (
      <div className="page">
        <p className="erro">{erro}</p>
      </div>
    );
  if (!player)
    return (
      <div className="page">
        <Carregando />
      </div>
    );

  const totalProximo = player.xpTotal + player.xpParaProximoNivel;
  const pct = totalProximo > 0 ? Math.round((player.xpTotal / totalProximo) * 100) : 100;

  return (
    <div className="page">
      <section className="card hero-card">
        <span className="avatar avatar-lg">{player.nome.slice(0, 2).toUpperCase()}</span>
        <div style={{ flex: 1, minWidth: 220, position: "relative" }}>
          <div className="muted">Bem-vindo de volta</div>
          <h1 className="display" style={{ fontSize: 26, color: "var(--gold)" }}>
            {player.nome} — Nível {player.nivel}
          </h1>
          <div className="progress-bar xp" style={{ marginTop: 12 }}>
            <span className="fill" style={{ width: `${pct}%` }} />
          </div>
          <div className="muted" style={{ marginTop: 8 }}>
            {player.xpTotal} XP · faltam {player.xpParaProximoNivel} pro nível {player.nivel + 1}
          </div>
        </div>
      </section>

      <div className="grid grid-4">
        <Stat rotulo="Nível" valor={player.nivel} Icon={IconTrophy} cor="var(--info)" />
        <Stat rotulo="XP" valor={player.xpTotal} Icon={IconZap} cor="var(--primary)" />
        <Stat rotulo="Gold" valor={player.gold} Icon={IconCoins} cor="var(--gold)" />
        <Stat rotulo="Streak" valor={`${player.streakDias} dias`} Icon={IconFlame} cor="var(--streak)" />
      </div>

      {player.pocaoStreakAtiva && (
        <p className="badge badge-info" style={{ alignSelf: "flex-start" }}>
          <IconShield /> Poção de streak ativa
        </p>
      )}

      <div className="grid grid-2">
        <div className="card">
          <div className="section-title" style={{ marginTop: 0 }}>
            <IconActivity size={16} /> Atividade (30 dias)
          </div>
          {atividade ? <AtividadeChart dias={atividade} /> : <Carregando />}
        </div>
        <div className="card" style={{ display: "flex", flexDirection: "column", alignItems: "center" }}>
          <div className="section-title" style={{ marginTop: 0, alignSelf: "flex-start" }}>
            <IconRadar size={16} /> Radar de habilidades
          </div>
          {radar ? <RadarChart eixos={radar} /> : <Carregando />}
        </div>
      </div>

      {erro && <p className="erro">{erro}</p>}
    </div>
  );
}

function Stat({ rotulo, valor, Icon, cor }) {
  return (
    <div className="card stat-card">
      <span className="icon-wrap" style={{ background: `color-mix(in srgb, ${cor} 18%, transparent)`, color: cor }}>
        <Icon size={18} />
      </span>
      <span className="muted">{rotulo}</span>
      <strong style={{ color: cor }}>{valor}</strong>
    </div>
  );
}
