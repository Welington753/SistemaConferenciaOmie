# Script para limpar o banco de dados e arquivos temporários do sistema

$appData = [Environment]::GetFolderPath("LocalApplicationData")
$dbPath = Join-Path $appData "SistemaConferenciaPedidos\Banco\conferencia.db"
$pdfTempDir = Join-Path $appData "SistemaConferenciaPedidos\Etiquetas"

Write-Host "Iniciando limpeza do ambiente..."

if (Test-Path $dbPath) {
    Remove-Item $dbPath -Force
    Write-Host "Banco de dados removido: $dbPath"
} else {
    Write-Host "Banco de dados não encontrado."
}

if (Test-Path $pdfTempDir) {
    Remove-Item -Path "$pdfTempDir\*" -Recurse -Force
    Write-Host "Arquivos temporários e PDFs gerenciados removidos de: $pdfTempDir"
} else {
    Write-Host "Pasta de PDFs gerenciados não encontrada."
}

Write-Host "Limpeza concluída! Ao abrir o sistema, um novo banco será criado limpo."
