using CodeQuest.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CodeQuest.Data;

/// <summary>
/// Contexto EF Core do CodeQuest (PostgreSQL via Npgsql). Mantém apenas mapeamento e configuração
/// de persistência — nenhuma regra de jogo vive aqui (isso é responsabilidade dos serviços).
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Modulo> Modulos => Set<Modulo>();
    public DbSet<ModuloPrereq> ModuloPrereqs => Set<ModuloPrereq>();
    public DbSet<Topico> Topicos => Set<Topico>();
    public DbSet<Missao> Missoes => Set<Missao>();
    public DbSet<Exercicio> Exercicios => Set<Exercicio>();
    public DbSet<MissaoExercicio> MissaoExercicios => Set<MissaoExercicio>();
    public DbSet<Tentativa> Tentativas => Set<Tentativa>();
    public DbSet<Revisao> Revisoes => Set<Revisao>();
    public DbSet<Teste> Testes => Set<Teste>();
    public DbSet<SessaoFoco> SessoesFoco => Set<SessaoFoco>();
    public DbSet<Projeto> Projetos => Set<Projeto>();
    public DbSet<ItemLoja> ItensLoja => Set<ItemLoja>();
    public DbSet<CompraLoja> ComprasLoja => Set<CompraLoja>();
    public DbSet<Duvida> Duvidas => Set<Duvida>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);

        // ---- Chaves compostas (relações N:N) ----
        model.Entity<ModuloPrereq>().HasKey(x => new { x.ModuloId, x.RequerModuloId });
        model.Entity<MissaoExercicio>().HasKey(x => new { x.MissaoId, x.ExercicioId });

        // ModuloPrereq referencia Modulo duas vezes: desliga cascade para evitar ciclos de exclusão.
        model.Entity<ModuloPrereq>()
            .HasOne(x => x.Modulo).WithMany(m => m.PreRequisitos)
            .HasForeignKey(x => x.ModuloId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<ModuloPrereq>()
            .HasOne(x => x.RequerModulo).WithMany()
            .HasForeignKey(x => x.RequerModuloId).OnDelete(DeleteBehavior.Restrict);

        // ---- Relação 1:1 Tentativa → Revisao ----
        model.Entity<Revisao>()
            .HasOne(r => r.Tentativa).WithOne(t => t.Revisao)
            .HasForeignKey<Revisao>(r => r.TentativaId);

        // ---- Player: gold nunca negativo (RN) ----
        // Aspas no nome da coluna: o Postgres preserva a caixa só com identificador aspado.
        model.Entity<Player>().ToTable(t => t.HasCheckConstraint("CK_Player_Gold", "\"Gold\" >= 0"));

        // ---- Faixas 0–100 da seção 2 do doc de requisitos (notas e nível estimado) ----
        model.Entity<Topico>().ToTable(t =>
            t.HasCheckConstraint("CK_Topico_NivelEstimado", "\"NivelEstimado\" BETWEEN 0 AND 100"));
        model.Entity<Tentativa>().ToTable(t =>
            t.HasCheckConstraint("CK_Tentativa_Nota", "\"Nota\" BETWEEN 0 AND 100"));
        model.Entity<Teste>().ToTable(t =>
            t.HasCheckConstraint("CK_Teste_Nota", "\"Nota\" IS NULL OR \"Nota\" BETWEEN 0 AND 100"));
        model.Entity<Modulo>().ToTable(t =>
            t.HasCheckConstraint("CK_Modulo_NotaBoss", "\"NotaBoss\" IS NULL OR \"NotaBoss\" BETWEEN 0 AND 100"));

        // ---- SessaoFoco aponta para projeto OU tópico — exatamente um dos dois (seção 2.2) ----
        model.Entity<SessaoFoco>().ToTable(t =>
            t.HasCheckConstraint("CK_SessaoFoco_Alvo", "num_nonnulls(\"ProjetoId\", \"TopicoId\") = 1"));

        // ---- Teste aponta para tópico (RF17) OU módulo — boss fight (RF12/RN08) ----
        model.Entity<Teste>().ToTable(t =>
            t.HasCheckConstraint("CK_Teste_Alvo", "num_nonnulls(\"ModuloId\", \"TopicoId\") = 1"));
        model.Entity<Teste>()
            .HasOne(x => x.Modulo).WithMany()
            .HasForeignKey(x => x.ModuloId).OnDelete(DeleteBehavior.Cascade);

        // ---- Usuario ↔ Player (1:1) e login único ----
        model.Entity<Usuario>().HasIndex(u => u.Login).IsUnique();
        model.Entity<Player>()
            .HasOne(p => p.Usuario).WithOne(u => u.Player)
            .HasForeignKey<Player>(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // ---- Regras de valor ----
        model.Entity<ItemLoja>().Property(i => i.Ativo).HasDefaultValue(true);
        model.Entity<Topico>().Property(t => t.Prioridade).HasDefaultValue(2);

        // ---- Enums como TEXT (legível no banco, imune a reordenação) ----
        ConverterEnumsParaTexto(model);
    }

    /// <summary>
    /// Converte automaticamente toda propriedade enum para TEXT. Fazendo por convenção
    /// (e não enum a enum) o mapeamento fica aberto a extensão: novos enums já entram
    /// como string sem tocar aqui (OCP).
    /// </summary>
    private static void ConverterEnumsParaTexto(ModelBuilder model)
    {
        foreach (var entity in model.Model.GetEntityTypes())
        {
            foreach (var prop in entity.GetProperties())
            {
                var tipo = Nullable.GetUnderlyingType(prop.ClrType) ?? prop.ClrType;
                if (!tipo.IsEnum) continue;

                var conversor = (ValueConverter)Activator.CreateInstance(
                    typeof(EnumToStringConverter<>).MakeGenericType(tipo))!;
                prop.SetValueConverter(conversor);
            }
        }
    }
}
