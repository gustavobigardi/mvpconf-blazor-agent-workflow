using System.Runtime.CompilerServices;
namespace MultiAgentWorkflow.Core;

public sealed class AgentWorkflowDemoService
{
    private readonly FoundryOptions _options;
    private readonly IReadOnlyList<IWorkflowAgent> _agents;

    public AgentWorkflowDemoService(FoundryOptions options)
    {
        _options = options;
        _agents = [
            new PlannerAgent(),
            new ResearchAgent(),
            new ExecutorAgent(),
            new ReviewerAgent()
        ];
    }

    public FoundryOptions Options => _options;
    public IReadOnlyList<IWorkflowAgent> Agents => _agents;

    public async IAsyncEnumerable<WorkflowEvent> RunAsync(
        WorkflowRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var context = new WorkflowContext(request);

        yield return context.AddEvent(
            WorkflowEventType.Started,
            "Workflow",
            "Workflow iniciado",
            _options.IsConfigured
                ? "Configuracao Foundry detectada; o fluxo esta pronto para ser conectado ao runtime real."
                : "Modo simulado ativo para demo local e previsivel.");

        foreach (var agent in _agents)
        {
            yield return context.AddEvent(
                WorkflowEventType.AgentStarted,
                agent.Name,
                $"{agent.Name} trabalhando",
                agent.Responsibility);

            await agent.ExecuteAsync(context, cancellationToken);

            yield return context.AddEvent(
                WorkflowEventType.AgentCompleted,
                agent.Name,
                $"{agent.Name} concluiu",
                CompletedMessageFor(agent.Name, context),
                PayloadFor(agent.Name, context));

            if (agent is ExecutorAgent && request.RequireHumanApproval)
            {
                yield return context.AddEvent(
                    WorkflowEventType.ApprovalRequired,
                    "Human-in-the-loop",
                    "Checkpoint humano",
                    "Antes da revisao final, o fluxo pausa para confirmacao do operador.");

                await Task.Delay(350, cancellationToken);
            }
        }

        yield return context.AddEvent(
            WorkflowEventType.Completed,
            "Workflow",
            "Workflow concluido",
            "Plano final pronto para a experiencia Blazor.",
            BuildFinalSummary(context));
    }

    private static string CompletedMessageFor(string agentName, WorkflowContext context)
    {
        return agentName switch
        {
            "Planner" => "Plano inicial criado.",
            "Researcher" => "Contexto e criterios levantados.",
            "Executor" => "Acoes propostas e dependencias mapeadas.",
            "Reviewer" => "Riscos e proximos passos validados.",
            _ => "Etapa concluida."
        };
    }

    private static string PayloadFor(string agentName, WorkflowContext context)
    {
        return agentName switch
        {
            "Planner" => context.Plan,
            "Researcher" => context.Evidence,
            "Executor" => context.ExecutionNotes,
            "Reviewer" => context.Review,
            _ => string.Empty
        };
    }

    private static string BuildFinalSummary(WorkflowContext context)
    {
        return string.Join(Environment.NewLine + Environment.NewLine, [
            "Plano:",
            context.Plan,
            "Evidencias:",
            context.Evidence,
            "Execucao:",
            context.ExecutionNotes,
            "Revisao:",
            context.Review
        ]);
    }
}
