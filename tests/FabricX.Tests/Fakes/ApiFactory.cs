using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FabricX.Tests.Fakes;

public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _arquivoBanco = Path.Combine(Path.GetTempPath(), $"fabricx-teste-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_arquivoBanco};Pooling=False");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (File.Exists(_arquivoBanco))
            File.Delete(_arquivoBanco);
    }
}
