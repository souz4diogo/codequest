import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import BateriaQuestoes from "../components/BateriaQuestoes.jsx";
import GestaoTopicos from "../components/GestaoTopicos.jsx";
import Carregando from "../components/Carregando.jsx";
import {
  IconLock,
  IconCheckCircle,
  IconChevronRight,
  IconSwords,
  IconTrophy,
  IconXCircle,
  IconTree,
  IconWalker,
} from "../components/icons.jsx";

const STATUS_INFO = {
  Bloqueado: { texto: "Bloqueado", Icon: IconLock, badge: "badge-neutro" },
  Liberado: { texto: "Liberado", Icon: IconChevronRight, badge: "badge-info" },
  Concluido: { texto: "Concluído", Icon: IconCheckCircle, badge: "badge-success" },
};

// Espelha PoliticaArvore.NivelMinimoTopico (backend, RN08): só mostra o boss quando ele
// realmente está liberado — evita clicar e levar um 400 do servidor.
const NIVEL_MINIMO_BOSS = 60;

function bossLiberado(modulo) {
  return modulo.topicos.length > 0 && modulo.topicos.every((t) => t.nivelEstimado >= NIVEL_MINIMO_BOSS);
}

// Progresso visual até o boss: o gargalo é sempre o tópico mais fraco, porque o boss só
// libera quando TODOS os tópicos passam de NIVEL_MINIMO_BOSS (mesma regra de bossLiberado).
function progressoAteBoss(modulo) {
  if (modulo.topicos.length === 0) return 0;
  const maisFraco = Math.min(...modulo.topicos.map((t) => t.nivelEstimado));
  return Math.min(100, Math.round((maisFraco / NIVEL_MINIMO_BOSS) * 100));
}

// Árvore de habilidades (RF06): módulos bloqueados/liberados conforme pré-requisitos, com
// boss fight (RF12/RN08) para concluir um módulo liberado e desbloquear os dependentes.
export default function Arvore() {
  const [modulos, setModulos] = useState(null);
  const [erro, setErro] = useState(null);
  const [bossModulo, setBossModulo] = useState(null);

  async function carregar() {
    try {
      setModulos(await api.arvore());
    } catch (err) {
      setErro(err.message);
    }
  }

  useEffect(() => {
    carregar();
  }, []);

  if (bossModulo) {
    return (
      <div className="page">
        <BossFight
          modulo={bossModulo}
          onSair={() => {
            setBossModulo(null);
            carregar();
          }}
        />
      </div>
    );
  }

  return (
    <div className="page">
      <div className="page-header">
        <h1>Árvore de habilidades</h1>
        <p>Cada módulo libera o próximo depois que o boss cair.</p>
      </div>
      {erro && <p className="erro">{erro}</p>}
      {!modulos ? (
        <Carregando />
      ) : (
        <div className="grid grid-4">
          {modulos.map((m) => (
            <CardModulo key={m.id} modulo={m} modulos={modulos} onEnfrentarBoss={() => setBossModulo(m)} />
          ))}
        </div>
      )}
    </div>
  );
}

