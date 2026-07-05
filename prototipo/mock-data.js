/* CodeQuest — dados mockados compartilhados (sem backend). Textos em pt-BR. */
window.CQ = (function () {
  const player = {
    nome: "Diogo",
    nivel: 12,
    titulo: "Aprendiz de LINQ",
    xpAtual: 1240,
    xpProximo: 2450,
    gold: 185,
    streak: 12,
    avatarIniciais: "DS",
  };

  // Títulos por nível para o level-up.
  const titulosPorNivel = {
    12: "Aprendiz de LINQ",
    13: "Caçador de Bugs",
    14: "Domador de Async",
  };

  const missoesHoje = [
    {
      id: 1, tipo: "Diária", dificuldade: "medium",
      titulo: "Refatorar 3 loops com LINQ",
      descricao: "Troque foreach por Select/Where em três trechos do seu projeto.",
      xp: 30, gold: 10, status: "pendente",
    },
    {
      id: 2, tipo: "Diária", dificuldade: "easy",
      titulo: "Revisar exceções personalizadas",
      descricao: "Releia como criar e lançar exceptions custom.",
      xp: 15, gold: 5, status: "concluida",
    },
    {
      id: 3, tipo: "Semanal", dificuldade: "hard",
      titulo: "Implementar repositório genérico",
      descricao: "Um Repository<T> com CRUD assíncrono sobre EF Core.",
      xp: 80, gold: 27, status: "pendente",
    },
  ];

  const modulos = [
    { id: 1, nome: "Fundamentos C#", icone: "square-code", estado: "concluido", progresso: 100 },
    { id: 2, nome: "Orientação a Objetos", icone: "boxes", estado: "concluido", progresso: 100 },
    { id: 3, nome: "Coleções e Genéricos", icone: "layers", estado: "concluido", progresso: 100 },
    {
      id: 4, nome: "C# Intermediário (LINQ/Async)", icone: "git-branch", estado: "atual", progresso: 38,
      topicos: [
        { nome: "LINQ", nivel: 45 },
        { nome: "Async / await", nivel: 20 },
        { nome: "Delegates e eventos", nivel: 50 },
      ],
    },
    { id: 5, nome: "Tratamento de Erros", icone: "shield-alert", estado: "liberado", progresso: 15,
      topicos: [{ nome: "Exceptions", nivel: 30 }, { nome: "Debugging", nivel: 10 }] },
    { id: 6, nome: "Async e Concorrência", icone: "zap", estado: "bloqueado", progresso: 0, requer: "LING/Async ≥ 60" },
    { id: 7, nome: "Acesso a Dados (EF Core)", icone: "database", estado: "bloqueado", progresso: 0, requer: "Coleções ≥ 60" },
    { id: 8, nome: "APIs Web (ASP.NET)", icone: "globe", estado: "bloqueado", progresso: 0, requer: "EF Core ≥ 60" },
    { id: 9, nome: "Testes Automatizados", icone: "flask-conical", estado: "bloqueado", progresso: 0, requer: "POO ≥ 60" },
    { id: 10, nome: "Arquitetura e SOLID", icone: "compass", estado: "bloqueado", progresso: 0, requer: "APIs + Testes" },
  ];

  const boss = {
    modulo: "C# Intermediário",
    nome: "O Guardião do LINQ",
    descricao: "Prove domínio de LINQ e async. 8 questões encadeadas.",
    questoesTotal: 8, questoesRestantes: 8, elegivel: true,
  };

  const exercicios = [
    {
      id: 1, topico: "LINQ", dificuldade: "medium", formato: "multipla",
      enunciado: "Qual expressão retorna os nomes dos usuários ativos ordenados alfabeticamente?",
      codigo: null,
      opcoes: [
        { id: "a", texto: "users.Where(u => u.Ativo).Select(u => u.Nome).OrderBy(n => n)", correta: true },
        { id: "b", texto: "users.Select(u => u.Nome).Where(u => u.Ativo).OrderBy(n => n)", correta: false },
        { id: "c", texto: "users.OrderBy(u => u.Nome).Select(u => u.Ativo)", correta: false },
        { id: "d", texto: "users.Filter(u => u.Ativo).Map(u => u.Nome)", correta: false },
      ],
      xp: 30, gold: 10,
      feedbackErro: "Quase! O Where precisa vir antes do Select, senão você filtra por 'Ativo' numa sequência de strings. Esse conceito volta pra revisão em 1 dia.",
    },
    {
      id: 2, topico: "Exceções", dificuldade: "hard", formato: "codigo",
      enunciado: "Complete o método para lançar uma SaldoInsuficienteException quando o valor for maior que o saldo.",
      codigo: "public void Sacar(decimal valor)\n{\n    // seu código aqui\n\n    Saldo -= valor;\n}",
      opcoes: null,
      xp: 45, gold: 15,
      feedbackErro: "Boa tentativa! Faltou lançar a exceção ANTES de subtrair do saldo. Esse conceito volta pra revisão em 1 dia.",
    },
  ];

  const revisoes = [
    { id: 1, topico: "LINQ", quando: "hoje", ciclo: 1 },
    { id: 2, topico: "Async", quando: "hoje", ciclo: 2 },
    { id: 3, topico: "Exceções", quando: "amanhã", ciclo: 1 },
  ];

  const itensLoja = [
    { id: 1, nome: "Poção de Streak", icone: "flask-round", preco: 50, descricao: "Protege 1 dia de streak perdido.", inventario: 1 },
    { id: 2, nome: "Dica Extra", icone: "lightbulb", preco: 20, descricao: "Uma dica da IA em qualquer exercício." },
    { id: 3, nome: "Tema Escuro Premium", icone: "palette", preco: 30, descricao: "Paleta 'obsidiana' para a interface." },
    { id: 4, nome: "Boost XP 2x (1h)", icone: "rocket", preco: 80, descricao: "Dobra o XP por uma hora." },
    { id: 5, nome: "Skin Pixel Art", icone: "gamepad-2", preco: 100, descricao: "Visual retrô estilo Tibia." },
  ];

  const heatmap = (function () {
    // 30 dias de intensidade 0–4 (mock determinístico).
    const arr = [];
    const seed = [0,1,2,3,4,2,1,0,2,3,4,4,3,2,1,0,1,2,3,4,3,2,4,4,3,2,1,3,4,2];
    for (let i = 0; i < 30; i++) arr.push(seed[i % seed.length]);
    return arr;
  })();

  const mentor = {
    contexto: { topico: "LINQ", nivel: 45 },
    mensagens: [
      { de: "ia", texto: "Opa! Vi que você tá em LINQ (nível 45). Manda a dúvida que a gente destrava isso." },
      { de: "user", texto: "Qual a diferença entre First e FirstOrDefault?" },
      {
        de: "ia",
        texto: "Ótima pergunta. `First` lança exceção se não achar nada; `FirstOrDefault` devolve o valor padrão (null pra referência, 0 pra int). Use FirstOrDefault quando 'não encontrar' é um caso normal:",
        codigo: "var user = lista.FirstOrDefault(u => u.Id == id);\nif (user is null) return NotFound();",
      },
      { de: "user", texto: "E se a lista tiver vários que batem?" },
      { de: "ia", texto: "Ambos pegam só o PRIMEIRO na ordem atual. Se quiser garantir unicidade, use `Single`/`SingleOrDefault` — eles lançam se houver mais de um. Bom pra validar invariantes." },
    ],
  };

  return { player, titulosPorNivel, missoesHoje, modulos, boss, exercicios, revisoes, itensLoja, heatmap, mentor };
})();
