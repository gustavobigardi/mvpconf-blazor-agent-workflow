# Workflows Multi-Agent com Microsoft Agent Workflow

Demo em .NET, C# e Blazor para a palestra do MVP Conf Regional Sorocaba dentro do MOBI Festival 2026.

O projeto foi pensado para palco:

- roda localmente em modo simulado, sem depender de internet;
- chama Azure AI Foundry de verdade quando `Foundry:Mode=Foundry` esta configurado;
- mostra uma UI Blazor acompanhando eventos de um workflow multi-agent;
- separa agentes por responsabilidade: Planner, Researcher, Executor e Reviewer;
- inclui checkpoint humano para demonstrar human-in-the-loop;
- deixa a configuracao Foundry documentada para evoluir a demo para agentes/modelos reais.

## Estrutura

```text
src/
  MultiAgentWorkflow.Core/      # Workflow, agentes e modelos da demo
  MultiAgentWorkflow.Demo/      # Blazor Server/Web App
tests/
  MultiAgentWorkflow.Tests/     # Testes do fluxo
docs/
  foundry-setup.md              # Como configurar Azure AI Foundry
  demo-runbook.md               # Roteiro de apresentacao
samples/
  incidente-checkout/           # Cenario comum para participantes repetirem a demo
```

## Rodar localmente

```bash
dotnet restore
dotnet run --project src/MultiAgentWorkflow.Demo
```

Abra a URL exibida no terminal. A aplicacao inicia em `Foundry:Mode=Simulated`, que e o modo recomendado para apresentacao ao vivo.

## Testes

```bash
dotnet test
```

## Configurar Foundry

Leia [docs/foundry-setup.md](docs/foundry-setup.md). O modo real usa o endpoint OpenAI-compatible do Azure AI Foundry e chama o deployment configurado em cada agente do workflow.

## Roteiro de demo

Leia [docs/demo-runbook.md](docs/demo-runbook.md) para um fluxo de 8-10 minutos alinhado aos slides.

## Material para participantes

O caso demonstrado fica em [samples/incidente-checkout](samples/incidente-checkout). Ele traz entrada, contexto mockado, roteiro de exploracao e ideias de extensao para transformar a demo em exercicio.
