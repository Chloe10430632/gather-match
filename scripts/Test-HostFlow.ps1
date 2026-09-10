#requires -Version 7.5
<#
Run against the local API connected to the DEVELOPMENT database only.
Creates two labelled test accounts and two activities; never deletes data.
Credentials, cookies and the share token stay in memory and are never printed.
#>
param(
    [uri]$BaseUrl = 'http://localhost:5138',
    [short]$ActivityTypeId = 1,
    [short]$CityId = 1
)

$ErrorActionPreference = 'Stop'
Import-Module Microsoft.PowerShell.Utility
if (-not $BaseUrl.IsLoopback) { throw 'Only a loopback API URL is allowed.' }
$runId = 'host-flow-' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss') + '-' + [guid]::NewGuid().ToString('N').Substring(0, 6)
$ownerSession = [Microsoft.PowerShell.Commands.WebRequestSession]::new()
$otherSession = [Microsoft.PowerShell.Commands.WebRequestSession]::new()
$anonymous = [Microsoft.PowerShell.Commands.WebRequestSession]::new()
$checks = 0

function Assert-True([bool]$Condition, [string]$Label) {
    if (-not $Condition) { throw "Check failed: $Label" }
    $script:checks++
}

function Invoke-Check([string]$Method, [string]$Path, $Session, $Body, [int]$ExpectedStatus, [string]$ErrorCode = '') {
    $parameters = @{
        Uri = [uri]::new($BaseUrl, $Path)
        Method = $Method
        WebSession = $Session
        SkipHttpErrorCheck = $true
        TimeoutSec = 45
    }
    if ($null -ne $Body) {
        $parameters.ContentType = 'application/json; charset=utf-8'
        $parameters.Body = ConvertTo-Json -InputObject $Body -Depth 10 -Compress
    }
    try { $response = Invoke-WebRequest @parameters }
    catch { throw "Transport failure: $Method $Path (request and response omitted)." }
    Assert-True ($response.StatusCode -eq $ExpectedStatus) "$Method $Path expected $ExpectedStatus, got $($response.StatusCode)"
    try { $json = $response.Content | ConvertFrom-Json -DateKind String }
    catch { throw "Invalid JSON: $Method $Path (response omitted)." }
    Assert-True (-not [string]::IsNullOrWhiteSpace($json.traceId)) "$Method $Path traceId"
    Assert-True ($json.success -eq ($ExpectedStatus -lt 400)) "$Method $Path success envelope"
    if ($ErrorCode) { Assert-True ($json.error.code -eq $ErrorCode) "$Method $Path error code" }
    return @{ Json = $json; Headers = $response.Headers }
}

function New-TestHost([string]$Suffix, $Session) {
    $email = "$runId-$Suffix@example.invalid"
    $password = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32)) + 'aA1!'
    $registration = Invoke-Check POST '/api/auth/register' $Session @{
        email = $email; password = $password; displayName = "TEST $Suffix $runId"
    } 201
    Write-Output "Created test account: id=$($registration.Json.data.id), email=$email"
    $null = Invoke-Check POST '/api/auth/login' $Session @{ email = $email; password = [guid]::NewGuid().ToString() } 401 'invalid_credentials'
    $login = Invoke-Check POST '/api/auth/login' $Session @{ email = $email; password = $password } 200
    Assert-True ($login.Json.data.id -eq $registration.Json.data.id) 'Login returns registered user'
    $password = $null
}

