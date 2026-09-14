import { useEffect, useState } from "react";
import { api } from "../api/client.js";

// Gestão de tópicos de um módulo (RF05): criar, editar nome/prioridade e arquivar/desarquivar.
export default function GestaoTopicos({ moduloId }) {
  const [topicos, setTopicos] = useState(null);
  const [erro, setErro] = useState(null);

  async function carregar() {
    try {
      setTopicos(await api.listarTopicosModulo(moduloId));
    } catch (err) {
      setErro(err.message);
    }
  }

  useEffect(() => {
    carregar();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [moduloId]);

  if (erro) return <p className="erro">{erro}</p>;
  if (!topicos) return <p className="muted">Carregando…</p>;

  return (
    <div>
      {topicos.map((t) => (
        <LinhaTopico key={t.id} moduloId={moduloId} topico={t} onMudou={carregar} />
      ))}
      <NovoTopico moduloId={moduloId} onCriado={carregar} onErro={setErro} />
    </div>
  );
}

function LinhaTopico({ moduloId, topico, onMudou }) {
  const [editando, setEditando] = useState(false);
  const [nome, setNome] = useState(topico.nome);
  const [prioridade, setPrioridade] = useState(topico.prioridade);
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState(null);

  async function salvar() {
    setSalvando(true);
    setErro(null);
    try {
      await api.editarTopico(moduloId, topico.id, nome.trim(), Number(prioridade));
      setEditando(false);
      onMudou();
    } catch (err) {
      setErro(err.message);
    } finally {
      setSalvando(false);
    }
  }

  async function alternarArquivado() {
    setErro(null);
    try {
      await api.arquivarTopico(moduloId, topico.id, !topico.arquivado);
      onMudou();
    } catch (err) {
      setErro(err.message);
    }
  }

  return (
    <div
      style={{
        display: "flex",
        alignItems: "center",
        gap: 8,
        padding: "8px 0",
        borderBottom: "1px solid var(--border)",
        opacity: topico.arquivado ? 0.5 : 1,
      }}
    >
      {editando ? (
        <>
          <input className="campo" value={nome} onChange={(e) => setNome(e.target.value)} style={{ flex: 1 }} />
          <select className="campo" value={prioridade} onChange={(e) => setPrioridade(e.target.value)}>
            <option value={1}>Alta</option>
            <option value={2}>Normal</option>
            <option value={3}>Baixa</option>
          </select>
          <button className="btn btn-primary" onClick={salvar} disabled={salvando || !nome.trim()}>
            Salvar
          </button>
          <button className="btn btn-ghost" onClick={() => setEditando(false)}>
            Cancelar
          </button>
        </>
      ) : (
        <>
          <span style={{ flex: 1 }}>
            {topico.nome} <span className="muted">— nível {topico.nivelEstimado}</span>
          </span>
          <button className="btn btn-ghost" onClick={() => setEditando(true)}>
            Editar
          </button>
          <button className="btn btn-ghost" onClick={alternarArquivado}>
            {topico.arquivado ? "Desarquivar" : "Arquivar"}
          </button>
        </>
      )}
      {erro && <span className="erro">{erro}</span>}
    </div>
  );
}

function NovoTopico({ moduloId, onCriado, onErro }) {
  const [nome, setNome] = useState("");
  const [prioridade, setPrioridade] = useState(2);
  const [criando, setCriando] = useState(false);

  async function criar() {
    if (!nome.trim()) return;
    setCriando(true);
    onErro(null);
    try {
      await api.criarTopico(moduloId, nome.trim(), Number(prioridade));
      setNome("");
      onCriado();
    } catch (err) {
      onErro(err.message);
    } finally {
      setCriando(false);
    }
  }

  return (
    <div style={{ display: "flex", gap: 8, marginTop: 10 }}>
      <input
        className="campo"
        placeholder="Novo tópico"
        value={nome}
        onChange={(e) => setNome(e.target.value)}
        style={{ flex: 1 }}
      />
      <select className="campo" value={prioridade} onChange={(e) => setPrioridade(e.target.value)}>
        <option value={1}>Alta</option>
        <option value={2}>Normal</option>
        <option value={3}>Baixa</option>
      </select>
      <button className="btn btn-primary" onClick={criar} disabled={criando || !nome.trim()}>
        Adicionar
      </button>
    </div>
  );
}
