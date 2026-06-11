using Microsoft.AspNetCore.DataProtection;
using MultiAgentWorkflow.Demo.Components;
using MultiAgentWorkflow.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".keys")));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped(sp =>
{
    var section = sp.GetRequiredService<IConfiguration>()
        .GetSection(FoundryOptions.SectionName);

    var options = new FoundryOptions
    {
        Mode = section["Mode"] ?? "Simulated",
        ProjectEndpoint = section["ProjectEndpoint"],
        OpenAIEndpoint = section["OpenAIEndpoint"],
        ModelDeploymentName = section["ModelDeploymentName"],
        AgentName = section["AgentName"] ?? "multi-agent-workflow-demo",
        ApiKey = section["ApiKey"],
        MaxOutputTokens = int.TryParse(section["MaxOutputTokens"], out var maxOutputTokens)
            ? maxOutputTokens
            : 420,
        Temperature = float.TryParse(section["Temperature"], out var temperature)
            ? temperature
            : 0f
    };

    return new AgentWorkflowDemoService(options);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
