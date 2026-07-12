namespace CodeQuest.Services.Ai;

/// <summary>
/// Banco de cenários — o segredo anti-genérico (seção 6 do motor-ia): mesmo tópico + mesma
/// dificuldade + cenário diferente = exercício que parece novo. Metade dos cenários é de
/// jogo, de propósito, pelo perfil do jogador. Um é sorteado e injetado como obrigatório
/// no prompt de geração.
/// </summary>
public static class Cenarios
{
    private static readonly string[] Banco =
    [
        "inventário e loot de um MMORPG",
        "sistema de guild e raids",
        "leilão de itens raros",
        "carrinho de e-commerce",
        "fila de pedidos de iFood",
        "caixa de banco digital",
        "placar de campeonato de futebol",
        "biblioteca com empréstimos",
        "estoque de farmácia",
        "playlist de música",
        "sistema de matrícula de escola",
        "rastreio de encomendas",
        "ponto eletrônico de funcionários",
        "reserva de assentos de cinema",
        "bot de Discord",
        "sensor de temperatura IoT",
        "ranking de speedrun",
        "caixa de supermercado",
        "sistema de tickets de suporte",
        "agenda de consultas médicas",
    ];

    public static string Sortear() => Banco[Random.Shared.Next(Banco.Length)];
}
