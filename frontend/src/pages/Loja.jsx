import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import Nav from "../components/Nav.jsx";

// Loja (RF20): lista os itens ativos e permite comprar. O gold é autoridade do backend (RN09) —
// aqui só mostramos o saldo devolvido pela compra e desabilitamos o que o jogador não pode pagar.
export default function Loja() {
  const [itens, setItens] = useState(null);
  const [player, setPlayer] = useState(null);
  const [erro, setErro] = useState(null);
  const [feedback, setFeedback] = useState(null);
  const [comprando, setComprando] = useState(null);

  async function carregar() {
    try {
      const [lista, p] = await Promise.all([api.itensLoja(), api.obterPlayer()]);
      setItens(lista);
      setPlayer(p);
    } catch (err) {
      setErro(err.message);
    }
  }

  useEffect(() => {
    carregar();
  }, []);

  async function comprar(item) {
    setErro(null);
    setFeedback(null);
    setComprando(item.id);
    try {
      const compra = await api.comprarItem(item.id);
      setPlayer((p) => ({ ...p, gold: compra.goldRestante }));
      setFeedback(`Comprou "${item.nome}" por ${compra.custoGoldPago} gold. Saldo: ${compra.goldRestante}.`);
    } catch (err) {
      setErro(err.message);
    } finally {
      setComprando(null);
    }
  }

  return (
    <div className="content">
      <Nav />

      <section className="card hero">
        <div style={{ flex: 1 }}>
          <div className="muted">Seu saldo</div>
          <div className="display" style={{ fontSize: 28, color: "var(--gold)" }}>
            {player ? `${player.gold} gold` : "…"}
          </div>
        </div>
      </section>

      {feedback && <p className="feedback">{feedback}</p>}
      {erro && <p className="erro">{erro}</p>}

      <div className="section-title">Itens disponíveis</div>
      {!itens ? (
        <p className="muted">Carregando…</p>
      ) : itens.length === 0 ? (
        <p className="muted">Nenhum item à venda no momento.</p>
      ) : (
        <div className="grid grid-4">
          {itens.map((item) => {
            const semGold = player ? player.gold < item.custoGold : true;
            return (
              <div className="card stat" key={item.id}>
                <strong>{item.nome}</strong>
                <span className="muted" style={{ color: "var(--gold)" }}>
                  {item.custoGold} gold
                </span>
                <button
                  className="btn btn-primary btn-block"
                  disabled={semGold || comprando === item.id}
                  onClick={() => comprar(item)}
                  style={{ marginTop: 8 }}
                >
                  {comprando === item.id ? "Comprando…" : semGold ? "Gold insuficiente" : "Comprar"}
                </button>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
