# 一键编译发布版：输出 dist_run\PCL JT Pro Max.exe + runtime\ 运行时目录
# 需要 .NET SDK 8+（https://dotnet.microsoft.com/download），不需要 Visual Studio。
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'

$dotnet = (Get-Command dotnet -ErrorAction SilentlyContinue).Source
if (-not $dotnet -and (Test-Path "$env:USERPROFILE\.dotnet\dotnet.exe")) { $dotnet = "$env:USERPROFILE\.dotnet\dotnet.exe" }
if (-not $dotnet) { throw '找不到 dotnet，请先安装 .NET SDK' }

# 1. 编译主程序（自包含、win-x64、文件夹版，所有 DLL 进 dist\）
& $dotnet publish "$root\src\OpenLauncher\OpenLauncher.csproj" -c Release -r win-x64 `
    --self-contained -p:PublishSingleFile=false -p:PublishTrimmed=false `
    -p:AppendTargetFrameworkToOutputPath=false -o "$root\dist"
if ($LASTEXITCODE -ne 0) { throw '编译失败' }

# 2. 组装成「根目录启动器 + runtime\」结构
$out = Join-Path $root 'dist_run'
New-Item -ItemType Directory -Force $out | Out-Null
$runtime = Join-Path $out 'runtime'
New-Item -ItemType Directory -Force $runtime | Out-Null
Copy-Item "$root\dist\*" $runtime -Recurse -Force
Copy-Item "$root\dist\PCL JT Pro Max.exe" $out -Force

$exe = Get-Item "$out\PCL JT Pro Max.exe"
$hash = (Get-FileHash $exe -Algorithm SHA256).Hash.ToLower()
"已输出 $($exe.FullName)"
"配套运行时目录 $runtime"
"大小 $([math]::Round($exe.Length / 1KB)) KB   SHA-256 $hash"
