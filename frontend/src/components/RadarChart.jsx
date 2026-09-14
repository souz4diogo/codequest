// Radar de habilidades por módulo (RF08): um eixo por módulo, um único sujeito (o jogador) —
// uma cor sólida, sem legenda. Rótulos e anéis usam tokens de texto/borda, nunca a cor da série.
const TAMANHO = 220;
const CENTRO = TAMANHO / 2;
const RAIO = TAMANHO / 2 - 36;
const ANEIS = [25, 50, 75, 100];

export default function RadarChart({ eixos }) {
  if (eixos.length < 3) {
    return <p className="muted">Libere pelo menos 3 módulos para ver o radar.</p>;
  }

  const angulo = (i) => (Math.PI * 2 * i) / eixos.length - Math.PI / 2;
  const ponto = (i, valor) => {
    const r = (valor / 100) * RAIO;
    return [CENTRO + r * Math.cos(angulo(i)), CENTRO + r * Math.sin(angulo(i))];
  };

  const pontosSerie = eixos.map((e, i) => ponto(i, e.nivelMedio));
  const pathSerie = pontosSerie.map((p) => p.join(",")).join(" ");

  return (
    <svg
      viewBox={`0 0 ${TAMANHO} ${TAMANHO}`}
      width={TAMANHO}
      height={TAMANHO}
      role="img"
      aria-label="Radar de habilidades por módulo"
    >
      {ANEIS.map((nivel) => (
        <polygon
          key={nivel}
          points={eixos.map((_, i) => ponto(i, nivel).join(",")).join(" ")}
          fill="none"
          stroke="var(--border)"
          strokeWidth={1}
        />
      ))}

      {eixos.map((_, i) => {
        const [x, y] = ponto(i, 100);
        return <line key={i} x1={CENTRO} y1={CENTRO} x2={x} y2={y} stroke="var(--border)" strokeWidth={1} />;
      })}

      <polygon points={pathSerie} fill="color-mix(in srgb, var(--info) 30%, transparent)" stroke="var(--info)" strokeWidth={2} />
      {pontosSerie.map(([x, y], i) => (
        <circle key={i} cx={x} cy={y} r={3} fill="var(--info)" />
      ))}

      {eixos.map((e, i) => {
        const [x, y] = ponto(i, 118);
        return (
          <text
            key={e.modulo}
            x={x}
            y={y}
            fontSize={10}
            fill="var(--text-secondary)"
            textAnchor="middle"
            dominantBaseline="middle"
          >
            {e.modulo}
          </text>
        );
      })}
    </svg>
  );
}
