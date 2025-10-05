Add-Type -AssemblyName System.Drawing

# Lista nazw awatarów
$avatars = @(
    "turtle_default",
    "robot_blue", 
    "cat_smart",
    "owl_professor",
    "fox_zen",
    "dragon_wisdom",
    "phoenix_energy",
    "cosmic_brain",
    "time_master"
)

# Ścieżka do folderu Images
$imagesPath = "C:\Users\slawek\RiderProjects\HACKYEAH-BIOHACKING\NeuroMate\NeuroMate\Resources\Images"

# Tworzenie małych obrazów PNG (32x32 pikseli)
foreach ($avatar in $avatars) {
    $bitmap = New-Object System.Drawing.Bitmap(32, 32)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    
    # Różne kolory dla różnych awatarów
    $color = switch ($avatar) {
        "turtle_default" { [System.Drawing.Color]::Green }
        "robot_blue" { [System.Drawing.Color]::Blue }
        "cat_smart" { [System.Drawing.Color]::Orange }
        "owl_professor" { [System.Drawing.Color]::Brown }
        "fox_zen" { [System.Drawing.Color]::Red }
        "dragon_wisdom" { [System.Drawing.Color]::Purple }
        "phoenix_energy" { [System.Drawing.Color]::OrangeRed }
        "cosmic_brain" { [System.Drawing.Color]::DarkBlue }
        "time_master" { [System.Drawing.Color]::Gold }
        default { [System.Drawing.Color]::Gray }
    }
    
    $brush = New-Object System.Drawing.SolidBrush($color)
    $graphics.FillRectangle($brush, 0, 0, 32, 32)
    
    # Zapisz obraz PNG
    $filePath = Join-Path $imagesPath "$avatar.png"
    $bitmap.Save($filePath, [System.Drawing.Imaging.ImageFormat]::Png)
    
    $graphics.Dispose()
    $bitmap.Dispose()
    $brush.Dispose()
    
    Write-Host "Utworzono: $filePath"
}

Write-Host "Wszystkie obrazy PNG zostały utworzone!"
