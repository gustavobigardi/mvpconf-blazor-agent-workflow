using MultiAgentWorkflow.Core;

namespace MultiAgentWorkflow.Tests;

public sealed class AgentWorkflowDemoServiceTests
{
    [Fact]
    public async Task RunAsync_EmitsExpectedAgentSequence()
    {
        var service = new AgentWorkflowDemoService(new FoundryOptions());
        var request = new WorkflowRequest("Triar incidente de checkout intermitente", RequireHumanApproval: true);
        var approvalWaited = false;

        var events = new List<WorkflowEvent>();
        await foreach (var item in service.RunAsync(request, _ =>
        {
            approvalWaited = true;
            return Task.CompletedTask;
        }))
        {
            events.Add(item);
        }

        Assert.Equal(12, events.Count);
        Assert.Equal(WorkflowEventType.Started, events[0].Type);
        Assert.Contains(events, item => item.Agent == "Planner" && item.Type == WorkflowEventType.AgentCompleted);
        Assert.Contains(events, item => item.Agent == "Researcher" && item.Type == WorkflowEventType.AgentCompleted);
        Assert.Contains(events, item => item.Agent == "Executor" && item.Type == WorkflowEventType.AgentCompleted);
        Assert.Contains(events, item => item.Agent == "Human-in-the-loop" && item.Type == WorkflowEventType.ApprovalRequired);
        Assert.Contains(events, item => item.Agent == "Human-in-the-loop" && item.Type == WorkflowEventType.ApprovalGranted);
        Assert.True(approvalWaited);
        Assert.Equal(WorkflowEventType.Completed, events[^1].Type);
    }

    [Fact]
    public void FoundryOptions_ReportsMissingValues()
    {
        var options = new FoundryOptions { Mode = "Foundry" };

        var missing = options.MissingValues();

        Assert.Contains("Foundry:ProjectEndpoint ou Foundry:OpenAIEndpoint", missing);
        Assert.Contains("Foundry:ModelDeploymentName", missing);
        Assert.False(options.IsConfigured);
    }

    [Fact]
    public void ResolveOpenAIEndpoint_StripsProjectPath()
    {
        var options = new FoundryOptions
        {
            ProjectEndpoint = "https://demo.services.ai.azure.com/api/projects/workflows",
            ModelDeploymentName = "gpt-demo",
            Mode = "Foundry"
        };

        Assert.Equal("https://demo.services.ai.azure.com/openai/v1/", options.ResolveOpenAIEndpoint().ToString());
    }

    [Fact]
    public void ResolveOpenAIEndpoint_PreservesExplicitOpenAIEndpoint()
    {
        var options = new FoundryOptions
        {
            OpenAIEndpoint = "https://demo.services.ai.azure.com/openai/v1",
            ModelDeploymentName = "gpt-demo",
            Mode = "Foundry"
        };

        Assert.Equal("https://demo.services.ai.azure.com/openai/v1/", options.ResolveOpenAIEndpoint().ToString());
    }

    [Fact]
    public void IsUsablePortugueseOutput_AcceptsOperationalPortuguese()
    {
        const string output = """
        1. Confirmar impacto do incidente com atendimento e metricas de erro.
        2. Acionar mitigacao temporaria para reduzir falhas no checkout.
        3. Comunicar proxima atualizacao para produto e suporte.
        """;

        Assert.True(FoundryChatClient.IsUsablePortugueseOutput(output));
    }

    [Fact]
    public void IsUsablePortugueseOutput_RejectsCorruptedMixedScriptText()
    {
        const string output = """
        1. Recepção e classificação do incidente: Coletar resume São characteristics पहुचल replay लाभ grave navegadorSistema прокурат Vente군 incubCE aporte WeirdBonrawl.
        """;

        Assert.False(FoundryChatClient.IsUsablePortugueseOutput(output));
    }
}
