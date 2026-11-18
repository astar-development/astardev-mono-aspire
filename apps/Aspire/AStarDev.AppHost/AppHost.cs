using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// AppHostDirectory follows the cwd (e.g. `aspire run` from repo root), so anchor on this source file instead.
string Project(string relative, [System.Runtime.CompilerServices.CallerFilePath] string file = "") =>
    Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, relative));

builder.AddDotnetProject("AStarDev-Web", Project("../../web/AStarDev.Web/AStarDev.Web.csproj"))
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.AddDotnetProject("Fab4Kids-Web", Project("../../web/Fab4Kids/Fab4Kids.Web.csproj"))
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

// Desktop app: its OTel config keys are not the OTEL_* vars Aspire injects, so map them here.
// Empty connection string keeps Application Insights off for local runs; OTLP goes to this dashboard only.
builder.AddDotnetProject("CloudSync", Project("../../desktop/AStarDev.CloudSync/AStarDev.CloudSync/AStarDev.CloudSync.csproj"))
    .WithEnvironment("ApplicationInsights__ConnectionString", "")
    .WithEnvironment("OpenTelemetry__OtlpEndpoint", builder.Configuration["ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL"] ?? "");

builder.Build().Run();
