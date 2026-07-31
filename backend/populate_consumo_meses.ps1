$token = "aoIkRNN8xjBAJyLoDXggCYTuJw3oAY4dJtxyqDEhdGM="
$base  = "http://localhost:5295/api/AparatosConsumoHistorico/registrar_lote"
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type"  = "application/json"
}

# Dispositivos: sk_aparato_id => perfil de consumo (potencia base W, variacion)
$dispositivos = @(
    @{ id = 1; nombre = "Bocina Principal";   potBase = 75.0;  potVar = 20.0; corrBase = 0.63; corrVar = 0.17 },
    @{ id = 2; nombre = "Bocina Secundaria";  potBase = 60.0;  potVar = 15.0; corrBase = 0.50; corrVar = 0.13 },
    @{ id = 3; nombre = "Bocina Banio";       potBase = 30.0;  potVar = 10.0; corrBase = 0.25; corrVar = 0.08 }
)

# Meses a poblar: inicio y fin
$rangos = @(
    @{ inicio = [datetime]"2026-04-01"; fin = [datetime]"2026-04-30" },
    @{ inicio = [datetime]"2026-05-01"; fin = [datetime]"2026-05-31" },
    @{ inicio = [datetime]"2026-06-01"; fin = [datetime]"2026-06-30" }
)

$rng = [System.Random]::new(42)

function Get-Ruido($base, $variacion, $hora) {
    # Consumo mas alto en manana (8-12) y tarde-noche (18-23)
    $factorHora = 1.0
    if ($hora -ge 8  -and $hora -le 12) { $factorHora = 1.25 }
    if ($hora -ge 18 -and $hora -le 23) { $factorHora = 1.15 }
    if ($hora -ge 0  -and $hora -le 5)  { $factorHora = 0.30 }  # madrugada, casi apagado
    $ruido = ($rng.NextDouble() * 2 - 1) * $variacion
    return [math]::Max(0.5, ($base + $ruido) * $factorHora)
}

$totalEnviados = 0

foreach ($rango in $rangos) {
    $mes = $rango.inicio.ToString("MMMM yyyy")
    Write-Host ""
    Write-Host "=== Procesando $mes ===" -ForegroundColor Cyan

    # Energias acumuladas por dispositivo (reset al inicio de cada mes)
    $energiaAcum = @{ 1 = 0.0; 2 = 0.0; 3 = 0.0 }

    $fecha = $rango.inicio
    while ($fecha -le $rango.fin) {
        $lote = @()

        foreach ($dev in $dispositivos) {
            for ($hora = 0; $hora -lt 24; $hora++) {
                $fechaMedicion = $fecha.AddHours($hora)
                $pot  = Get-Ruido $dev.potBase  $dev.potVar  $hora
                $corr = Get-Ruido $dev.corrBase $dev.corrVar $hora

                # Energia acumulada (Wh) = suma de potencia * 1h
                $energiaAcum[$dev.id] += $pot
                $enWh = [math]::Round($energiaAcum[$dev.id], 3)

                $lote += @{
                    sk_aparato_id = $dev.id
                    corriente_a   = [math]::Round($corr, 4)
                    potencia_w    = [math]::Round($pot,  4)
                    energia_wh    = $enWh
                    fecha_medicion = $fechaMedicion.ToString("o")
                }
            }
        }

        # Enviar lote del dia (72 lecturas: 3 dispositivos x 24 horas)
        $body = $lote | ConvertTo-Json -Depth 3
        try {
            $resp = Invoke-RestMethod -Method POST -Uri $base -Headers $headers -Body $body -ErrorAction Stop
            $totalEnviados += $lote.Count
            Write-Host "  $($fecha.ToString('dd/MM/yyyy'))  -> $($lote.Count) lecturas enviadas" -ForegroundColor Green
        } catch {
            Write-Host "  ERROR en $($fecha.ToString('dd/MM/yyyy')): $_" -ForegroundColor Red
        }

        $fecha = $fecha.AddDays(1)
    }

    Write-Host "  Mes completado. Acumulado Bocina Principal: $([math]::Round($energiaAcum[1]/1000,2)) kWh" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "DONE. Total lecturas insertadas: $totalEnviados" -ForegroundColor Magenta
