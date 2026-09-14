import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import Nav from "../components/Nav.jsx";
import BateriaQuestoes from "../components/BateriaQuestoes.jsx";
import GestaoTopicos from "../components/GestaoTopicos.jsx";

const CORES_STATUS = {
  Liberado: "var(--info)",
  Concluido: "var(--success)",
  Bloqueado: "var(--text-muted)",
};

const ROTULOS_STATUS = {
  Bloqueado: "🔒 Bloqueado",
  Liberado: "▶ Liberado",
  Concluido: "✅ Concluído",
};

// Espelha PoliticaArvore.NivelMinimoTopico (backend, RN08): só mostra o boss quando ele
// realmente está liberado — evita clicar e levar um 400 do servidor.
const NIVEL_MINIMO_BOSS = 60;

function bossLiberado(modulo) {
  return modulo.topicos.length > 0 && modulo.topicos.every((t) => t.nivelEstimado >= NIVEL_MINIMO_BOSS);
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
      <div className="content">
        <Nav />
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
    <div className="content">
      <Nav />
      <div className="section-title" style={{ marginTop: 0 }}>
        Árvore de habilidades
      </div>
      {erro && <p className="erro">{erro}</p>}
      {!modulos ? (
        <p className="muted">Carregando…</p>
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
  const requeridos = modulo.requerModuloIds
    .map((id) => modulos.find((m) => m.id === id)?.nome)
    .filter(Boolean);

  return (
    <div className="card stat" style={{ opacity: bloqueado ? 0.6 : 1 }}>
      <strong>{modulo.nome}</strong>
      <span className="muted" style={{ color: CORES_STATUS[modulo.status] }}>
        {ROTULOS_STATUS[modulo.status] ?? modulo.status}
      </span>

      {modulo.topicos.length > 0 && (
        <ul className="muted" style={{ marginTop: 8, paddingLeft: 18 }}>
          {modulo.topicos.map((t) => (
            <li key={t.id}>
              {t.nome} — {t.nivelEstimado}/100
            </li>
          ))}
        </ul>
      )}

      {bloqueado && requeridos.length > 0 && (
        <p className="muted" style={{ marginTop: 8, fontSize: 12 }}>
          Requer: {requeridos.join(", ")}
        </p>
      )}

      {modulo.status === "Liberado" && bossLiberado(modulo) && (
        <button className="btn btn-primary btn-block" style={{ marginTop: 10 }} onClick={onEnfrentarBoss}>
          ⚔ Enfrentar o Boss
        </button>
      )}
      {modulo.status === "Liberado" && !bossLiberado(modulo) && (
        <p className="muted" style={{ marginTop: 8, fontSize: 12 }}>
          Boss libera com todos os tópicos ≥ {NIVEL_MINIMO_BOSS}/100.
        </p>
      )}

      <button className="btn btn-ghost btn-block" style={{ marginTop: 8 }} onClick={() => setGerenciando((v) => !v)}>
        {gerenciando ? "Fechar gestão de tópicos" : "🛠 Gerenciar tópicos"}
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
    <div>
      <div className="section-title" style={{ marginTop: 0 }}>
        Boss fight · {modulo.nome}
      </div>

      {erro && (
        <>
          <p className="erro">{erro}</p>
          <button className="btn btn-ghost" onClick={onSair}>
            Voltar
          </button>
        </>
      )}

      {gerando && !erro && <p className="muted">Convocando o boss…</p>}

      {teste && !resultado && (
        <BateriaQuestoes questoes={teste.questoes} onEnviar={enviar} enviando={enviando} />
      )}

      {resultado && (
        <div className="card">
          <strong style={{ color: resultado.aprovado ? "var(--success)" : "var(--medium)" }}>
            {resultado.aprovado ? "Boss derrotado!" : "Não foi dessa vez"} · Nota {resultado.nota}
          </strong>
          <p className="muted">
            {resultado.acertos.filter(Boolean).length} de {resultado.acertos.length} corretas.
          </p>
          {resultado.aprovado && resultado.modulosLiberados.length > 0 && (
            <p className="feedback">Módulos liberados: {resultado.modulosLiberados.join(", ")}</p>
          )}
          <button className="btn btn-primary" style={{ marginTop: 8 }} onClick={onSair}>
            Voltar à árvore
          </button>
        </div>
      )}
    </div>
  );
}
