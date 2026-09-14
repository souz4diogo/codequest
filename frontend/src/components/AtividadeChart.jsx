// Heatmap de atividade dos últimos 30 dias (RF24): magnitude → uma única cor sequencial,
// mais escura quanto mais atividades concluídas no dia. Sem série/legenda: é um único sujeito.
export default function AtividadeChart({ dias }) {
  const max = Math.max(1, ...dias.map((d) => d.quantidade));

  return (
    <div style={{ display: "flex", gap: 4, flexWrap: "wrap" }} role="img" aria-label="Atividade dos últimos 30 dias">
      {dias.map((d) => {
        const intensidade = d.quantidade === 0 ? 0.08 : 0.25 + 0.75 * (d.quantidade / max);
        return (
          <div
            key={d.data}
            title={`${formatarData(d.data)}: ${d.quantidade} ${d.quantidade === 1 ? "atividade" : "atividades"}`}
            style={{
              width: 16,
              height: 16,
              borderRadius: 4,
              background: `color-mix(in srgb, var(--info) ${Math.round(intensidade * 100)}%, var(--bg-surface))`,
            }}
          />
        );
      })}
    </div>
  );
}

function formatarData(iso) {
  const [ano, mes, dia] = iso.split("-");
  return `${dia}/${mes}`;
}
