# Preparacao para a certificacao Microsoft AI-200

Plano de estudos e laboratorio pratico para o exame **AI-200: Desenvolvendo solucoes de nuvem de IA no Azure**, com foco em implementacao de back-end, SDKs, dados vetoriais, mensageria, conteineres e operacao segura de workloads de IA.

## Objetivo

Realizar a prova em **16 de outubro de 2026**. O plano parte do conhecimento forte em C#/.NET e infraestrutura Azure, usando Python apenas onde ele fizer parte do fluxo pratico ou do objetivo avaliado.

## Escopo oficial

O cronograma segue o [guia oficial de estudo da AI-200](https://learn.microsoft.com/pt-br/credentials/certifications/resources/study-guides/ai-200), consultado em 23 de agosto de 2026:

| Domínio avaliado | Peso |
| --- | --- |
| Desenvolver solucoes em contêineres no Azure | 20-25% |
| Desenvolver solucoes de IA usando servicos de gerenciamento de dados do Azure | 25-30% |
| Conectar e consumir servicos do Azure | 20-25% |
| Proteger, monitorar e solucionar problemas de solucoes do Azure | 20-25% |

## Cronograma de estudos

Ritmo sugerido: 5 sessoes por semana, de 60 a 90 minutos, mais um bloco pratico no fim de semana. Cada semana deve terminar com um resumo em `docs/`, um laboratorio funcional em `labs/` e uma lista de duvidas ou pontos fracos.

| Semana | Periodo | Conteudo oficial e foco pratico | Entrega |
| --- | --- | --- | --- |
| 1 | 24-30 ago | **Fundamentos e contêineres:** revisar o modelo de desenvolvimento de solucoes de IA no Azure, SDKs Azure/terceiros, Dockerfiles, imagens, tags, Registro de Contêiner do Azure (ACR), build e versionamento com ACR Tasks. | Resumo de contêineres em `docs/` e imagem .NET ou Python publicada no ACR em `labs/`. |
| 2 | 31 ago-6 set | **Hospedagem e orquestracao:** App Service para contêineres, variaveis de ambiente e segredos; Azure Container Apps, ambientes e revisoes; KEDA e dimensionamento orientado a eventos. | Implantacao de uma API conteinerizada no Container Apps, com revisoes e escala documentadas. |
| 3 | 7-13 set | **AKS e observabilidade de contêineres:** manifestos Kubernetes, deploy, configuracao, logs, eventos, conectividade ponta a ponta e troubleshooting de AKS/Container Apps. | Manifestos Kubernetes e runbook de diagnostico em `labs/` e `docs/`. |
| 4 | 14-20 set | **Cosmos DB for NoSQL:** conexao e consultas via SDK, particionamento, politicas de indexacao, RUs, niveis de consistencia, embeddings, busca por similaridade vetorial e Change Feed Processor. | Mini pipeline de ingestao e busca semantica com Change Feed. |
| 5 | 21-27 set | **Azure Database for PostgreSQL:** conexoes e consultas por SDK, modelagem, tipos de dados, indices, pgvector, computacao/memoria/armazenamento, busca vetorial, RAG com filtros de metadados e pool de conexoes. | API .NET ou Python de RAG usando PostgreSQL e pgvector, com comparacao de indices. |
| 6 | 28 set-4 out | **Redis e integracao:** Azure Managed Redis, cache, expiracao, invalidacao e indexacao vetorial; Azure Service Bus com filas, mensagens, dead-letter, topicos e assinaturas; Azure Event Grid com filtros, eventos customizados e novas tentativas. | Fluxo assincrono com Service Bus/Event Grid e cache de respostas ou embeddings em Redis. |
| 7 | 5-11 out | **Azure Functions e seguranca:** Functions como APIs sem servidor, triggers, bindings, configuracao e deploy; Key Vault, identidade gerenciada, rotacao/recuperacao de segredos e Azure App Configuration. | Function integrada aos servicos anteriores, sem segredos no codigo, com configuracao por ambiente. |
| 8 | 12-16 out | **Monitoramento, revisao e prova:** OpenTelemetry para rastreamento distribuido, KQL para logs/metricas, troubleshooting completo, revisao dos quatro dominios, simulados e prova na sexta-feira. | Dois simulados cronometrados, caderno de erros e checklist final. **Exame: 16 out.** |

## Como estudar

1. **Teoria:** registrar em `docs/` o que cada servico resolve, limites, configuracao, SDKs, seguranca, custos e decisoes de arquitetura.
2. **Implementacao:** validar cada assunto com codigo em `labs/`, preferencialmente em C#/.NET quando isso nao conflitar com um objetivo explicito de Python.
3. **Comparacao:** para cada servico, anotar quando escolher a alternativa Azure adequada: Cosmos DB versus PostgreSQL, Service Bus versus Event Grid, Container Apps versus AKS, cache versus banco vetorial.
4. **Revisao:** ao final da semana, responder perguntas sem consultar material e atualizar o caderno de erros.
5. **Custos:** destruir ou pausar recursos Azure ao terminar cada laboratorio e registrar os comandos de limpeza.

## Estrutura do repositorio

```text
.
├── docs/   # Resumos teoricos, mapas de decisao e caderno de erros
├── labs/   # Codigo, manifestos, scripts e instrucoes de laboratorio
├── README.md
└── instrucoes-agente.md
```

## Referencias principais

- [Guia oficial de estudo da AI-200](https://learn.microsoft.com/pt-br/credentials/certifications/resources/study-guides/ai-200)
- [Certificacao Desenvolvedor Associado de IA na Nuvem do Azure](https://learn.microsoft.com/pt-br/credentials/certifications/azure-ai-cloud-developer-associate/?practice-assessment-type=certification)
- [Curso oficial AI-200T00: Desenvolver solucoes de nuvem de IA no Azure](https://learn.microsoft.com/pt-br/training/courses/ai-200t00)
- [Microsoft Learn](https://learn.microsoft.com/pt-br/training/)
- [Azure Architecture Center](https://learn.microsoft.com/pt-br/azure/architecture/)
- [Documentacao do Azure](https://learn.microsoft.com/pt-br/azure/)

## Percurso recomendado no Microsoft Learn

O curso AI-200T00 esta organizado em nove modulos e totaliza 120 horas. Para iniciar, estudar o modulo **Implementar hospedagem de aplicativos conteinerizados no Azure**, que aborda ACR, build e gerenciamento de imagens e hospedagem no App Service. Em seguida, avancar para **Implantar e gerenciar aplicativos no Azure Container Apps**, conectando o conteudo ao diagnostico registrado em `docs/01-diagnostico-containers.md`.

Ordem sugerida dos modulos do curso:

1. [Implementar hospedagem de aplicativos conteinerizados no Azure](https://learn.microsoft.com/pt-br/training/modules/implement-container-app-hosting-azure/)
2. Implantar e gerenciar aplicativos no Azure Container Apps
3. Implantar e monitorar aplicativos no Azure Kubernetes Service
4. Desenvolver solucoes de IA com o Azure Cosmos DB
5. Desenvolver solucoes de IA com o Azure Database for PostgreSQL
6. Aprimorar solucoes de IA com o Azure Managed Redis
7. Integrar servicos de back-end em solucoes de IA
8. Gerenciar segredos e configuracao de aplicativos
9. Observar e solucionar problemas de aplicativos

O primeiro modulo e a tarefa de hoje. Leia as unidades sobre ACR e hospedagem, anote as diferencas entre App Service e Container Apps e pare antes de entrar em AKS. A proxima aula usa suas anotacoes para construir o laboratorio pratico.

> O escopo e os servicos podem ser atualizados pela Microsoft. Antes de agendar ou realizar a prova, conferir novamente o guia oficial e a disponibilidade dos recursos.
