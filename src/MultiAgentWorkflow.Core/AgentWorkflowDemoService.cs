using System.Runtime.CompilerServices;
namespace MultiAgentWorkflow.Core;

public sealed class AgentWorkflowDemoService
{
    private readonly FoundryOptions _options;
    private readonly IReadOnlyList<IWorkflowAgent> _agents;

    public AgentWorkflowDemoService(FoundryOptions options)
    {
        _options = options;
        var chatClient = _options.IsConfigured ? new FoundryChatClient(_options) : null;
        _agents = [
            new PlannerAgent(chatClient),
            new ResearchAgent(chatClient),
            new ExecutorAgent(chatClient),
            new ReviewerAgent(chatClient)
        ];
    }

    public FoundryOptions Options => _options;
    public IReadOnlyList<IWorkflowAgent> Agents => _agents;

    public async IAsyncEnumerable<WorkflowEvent> RunAsync(
        WorkflowRequest request,
        Func<CancellationToken, Task>? waitForHumanApproval = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var context = new WorkflowContext(request);

        yield return context.AddEvent(
            WorkflowEventType.Started,
            "Workflow",
            "Workflow iniciado",
            _options.IsConfigured
                ? $"Foundry ativo: chamando deployment '{_options.ModelDeploymentName}' em {_options.ResolveOpenAIEndpoint()}."
                : "Modo simulado ativo para demo local e previsivel.");

        foreach (var agent in _agents)
        {
            yield return context.AddEvent(
                WorkflowEventType.AgentStarted,
                agent.Name,
                $"{agent.Name} trabalhando",
                agent.Responsibility);

            Exception? foundryException = null;
            try
            {
                await agent.ExecuteAsync(context, cancellationToken);
            }
            catch (Exception ex) when (_options.IsConfigured)
            {
                foundryException = ex;
            }

            if (foundryException is not null)
            {
                yield return context.AddEvent(
                    WorkflowEventType.AgentCompleted,
                    agent.Name,
                    $"{agent.Name} falhou ao chamar Foundry",
                    "Verifique endpoint, deployment, permissao RBAC/API key e conectividade.",
                    foundryException.Message);

                yield return context.AddEvent(
                    WorkflowEventType.Completed,
                    "Workflow",
                    "Workflow interrompido",
                    "A chamada real ao Foundry falhou. Altere Foundry:Mode para Simulated para usar o plano B de palco.");
                yield break;
            }

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
                    "Antes da revisao final, o fluxo esta aguardando aprovacao do operador.");

                if (waitForHumanApproval is not null)
                {
                    await waitForHumanApproval(cancellationToken);
                }

                yield return context.AddEvent(
                    WorkflowEventType.ApprovalGranted,
                    "Human-in-the-loop",
                    "Aprovacao recebida",
                    "Operador liberou a revisao final. O Reviewer pode continuar.");
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
