# This script initializes Terraform and
# runs a plan using the specified variable file.
# Run from the root of the repository.
$APPPATH = "C:/Users/vince/Sources/huybrechtsxyz/"

# Load the required modules
. "$APPPATH/scripts/psfunctions.ps1"

# Load the secrets from the secrets file
Get-EnvVarsFromFile "$APPPATH/.app/develop.env"
Get-SecretsAsVars

$env:TF_VAR_api_key = $env:KAMATERA_API_KEY
$env:TF_VAR_api_secret = $env:KAMATERA_API_SECRET
$env:TF_VAR_environment = $Env:ENVIRONMENT
$env:TF_VAR_password = $Env:APP_ROOT_PASSWORD
$env:TF_TOKEN_app_terraform_io = $Env:TF_API_SECRET

# Copy terraform code to the app folder
if (-not (Test-Path "$APPPATH/.app/deploy")) {
    New-Item -ItemType Directory -Path "$APPPATH/.app/deploy" -Force
}
remove-item -Path "$APPPATH/.app/deploy/*" -Recurse -Force
Copy-Item -Path "$APPPATH/deploy/*" -Destination "$APPPATH/.app/deploy" -Recurse -Force
Copy-Item -Path "$APPPATH/src/develop.env" -Destination "$APPPATH/.app" -Recurse -Force

# Copy the terraform template file to the app folder
(Get-Content -Raw -Path "$APPPATH/.app/deploy/main.template.tf") -Replace `
    '\$ENVIRONMENT', $env:ENVIRONMENT | `
    Set-Content -Path "$APPPATH/.app/deploy/main.tf"
Remove-Item -Path "$APPPATH/.app/deploy/main.template.tf" -Force

# Initialize Terraform (if not already initialized)
./.app/terraform.exe `
    -chdir="$APPPATH/.app/deploy" `
    init
    
# Initialize Terraform (if not already initialized)
# ./.app/terraform.exe `
#     -chdir="$APPPATH/.app/deploy" `
#     workspace select huybrechts-xyz-$Env:ENVIRONMENT

# Initialize Terraform (if not already initialized)
./.app/terraform.exe `
    -chdir="$APPPATH/.app/deploy" `
    init

# Run Terraform plan
./.app/terraform.exe `
    -chdir="$APPPATH/.app/deploy" `
    plan `
    -input=false `
    -var-file "$APPPATH/.app/deploy/vars-develop.tfvars" `
