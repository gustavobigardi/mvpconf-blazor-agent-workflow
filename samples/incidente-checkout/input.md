# Entrada da demo

Cole este objetivo na UI:

```text
Triar um incidente de checkout intermitente apos deploy: clientes relatam erro ao finalizar compra, impacto alto e causa ainda desconhecida.
```

## Contexto para o apresentador

- Um deploy saiu ha cerca de 40 minutos.
- O atendimento abriu varios chamados de clientes que nao conseguem finalizar compra.
- Parte dos pagamentos aparece autorizada, mas o pedido nao e confirmado.
- O time ainda nao sabe se a causa esta no frontend, API de checkout, antifraude ou integracao de pagamento.

## Por que esse caso funciona bem

- E familiar para desenvolvedores, produto e negocio.
- Tem urgencia sem depender de dominio especifico.
- Permite mostrar agentes com responsabilidades diferentes.
- Permite demonstrar human-in-the-loop antes de recomendar rollback/mitigacao.
