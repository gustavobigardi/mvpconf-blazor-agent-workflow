namespace MultiAgentWorkflow.Core;

public sealed record WorkflowRequest(
    string Goal,
    bool RequireHumanApproval,
    string Audience = "desenvolvedores .NET");

public sealed record WorkflowEvent(
    WorkflowEventType Type,
    string Agent,
    string Title,
    string Message,
    DateTimeOffset Timestamp,
    string? Payload = null);

public enum WorkflowEventType
{
    Started,
    AgentStarted,
    AgentCompleted,
    ApprovalRequired,
    ApprovalGranted,
    Completed
}

public sealed record WorkflowResult(
    string Goal,
    string Plan,
    string Evidence,
    string ExecutionNotes,
    string Review,
    IReadOnlyList<WorkflowEvent> Events);

public sealed class WorkflowContext
{
    private readonly List<WorkflowEvent> _events = [];

    public WorkflowContext(WorkflowRequest request)
    {
        Request = request;
    }

    public WorkflowRequest Request { get; }
    public IReadOnlyList<WorkflowEvent> Events => _events;
    public string Plan { get; set; } = string.Empty;
    public string Evidence { get; set; } = string.Empty;
    public string ExecutionNotes { get; set; } = string.Empty;
    public string Review { get; set; } = string.Empty;

    public WorkflowEvent AddEvent(WorkflowEventType type, string agent, string title, string message, string? payload = null)
    {
        var item = new WorkflowEvent(type, agent, title, message, DateTimeOffset.Now, payload);
        _events.Add(item);
        return item;
    }

    public WorkflowResult ToResult()
    {
        return new WorkflowResult(Request.Goal, Plan, Evidence, ExecutionNotes, Review, Events.ToArray());
    }
}
