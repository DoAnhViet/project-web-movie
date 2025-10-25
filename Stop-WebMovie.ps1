param(
    [switch]$Force
)

Write-Host "Stopping WebMovie processes and freeing bin\Debug\net7.0\WebMovie.exe (if running)..." -ForegroundColor Cyan

$stopped = $false

# Try to stop by process name
$byName = Get-Process -Name WebMovie -ErrorAction SilentlyContinue
if ($byName) {
    foreach ($p in $byName) {
        Write-Host "Stopping process $($p.ProcessName) (Id=$($p.Id))" -ForegroundColor Yellow
        try {
            Stop-Process -Id $p.Id -Force:$Force -ErrorAction Stop
            Write-Host "Stopped PID $($p.Id)" -ForegroundColor Green
            $stopped = $true
        } catch {
            Write-Host ("Failed to stop PID {0}: {1}" -f $($p.Id), $_.Exception.Message) -ForegroundColor Red
        }
    }
}

# Try to find the process owning port 5001 (HTTPS) and stop it
try {
    $conn = Get-NetTCPConnection -LocalPort 5001 -State Listen -ErrorAction SilentlyContinue
    if ($conn) {
        $ownerPid = $conn.OwningProcess
        if ($ownerPid -and -not ($byName | Where-Object { $_.Id -eq $ownerPid })) {
            Write-Host "Process owning port 5001: PID=$ownerPid" -ForegroundColor Yellow
            try {
                Stop-Process -Id $ownerPid -Force:$Force -ErrorAction Stop
                Write-Host "Stopped PID $ownerPid (was using port 5001)" -ForegroundColor Green
                $stopped = $true
            } catch {
                Write-Host ("Failed to stop PID {0}: {1}" -f $ownerPid, $_.Exception.Message) -ForegroundColor Red
            }
        }
    }
} catch {
    # Get-NetTCPConnection may not exist on older Windows; ignore
}

if (-not $stopped) {
    Write-Host "No WebMovie process found by name or port 5001. If you still see a locked WebMovie.exe, try closing it in Task Manager or rebooting." -ForegroundColor Yellow
}

Write-Host "Done." -ForegroundColor Cyan
