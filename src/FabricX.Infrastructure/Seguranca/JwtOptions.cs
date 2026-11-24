namespace FabricX.Infrastructure.Seguranca;

public class JwtOptions
{
    public const string Secao = "Jwt";
    public const int TamanhoMinimoDaChave = 32;

    public string Chave { get; set; } = string.Empty;
    public string Emissor { get; set; } = string.Empty;
    public string Audiencia { get; set; } = string.Empty;
    public int ExpiraEmMinutos { get; set; } = 60;
}
