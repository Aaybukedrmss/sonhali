# SQL Server bağlantı bilgileri
$server = "DESKTOP-FF0LMVI\SQLEXPRESS"
$database = "ShopCo"
$sqlScript = Get-Content "fix_database.sql" -Raw

# SQL Server'a bağlan ve script'i çalıştır
try {
    $connectionString = "Server=$server;Database=$database;Trusted_Connection=True;TrustServerCertificate=True;"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    $command = New-Object System.Data.SqlClient.SqlCommand($sqlScript, $connection)
    $command.CommandTimeout = 30
    
    Write-Host "Veritabanına bağlanıldı. Script çalıştırılıyor..." -ForegroundColor Green
    
    $result = $command.ExecuteNonQuery()
    
    Write-Host "Script başarıyla çalıştırıldı!" -ForegroundColor Green
    Write-Host "Eklenen/değiştirilen satır sayısı: $result" -ForegroundColor Yellow
}
catch {
    Write-Host "Hata oluştu: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    if ($connection -and $connection.State -eq 'Open') {
        $connection.Close()
        Write-Host "Veritabanı bağlantısı kapatıldı." -ForegroundColor Gray
    }
}
