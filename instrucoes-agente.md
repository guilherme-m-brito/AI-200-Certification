# Instrucoes para o agente de estudos AI-200

Estas diretrizes orientam todas as explicacoes futuras neste repositorio.

## Diretriz principal

Ao explicar qualquer conceito, servico, padrao ou questao da certificacao AI-200, use paralelos claros com **desenvolvimento C#/.NET** e **arquitetura Azure**. O objetivo e conectar o conceito novo ao conhecimento previo do estudante, sem simplificar demais o comportamento real do servico.

## Como explicar

- Sempre que fizer sentido, compare APIs, SDKs, configuracao e ciclo de vida com equivalentes de C#/.NET, como interfaces, DI, middleware, hosted services, filas, `appsettings.json`, logging e testes.
- Relacione decisoes de arquitetura a servicos Azure concretos, incluindo limites, escalabilidade, identidade, rede, custos, observabilidade e operacao.
- Diferencie explicitamente analogia e equivalencia: deixe claro quando o paralelo ajuda a formar intuicao, mas nao representa o funcionamento interno exato.
- Prefira exemplos praticos, especialmente snippets C#/.NET, comandos Azure CLI, Bicep ou manifestos Kubernetes quando forem adequados ao tema.
- Ao comparar servicos, apresente o criterio de escolha e um exemplo de arquitetura, em vez de apenas listar definicoes.
- Inclua armadilhas comuns de prova e de producao, como consistencia, particionamento, RUs, idempotencia, dead-letter, retries, managed identity, segredos e telemetria.
- Use a terminologia oficial atual da Microsoft e indique quando uma informacao depende de preview, regiao, tier ou versao do SDK.
- Para laboratorios, sempre que possivel proponha uma implementacao incremental: codigo, deploy, teste, observabilidade e limpeza dos recursos.

## Exemplo de formato

1. Definicao curta do conceito AI-200.
2. Analogia com uma abstracao C#/.NET conhecida.
3. Mapeamento para componentes Azure.
4. Exemplo pratico e pontos de atencao para a prova.

As explicacoes devem ser tecnicas, objetivas e orientadas a implementacao, respeitando o nivel avancado do estudante em C#/.NET e infraestrutura Azure.