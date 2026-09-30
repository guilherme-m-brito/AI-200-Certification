# Tags, Versionamento e Ciclo de Vida de Imagens no ACR

Data de Estudo: 29 de setembro de 2026
Referencia oficial: [Microsoft Learn - Imagens de tag e versao](https://learn.microsoft.com/pt-br/training/modules/store-manage-containers-azure-container-registry/4-tag-version-images?pivots=text)

---

## 1. O problema central: Tags estaveis vs. Tags unicas

A estrategia de tag define confiabilidade de deploy, capacidade de rollback e manutencao das imagens. A tag e apenas um **ponteiro mutavel** para um digest imutavel (visto no resumo `02-acr-conceitos.md`).

*Analogia .NET:* pense na diferenca entre referenciar um pacote NuGet como `1.*` (floating version, pega o mais recente) versus fixar `1.2.0` no `.csproj`. A floating version e conveniente mas nao reproduzivel; a versao fixa e previsivel e auditavel.

### Tags estaveis (`v1`, `v1.2`, `latest`)
Reutilizadas em varios pushes. Ao publicar uma nova imagem com uma tag existente, a tag passa a apontar para a imagem nova; a imagem antiga continua no registro, mas **perde a referencia de tag** (vira "orfa").

Boas para:
* **Imagens base** que recebem patches de seguranca (builds dependentes pegam a correcao automaticamente).
* **Ambientes de desenvolvimento** (sempre o mais recente sem mexer na config de deploy).
* **Entrega continua** onde o consumidor sempre deve receber a versao atual.

Desvantagem: **imprevisibilidade**. Nos diferentes podem puxar a mesma tag em momentos diferentes e receber imagens diferentes. Em APIs de inferencia de IA isso gera comportamento inconsistente (nos rodando versoes de modelo distintas).

### Tags unicas (`v1.2.0-build456`, `20260102-abc123`)
Nunca reutilizadas. Cada push cria uma tag nova, preservando todas as versoes anteriores.

Boas para:
* **Producao** — cada no do cluster puxa exatamente a mesma imagem.
* **Trilha de auditoria** — sabe-se exatamente o que estava implantado em cada momento.
* **Rollback** — basta apontar para uma versao anterior conhecida.
* **Conformidade** — comprovar qual imagem rodava durante um incidente.

Contrapartida: exige atualizar a config de deploy a cada versao. Em producao isso e considerado **beneficio**, porque toda mudanca passa a ser intencional.

---

## 2. Versionamento semantico (SemVer): `MAJOR.MINOR.PATCH`

Mesmo contrato do SemVer que voce ja usa em pacotes NuGet:

* **MAJOR:** mudanca incompativel (breaking change), ex.: alteracao de contrato de API ou recurso removido.
* **MINOR:** novo recurso retrocompativel.
* **PATCH:** correcao de bug ou patch de seguranca sem mudar a API.

> **Nota / armadilha de prova:** o gatilho para subir cada numero e a **compatibilidade**, nao o tamanho da mudanca. Remover um campo obrigatorio, renomear/remover um endpoint ou alterar o contrato de resposta e **breaking change** e incrementa o **MAJOR** (`1.4.2` -> `2.0.0`), zerando MINOR e PATCH. So use MINOR (`1.5.0`) quando o recurso novo for **retrocompativel**. Regra rapida: "quebrou quem ja consumia" -> sobe o MAJOR.

Progressao de exemplo para uma API de inferencia:

```text
inference-api:1.0.0    # Release inicial
inference-api:1.0.1    # Correcao no pre-processamento
inference-api:1.1.0    # Novo endpoint de modelo
inference-api:2.0.0    # Breaking change na API
```

### Combinar tags estaveis + unicas (estrategia recomendada)
Publicar a mesma imagem sob varias tags da flexibilidade ao consumidor escolher a politica de atualizacao:

```text
inference-api:1        # Aponta para o ultimo 1.x.x  (tag estavel)
inference-api:1.1      # Aponta para o ultimo 1.1.x  (tag estavel)
inference-api:1.1.0    # Patch especifico            (tag unica)
```

Quem referencia `:1` recebe atualizacoes dentro da major; quem referencia `:1.1.0` fica travado no patch exato.

*Analogia .NET:* identico ao comportamento de `[1.0,2.0)` vs. `1.1.0` em ranges de versao NuGet.

---

## 3. Padroes de tags unicas para rastreabilidade

| Padrao | Exemplo | Vincula a... |
| :--- | :--- | :--- |
| **Build ID** (CI/CD) | `inference-api:build-4567` | Execucao do pipeline (logs, testes, artefatos) |
| **Git commit SHA** | `inference-api:abc123f` | Codigo-fonte exato (nao precisa acessar o CI/CD) |
| **Timestamp** | `inference-api:20260102-143022` | Ordenacao cronologica / idade da imagem |
| **Combinado** | `inference-api:v1.2.0-build4567-abc123f` | Tudo acima (ideal em resposta a incidentes) |

Em ACR Tasks, a variavel `{{.Run.ID}}` gera um identificador unico por build automaticamente:

```bash
az acr build --registry myregistry \
  --image inference-api:v1.2.0-{{.Run.ID}} .
```

---

## 4. A tag `latest` — armadilha classica de prova e producao

`latest` e o padrao do Docker quando voce faz push/pull sem especificar tag. Conveniente, mas problematico:

* **Deploys inconsistentes:** nos puxam `latest` em momentos diferentes e recebem imagens diferentes.
* **Atualizacoes imprevisiveis:** o deploy muda quando alguem faz push, sem deploy intencional.
* **Troubleshooting dificil:** nao se sabe qual versao esta rodando.

Regra para a prova: em manifestos Kubernetes e configs de producao, **sempre usar tag explicita**.

```yaml
# Evite em producao
image: myregistry.azurecr.io/inference-api:latest

# Use versoes explicitas
image: myregistry.azurecr.io/inference-api:v1.2.0
```

Reserve `latest` para desenvolvimento, onde conveniencia supera consistencia.

---

## 5. Bloquear imagens implantadas (image lock)

O ACR permite bloquear imagens de producao para evitar exclusao ou sobrescrita acidental. Desabilita escrita via `--write-enabled false`:

```bash
az acr repository update \
  --name myregistry \
  --image inference-api:v1.2.0 \
  --write-enabled false
```

Imagem bloqueada:
* **Nao pode ser excluida** (nem por administradores).
* **Nao pode ser sobrescrita** (push com a mesma tag falha).
* **Sobrevive a politicas de retencao** (a limpeza automatica nao a remove).
* **Garante estabilidade** do workload de producao.

Para aposentar a versao, desbloqueie antes:

```bash
az acr repository update \
  --name myregistry \
  --image inference-api:v1.2.0 \
  --write-enabled true
```

*Analogia .NET:* equivale a marcar uma versao de pacote como "listed/locked" em um feed para impedir republicacao acidental daquela versao.

---

## 6. Limpeza de imagens nao etiquetadas (untagged / orfas)

Quando uma tag estavel e reutilizada, a imagem antiga fica **sem tag** e continua consumindo armazenamento. Com o tempo, essas orfas aumentam o custo.

### `acr purge` sob demanda
Roda como um container dentro do ACR Tasks:

```bash
az acr run --registry myregistry \
  --cmd "acr purge --filter 'inference-api:.*' --untagged --ago 30d" \
  /dev/null
```

Remove imagens sem tag do repositorio `inference-api` com mais de 30 dias. O `--filter` usa regex sobre o nome do repositorio.

### `acr purge` agendado (tarefa recorrente)

```bash
az acr task create \
  --registry myregistry \
  --name cleanup-untagged \
  --cmd "acr purge --filter '.*:.*' --untagged --ago 7d" \
  --schedule "0 0 * * 0" \
  --context /dev/null
```

Roda semanalmente (cron `0 0 * * 0` = domingo 00:00) e remove orfas com mais de 7 dias em todos os repositorios.

### Politicas de retencao (somente tier Premium)
Alternativa mais simples que a tarefa agendada: uma unica politica no nivel do registro remove automaticamente manifestos sem tag apos N dias, sem gerenciar schedule nem filtros.

*Ponto de atencao da prova:* politica de retencao = **Premium**; `acr purge` via Task = qualquer tier.

---

## 7. Praticas recomendadas (checklist para a prova)

1. **Tags unicas em producao** — consistencia entre todos os nos.
2. **Tags estaveis para imagens base** — deixar patches de seguranca fluirem via gatilho de imagem base.
3. **Bloquear imagens de producao** — evitar exclusao acidental; desbloquear so ao aposentar.
4. **Politicas de retencao / limpeza** — remover orfas e controlar custo de armazenamento.
5. **Incluir metadados de build** — build ID, commit SHA e timestamp para auditoria.
6. **Documentar o esquema de tags** — deixar claro o que e estavel vs. unico e quando usar cada um.

---

## Resumo de decisao rapida

| Situacao | Escolha |
| :--- | :--- |
| Deploy em producao | Tag unica + imagem bloqueada |
| Imagem base com patches | Tag estavel + gatilho de base image update |
| Ambiente de dev | Tag estavel / `latest` aceitavel |
| Rastrear codigo-fonte de uma imagem | Tag com Git commit SHA |
| Rastrear pipeline que gerou a imagem | Tag com build ID / `{{.Run.ID}}` |
| Limpar orfas em tier nao-Premium | ACR Task agendada com `acr purge` |
| Limpar orfas em tier Premium | Politica de retencao no registro |
