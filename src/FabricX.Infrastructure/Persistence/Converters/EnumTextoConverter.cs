using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FabricX.Infrastructure.Persistence.Converters;

public class EnumTextoConverter<TEnum> : ValueConverter<TEnum, string> where TEnum : struct, Enum
{
    public EnumTextoConverter() : base(valor => ParaTexto(valor), texto => ParaEnum(texto))
    {
    }

    private static string ParaTexto(TEnum valor)
    {
        var nome = valor.ToString();
        var texto = new StringBuilder();

        for (var i = 0; i < nome.Length; i++)
        {
            if (i > 0 && char.IsUpper(nome[i]))
                texto.Append('_');

            texto.Append(char.ToUpperInvariant(nome[i]));
        }

        return texto.ToString();
    }

    private static TEnum ParaEnum(string texto)
    {
        foreach (var valor in Enum.GetValues<TEnum>())
        {
            if (ParaTexto(valor) == texto)
                return valor;
        }

        throw new InvalidOperationException($"Valor '{texto}' inválido para {typeof(TEnum).Name}.");
    }
}
