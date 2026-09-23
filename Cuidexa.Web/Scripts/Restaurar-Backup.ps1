<#
.SINOPSIS
    Descifra un backup de Cuidexa (formato de BackupService/BackupCifrado)
    y, opcionalmente, lo restaura con pg_restore.

.DESCRIPCION
    Nunca se ejecuta automáticamente — una restauración es una operación
    deliberada y manual. El fichero cifrado tiene el formato:
    nonce (12 bytes) + texto cifrado (AES-256-GCM) + tag de autenticación
    (16 bytes), igual que Services/BackupCifrado.cs.

.EJEMPLO
    ./Restaurar-Backup.ps1 -ArchivoCifrado backups/cuidexa-20260923-020000.dump.enc -ClaveBase64 "..." -SalidaDump restaurado.dump

.EJEMPLO (con restauración directa a una base de datos de pruebas)
    ./Restaurar-Backup.ps1 -ArchivoCifrado backups/cuidexa-20260923-020000.dump.enc -ClaveBase64 "..." -SalidaDump restaurado.dump -ConnectionString "Host=localhost;Port=5432;Database=cuidexa_restaurada;Username=postgres;Password=..." -PgRestorePath "pg_restore"
#>
param(
    [Parameter(Mandatory = $true)][string]$ArchivoCifrado,
    [Parameter(Mandatory = $true)][string]$ClaveBase64,
    [Parameter(Mandatory = $true)][string]$SalidaDump,
    [string]$ConnectionString,
    [string]$PgRestorePath = "pg_restore"
)

$ErrorActionPreference = "Stop"

$NonceSizeBytes = 12
$TagSizeBytes = 16

$datos = [System.IO.File]::ReadAllBytes($ArchivoCifrado)
if ($datos.Length -le ($NonceSizeBytes + $TagSizeBytes)) {
    throw "El archivo cifrado es demasiado pequeño para contener nonce+tag — ¿ruta correcta?"
}

$nonce = $datos[0..($NonceSizeBytes - 1)]
$tag = $datos[($datos.Length - $TagSizeBytes)..($datos.Length - 1)]
$cipherLength = $datos.Length - $NonceSizeBytes - $TagSizeBytes
$cipherBytes = $datos[$NonceSizeBytes..($NonceSizeBytes + $cipherLength - 1)]

$clave = [Convert]::FromBase64String($ClaveBase64)
$plainBytes = New-Object byte[] $cipherLength

$aesGcm = [System.Security.Cryptography.AesGcm]::new($clave, $TagSizeBytes)
try {
    $aesGcm.Decrypt($nonce, $cipherBytes, $tag, $plainBytes)
}
finally {
    $aesGcm.Dispose()
}

[System.IO.File]::WriteAllBytes($SalidaDump, $plainBytes)
Write-Host "Backup descifrado en: $SalidaDump ($([Math]::Round($plainBytes.Length / 1MB, 2)) MB)"

if ($ConnectionString) {
    $conexionEnmascarada = $ConnectionString -replace "password=[^;]*", "password=***"
    Write-Host "Restaurando con $PgRestorePath sobre: $conexionEnmascarada"
    & $PgRestorePath "--dbname=$ConnectionString" "--clean" "--if-exists" $SalidaDump
    if ($LASTEXITCODE -ne 0) {
        throw "pg_restore terminó con código $LASTEXITCODE"
    }
    Write-Host "Restauración completada."
}
else {
    Write-Host "Sin -ConnectionString: solo se ha descifrado el .dump, no se ha restaurado nada."
}
