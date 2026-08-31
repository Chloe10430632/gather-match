param(
    [string]$OutputPath = (
        Join-Path $PSScriptRoot '..\asp-gather-match\asp-gather-match\asp-gather-match\Data\SeedData\taiwan-location-reference.json'
    )
)

$ErrorActionPreference = 'Stop'
$sourceBaseUrl = 'https://api.nlsc.gov.tw/other'
$generatedAt = [DateTimeOffset]::UtcNow

[xml]$countyXml = (Invoke-WebRequest -UseBasicParsing "$sourceBaseUrl/ListCounty").Content
$cities = [System.Collections.Generic.List[object]]::new()
$districtCodes = [System.Collections.Generic.HashSet[string]]::new()
$cityCodes = [System.Collections.Generic.HashSet[string]]::new()
$citySortOrder = 0

foreach ($county in $countyXml.countyItems.countyItem) {
    $citySortOrder++
    $cityGovernmentCode = [string]$county.countycode01

    if ($cityGovernmentCode -notmatch '^\d{5}$') {
        throw "Invalid city code: $cityGovernmentCode"
    }

    if (-not $cityCodes.Add($cityGovernmentCode)) {
        throw "Duplicate city code: $cityGovernmentCode"
    }

    [xml]$townXml = (
        Invoke-WebRequest -UseBasicParsing "$sourceBaseUrl/ListTown1/$($county.countycode)"
    ).Content

    $districts = [System.Collections.Generic.List[object]]::new()
    $districtSortOrder = 0

    foreach ($town in $townXml.townItems.townItem) {
        $districtSortOrder++
        $districtGovernmentCode = [string]$town.towncode

        if ($districtGovernmentCode -notmatch '^\d{8}$') {
            throw "Invalid district code: $districtGovernmentCode"
        }

        if (-not $districtCodes.Add($districtGovernmentCode)) {
            throw "Duplicate district code: $districtGovernmentCode"
        }

        $districts.Add([ordered]@{
            governmentCode = $districtGovernmentCode
            name = [string]$town.townname
            sortOrder = $districtSortOrder
        })
    }

    $cities.Add([ordered]@{
        governmentCode = $cityGovernmentCode
        name = [string]$county.countyname
        sortOrder = $citySortOrder
        districts = $districts
    })
}

if ($cities.Count -ne 22) {
    throw "Expected 22 cities/counties, received $($cities.Count)."
}

if ($districtCodes.Count -lt 350) {
    throw "Expected at least 350 districts, received $($districtCodes.Count)."
}

$snapshot = [ordered]@{
    source = '內政部國土測繪中心 ListCounty / ListTown1'
    sourceUrl = $sourceBaseUrl
    generatedAt = $generatedAt.ToString('O')
    isCompleteSnapshot = $true
    cities = $cities
}

$resolvedOutputPath = [System.IO.Path]::GetFullPath($OutputPath)
$outputDirectory = Split-Path -Parent $resolvedOutputPath
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
$snapshot | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $resolvedOutputPath -Encoding utf8

Write-Output "Created snapshot: $resolvedOutputPath"
Write-Output "Cities/counties: $($cities.Count)"
Write-Output "Districts: $($districtCodes.Count)"
