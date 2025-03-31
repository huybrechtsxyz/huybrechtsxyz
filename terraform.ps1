# FUNCTION: Read jsons and create docker secrets

# Define Kamatera credentials
$env:KAMATERA_API_CLIENT_ID = "74094cd5c060909c538c446636f0eb42"
$env:KAMATERA_API_SECRET = "04ce1deba6f17029b8b6b2a386178597"

# Set Terraform working directory (update this path as needed)
$terraformDir = "C:\Users\vince\Sources\huybrechtsxyz\.app"
Set-Location -Path $terraformDir

# Initialize Terraform (if not already initialized)
./terraform.exe init

# Run Terraform plan
./terraform.exe plan -var-file "develop.auto.tfvars" -out develop-plan.out