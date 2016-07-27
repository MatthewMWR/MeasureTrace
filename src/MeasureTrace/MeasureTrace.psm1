Add-Type -Path (Join-Path $psscriptroot "MeasureTrace.dll")
Update-TypeData -TypeName MeasureTrace.TraceModel.Trace -MemberType ScriptProperty -MemberName Measurements -Value {$this.GetMeasurementsAll() | Group-Object -Property {$_.GetType().Name} -AsHashTable -AsString} -Force
$ErrorActionPreference = 'Stop'
<#
.Synopsis
   Derives measurements (facts as per an observer) from ETL files. Wraps TraceEvent.
.DESCRIPTION
    Derives measurements (facts as per an observer) from ETL files. Wraps TraceEvent.
.EXAMPLE
   Measure-Trace -Path .\WprPerformanceLog.etl
.INPUTS
   System.String (path to ETL)
.OUTPUTS
   MeasureTrace.TraceModel.Trace
#>	
function Measure-Trace {
	[OutputType([MeasureTrace.TraceModel.Trace])]
	param(
		[Parameter(Mandatory=$true, ValueFromPipeline=$true)]
		[string[]]$Path
	)
	begin{}
	process{
		foreach($item in $Path){
			$resolvedPathInfo = Resolve-Path -Path $item
			$null = Test-Path -Path $resolvedPathInfo.Path -ErrorAction Stop
			$tj = New-Object -TypeName MeasureTrace.TraceJob -ArgumentList $resolvedPathInfo.Path
			$tj.StageForProcessing()
			[MeasureTrace.TraceJobExtension]::RegisterCalipersAllKnown($tj)
			$tj.Measure()
			$tj.Dispose()
			$tj = $null
		}
	}
}