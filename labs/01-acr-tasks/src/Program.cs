using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Versao da imagem injetada em build time (ver Dockerfile ARG IMAGE_VERSION).
// Analogia .NET: equivale a ler um AssemblyInformationalVersion, mas a origem
// e um argumento de build do container, nao o .csproj.
var imageVersion = Environment.GetEnvironmentVariable("IMAGE_VERSION") ?? "dev-local";

// Endpoint de saude: usado para verificar a imagem depois do deploy.
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// Endpoint que expoe a versao: prova que a tag/digest implantado e o esperado.
app.MapGet("/version", () => Results.Ok(new
{
    service = "document-inference-api",
    imageVersion,
    runtime = Environment.Version.ToString()
}));

// Endpoint de "inferencia" simulada, alinhado ao caso do docs/01-diagnostico-containers.md.
app.MapPost("/classify", (ClassificationRequest request) =>
{
    var label = string.IsNullOrWhiteSpace(request.Text) || request.Text.Length < 20
        ? "short-document"
        : "long-document";

    return Results.Ok(new ClassificationResponse(label, imageVersion));
});

app.Run();

record ClassificationRequest(string Text);
record ClassificationResponse(string Label, string ServedByImageVersion);
