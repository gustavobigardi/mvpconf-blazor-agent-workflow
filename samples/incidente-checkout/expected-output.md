# Saida esperada

A saida exata varia conforme o objetivo, mas a demo deve produzir este arco:

## Planner

Deve registrar o incidente, classificar impacto, separar coleta de evidencias, mitigacao e comunicacao.

## Researcher

Deve listar sinais como:

- falhas no endpoint de checkout;
- correlacao com deploy recente;
- divergencia entre pagamento autorizado e pedido confirmado;
- impacto percebido pelo atendimento.

## Executor

Deve propor acoes concretas:

- avaliar feature flag ou rollback;
- comparar logs entre versoes;
- abrir war room curto;
- comunicar status inicial.

## Human-in-the-loop

Deve aparecer um checkpoint antes da revisao final, reforcando que nem todo workflow de IA deve ser 100% autonomo.

## Reviewer

Deve validar o plano e apontar riscos de rollback/comunicacao.
