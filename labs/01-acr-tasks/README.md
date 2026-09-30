# Lab 01 — Build e gerenciamento de imagens com ACR Tasks

Reproduz, de forma local e repetivel, o exercicio oficial [Criar e gerenciar uma imagem de contêiner com tarefas do ACR](https://learn.microsoft.com/pt-br/training/modules/store-manage-containers-azure-container-registry/5-exercise-build-manage-acr-tasks), consolidando o conteudo das unidades de ACR (`docs/02`, `docs/03`, `docs/04`).

## Objetivo

Construir a imagem de uma API mínima ASP.NET Core **inteiramente na nuvem** (sem Docker local), versiona-la com tags semanticas + tag unica, verificar a imagem, bloquear a versao de producao e limpar tudo ao final.

## Conceitos exercitados

| Conceito (unidade) | Onde aparece no lab |
| :--- | :--- |
| Quick Task `az acr build` (03) | Passo 3 |
| Tag unica com `{{.Run.ID}}` (03/04) | Passo 4 |
| Tag estavel `v1` + SemVer (04) | Passo 5 |
| Verificar imagem / listar tags (04) | Passo 6 |
| Bloquear imagem de producao (04) | Passo 7 |
| Limpeza de orfas com `acr purge` (04) | Passo 8 |
| Limpeza de recursos / custo (instrucoes-agente) | Passo 9 |

## Pre-requisitos

- Azure CLI atualizada (`az version`).
- Assinatura Azure. **Atencao:** a propria unidade oficial avisa que execucoes de ACR Tasks estao suspensas para creditos gratuitos; use plano **pay-as-you-go** ou pago.
- **Nao** e necessario Docker instalado — esse e justamente o ponto do `az acr build`.
- Login: `az login` (e `az account set --subscription <ID>` se tiver mais de uma).

> Todos os comandos abaixo sao PowerShell (ambiente Windows). As variaveis usam a sintaxe `$env:` / `$var`.

---

## Passo 1 — Definir variaveis

O nome do ACR precisa ser globalmente unico, so letras/numeros minusculos, 5-50 caracteres. O sufixo aleatorio evita conflito.

```powershell
$rg       = "rg-ai200-acrlab"
$location = "eastus"
$suffix   = -join ((48..57) + (97..122) | Get-Random -Count 6 | ForEach-Object { [char]$_ })
$acrName  = "acrai200$suffix"
$repo     = "ia/document-inference-api"   # repositorio com namespace 'ia' (unidade 02)

Write-Host "Resource Group: $rg"
Write-Host "ACR:            $acrName"
```

## Passo 2 — Criar o Resource Group e o ACR

Tier `Basic` e suficiente para o lab. (Politica de retencao nativa exigiria `Premium` — ver unidade 04.)

```powershell
az group create --name $rg --location $location

az acr create `
  --resource-group $rg `
  --name $acrName `
  --sku Basic
```

## Passo 3 — Quick Task: build na nuvem (`az acr build`)

Envia o contexto local (pasta `src/`, que contem o `Dockerfile`) para o ACR, que atua como o motor Docker. Nao usa Docker local.

```powershell
az acr build `
  --registry $acrName `
  --image "$($repo):v1.0.0" `
  --build-arg IMAGE_VERSION=v1.0.0 `
  ./src
```

## Passo 4 — Tag unica com `{{.Run.ID}}`

`{{.Run.ID}}` e expandido pelo ACR e gera um identificador unico por execucao — a tag de rastreabilidade da unidade 04. Combina SemVer + run ID.

```powershell
az acr build `
  --registry $acrName `
  --image "$($repo):v1.0.1-{{.Run.ID}}" `
  --build-arg IMAGE_VERSION=v1.0.1 `
  ./src
```

## Passo 5 — Tag estavel apontando para o ultimo patch

Publicar a mesma imagem sob a tag estavel `v1`. Quem consumir `:v1` recebe o ultimo `1.x.x`.

```powershell
az acr build `
  --registry $acrName `
  --image "$($repo):v1.0.1" `
  --image "$($repo):v1" `
  --build-arg IMAGE_VERSION=v1.0.1 `
  ./src
```

## Passo 6 — Verificar a imagem e listar tags

```powershell
# Repositorios no registro
az acr repository list --name $acrName --output table

# Tags do repositorio (deve mostrar v1, v1.0.0, v1.0.1 e a tag unica)
az acr repository show-tags --name $acrName --repository $repo --output table

# Detalhe de um manifesto (mostra o digest imutavel)
az acr repository show `
  --name $acrName `
  --image "$($repo):v1" `
  --output jsonc
```

## Passo 7 — Bloquear a imagem de producao (image lock)

Impede exclusao e sobrescrita da versao que serve trafego (unidade 04).

```powershell
az acr repository update `
  --name $acrName `
  --image "$($repo):v1.0.1" `
  --write-enabled false
```

Teste que o bloqueio funciona (o push abaixo **deve falhar**):

```powershell
# Esperado: erro, pois a tag esta bloqueada
az acr build `
  --registry $acrName `
  --image "$($repo):v1.0.1" `
  --build-arg IMAGE_VERSION=hack `
  ./src
```

Para aposentar a versao mais tarde, desbloqueie antes:

```powershell
az acr repository update `
  --name $acrName `
  --image "$($repo):v1.0.1" `
  --write-enabled true
```

## Passo 8 — Limpar imagens orfas (`acr purge`)

Roda como container dentro do ACR Tasks. O `--dry-run` mostra o que seria removido sem apagar nada.

```powershell
# Simulacao (nao apaga)
az acr run --registry $acrName `
  --cmd "acr purge --filter '$($repo):.*' --untagged --ago 0d --dry-run" `
  /dev/null
```

> `/dev/null` = contexto vazio. O `acr purge` nao constroi imagem, entao nao ha codigo-fonte para enviar (ver explicacao no fim deste README).

## Passo 9 — Limpeza dos recursos (evitar custo)

Apagar o Resource Group remove ACR e todas as imagens de uma vez.

```powershell
az group delete --name $rg --yes --no-wait
```

Ou rode o script pronto: `./cleanup.ps1 -ResourceGroup rg-ai200-acrlab`

---

## Verificacao opcional (build/run local com Docker)

Se voce tiver Docker Desktop e quiser validar a API antes de subir ao ACR:

```powershell
docker build -t document-inference-api:local --build-arg IMAGE_VERSION=local ./src
docker run --rm -p 8080:8080 document-inference-api:local
# Em outro terminal:
curl http://localhost:8080/version
curl -X POST http://localhost:8080/classify -H "Content-Type: application/json" -d '{"text":"documento de exemplo com mais de vinte caracteres"}'
```

## Endpoints da API

| Metodo | Rota | Descricao |
| :--- | :--- | :--- |
| GET | `/health` | Health check. |
| GET | `/version` | Retorna a `IMAGE_VERSION` gravada no build — prova qual tag esta rodando. |
| POST | `/classify` | Classificacao simulada; ecoa a versao da imagem que respondeu. |

## Por que `/dev/null` no `acr purge`?

`/dev/null` e o "dispositivo nulo" do Linux: descarta o que recebe e le vazio. No `az acr run`/`az acr task create`, o ultimo argumento (ou `--context`) e o **contexto de build** — normalmente a pasta com o codigo-fonte e o `Dockerfile`. Como `acr purge` apenas apaga manifestos e nao constroi nada, nao ha contexto para enviar; `/dev/null` sinaliza "contexto intencionalmente vazio". Analogia .NET: passar `string.Empty` de proposito onde um metodo pede um caminho que aquela operacao nao usa.
