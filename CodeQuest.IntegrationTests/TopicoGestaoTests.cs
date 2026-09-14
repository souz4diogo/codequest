using System.Net;
using CodeQuest.Dtos;
using CodeQuest.IntegrationTests.Suporte;
using Xunit;

namespace CodeQuest.IntegrationTests;

[Collection(CodeQuestCollection.Nome)]
public class TopicoGestaoTests
{
    private readonly CodeQuestApiFactory _factory;

    public TopicoGestaoTests(CodeQuestApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Criar_TopicoValido_ComecaComNivelZero()
    {
        var (modulo, _) = await _factory.CriarModuloComTopicoAsync();
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PostarJsonAsync($"/api/modulos/{modulo.Id}/topicos",
            new SalvarTopicoRequest("Novo tópico", Prioridade: 1));
        resposta.EnsureSuccessStatusCode();
        var topico = await resposta.LerJsonAsync<TopicoGestaoDto>();

        Assert.Equal("Novo tópico", topico!.Nome);
        Assert.Equal(0, topico.NivelEstimado);
        Assert.False(topico.Arquivado);
    }

    [Fact]
    public async Task Criar_NomeVazio_RetornaBadRequest()
    {
        var (modulo, _) = await _factory.CriarModuloComTopicoAsync();
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PostarJsonAsync($"/api/modulos/{modulo.Id}/topicos",
            new SalvarTopicoRequest("   ", Prioridade: 1));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Criar_PrioridadeForaDoIntervalo_RetornaBadRequest()
    {
        var (modulo, _) = await _factory.CriarModuloComTopicoAsync();
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PostarJsonAsync($"/api/modulos/{modulo.Id}/topicos",
            new SalvarTopicoRequest("Tópico", Prioridade: 99));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Criar_ModuloInexistente_RetornaBadRequest()
    {
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PostarJsonAsync("/api/modulos/999999/topicos",
            new SalvarTopicoRequest("Tópico", Prioridade: 1));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Editar_AtualizaNomeEPrioridade()
    {
        var (modulo, topico) = await _factory.CriarModuloComTopicoAsync();
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PutAsJsonComEnumAsync($"/api/modulos/{modulo.Id}/topicos/{topico.Id}",
            new SalvarTopicoRequest("Nome editado", Prioridade: 3));
        resposta.EnsureSuccessStatusCode();
        var editado = await resposta.LerJsonAsync<TopicoGestaoDto>();

        Assert.Equal("Nome editado", editado!.Nome);
        Assert.Equal(3, editado.Prioridade);
    }

    [Fact]
    public async Task Editar_TopicoDeOutroModulo_RetornaBadRequest()
    {
        var (moduloA, topicoDeA) = await _factory.CriarModuloComTopicoAsync();
        var (moduloB, _) = await _factory.CriarModuloComTopicoAsync();
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PutAsJsonComEnumAsync($"/api/modulos/{moduloB.Id}/topicos/{topicoDeA.Id}",
            new SalvarTopicoRequest("Tentando editar cruzado", Prioridade: 1));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Arquivar_MarcaTopicoComoArquivado()
    {
        var (modulo, topico) = await _factory.CriarModuloComTopicoAsync();
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PostarJsonAsync($"/api/modulos/{modulo.Id}/topicos/{topico.Id}/arquivar",
            new ArquivarTopicoRequest(true));
        resposta.EnsureSuccessStatusCode();
        var arquivado = await resposta.LerJsonAsync<TopicoGestaoDto>();

        Assert.True(arquivado!.Arquivado);
    }

    [Fact]
    public async Task Listar_IncluiTopicosArquivadosNaGestao()
    {
        var (modulo, topico) = await _factory.CriarModuloComTopicoAsync();
        var (cliente, _) = await _factory.RegistrarELogarAsync();
        await cliente.PostarJsonAsync($"/api/modulos/{modulo.Id}/topicos/{topico.Id}/arquivar", new ArquivarTopicoRequest(true));

        var topicos = await cliente.ObterJsonAsync<List<TopicoGestaoDto>>($"/api/modulos/{modulo.Id}/topicos");

        Assert.Contains(topicos!, t => t.Id == topico.Id && t.Arquivado);
    }
}
