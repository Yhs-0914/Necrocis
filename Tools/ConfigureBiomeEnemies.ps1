$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$configRoot = Join-Path $root 'Assets/_Project/Data/BiomeConfigs'
$spriteRoot = Join-Path $root 'Assets/_Project/Art/Images/Enemies/BiomeExclusive'
$utf8 = New-Object System.Text.UTF8Encoding($false)
$original = [IO.File]::ReadAllText((Join-Path $configRoot 'IntestineEnemySpawnConfig.asset'))
$blocks = [regex]::Matches($original, '(?ms)^  - name: .*?(?=^  - name: |\z)')
$templates = @{}
foreach ($block in $blocks) {
    $name = [regex]::Match($block.Value, '^  - name: ([^\r\n]+)').Groups[1].Value
    $templates[$name] = $block.Value.TrimEnd()
}
# Id, runtime name, biome, ranged, HP, movement, range, cooldown, reward.
$roster = @(
    @('01_stomach_gulp','StomachGulp','Stomach',1,4,0.8,7,1.5,15),
    @('02_stomach_mucus_snail','StomachMucusSnail','Stomach',0,6,0.8,1.5,1.1,10),
    @('03_stomach_overfed_sac','StomachOverfedSac','Stomach',1,6,0.8,6.5,1.8,15),
    @('04_intestine_foldworm','IntestineFoldworm','Intestine',0,6,1.1,1.5,1,10),
    @('05_intestine_villus_hand','IntestineVillusHand','Intestine',0,6,0.8,1.8,1.2,10),
    @('06_intestine_knotted_eel','IntestineKnottedEel','Intestine',0,8,0.9,1.7,1.2,12),
    @('07_liver_suture_cell','LiverSutureCell','Liver',1,4,0.8,7,1.5,15),
    @('08_liver_clot','LiverClot','Liver',0,8,0.8,1.5,1.2,12),
    @('09_liver_coagulation_core','LiverCoagulationCore','Liver',1,6,0.8,6.5,1.8,15),
    @('10_lung_breath_bubble','LungBreathBubble','Lung',1,4,0.8,7,1.5,15),
    @('11_lung_cilia_brush','LungCiliaBrush','Lung',0,6,1.1,1.7,1,10),
    @('12_lung_overinflated_alveoli','LungOverinflatedAlveoli','Lung',1,6,0.8,6.5,1.8,15)
)
function Set-Field([string]$block, [string]$field, [string]$value) {
    $pattern = '(?m)^    ' + [regex]::Escape($field) + ':.*(?:\r?\n    - [^\r\n]*)*'
    if ($block -notmatch $pattern) { return $block + "`n    ${field}: $value" }
    return [regex]::Replace($block, $pattern, "    ${field}: $value")
}
$generated = @{}
for ($i = 0; $i -lt $roster.Count; $i++) {
    $entry = $roster[$i]
    $id,$name,$biome,$ranged,$hp,$speed,$range,$cooldown,$reward = $entry
    $templateName = if ($ranged) { 'BCell' } else { 'NKCell' }
    $block = $templates[$templateName] -replace '^  - name: [^\r\n]+', "  - name: $name"
    $density = if ($biome -eq 'Intestine') { '0.02' } else { '0.05' }
    $values = @{
        density=$density; minDistance='6'; poissonSalt=(1601+$i); maxAlive='2';
        activationRadius='55'; respawnCooldown='6'; spawnRadius='2';
        moveSpeed=$speed; maxHealth=$hp; attackDamage='1'; attackRange=$range;
        attackCooldown=$cooldown; expReward=$reward; isRanged=$ranged;
        isElite='0'; chargesAtPlayer='0'; splitsOnDeath='0'; leavesDebrisOnDeath='0';
        chaseRadius='10'; leashRadius='14'; scale='{x: 1, y: 1, z: 1}';
        colliderSize='{x: 0.85, y: 1.2, z: 0.85}'; colliderCenter='{x: 0, y: 0.6, z: 0}';
        expandColliderOnAttack='0'; animationSpeed='0.16'; attackAnimationSpeed='0.14';
        deathAnimationSpeed='0.16'; attackSpritesUp='[]'; attackSpritesDown='[]';
        projectileSpeed='6'; projectileLifeTime='3'; projectileScale='{x: 0.3, y: 0.3, z: 0.3}'
    }
    foreach ($key in $values.Keys) { $block = Set-Field $block $key ([string]$values[$key]) }
    foreach ($action in @('Idle','Move','Attack','Death')) {
        $refs = @()
        for ($frame = 0; $frame -lt 4; $frame++) {
            $meta = Join-Path $spriteRoot "$id/${action}_$frame.png.meta"
            $guid = [regex]::Match([IO.File]::ReadAllText($meta), 'guid: ([a-f0-9]+)').Groups[1].Value
            if (!$guid) { throw "Missing sprite GUID: $meta" }
            $refs += "    - {fileID: 21300000, guid: $guid, type: 3}"
        }
        $block = Set-Field $block ($action.ToLower()+'Sprites') ("`n" + ($refs -join "`n"))
    }
    if (!$generated.ContainsKey($biome)) { $generated[$biome] = @() }
    $generated[$biome] += [regex]::Replace($block, '(?m)[ \t]+\r?$', '')
}
foreach ($biome in @('Stomach','Intestine','Liver','Lung')) {
    $path = Join-Path $configRoot ($biome+'EnemySpawnConfig.asset')
    $existing = [IO.File]::ReadAllText($path)
    $header = [regex]::Split($existing, '(?m)^  enemySpawnRules:')[0].TrimEnd()
    if ($header -notmatch 'normalKillsPerElite:') { $header += "`n  normalKillsPerElite: 10" }
    $retained = @()
    foreach ($match in [regex]::Matches($existing, '(?ms)^  - name: .*?(?=^  - name: |\z)')) {
        $name = [regex]::Match($match.Value, '^  - name: ([^\r\n]+)').Groups[1].Value
        if ($name -notin $roster.ForEach({$_[1]})) { $retained += $match.Value.TrimEnd() }
    }
    $content = $header + "`n  enemySpawnRules:`n" + (($retained + $generated[$biome]) -join "`n") + "`n"
    [IO.File]::WriteAllText($path, ($content -replace "`r`n", "`n"), $utf8)
    Write-Output "$biome : added/updated 3 exclusive normal enemies; retained $($retained.Count) existing rules"
}
