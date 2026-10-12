param([string]$UnityEditorPath)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
# Use Unity serialization so MonsterDefinition references and existing rules survive reruns.
if ($UnityEditorPath) {
    & $UnityEditorPath -batchmode -quit -projectPath $projectRoot -executeMethod NecrocisEditor.BiomeEnemySetup.Run
} elseif (Get-Command unity -ErrorAction SilentlyContinue) {
    & unity run $projectRoot -- -executeMethod NecrocisEditor.BiomeEnemySetup.Run
} else {
    throw 'Install Unity CLI or pass -UnityEditorPath. In an open Editor use Necrocis > Balance > Configure Biome Normal Enemies.'
}
if ($LASTEXITCODE -ne 0) { throw "Unity enemy configuration failed (exit $LASTEXITCODE)." }