try {
    $null = Invoke-Check GET '/api/activities/1' $anonymous $null 401 'unauthorized'
    New-TestHost 'owner' $ownerSession
    New-TestHost 'other' $otherSession

    $deadline = [DateTimeOffset]::UtcNow.AddDays(2).ToOffset([TimeSpan]::FromHours(8)).ToString('o')
    $request = @{
        title = "TEST $runId"
        activityTypeId = $ActivityTypeId; cityId = $CityId
        budgetMin = 100; budgetMax = 500; deadlineAt = $deadline
        dateOptions = @(
            @{ optionDate = [DateTime]::UtcNow.AddDays(3).ToString('yyyy-MM-dd'); startTime = '18:00:00'; endTime = '20:00:00' }
            @{ optionDate = [DateTime]::UtcNow.AddDays(4).ToString('yyyy-MM-dd'); startTime = '18:00:00'; endTime = '20:00:00' }
        )
        placeOptions = @(
            @{ displayLabel = "TEST place A $runId"; customAddress = 'Test address A' }
            @{ displayLabel = "TEST place B $runId" }
        )
    }
    $null = Invoke-Check POST '/api/activities' $anonymous $request 401 'unauthorized'
    $created = Invoke-Check POST '/api/activities' $ownerSession $request 201
    $activityId = $created.Json.data.id
    Write-Output "Created test activity: id=$activityId, label=TEST $runId (2 date options, 2 place options)"
    Assert-True (-not [string]::IsNullOrWhiteSpace($created.Json.data.shareToken)) 'Creation returns share token'
    Assert-True (($created.Headers.Location -join '') -eq "/api/activities/$activityId") 'Created Location'
    $created = $null

    $read = Invoke-Check GET "/api/activities/$activityId" $ownerSession $null 200
    Assert-True ($read.Json.data.title -eq $request.title) 'Persisted title'
    Assert-True ($read.Json.data.dateOptions.Count -eq 2 -and $read.Json.data.placeOptions.Count -eq 2) 'Persisted aggregate'
    # PostgreSQL timestamps have microsecond precision; .NET also carries 100 ns ticks.
    Assert-True ([Math]::Abs((([DateTimeOffset]$read.Json.data.deadlineAt) - ([DateTimeOffset]$deadline)).Ticks) -lt 10) 'Deadline preserves instant within PostgreSQL precision'
    Assert-True (([DateTimeOffset]$read.Json.data.deadlineAt).Offset -eq [TimeSpan]::Zero) 'Deadline returned as UTC'
    Assert-True (($read.Json.data | ConvertTo-Json -Depth 10) -notmatch '(?i)token|hash') 'Read excludes tokens and hashes'
    $null = Invoke-Check GET "/api/activities/$activityId" $otherSession $null 404 'not_found'
    $null = Invoke-Check GET '/api/activities/9223372036854775807' $ownerSession $null 404 'not_found'

    $path = "/api/activities/$activityId"
    $before = $read.Json.data
    $fixedFields = @('id', 'activityTypeId', 'budgetMin', 'budgetMax', 'currencyCode', 'cityId', 'districtId', 'deadlineAt', 'status', 'createdAt', 'placeOptions')
    $fixedBefore = $before | Select-Object -Property $fixedFields | ConvertTo-Json -Depth 10 -Compress
    $firstDateId = $before.dateOptions[0].id
    $update = @{
        title = "TEST updated $runId"
        dateOptions = @(@{ id = $firstDateId; optionDate = [DateTime]::UtcNow.AddDays(5).ToString('yyyy-MM-dd'); startTime = '18:00:00'; endTime = '20:00:00' })
    }
    $null = Invoke-Check PATCH $path $anonymous $update 401 'unauthorized'
    $null = Invoke-Check PATCH $path $otherSession $update 404 'not_found'
    $null = Invoke-Check PATCH '/api/activities/9223372036854775807' $ownerSession $update 404 'not_found'
    foreach ($field in @('deadlineAt', 'activityTypeId', 'placeOptions', 'budgetMin', 'budgetMax', 'cityId', 'districtId', 'hostUserId', 'status')) {
        $invalid = @{ title = 'Should not persist'; $field = 1 }
        $null = Invoke-Check PATCH $path $ownerSession $invalid 400 'validation_failed'
    }
    $null = Invoke-Check PATCH $path $ownerSession @{} 400 'validation_failed'
    $null = Invoke-Check PATCH $path $ownerSession @{ title = ' ' } 400 'validation_failed'
    $null = Invoke-Check PATCH $path $ownerSession @{ title = ('x' * 101) } 400 'validation_failed'
    $null = Invoke-Check PATCH $path $ownerSession @{ dateOptions = @() } 400 'validation_failed'
    $null = Invoke-Check PATCH $path $ownerSession @{ dateOptions = @($null) } 400 'validation_failed'
    $null = Invoke-Check PATCH $path $ownerSession @{ dateOptions = @(@{ id = $firstDateId }) } 400 'validation_failed'
    $null = Invoke-Check PATCH $path $ownerSession @{ dateOptions = @(@{ id = [long]::MaxValue; optionDate = $update.dateOptions[0].optionDate }) } 400 'validation_failed'
    $null = Invoke-Check PATCH $path $ownerSession @{ dateOptions = @($update.dateOptions[0], $update.dateOptions[0]) } 400 'validation_failed'
    $null = Invoke-Check PATCH $path $ownerSession @{
        title = 'Should not persist'
        dateOptions = @(@{ id = $firstDateId; optionDate = $before.dateOptions[1].optionDate; startTime = '18:00:00' })
    } 400 'validation_failed'
    $null = Invoke-Check PATCH $path $ownerSession @{
        title = 'Should not persist'
        dateOptions = @(@{ id = $firstDateId; optionDate = $update.dateOptions[0].optionDate; startTime = '20:00:00'; endTime = '19:00:00' })
    } 400 'validation_failed'
    $unchanged = Invoke-Check GET $path $ownerSession $null 200
    Assert-True (($unchanged.Json.data | ConvertTo-Json -Depth 10 -Compress) -eq ($before | ConvertTo-Json -Depth 10 -Compress)) 'Rejected updates leave activity unchanged'

    $patched = Invoke-Check PATCH $path $ownerSession $update 200
    Assert-True ($patched.Json.data.title -eq $update.title) 'PATCH returns new title'
    $updated = (Invoke-Check GET $path $ownerSession $null 200).Json.data
    Assert-True ($updated.title -eq $update.title) 'Updated title persisted'
    Assert-True ($updated.dateOptions[0].optionDate -eq $update.dateOptions[0].optionDate) 'Updated date persisted'
    Assert-True ($updated.dateOptions[0].id -eq $firstDateId -and $updated.dateOptions.Count -eq 2) 'Date identities and count preserved'
    Assert-True (($updated.dateOptions[1] | ConvertTo-Json -Compress) -eq ($before.dateOptions[1] | ConvertTo-Json -Compress)) 'Unmentioned date preserved'
    Assert-True (($updated | Select-Object -Property $fixedFields | ConvertTo-Json -Depth 10 -Compress) -eq $fixedBefore) 'All immutable fields preserved'
    Assert-True ([DateTimeOffset]$updated.updatedAt -gt [DateTimeOffset]$before.updatedAt) 'UpdatedAt advanced'
    Assert-True (($updated | ConvertTo-Json -Depth 10) -notmatch '(?i)token|hash') 'Update excludes secrets'

    # Both dates use the same start time, exercising the real PostgreSQL unique index during a swap.
    $swap = @{ dateOptions = @(
        @{ id = $updated.dateOptions[0].id; optionDate = $updated.dateOptions[1].optionDate; startTime = '18:00:00'; endTime = '20:00:00' }
        @{ id = $updated.dateOptions[1].id; optionDate = $updated.dateOptions[0].optionDate; startTime = '18:00:00'; endTime = '20:00:00' }
    ) }
    $null = Invoke-Check PATCH $path $ownerSession $swap 200
    $swapped = (Invoke-Check GET $path $ownerSession $null 200).Json.data
    Assert-True ($swapped.dateOptions[0].optionDate -eq $swap.dateOptions[0].optionDate -and $swapped.dateOptions[1].optionDate -eq $swap.dateOptions[1].optionDate) 'Date swap persisted atomically'
    Assert-True (($swapped | Select-Object -Property $fixedFields | ConvertTo-Json -Depth 10 -Compress) -eq $fixedBefore) 'Date swap preserves immutable fields'
    $null = Invoke-Check PATCH $path $ownerSession @{ title = "TEST final $runId" } 200

    # Use a separate short-lived TEST activity; never change an existing deadline to force expiry.
    $expiringRequest = $request.Clone()
    $expiringRequest.title = "TEST expiry $runId"
    $expiry = [DateTimeOffset]::UtcNow.AddSeconds(8)
    $expiringRequest.deadlineAt = $expiry.ToString('o')
    $expiring = Invoke-Check POST '/api/activities' $ownerSession $expiringRequest 201
    $expiryId = $expiring.Json.data.id
    $expiring = $null
    Write-Output "Created expiry test activity: id=$expiryId, label=TEST expiry $runId (2 date options, 2 place options)"
    $remainingMs = [Math]::Max(0, ($expiry - [DateTimeOffset]::UtcNow).TotalMilliseconds + 100)
    Start-Sleep -Milliseconds ([int]$remainingMs)
    $null = Invoke-Check PATCH "/api/activities/$expiryId" $ownerSession @{ title = 'Should not persist' } 409 'activity_not_editable'
    $expiredRead = Invoke-Check GET "/api/activities/$expiryId" $ownerSession $null 200
    Assert-True ($expiredRead.Json.data.title -eq $expiringRequest.title) 'Expired activity unchanged'

    $null = Invoke-Check POST '/api/auth/logout' $ownerSession $null 200
    $null = Invoke-Check GET "/api/activities/$activityId" $ownerSession $null 401 'unauthorized'
    $null = Invoke-Check POST '/api/activities' $ownerSession $request 401 'unauthorized'
    $null = Invoke-Check PATCH $path $ownerSession $update 401 'unauthorized'
    $null = Invoke-Check POST '/api/auth/logout' $ownerSession $null 200
    Write-Output "PASS: $checks checks. RunId=$runId. Test records retained; no deletion or schema changes."
}
finally {
    # Best-effort logout even if an assertion fails; never output cookies or responses.
    foreach ($session in @($ownerSession, $otherSession)) {
        try {
            $null = Invoke-WebRequest -Uri ([uri]::new($BaseUrl, '/api/auth/logout')) -Method Post -WebSession $session -SkipHttpErrorCheck -TimeoutSec 10
        } catch { }
    }
}
