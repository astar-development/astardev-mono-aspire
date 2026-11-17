var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.AStarDev_Web>("AStarDev-Web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Fab4Kids_Web>("Fab4Kids-Web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.Build().Run();
