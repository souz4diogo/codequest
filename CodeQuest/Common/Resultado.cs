namespace CodeQuest.Common;

/// <summary>
/// Resultado de uma operação de negócio. Evita usar exceções para falhas esperadas
/// (ex.: gold insuficiente), deixando o fluxo explícito para as páginas.
/// </summary>
public class Resultado
{
    public bool Sucesso { get; }
    public string? Erro { get; }

    protected Resultado(bool sucesso, string? erro)
    {
        Sucesso = sucesso;
        Erro = erro;
    }

    public static Resultado Ok() => new(true, null);
    public static Resultado Falha(string erro) => new(false, erro);

    public static Resultado<T> Ok<T>(T valor) => Resultado<T>.Ok(valor);
    public static Resultado<T> Falha<T>(string erro) => Resultado<T>.Falha(erro);
}

/// <summary>Resultado que carrega um valor em caso de sucesso.</summary>
public sealed class Resultado<T> : Resultado
{
    public T? Valor { get; }

    private Resultado(bool sucesso, T? valor, string? erro) : base(sucesso, erro)
    {
        Valor = valor;
    }

    public static Resultado<T> Ok(T valor) => new(true, valor, null);
    public static new Resultado<T> Falha(string erro) => new(false, default, erro);
}
