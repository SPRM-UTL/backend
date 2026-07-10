$ErrorActionPreference = "Stop"

$rand = Get-Random
$email = "test$rand@itgu.com"
$pass = "Password123!"

# Register
$regJson = "{ `"nombre`": `"Test User`", `"correo`": `"$email`", `"contrasenia`": `"$pass`" }"
Write-Host "Registering $email..."
$regResp = Invoke-RestMethod -Uri "http://localhost:5295/api/Auth/register" -Method Post -Body $regJson -ContentType "application/json"
Write-Host "Register Resp: $($regResp | ConvertTo-Json)"

# Login
$loginJson = "{ `"correo`": `"$email`", `"contrasenia`": `"$pass`" }"
Write-Host "Logging in..."
$loginResp = Invoke-RestMethod -Uri "http://localhost:5295/api/Auth/login" -Method Post -Body $loginJson -ContentType "application/json"
$token = $loginResp.data.token

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
    Write-Host "Creating Gesto..."
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
