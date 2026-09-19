import { IconLoader } from "./icons.jsx";

// Indicador de carregamento padrão das telas — evita repetir o mesmo <p> em cada página.
export default function Carregando({ texto = "Carregando…", tamanho = 16 }) {
  return (
    <p className="loading-row">
      <IconLoader size={tamanho} /> {texto}
    </p>
  );
}
