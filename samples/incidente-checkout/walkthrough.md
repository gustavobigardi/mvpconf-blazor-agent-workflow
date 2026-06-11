# Walkthrough para participantes

## 1. Rodar a demo

```bash
dotnet run --project src/MultiAgentWorkflow.Demo
```

Abra a URL exibida pelo terminal.

## 2. Executar o cenario

1. Copie o objetivo de `input.md`.
2. Cole no campo da UI.
3. Mantenha `Pausar para aprovacao humana` ligado.
4. Clique em `Executar workflow`.

## 3. Observar o workflow

Durante a execucao, procure:

- evento inicial indicando modo simulado ou Foundry;
- Planner criando etapas;
- Researcher trazendo sinais mockados;
- Executor propondo mitigacao;
- checkpoint humano;
- Reviewer apontando riscos;
- evento final com consolidado.

## 4. Experimentos rapidos

Troque o objetivo por um destes:

```text
Triar instabilidade em login social apos atualizacao de identidade.
```

```text
Investigar aumento de latencia em API de pedidos depois de migracao de banco.
```

```text
Criar plano de comunicacao para incidente de indisponibilidade parcial no app mobile.
```

## 5. Evolucoes sugeridas

- Persistir eventos em banco.
- Adicionar botao real de aprovacao humana.
- Trocar agentes simulados por chamadas ao Foundry.
- Adicionar ferramenta mockada de consulta a logs.
- Gerar resumo final em Markdown para abrir issue automaticamente.
