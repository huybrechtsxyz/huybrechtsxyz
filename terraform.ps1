# FUNCTION: Read jsons and create docker secrets

# Define Kamatera credentials
$env:KAMATERA_API_CLIENT_ID = ""
$env:KAMATERA_API_SECRET = ""

# Set Terraform working directory (update this path as needed)
$terraformDir = "C:\Users\vince\Sources\huybrechtsxyz\.app"
Set-Location -Path $terraformDir

# Initialize Terraform (if not already initialized)
./terraform.exe init

# Run Terraform plan
./terraform.exe plan -var-file "develop.auto.tfvars" -out develop-plan.out
