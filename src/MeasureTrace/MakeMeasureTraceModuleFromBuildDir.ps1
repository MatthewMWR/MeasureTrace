param($Source = $psscriptroot, $Destination = $psscriptroot)
$destinationFolderFull = Join-Path $psscriptroot "MeasureTrace"
if(Test-Path -Path $destinationFolderFull -ErrorAction SilentlyContinue){ Remove-Item -Path $destinationFolderFull -Recurse -Force}
New-Item -ItemType Directory -Path $destinationFolderFull
cd $Source
Copy-Item "MeasureTrace.dll","MeasureTrace.psd1","MeasureTrace.psm1","Microsoft.Diagnostics.Tracing.TraceEvent.dll" $destinationFolderFull
return $destinationFolderFull