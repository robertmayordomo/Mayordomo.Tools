param(
    [string]$ProjectFile = "Mayordomo.Tools\Mayordomo.Tools.csproj"
)

# Stop on any error
$ErrorActionPreference = "Stop"

try {
    # Get the full path to the project file
    $fullPath = (Resolve-Path $ProjectFile).Path

    # Read the project file as XML
    [xml]$csproj = Get-Content -Path $fullPath

    # Get the current version
    $version = $csproj.Project.PropertyGroup.Version
    if (-not $version) {
        Write-Error "Could not find the <Version> tag in $ProjectFile"
        exit 1
    }

    # Split version into parts
    $versionParts = $version.Split('.')
    if ($versionParts.Length -ne 3) {
        Write-Error "Version '$version' is not in a valid Major.Minor.Patch format."
        exit 1
    }

    # Increment the patch version
    $major = $versionParts[0]
    $minor = $versionParts[1]
    $patch = [int]$versionParts[2] + 1

    # Create the new version string
    $newVersion = "$major.$minor.$patch"

    # Update the XML and save the file
    $csproj.Project.PropertyGroup.Version = $newVersion
    $csproj.Save($fullPath)

    Write-Host "Version incremented to $newVersion in $ProjectFile"

    # Stage the updated project file
    git add $fullPath
    Write-Host "$ProjectFile staged for commit."

}
catch {
    Write-Error "An error occurred: $_ "
    exit 1
}

exit 0
