<#
.SYNOPSIS
    Remove os recursos Azure criados no Lab 01 (ACR Tasks).
.DESCRIPTION
    Apaga o Resource Group inteiro, o que remove o ACR e todas as imagens.
    Registrar a limpeza faz parte da diretriz de controle de custo do repositorio.
.PARAMETER ResourceGroup
    Nome do Resource Group a remover. Padrao: rg-ai200-acrlab.
.EXAMPLE
    ./cleanup.ps1
.EXAMPLE
    ./cleanup.ps1 -ResourceGroup rg-ai200-acrlab
#>
param(
    [string]$ResourceGroup = "rg-ai200-acrlab"
)

$exists = az group exists --name $ResourceGroup | ConvertFrom-Json

if (-not $exists) {
    Write-Host "Resource Group '$ResourceGroup' nao existe. Nada a remover."
    return
}

Write-Host "Removendo Resource Group '$ResourceGroup' (ACR e imagens incluidos)..."
az group delete --name $ResourceGroup --yes --no-wait
Write-Host "Exclusao iniciada em background (--no-wait)."
