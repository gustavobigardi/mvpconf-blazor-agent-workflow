# Runbook da demo

Tempo recomendado: 8-10 minutos.

## Cenario

Use um caso comum para qualquer participante: triagem de incidente depois de deploy.

```text
Triar um incidente de checkout intermitente apos deploy: clientes relatam erro ao finalizar compra, impacto alto e causa ainda desconhecida.
```

Esse cenario fica no repositorio em `samples/incidente-checkout/` para o participante repetir a demo depois.

## Abertura

Explique que a UI Blazor e o cockpit do workflow: o usuario nao espera um texto final aparecer do nada; ele acompanha etapas, agentes e checkpoints.

## Passo 1: objetivo

Use o objetivo padrao ou cole o texto do arquivo `samples/incidente-checkout/input.md`:

```text
Triar um incidente de checkout intermitente apos deploy: clientes relatam erro ao finalizar compra, impacto alto e causa ainda desconhecida.
```

## Passo 2: executar workflow

Clique em `Executar workflow`.

Mostre que o primeiro evento registra se a aplicacao esta em modo simulado ou Foundry. No modo Foundry, esse evento mostra o endpoint normalizado e o deployment usado.

## Passo 3: agentes especializados

Narrativa sugerida:

- Planner: transforma objetivo aberto em etapas.
- Researcher: levanta contexto e restricoes.
- Executor: propoe acoes.
- Reviewer: critica o resultado antes de entregar.

## Passo 4: human-in-the-loop

Mantenha a opcao de aprovacao humana ligada. Quando o checkpoint aparecer, explique:

```text
Nem todo workflow de IA deve ser 100% autonomo. Alguns pontos pedem aprovacao, ajuste ou coleta de contexto humano.
```

## Passo 5: fechamento

No evento final, destaque:

- o fluxo ficou observavel;
- cada agente teve fronteira clara;
- a UI conseguiu exibir progresso;
- o modo Foundry troca as respostas deterministicas por chamadas reais ao deployment.
- o participante tem um caso pronto para executar, alterar e evoluir no repo.

## Plano B

Se algo falhar:

1. rode em `Foundry:Mode=Simulated`;
2. use o objetivo padrao;
3. explique que o importante na palestra e a arquitetura do workflow, nao o endpoint externo.
