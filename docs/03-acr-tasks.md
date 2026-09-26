# ACR Tasks (Tarefas do Azure Container Registry)

Data de Estudo: 23 de setembro de 2026  
Referencia oficial: [Microsoft Learn - ACR Tasks](https://learn.microsoft.com/pt-br/training/modules/store-manage-containers-azure-container-registry/3-acr-tasks?pivots=text)

---

## 1. O que e o ACR Tasks?

O **ACR Tasks** e um conjunto de recursos dentro do Azure Container Registry que fornece capacidades de compilacao de imagens de conteiner baseadas em nuvem, automacao e gerenciamento de patches do sistema operacional/framework.

### O paralelo direto com o Azure DevOps (Pipelines / Releases)

| Conceito | Azure Pipelines / Releases | ACR Tasks |
| :--- | :--- | :--- |
| **Onde executa?** | Em um *Agent/Runner* (Hosted ou Self-Hosted) | **Nativamente na nuvem do Azure**, dentro da infraestrutura do ACR |
| **Foco principal** | CI/CD completo (Testes de unidade, builds de aplicacao, aprovacoes, deploy em varios ambientes) | **Build, empacotamento e atualizacao de imagens Docker/OCI** |
| **Dependencia do Docker** | O Agent precisa do *Docker Daemon* instalado para rodar `docker build` | O ACR atua como o proprio motor Docker na nuvem, dispensando o Docker instalado na maquina local do dev |

> **Resumo:** O ACR Tasks funciona como uma "mini-pipeline interna" nativa do registro de conteineres.

---

## 2. Tipos de Gatilhos e Funcionalidades (Relevantes para a AI-200)

O exame avalia como automatizar o ciclo de vida e atualizacao de imagens no ACR usando três tipos principais de tarefas:

### 1. Quick Task (`az acr build`)
* **O que faz:** Compila uma imagem sob demanda enviando o contexto local do seu codigo (incluindo o `Dockerfile`) diretamente para o Azure.
* **Vantagem:** Nao exige o Docker Desktop/Engine instalado no seu notebook. A compilacao usa o poder de processamento do Azure.
* *Analogia .NET:* Como rodar um `dotnet publish` diretamente em um servidor na nuvem.

### 2. Gatilhos de Codigo-Fonte (Source Code Triggers)
* **O que faz:** Monitora alteracoes em repositorios Git (GitHub, Azure Repos ou Bitbucket). 
* **Fluxo:** Ao realizar um `git push` ou criar um *Pull Request* em uma branch especificada, o ACR Task ativa automaticamente a recompilacao da imagem.

### 3. Gatilhos de Atualizacao de Imagem Base (Base Image Update Triggers) — **Ponto de Atencao do Exame!**
* **O que faz:** Se a sua imagem utiliza uma imagem base (ex: `mcr.microsoft.com/dotnet/aspnet:8.0`) e essa imagem base for atualizada pelo provedor (para aplicar correções de segurança ou patches no OS/framework), o ACR Task detecta a mudanca e **recompila automaticamente a imagem do seu aplicativo**.
* **Objetivo:** Garantir que contêineres de IA e back-end estejam sempre atualizados contra vulnerabilidades conhecidas sem intervencao manual.

---

## 3. Tarefas Multi-Etapa (Multi-Step Tasks)

Para cenarios mais complexos, o ACR Tasks suporta tarefas compostas definidas em arquivos YAML, permitindo:
1. Compilar multiplas imagens de conteiner em paralelo ou sequencia.
2. Executar testes automatizados sobre os conteineres compilados antes de envia-los ao registro (*push*).

---

## 4. O que focar sobre comandos CLI (`az acr`) na Prova AI-200

Nao e necessario memorizar todos os parametros de cor. A prova exige **reconhecer a intencao dos comandos** e saber preencher lacunas (*drag-and-drop* ou listas suspensas).

### Principais comandos e parametros:

* **`az acr build`**: Build pontual/rapido (Quick Task). Envia o contexto e gera a imagem na nuvem imediatamente.
* **`az acr task create`**: Cria uma tarefa de automacao persistente com gatilhos.

### Como referenciar Repositorios / Coleções de imagens:
O nome do repositorio (colecao) e a tag sao passados no parametro `--image` ou `-t`:

```bash
az acr task create \
  --registry meuregistro \
  --name task-build-api \
  --image ia/document-inference-api:v1.0.0 \
  --context https://github.com/meu-usuario/meu-repo.git#main \
  --file Dockerfile \
  --commit-trigger-enabled true
```

* **Estrutura do parametro `--image` / `-t`:**
  * `ia/document-inference-api`: Define o **repositorio / colecao** (usando namespace `ia`).
  * `:v1.0.0`: Define a **tag / versao** da imagem.
  * O **registry** (`meuregistro.azurecr.io`) e inferido pelo parametro `--registry` / `-r`.

### Resumo dos parametros essenciais:
* `--registry` / `-r`: Nome do recurso ACR.
* `--name` / `-n`: Nome da tarefa no ACR.
* `--image` / `-t`: Nome do repositorio + tag da imagem (`[namespace]/[repositorio]:[tag]`).
* `--context` / `-c`: Origem do codigo fonte (repositorio Git ou diretorio local).
* `--file` / `-f`: Caminho do `Dockerfile`.
* `--commit-trigger-enabled`: Habilita gatilho por `git push`.
* `--base-image-trigger-enabled`: Habilita gatilho por atualizacao da imagem base.

