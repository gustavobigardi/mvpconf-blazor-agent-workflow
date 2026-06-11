# Configurando Azure AI Foundry para a demo

Este guia prepara o ambiente para executar a demo conectada ao Azure AI Foundry. A aplicacao roda em modo simulado por padrao para ser confiavel durante a palestra, mas quando `Foundry:Mode=Foundry` e um deployment sao configurados, cada agente chama o endpoint OpenAI-compatible do Foundry.

## 1. Criar ou selecionar um projeto Foundry

1. Acesse o portal do Azure AI Foundry.
2. Crie ou selecione um Hub/Project para a demo.
3. Garanta que seu usuario tenha permissao para usar o projeto e os deployments de modelo.
4. Copie o Project endpoint exibido na area de Overview/Settings do projeto.

Formato comum:

```text
https://<resource-name>.services.ai.azure.com/api/projects/<project-name>
```

A demo tambem aceita o endpoint direto do OpenAI-compatible API:

```text
https://<resource-name>.services.ai.azure.com/openai/v1
```

## 2. Criar um deployment de modelo

1. No projeto Foundry, abra Models + endpoints.
2. Crie um deployment de modelo para chat/instruct, por exemplo `gpt-4.1-mini` ou outro modelo conversacional disponivel na sua assinatura/regiao.
3. Defina um nome simples para o deployment, por exemplo:

```text
gpt-4.1-mini-demo
```

4. Confirme que o deployment responde no Playground antes de plugar no codigo.

Evite usar um modelo base/non-instruct para esta demo. Sintoma comum de deployment inadequado: resposta com idiomas misturados, caracteres aleatorios ou texto sem sentido.

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

Se preferir informar diretamente o endpoint OpenAI-compatible:

```bash
dotnet user-secrets set "Foundry:OpenAIEndpoint" "https://<resource-name>.services.ai.azure.com/openai/v1" --project src/MultiAgentWorkflow.Demo
```

Alternativa por variaveis de ambiente:

```bash
export Foundry__Mode=Foundry
export Foundry__ProjectEndpoint="https://<project-endpoint>"
export Foundry__OpenAIEndpoint="https://<resource-name>.services.ai.azure.com/openai/v1"
export Foundry__ModelDeploymentName="gpt-4.1-mini-demo"
export Foundry__AgentName="multi-agent-workflow-demo"
```

Parametros recomendados para a palestra:

```bash
dotnet user-secrets set "Foundry:Temperature" "0" --project src/MultiAgentWorkflow.Demo
dotnet user-secrets set "Foundry:MaxOutputTokens" "420" --project src/MultiAgentWorkflow.Demo
```

## 5. Autenticacao

A demo tenta autenticar nesta ordem:

1. `Foundry:ApiKey`, se configurado.
2. Microsoft Entra ID via `DefaultAzureCredential`, se nao houver API key.

Para API key:

```bash
dotnet user-secrets set "Foundry:ApiKey" "<sua-api-key>" --project src/MultiAgentWorkflow.Demo
```

Para Entra ID, faca login e garanta RBAC:

```bash
az login
az account set --subscription "<subscription-id>"
```

Permissao minima recomendada para chamada de modelo: role com permissao de usuario do Foundry/Azure OpenAI no recurso/projeto, como `Foundry User` ou `Cognitive Services OpenAI User`, dependendo da forma como o recurso foi provisionado.

## 6. Como a demo chama o Foundry

O ponto de integracao esta em:

```text
src/MultiAgentWorkflow.Core/FoundryChatClient.cs
```

Quando `Foundry:Mode=Foundry`, o `AgentWorkflowDemoService` cria agentes com um `FoundryChatClient`. Cada etapa do workflow chama:

```text
POST <openai-endpoint>/chat/completions
```

O `model` enviado e o valor de `Foundry:ModelDeploymentName`.

Se a chamada falhar, a UI mostra um evento de erro com a mensagem retornada pelo Foundry e encerra o workflow sem quebrar a pagina.

Se o modelo responder com texto corrompido, o cliente faz uma segunda tentativa com instrucoes mais restritivas. Se a segunda tentativa tambem vier corrompida, o workflow e interrompido com uma mensagem orientando revisar deployment, temperatura e limite de tokens.

## 7. Checklist antes da palestra

- Projeto Foundry abre no portal.
- Deployment de modelo responde no Playground.
- `dotnet run --project src/MultiAgentWorkflow.Demo` inicia sem erro.
- UI mostra `Foundry configurado` quando `Foundry:Mode=Foundry` e os campos obrigatorios existem.
- Ao executar o workflow, o primeiro evento deve mostrar `Foundry ativo`.
- As respostas dos agentes devem vir em portugues, com bullets curtos e sem caracteres aleatorios.
- Se a rede falhar, volte para `Foundry:Mode=Simulated`.
- Plano B pronto: voltar `Foundry:Mode=Simulated`.

## 8. Cuidados

- Nao commite secrets.
- Nao dependa da rede para a primeira execucao em palco.
- Grave um video curto do fluxo real caso a rede do evento falhe.
- Tenha um prompt/objetivo conhecido para fechar a demo em menos de 10 minutos.
- Se aparecerem caracteres estranhos, confirme que o deployment e chat/instruct e reduza `Foundry:MaxOutputTokens`.
