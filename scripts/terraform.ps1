# This script initializes Terraform and
# runs a plan using the specified variable file.
# Run from the root of the repository.

# Load the required modules
. "./scripts/psfunctions.ps1"

# Load the secrets from the secrets file
Get-SecretsAsVars

$env:TF_VAR_api_key = $env:KAMATERA_API_KEY
$env:TF_VAR_api_secret = $env:KAMATERA_API_SECRET
$env:TF_VAR_environment = "develop"
$env:TF_VAR_password = "@Abcd1234"

# Initialize Terraform (if not already initialized)
./.app/terraform.exe `
    -chdir="C:\Users\vince\Sources\huybrechtsxyz\.app" `
    init

# Run Terraform plan
./.app/terraform.exe `
    -chdir="C:\Users\vince\Sources\huybrechtsxyz\.app" `
    plan `
    -var-file "C:\Users\vince\Sources\huybrechtsxyz\.app\develop.tfvars" `
    -out develop-plan.out
