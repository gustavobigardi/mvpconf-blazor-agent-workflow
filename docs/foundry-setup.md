# Configurando Azure AI Foundry para a demo

Este guia prepara o ambiente para evoluir a demo local para uma versao conectada ao Azure AI Foundry. A aplicacao atual roda em modo simulado por padrao para ser confiavel durante a palestra.

## 1. Criar ou selecionar um projeto Foundry

1. Acesse o portal do Azure AI Foundry.
2. Crie ou selecione um Hub/Project para a demo.
3. Garanta que seu usuario tenha permissao para usar o projeto e os deployments de modelo.
4. Copie o Project endpoint exibido na area de Overview/Settings do projeto.

## 2. Criar um deployment de modelo

1. No projeto Foundry, abra Models + endpoints.
2. Crie um deployment de modelo para chat, por exemplo `gpt-4.1-mini` ou outro modelo disponivel na sua assinatura/regiao.
3. Defina um nome simples para o deployment, por exemplo:

```text
gpt-4.1-mini-demo
```

4. Confirme que o deployment responde no Playground antes de plugar no codigo.

## 3. Configurar autenticacao local

Para desenvolvimento local, prefira login interativo do Azure:

```bash
az login
az account set --subscription "<subscription-id>"
```

Se o projeto usar RBAC, valide que seu usuario tem permissao adequada no projeto/recurso AI Services. Para CI/CD, use identidade gerenciada ou service principal, nunca credencial pessoal.

## 4. Configurar a aplicacao

Use user-secrets para nao gravar endpoint/deployment local no repositorio:

```bash
dotnet user-secrets init --project src/MultiAgentWorkflow.Demo
dotnet user-secrets set "Foundry:Mode" "Foundry" --project src/MultiAgentWorkflow.Demo
dotnet user-secrets set "Foundry:ProjectEndpoint" "https://<project-endpoint>" --project src/MultiAgentWorkflow.Demo
dotnet user-secrets set "Foundry:ModelDeploymentName" "gpt-4.1-mini-demo" --project src/MultiAgentWorkflow.Demo
dotnet user-secrets set "Foundry:AgentName" "multi-agent-workflow-demo" --project src/MultiAgentWorkflow.Demo
```

Alternativa por variaveis de ambiente:

```bash
export Foundry__Mode=Foundry
export Foundry__ProjectEndpoint="https://<project-endpoint>"
export Foundry__ModelDeploymentName="gpt-4.1-mini-demo"
export Foundry__AgentName="multi-agent-workflow-demo"
```

## 5. Onde plugar o SDK real

O ponto de troca esta em:

```text
src/MultiAgentWorkflow.Core/AgentWorkflowDemoService.cs
```

Hoje ele executa agentes simulados e emite eventos deterministicos. Para a versao conectada:

1. mantenha `WorkflowRequest`, `WorkflowEvent` e `WorkflowResult`;
2. substitua as implementacoes `PlannerAgent`, `ResearchAgent`, `ExecutorAgent` e `ReviewerAgent` por chamadas ao runtime/SDK escolhido;
3. preserve o streaming de eventos para a UI Blazor;
4. trate fallback: se Foundry nao estiver configurado, volte para modo simulado.

## 6. Checklist antes da palestra

- Projeto Foundry abre no portal.
- Deployment de modelo responde no Playground.
- `dotnet run --project src/MultiAgentWorkflow.Demo` inicia sem erro.
- UI mostra `Foundry configurado` quando `Foundry:Mode=Foundry` e os campos obrigatorios existem.
- Plano B pronto: voltar `Foundry:Mode=Simulated`.

## 7. Cuidados

- Nao commite secrets.
- Nao dependa da rede para a primeira execucao em palco.
- Grave um video curto do fluxo real caso a rede do evento falhe.
- Tenha um prompt/objetivo conhecido para fechar a demo em menos de 10 minutos.
