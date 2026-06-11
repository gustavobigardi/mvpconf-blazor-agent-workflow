namespace MultiAgentWorkflow.Core;

public interface IWorkflowAgent
{
    string Name { get; }
    string Responsibility { get; }
    Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken);
}

public sealed class PlannerAgent(IAgentChatClient? chatClient = null) : IWorkflowAgent
{
    public string Name => "Planner";
    public string Responsibility => "Quebra o objetivo em etapas executaveis.";

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken)
    {
        if (chatClient is not null)
        {
            context.Plan = await chatClient.CompleteAsync(
                Name,
                Responsibility,
                context,
                "Devolva uma lista numerada de 4 a 6 etapas para triagem e resposta inicial ao incidente.",
                cancellationToken);
            return;
        }

        await Task.Delay(450, cancellationToken);
        context.Plan = string.Join(Environment.NewLine, [
            $"1. Registrar o incidente: {context.Request.Goal}",
            "2. Classificar impacto, sistemas envolvidos e janela do deploy",
            "3. Acionar coleta de evidencias em logs, metricas e relatos de usuario",
            "4. Propor mitigacao imediata antes da investigacao completa",
            "5. Preparar comunicacao para negocio e time tecnico"
        ]);
    }
}

public sealed class ResearchAgent(IAgentChatClient? chatClient = null) : IWorkflowAgent
{
    public string Name => "Researcher";
    public string Responsibility => "Busca contexto, restricoes e criterios de qualidade.";

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken)
    {
        if (chatClient is not null)
        {
            context.Evidence = await chatClient.CompleteAsync(
                Name,
                Responsibility,
                context,
                "Liste sinais, evidencias e perguntas de investigacao. Use bullets curtos.",
                cancellationToken);
            return;
        }

        await Task.Delay(550, cancellationToken);
        context.Evidence = string.Join(Environment.NewLine, [
            "Sinal 1: aumento de falhas HTTP 500 no endpoint /checkout/confirmar.",
            "Sinal 2: primeira ocorrencia perto da janela do ultimo deploy.",
            "Sinal 3: pagamentos autorizados aparecem sem pedido confirmado em parte dos casos.",
            "Sinal 4: atendimento reporta impacto em clientes recorrentes e novos."
        ]);
    }
}

public sealed class ExecutorAgent(IAgentChatClient? chatClient = null) : IWorkflowAgent
{
    public string Name => "Executor";
    public string Responsibility => "Transforma plano e contexto em uma proposta concreta.";

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken)
    {
        if (chatClient is not null)
        {
            context.ExecutionNotes = await chatClient.CompleteAsync(
                Name,
                Responsibility,
                context,
                "Proponha acoes concretas em ordem de execucao: mitigacao, diagnostico, comunicacao e donos sugeridos.",
                cancellationToken);
            return;
        }

        await Task.Delay(600, cancellationToken);
        context.ExecutionNotes = string.Join(Environment.NewLine, [
            "Acao 1: ativar feature flag para rota de checkout anterior, se disponivel.",
            "Acao 2: comparar logs do deploy atual com a versao anterior.",
            "Acao 3: abrir war room curto com engenharia, produto e atendimento.",
            "Acao 4: comunicar status inicial: impacto, mitigacao em andamento e proxima atualizacao."
        ]);
    }
}

public sealed class ReviewerAgent(IAgentChatClient? chatClient = null) : IWorkflowAgent
{
    public string Name => "Reviewer";
    public string Responsibility => "Valida riscos, consistencia e prontidao para entrega.";

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken)
    {
        if (chatClient is not null)
        {
            context.Review = await chatClient.CompleteAsync(
                Name,
                Responsibility,
                context,
                "Revise o plano final. Aponte aprovacao, riscos, lacunas e proximo passo. Seja direto.",
                cancellationToken);
            return;
        }

        await Task.Delay(500, cancellationToken);
        context.Review = string.Join(Environment.NewLine, [
            "Aprovado: plano cobre mitigacao, investigacao e comunicacao.",
            "Risco: rollback sem validar pagamentos pendentes pode duplicar trabalho operacional.",
            "Risco: comunicacao vaga aumenta chamados repetidos.",
            "Proximo passo: confirmar dono de cada acao e horario da proxima atualizacao."
        ]);
    }
}
