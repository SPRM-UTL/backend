$connStr = "Server=localhost;Database=pruebaasp;Uid=root;Pwd=;"
Add-Type -Path "C:\Program Files\PowerShell\7\Modules\MySql.Data.dll" -ErrorAction SilentlyContinue

# Fallback: using basic HTTP to login as 'admin@itgu.com' or another user if needed
$loginJson = '{ "correo": "admin@itgu.com", "contrasenia": "admin123" }'
$loginResp = Invoke-RestMethod -Uri "http://localhost:5295/api/Auth/login" -Method Post -Body $loginJson -ContentType "application/json" -ErrorAction SilentlyContinue

$token = $loginResp.data.token
if (-not $token) {
    Write-Host "Failed to login"
    exit
}

Write-Host "Got token: $token"

$gestoJson = @"
{
  "sk_gesto_id": 0,
  "bk_gesto_id": 0,
  "nombre_gesto": "Prueba Combo Script",
  "identificador_ia": 0,
  "nivel_confianza_minimo": 0.5,
  "tipo_disparador_nombre": "COMBO",
  "sk_aparato_id": null,
  "pasos": [
    {
      "sk_gesto_paso_id": 0,
      "orden": 1,
      "es_activador": true,
      "nombre_gesto": "TE AMO ILY",
      "mano_objetivo": "ANY",
      "cuadros_requeridos": 15
    }
  ]
}
"@

try {
    $resp = Invoke-RestMethod -Uri "http://localhost:5295/api/Dim_Gestos" `
        -Method Post `
        -Body $gestoJson `
        -ContentType "application/json" `
        -Headers @{ "Authorization" = "Bearer $token" }
    
    Write-Host "Success! Response:"
    $resp | ConvertTo-Json -Depth 5
} catch {
    Write-Host "Error occurred!"
    Write-Host $_.Exception.Message
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $errBody = $reader.ReadToEnd()
        Write-Host "Error Body: $errBody"
    }
}
