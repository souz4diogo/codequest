import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import Carregando from "../components/Carregando.jsx";
import EstadoVazio from "../components/EstadoVazio.jsx";
import { IconCoins, IconShoppingBag, IconLoader, IconInbox } from "../components/icons.jsx";

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
    <div className="page">
      <div className="page-header">
        <h1>Loja</h1>
        <p>Troque gold por descanso — sem culpa.</p>
      </div>

      <section className="card hero-card">
        <span className="icon-wrap" style={{ width: 56, height: 56, borderRadius: 16, background: "var(--gold-soft)", color: "var(--gold)" }}>
          <IconCoins size={26} />
        </span>
        <div style={{ flex: 1 }}>
          <div className="muted">Seu saldo</div>
          <div className="display" style={{ fontSize: 30, color: "var(--gold)" }}>
            {player ? `${player.gold} gold` : "…"}
          </div>
        </div>
      </section>

      {feedback && <p className="feedback">{feedback}</p>}
      {erro && <p className="erro">{erro}</p>}

      <div className="section-title">
        <IconShoppingBag size={16} /> Itens disponíveis
      </div>
      {!itens ? (
        <Carregando />
      ) : itens.length === 0 ? (
        <EstadoVazio icon={IconInbox}>
          <p>Nenhum item à venda no momento.</p>
        </EstadoVazio>
      ) : (
        <div className="grid grid-4">
          {itens.map((item) => {
            const semGold = player ? player.gold < item.custoGold : true;
            return (
              <div className="card stat-card" key={item.id}>
                <span className="icon-wrap" style={{ background: "var(--gold-soft)", color: "var(--gold)" }}>
                  <IconShoppingBag size={16} />
                </span>
                <strong style={{ fontSize: 15 }}>{item.nome}</strong>
                <span className="rotulo-e-icone" style={{ color: "var(--gold)", fontSize: 13, fontWeight: 600 }}>
                  <IconCoins size={14} /> {item.custoGold}
                </span>
                <button
                  className="btn btn-gold btn-block"
                  disabled={semGold || comprando === item.id}
                  onClick={() => comprar(item)}
                  style={{ marginTop: 8 }}
                >
                  {comprando === item.id && <IconLoader size={15} />}
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
