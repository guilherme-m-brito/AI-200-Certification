# Diagnostico inicial: contêineres no Azure

Data: 23 de agosto de 2026
## Caso

API ASP.NET Core que expõe um endpoint de classificacao de documentos e precisa escalar com base em mensagens.

## Respostas e feedback

### 1. Onde hospedar a API?

**Resposta:** Azure Container Apps.

**Avaliacao:** correta. Container Apps combina hospedagem de contêineres, revisoes, escala automatica e KEDA, que permite escalar conforme filas ou outros eventos. App Service tambem poderia hospedar a API, mas nao e a escolha mais natural quando o requisito central e escala orientada por eventos.

Analogia .NET: Container Apps funciona como hospedar um `BackgroundService` ou uma API conteinerizada em uma plataforma que gerencia replicas e ciclo de vida; KEDA fornece a politica que decide quando criar ou remover replicas.

### 2. Quando AKS e justificavel?

**Resposta:** quando a solucao exige maior complexidade, rede dedicada, politicas especificas e mais customizacao.

**Avaliacao:** correta, com uma ressalva. AKS deve ser escolhido por requisitos concretos de controle e operacao Kubernetes, nao apenas porque a aplicacao e complexa. Exemplos: multiplos workloads Kubernetes, Helm e operadores, politicas de rede e admission, service mesh, workloads stateful ou requisitos avancados de scheduling e observabilidade.

Container Apps reduz a carga operacional. AKS oferece mais controle, mas transfere para a equipe mais responsabilidade por cluster, upgrades, seguranca e troubleshooting.

### 3. Onde armazenar a imagem Docker?

**Resposta esperada:** Azure Container Registry (ACR).

O ACR armazena imagens privadas, permite tags e versionamento, integra-se ao Azure e pode executar builds com ACR Tasks. O deploy deve referenciar uma tag imutavel ou, de preferencia, o digest da imagem para garantir reprodutibilidade.

Analogia .NET: ACR e o feed privado de pacotes NuGet, mas para artefatos de runtime. O pipeline publica a imagem e o Container Apps ou AKS faz o pull durante o deploy.

### 4. Como fornecer segredos?

**Resposta:** usar Key Vault e configuracao por variaveis.

**Ajuste importante:** variaveis de ambiente sao configuracao, nao um cofre. A abordagem recomendada e:

1. Habilitar identidade gerenciada no recurso que executa a API.
2. Dar a essa identidade apenas as permissoes necessarias no Key Vault.
3. Ler o segredo em runtime usando o SDK ou uma referencia de segredo suportada pelo servico.
4. Manter nomes de configuracao, endpoints e feature flags no App Configuration quando apropriado.

Nao colocar segredos no Dockerfile, no repositorio, em `appsettings.json` versionado ou em argumentos de build.

Analogia .NET: `IConfiguration` continua sendo a abstracao consumida pela aplicacao, mas a fonte de configuracao deixa de ser um arquivo local e passa a ser Key Vault/App Configuration.

### 5. Como fazer rollback?

No Container Apps, publicar uma nova **revisao** e direcionar o trafego para ela permite testar, fazer canary ou dividir trafego. Se a nova versao falhar, redirecionar o trafego para a revisao anterior realiza o rollback sem reconstruir a imagem.

Em AKS, o rollback normalmente envolve o historico de rollout do Deployment, por exemplo com `kubectl rollout undo`, ou a reversao do release via Helm.

## Resultado do diagnostico

- Container Apps, App Service e AKS: entendimento inicial bom.
- ACR: conceito a estudar primeiro.
- Seguranca: separar configuracao, identidade e segredo.
- Revisoes e rollback: estudar junto com deployment e traffic splitting.

## Proximo laboratorio

Criar uma API minima em ASP.NET Core, construir a imagem Docker, publica-la em um ACR e implanta-la no Azure Container Apps. Depois, adicionar uma segunda revisao e testar rollback. O laboratorio deve incluir a limpeza dos recursos Azure.
