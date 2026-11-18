var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.AStarDev_Web>("AStarDev-Web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Fab4Kids_Web>("Fab4Kids-Web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

// Desktop app: its OTel config keys are not the OTEL_* vars Aspire injects, so map them here.
// Empty connection string keeps Application Insights off for local runs; OTLP goes to this dashboard only.
builder.AddProject<Projects.AStarDev_CloudSync>("CloudSync")
    .WithEnvironment("ApplicationInsights__ConnectionString", "")
    .WithEnvironment("OpenTelemetry__OtlpEndpoint", builder.Configuration["ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL"] ?? "");

builder.Build().Run();
