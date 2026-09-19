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
    <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
      {questoes.map((q, qi) => (
        <div className="card" key={qi}>
          <div className="rotulo-e-icone muted" style={{ marginBottom: 8 }}>
            <span className="badge-letra">{qi + 1}</span>
          </div>
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
                  className={`opt${selecionada ? " selecionada" : ""}`}
                  onClick={() => escolher(qi, ai)}
                >
                  <span className="badge-letra">{String.fromCharCode(65 + ai)}</span>
                  <span>{texto}</span>
                </button>
              );
            })}
          </div>
        </div>
      ))}
      <button className="btn btn-primary" disabled={!completo || enviando} onClick={() => onEnviar(respostas)}>
        {enviando ? "Enviando…" : "Enviar respostas"}
      </button>
    </div>
  );
}
