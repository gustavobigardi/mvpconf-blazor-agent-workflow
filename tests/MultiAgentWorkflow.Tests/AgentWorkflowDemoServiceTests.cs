using MultiAgentWorkflow.Core;

namespace MultiAgentWorkflow.Tests;

public sealed class AgentWorkflowDemoServiceTests
{
    [Fact]
    public async Task RunAsync_EmitsExpectedAgentSequence()
    {
        var service = new AgentWorkflowDemoService(new FoundryOptions());
        var request = new WorkflowRequest("Triar incidente de checkout intermitente", RequireHumanApproval: true);

        var events = new List<WorkflowEvent>();
        await foreach (var item in service.RunAsync(request))
        {
            events.Add(item);
        }

        Assert.Equal(11, events.Count);
        Assert.Equal(WorkflowEventType.Started, events[0].Type);
        Assert.Contains(events, item => item.Agent == "Planner" && item.Type == WorkflowEventType.AgentCompleted);
        Assert.Contains(events, item => item.Agent == "Researcher" && item.Type == WorkflowEventType.AgentCompleted);
        Assert.Contains(events, item => item.Agent == "Executor" && item.Type == WorkflowEventType.AgentCompleted);
        Assert.Contains(events, item => item.Agent == "Human-in-the-loop" && item.Type == WorkflowEventType.ApprovalRequired);
        Assert.Equal(WorkflowEventType.Completed, events[^1].Type);
    }

    [Fact]
    public void FoundryOptions_ReportsMissingValues()
    {
        var options = new FoundryOptions { Mode = "Foundry" };

        var missing = options.MissingValues();

        Assert.Contains("Foundry:ProjectEndpoint", missing);
        Assert.Contains("Foundry:ModelDeploymentName", missing);
        Assert.False(options.IsConfigured);
    }
}
