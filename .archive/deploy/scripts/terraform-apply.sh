#!/bin/bash
set -euo pipefail
#cd "$(dirname "$0")/../deploy/terraform"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR/../terraform"

echo "[*] ...Generating main.tf from template"
envsubst < main.template.tf > main.tf
rm -f main.template.tf
cat main.tf

# Reason we do not save the plan
# Error: Saving a generated plan is currently not supported
# Terraform Cloud does not support saving the generated execution plan

echo "[*] ...Running terraform...INIT"
mkdir -p "$APP_PATH_TEMP"
terraform init

echo "[*] ...Running terraform...PLAN"
terraform plan -var-file="vars-${WORKSPACE}.tfvars" -input=false

echo "[*] ...Running terraform...APPLY"
terraform apply -auto-approve -var-file="vars-${WORKSPACE}.tfvars" -input=false

echo "[*] ...Reading Terraform output..."
terraform output -json serverdata | jq -c '.' | tee $APP_PATH_TEMP/tf_output.json

echo "[*] ...Terraform output saved to tf_output.json and $APP_PATH_TEMP/tf_output.json"