function CardModulo({ modulo, modulos, onEnfrentarBoss }) {
  const [gerenciando, setGerenciando] = useState(false);
  const bloqueado = modulo.status === "Bloqueado";
  const status = STATUS_INFO[modulo.status] ?? STATUS_INFO.Bloqueado;
  const requeridos = modulo.requerModuloIds
    .map((id) => modulos.find((m) => m.id === id)?.nome)
    .filter(Boolean);

  return (
    <div className={`card${bloqueado ? " card-desligado" : ""}`} style={{ display: "flex", flexDirection: "column", gap: 8 }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", gap: 8 }}>
        <strong>{modulo.nome}</strong>
        <span className={`badge ${status.badge}`}>
          <status.Icon /> {status.texto}
        </span>
      </div>

      {modulo.topicos.length > 0 && (
        <ul style={{ display: "flex", flexDirection: "column", gap: 4, marginTop: 4 }}>
          {modulo.topicos.map((t) => (
            <li key={t.id} className="muted" style={{ display: "flex", justifyContent: "space-between" }}>
              <span>{t.nome}</span>
              <span className="mono">{t.nivelEstimado}/100</span>
            </li>
          ))}
        </ul>
      )}

      {bloqueado && requeridos.length > 0 && (
        <p className="muted" style={{ fontSize: 12 }}>
          Requer: {requeridos.join(", ")}
        </p>
      )}

      {modulo.status === "Liberado" && (
        <div className="boss-track-wrap">
          <div className="boss-track">
            <div className="trilha">
              <span className="percorrido" style={{ width: `${progressoAteBoss(modulo)}%` }} />
            </div>
            <span className="andarilho" style={{ left: `${progressoAteBoss(modulo)}%` }}>
              <IconWalker size={14} />
            </span>
            <span className="boss-alvo">
              <IconSwords size={14} />
            </span>
          </div>
          <span className="muted" style={{ fontSize: 12, textAlign: "center" }}>
            {bossLiberado(modulo) ? "Boss liberado!" : `${progressoAteBoss(modulo)}% do caminho até o boss`}
          </span>
        </div>
      )}
      {modulo.status === "Liberado" && bossLiberado(modulo) && (
        <button className="btn btn-danger btn-block" onClick={onEnfrentarBoss}>
          <IconSwords size={16} /> Enfrentar o Boss
        </button>
      )}

      <button className="btn btn-ghost btn-block btn-sm" style={{ marginTop: 4 }} onClick={() => setGerenciando((v) => !v)}>
        {gerenciando ? "Fechar gestão de tópicos" : "Gerenciar tópicos"}
      </button>
      {gerenciando && <GestaoTopicos moduloId={modulo.id} />}
    </div>
  );
}

function BossFight({ modulo, onSair }) {
  const [teste, setTeste] = useState(null);
  const [resultado, setResultado] = useState(null);
  const [gerando, setGerando] = useState(true);
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState(null);

  useEffect(() => {
    api
      .iniciarBoss(modulo.id)
      .then(setTeste)
      .catch((err) => setErro(err.message))
      .finally(() => setGerando(false));
  }, [modulo.id]);

  async function enviar(respostas) {
    setEnviando(true);
    setErro(null);
    try {
      setResultado(await api.responderBoss(teste.id, respostas));
    } catch (err) {
      setErro(err.message);
    } finally {
      setEnviando(false);
    }
  }

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: 16 }}>
      <div
        className="card"
        style={{
          display: "flex",
          alignItems: "center",
          gap: 14,
          background: "linear-gradient(160deg, #2a1533, var(--bg-elevated))",
          borderColor: "#f43f5e44",
        }}
      >
        <span className="icon-wrap" style={{ width: 48, height: 48, borderRadius: 14, background: "var(--danger-soft)", color: "var(--rose)" }}>
          <IconSwords size={24} />
        </span>
        <div>
          <div className="muted">Boss fight</div>
          <h1 className="display" style={{ fontSize: 22 }}>
            {modulo.nome}
          </h1>
        </div>
      </div>

      {erro && (
        <>
          <p className="erro">{erro}</p>
          <button className="btn btn-ghost" onClick={onSair} style={{ alignSelf: "flex-start" }}>
            Voltar
          </button>
        </>
      )}

      {gerando && !erro && <Carregando texto="Convocando o boss…" />}

      {teste && !resultado && <BateriaQuestoes questoes={teste.questoes} onEnviar={enviar} enviando={enviando} />}

      {resultado && (
        <div className="card" style={{ display: "flex", flexDirection: "column", gap: 8 }}>
          <div className="rotulo-e-icone" style={{ color: resultado.aprovado ? "var(--success)" : "var(--danger)" }}>
            {resultado.aprovado ? <IconTrophy size={20} /> : <IconXCircle size={20} />}
            <strong>{resultado.aprovado ? "Boss derrotado!" : "Não foi dessa vez"} · Nota {resultado.nota}</strong>
          </div>
          <p className="muted">
            {resultado.acertos.filter(Boolean).length} de {resultado.acertos.length} corretas.
          </p>
          {resultado.aprovado && resultado.modulosLiberados.length > 0 && (
            <p className="feedback">
              <IconTree /> Módulos liberados: {resultado.modulosLiberados.join(", ")}
            </p>
          )}
          <button className="btn btn-primary" style={{ alignSelf: "flex-start" }} onClick={onSair}>
            Voltar à árvore
          </button>
        </div>
      )}
    </div>
  );
}
