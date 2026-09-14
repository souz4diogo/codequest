import { useState } from "react";
import Markdown from "react-markdown";

// Lista de questões de múltipla escolha (teste de tópico ou boss fight): coleta um índice de
// resposta por questão e só habilita enviar quando todas estiverem respondidas.
export default function BateriaQuestoes({ questoes, onEnviar, enviando }) {
  const [respostas, setRespostas] = useState(() => Array(questoes.length).fill(null));

  const completo = respostas.every((r) => r !== null);

  function escolher(indiceQuestao, indiceAlternativa) {
    setRespostas((r) => r.map((v, i) => (i === indiceQuestao ? indiceAlternativa : v)));
  }

  return (
    <div>
      {questoes.map((q, qi) => (
        <div className="card" key={qi} style={{ marginBottom: 12 }}>
          <strong>{qi + 1}.</strong>
          <div className="markdown">
            <Markdown>{q.enunciado}</Markdown>
          </div>
          <div className="grid" style={{ marginTop: 10 }}>
            {q.alternativas.map((texto, ai) => {
              const selecionada = respostas[qi] === ai;
              return (
                <button
                  type="button"
                  key={ai}
                  onClick={() => escolher(qi, ai)}
                  style={{
                    display: "flex",
                    gap: 10,
                    alignItems: "center",
                    textAlign: "left",
                    border: selecionada ? "1px solid var(--info)" : "1px solid var(--border)",
                    borderRadius: "var(--radius)",
                    padding: "10px 14px",
                    background: "var(--bg-surface)",
                    color: "var(--text-primary)",
                  }}
                >
                  <span
                    style={{
                      display: "inline-block",
                      width: 22,
                      textAlign: "center",
                      fontWeight: 700,
                      color: selecionada ? "var(--info)" : "var(--text-muted)",
                    }}
                  >
                    {String.fromCharCode(65 + ai)}
                  </span>
                  <span>{texto}</span>
                </button>
              );
            })}
          </div>
        </div>
      ))}
      <button
        className="btn btn-primary"
        disabled={!completo || enviando}
        onClick={() => onEnviar(respostas)}
      >
        {enviando ? "Enviando…" : "Enviar respostas"}
      </button>
    </div>
  );
}
