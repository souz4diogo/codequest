// Estado vazio padrão (lista sem itens ainda) — ícone + mensagem, reaproveitado em toda tela
// que lista algo que pode não existir ainda (missões, itens, dúvidas, tópicos...).
export default function EstadoVazio({ icon: Icon, children }) {
  return (
    <div className="card empty-state">
      <Icon size={32} />
      {children}
    </div>
  );
}
