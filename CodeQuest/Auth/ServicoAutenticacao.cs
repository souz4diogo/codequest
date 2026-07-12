using CodeQuest.Common;
using CodeQuest.Data;
using CodeQuest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest.Auth;

/// <summary>
/// Regras de registro e validação de credenciais. Não emite o token (isso é responsabilidade
/// do <see cref="IGeradorTokenJwt"/>, acionado pelo controller) — aqui fica só a lógica de
/// domínio: unicidade do login, hash da senha e criação do Player 1:1 no registro.
/// </summary>
public interface IServicoAutenticacao
{
    Task<Resultado<Usuario>> RegistrarAsync(string login, string senha, CancellationToken ct = default);
    Task<Resultado<Usuario>> ValidarCredenciaisAsync(string login, string senha, CancellationToken ct = default);
}

public sealed class ServicoAutenticacao : IServicoAutenticacao
{
    private const int TamanhoMinimoSenha = 6;

    private readonly AppDbContext _db;
    private readonly IPasswordHasher<Usuario> _hasher;

    public ServicoAutenticacao(AppDbContext db, IPasswordHasher<Usuario> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<Resultado<Usuario>> RegistrarAsync(string login, string senha, CancellationToken ct = default)
    {
        login = Normalizar(login);

        if (string.IsNullOrWhiteSpace(login))
            return Resultado.Falha<Usuario>("Informe um login.");
        if (senha.Length < TamanhoMinimoSenha)
            return Resultado.Falha<Usuario>($"A senha precisa ter ao menos {TamanhoMinimoSenha} caracteres.");
        if (await _db.Usuarios.AnyAsync(u => u.Login == login, ct))
            return Resultado.Falha<Usuario>("Este login já está em uso.");

        var usuario = new Usuario { Login = login };
        usuario.SenhaHash = _hasher.HashPassword(usuario, senha);

        // Todo usuário nasce com seu Player (relação 1:1). O Nome inicial é o próprio login.
        usuario.Player = new Player { Nome = login, Nivel = 1 };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync(ct);

        return Resultado.Ok(usuario);
    }

    public async Task<Resultado<Usuario>> ValidarCredenciaisAsync(string login, string senha, CancellationToken ct = default)
    {
        login = Normalizar(login);

        var usuario = await _db.Usuarios
            .Include(u => u.Player)
            .FirstOrDefaultAsync(u => u.Login == login, ct);

        // Mensagem genérica: não revela se foi o login ou a senha que falhou.
        if (usuario is null)
            return Resultado.Falha<Usuario>("Login ou senha inválidos.");

        var resultado = _hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
        if (resultado == PasswordVerificationResult.Failed)
            return Resultado.Falha<Usuario>("Login ou senha inválidos.");

        return Resultado.Ok(usuario);
    }

    private static string Normalizar(string login) => (login ?? string.Empty).Trim().ToLowerInvariant();
}
