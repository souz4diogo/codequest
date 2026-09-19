import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import Carregando from "../components/Carregando.jsx";
import EstadoVazio from "../components/EstadoVazio.jsx";
import {
  IconClipboardCheck,
  IconTrophy,
  IconActivity,
  IconXCircle,
  IconStar,
  IconInbox,
} from "../components/icons.jsx";

// Nível 0–100 por tópico já existe (Arvore/RN07); classificação puramente visual do relatório.
function classificar(nivel) {
  if (nivel >= 80) return { texto: "Dominado", badge: "badge-success" };
  if (nivel >= 40) return { texto: "Em progresso", badge: "badge-info" };
  return { texto: "Fraco", badge: "badge-danger" };
}

// Relatório de conhecimento: agrega o que já existe (árvore + dúvidas marcadas) numa visão só,
// sem endpoint novo — nível por tópico, pontos fracos e dúvidas pendentes de revisão.
export default function Relatorio() {
  const [modulos, setModulos] = useState(null);
  const [duvidas, setDuvidas] = useState(null);
  const [erro, setErro] = useState(null);

  useEffect(() => {
    Promise.all([api.arvore(), api.duvidas()])
      .then(([m, d]) => {
        setModulos(m);
        setDuvidas(d);
      })
      .catch((err) => setErro(err.message));
  }, []);

  if (erro)
    return (
      <div className="page">
        <p className="erro">{erro}</p>
      </div>
    );
  if (!modulos || !duvidas)
    return (
      <div className="page">
        <Carregando />
      </div>
    );

  const topicos = modulos.flatMap((m) => m.topicos.map((t) => ({ ...t, moduloNome: m.nome })));
  const mediaGeral = topicos.length > 0 ? Math.round(topicos.reduce((s, t) => s + t.nivelEstimado, 0) / topicos.length) : 0;
  const fracos = topicos.filter((t) => t.nivelEstimado < 40).sort((a, b) => a.nivelEstimado - b.nivelEstimado);
  const dominados = topicos.filter((t) => t.nivelEstimado >= 80).length;
  const paraRevisar = duvidas.filter((d) => d.marcadaParaRevisao);

  return (
    <div className="page">
      <div className="page-header">
        <h1>Relatório de conhecimento</h1>
        <p>Onde você está forte, onde está fraco, e o que ainda precisa revisar.</p>
      </div>

      <div className="grid grid-4">
        <Stat rotulo="Tópicos mapeados" valor={topicos.length} Icon={IconActivity} cor="var(--info)" />
        <Stat rotulo="Nível médio" valor={`${mediaGeral}/100`} Icon={IconTrophy} cor="var(--gold)" />
        <Stat rotulo="Dominados (≥80)" valor={dominados} Icon={IconStar} cor="var(--success)" />
        <Stat rotulo="Fracos (<40)" valor={fracos.length} Icon={IconXCircle} cor="var(--danger)" />
      </div>

      <div className="section-title">
        <IconXCircle size={16} /> Pontos fracos — priorize aqui
      </div>
      {fracos.length === 0 ? (
        <EstadoVazio icon={IconInbox}>
          <p>Nenhum tópico abaixo de 40/100.</p>
        </EstadoVazio>
      ) : (
        <div className="card" style={{ display: "flex", flexDirection: "column", gap: 10 }}>
          {fracos.map((t) => (
            <LinhaTopico key={t.id} topico={t} />
          ))}
        </div>
      )}

      <div className="section-title">
        <IconClipboardCheck size={16} /> Nível por módulo
      </div>
      <div className="grid grid-2">
        {modulos
          .filter((m) => m.topicos.length > 0)
          .map((m) => (
            <div className="card" key={m.id}>
              <strong style={{ fontSize: 14 }}>{m.nome}</strong>
              <div style={{ display: "flex", flexDirection: "column", gap: 10, marginTop: 10 }}>
                {m.topicos.map((t) => (
                  <LinhaTopico key={t.id} topico={t} escondeModulo />
                ))}
              </div>
            </div>
          ))}
      </div>

      <div className="section-title">
        <IconStar size={16} /> Dúvidas marcadas para revisão
      </div>
      {paraRevisar.length === 0 ? (
        <EstadoVazio icon={IconInbox}>
          <p>Nenhuma dúvida marcada.</p>
        </EstadoVazio>
      ) : (
        <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
          {paraRevisar.map((d) => (
            <div className="card card-tight" key={d.id} style={{ display: "flex", justifyContent: "space-between", gap: 12 }}>
              <span>{d.pergunta}</span>
              {d.topicoNome && <span className="badge badge-primary">{d.topicoNome}</span>}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

function LinhaTopico({ topico, escondeModulo }) {
  const status = classificar(topico.nivelEstimado);
  return (
    <div style={{ display: "flex", flexDirection: "column", gap: 4 }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", gap: 10 }}>
        <span style={{ fontSize: 13.5 }}>
          {topico.nome}
          {!escondeModulo && <span className="muted"> — {topico.moduloNome}</span>}
        </span>
        <span className={`badge ${status.badge}`} style={{ flexShrink: 0 }}>
          {status.texto} · {topico.nivelEstimado}/100
        </span>
      </div>
      <div className="progress-bar nivel">
        <span className="fill" style={{ width: `${topico.nivelEstimado}%` }} />
      </div>
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
