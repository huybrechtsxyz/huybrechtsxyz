# This script initializes Terraform and
# runs a plan using the specified variable file.
# Run from the root of the repository.
$RootPath = "C:/Users/vince/Sources/huybrechtsxyz/"
$AppPath = Join-Path -Path $RootPath -ChildPath "./app"

# Load the required modules
. "$RootPath/scripts/psfunctions.ps1"

# Load the secrets from the secrets file
Get-EnvVarsFromFile "$RootPath/src/develop.env"
Get-SecretsAsVars

$env:TF_VAR_api_key = $env:KAMATERA_API_KEY
$env:TF_VAR_api_secret = $env:KAMATERA_API_SECRET
$env:TF_VAR_workspace = $Env:WORKSPACE
$env:TF_VAR_environment = $Env:ENVIRONMENT
$env:TF_VAR_password = $Env:KAMATERA_ROOT_PASSWORD
$env:TF_TOKEN_app_terraform_io = $Env:TF_API_SECRET

# Copy terraform code to the app folder
if (-not (Test-Path "$AppPath/deploy")) {
    New-Item -ItemType Directory -Path "$AppPath/deploy" -Force
}
remove-item -Path "$AppPath/deploy/*" -Recurse -Force
Copy-Item -Path "$AppPath/deploy/*" -Destination "$AppPath/deploy" -Recurse -Force
Copy-Item -Path "$AppPath/src/develop.env" -Destination "$AppPath" -Recurse -Force

# Copy the terraform template file to the app folder
(Get-Content -Raw -Path "$AppPath/deploy/main.template.tf") -Replace `
    '\$ENVIRONMENT', $env:ENVIRONMENT | `
    Set-Content -Path "$AppPath/deploy/main.tf"
Remove-Item -Path "$AppPath/deploy/main.template.tf" -Force

# Initialize Terraform (if not already initialized)
./.app/terraform.exe `
    -chdir="$AppPath/deploy" `
    init

# Initialize Terraform (if not already initialized)
./.app/terraform.exe `
    -chdir="$AppPath/deploy" `
    init

# Run Terraform plan
./.app/terraform.exe `
    -chdir="$AppPath/deploy" `
    plan `
    -input=false `
    -var-file "$AppPath/deploy/vars-develop.tfvars" `
