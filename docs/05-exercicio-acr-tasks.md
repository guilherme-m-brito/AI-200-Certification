# Exercicio pratico: Build e gerenciamento de imagens com ACR Tasks

Data de Estudo: 30 de setembro de 2026
Referencia oficial: [Exercicio - Build and run a container image with ACR Tasks](https://microsoftlearning.github.io/mslearn-azure-ai/instructions/container-hosting/01-acr-tasks.html)
Arquivos de inicio: `labs/01-acr-tasks/acr-tasks-python/` (starter oficial `acr-tasks-python.zip`)

Fecha o modulo **Implementar hospedagem de aplicativos conteinerizados no Azure** na parte de ACR. Consolida na pratica o que foi visto em `docs/02` (conceitos), `docs/03` (ACR Tasks) e `docs/04` (tags e versionamento).

---

## 1. O que foi construido

Uma API **Flask** minima (Python) que simula um servico de inferencia:

| Rota | Retorno |
| :--- | :--- |
| `/` | Nome da API, versao e lista de endpoints |
| `/health` | `status: healthy` + `APP_VERSION` |
| `/predict` | Predicao simulada + `MODEL_VERSION` |

O `Dockerfile` grava a versao em variaveis de ambiente (`ENV APP_VERSION=1.0.0`, `ENV MODEL_VERSION=v1`) sobre a base `python:3.11-slim`.

*Paralelo .NET:* mesma ideia da API minima ASP.NET Core lendo a versao de uma env var; muda apenas o runtime (Flask/Gunicorn no lugar de Kestrel).

---

## 2. Setup do ambiente (o que o `azdeploy.py` faz)

O script de bootstrap:
1. Exige `az login` (le o object id do usuario).
2. Cria o **resource group** (`az group create`).
3. Cria o **ACR tier Basic** com nome derivado de hash do usuario (`acr` + 8 chars). No meu caso: `acrde673b17`.
4. Gera `.env` (bash) e `.env.ps1` (PowerShell) com `RESOURCE_GROUP`, `ACR_NAME`, `LOCATION`.

Sequencia executada (Windows/PowerShell):

```powershell
az login --tenant <TENANT_ID>          # MFA obrigatorio no login pessoal
az provider register --namespace Microsoft.ContainerRegistry
python azdeploy.py
. .\.env.ps1                            # dot-source carrega as variaveis
```

---

## 3. Comandos do exercicio

```powershell
# Build na nuvem (Quick Task) - contexto = ./api, sem Docker local
az acr build --registry $env:ACR_NAME --image inference-api:v1.0.0 ./api

# Verificar
az acr repository list --name $env:ACR_NAME --output table
az acr repository show-tags --name $env:ACR_NAME --repository inference-api --output table
az acr manifest list-metadata --registry $env:ACR_NAME --name inference-api --output table

# Rodar a imagem no agente do ACR e validar que o Flask importa
az acr run --registry $env:ACR_NAME `
  --cmd "$env:ACR_NAME.azurecr.io/inference-api:v1.0.0 python -c 'from app import app'" `
  /dev/null

# Segunda versao (o registro mantem as duas)
az acr build --registry $env:ACR_NAME --image inference-api:v1.1.0 ./api

# Historico de builds e lock da imagem de producao
az acr task list-runs --registry $env:ACR_NAME --output table
az acr repository update --name $env:ACR_NAME --image inference-api:v1.0.0 --write-enabled false
az acr repository show --name $env:ACR_NAME --image inference-api:v1.0.0   # writeEnabled: False

# Limpeza (essencial em pay-as-you-go)
az group delete --name $env:RESOURCE_GROUP --no-wait --yes
az group exists --name $env:RESOURCE_GROUP   # aguardar ate 'false'
```

Resultado do build: `Run ID: ca1 was successful after 28s`, digest `sha256:1a65796b...`. Tudo (build + push) executado na nuvem, sem Docker local.

---

## 4. Ligacao com a teoria

| Observado na saida | Conceito (unidade) |
| :--- | :--- |
| `Queued a build with ID: ca1` | Run ID da ACR Task (03) |
| `FROM python:3.11-slim` | Tag estavel de imagem base (04) |
| `v1.0.0: digest: sha256:1a65...` | Digest imutavel vs. tag mutavel (02) |
| `v1.0.0` e `v1.1.0` coexistindo | Registro mantem multiplas versoes (04) |
| `--write-enabled false` -> `writeEnabled: False` | Lock de imagem de producao (04) |
| `az acr run ... /dev/null` | Contexto vazio; executa a imagem em vez de buildar (03/04) |

---

## 5. Armadilhas encontradas (caderno de erros)

1. **Sintaxe de continuacao de linha:** colar comandos com `\` (bash) no PowerShell quebra. No PowerShell a continuacao e a crase `` ` ``, ou tudo em uma linha.
2. **`az login` sem assinatura / MFA:** no login pessoal, o tenant exigiu MFA (`AADSTS50076`). Solucao: `az login --tenant <TENANT_ID>` e concluir o MFA no navegador. Alternativa: `--use-device-code`.
3. **Execution Policy do PowerShell:** `. .\.env.ps1` falhou porque o script gerado nao e assinado. Solucao de escopo minimo: `Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass` (vale so na sessao).
4. **Placeholder literal na limpeza:** `az group delete --name <rg-name>` falha porque `<` e reservado no PowerShell. Substituir por `$env:RESOURCE_GROUP` ou o nome real.
5. **Isolamento de conta:** `az login` e global por usuario (`~/.azure`). Para nao misturar conta pessoal com a do trabalho, usar `AZURE_CONFIG_DIR` apontando para uma pasta separada nesta sessao.

---

## 5.1. Avaliacao do modulo (4/5) — questao errada

**Cenario:** a equipe cria imagens em estacoes de desenvolvedores, gerando resultados inconsistentes. Qual recurso do ACR garante que todas as imagens sejam criadas em um **ambiente controlado**?

- Resposta marcada (errada): *Namespaces do repositorio*.
- Resposta correta: **Compilacao rapida de Tarefas do ACR** (quick task / `az acr build`).

**Raciocinio:** a palavra-chave e "ambiente controlado / builds inconsistentes entre maquinas". O `az acr build` roda a compilacao no **agente do ACR na nuvem**, sempre no mesmo ambiente padronizado, independente do SO, da versao de Docker ou da maquina de cada dev. Elimina o "na minha maquina funciona".

Por que as demais nao servem:
* **Namespaces do repositorio:** apenas organizam nomes de imagens em "pastas" (`ia/document-inference-api`). E organizacao, nao controle de build.
* **Geo-replication:** distribui copias do registro por regioes (Premium). E sobre latencia de pull, nao sobre padronizar o build.

> Regra rapida: "build inconsistente entre maquinas" -> **ACR quick task (`az acr build`)**.

---

## 6. Checklist do modulo de ACR (concluido)

- [x] Conceitos de ACR: registry / repository / tag / digest (`docs/02`)
- [x] ACR Tasks: quick task, gatilhos, base image update (`docs/03`)
- [x] Tags, SemVer, `latest`, lock, purge/retencao (`docs/04`)
- [x] Exercicio pratico: build, run, versionamento, lock e limpeza (este resumo)

**Proximo modulo:** Implantar e gerenciar aplicativos no Azure Container Apps (ambientes, revisoes, KEDA e escala orientada a eventos), conectando ao diagnostico de `docs/01-diagnostico-containers.md`.
