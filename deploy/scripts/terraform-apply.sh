#!/bin/bash
set -euo pipefail
#cd "$(dirname "$0")/../deploy/terraform"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR/../terraform"

echo "[*] ...Generating main.tf from template"
envsubst < main.template.tf > main.tf
rm -f main.template.tf
cat main.tf

echo "[*] ...Running terraform...INIT"
mkdir -p $APP_PATH_TEMP
PLAN_OUTPUT=$(mktemp)
terraform init

# Use the plan output file to run apply instead of directly applying changes
# terraform plan -var-file="vars-${WORKSPACE}.tfvars" -input=false
# terraform apply -auto-approve -var-file="vars-${WORKSPACE}.tfvars" -input=false
echo "[*] ...Running terraform...PLAN"
terraform plan -var-file="vars-${WORKSPACE}.tfvars" -input=false -out="$PLAN_OUTPUT" > $APP_PATH_TEMP/plan.txt
if grep -q "No changes. Your infrastructure matches the configuration." $APP_PATH_TEMP/plan.txt; then
    echo "[*] ...Running terraform...No changes detected. Skipping apply."
else
    echo "[*] ...Running terraform...Changes detected. Running terraform APPLY..."
    terraform apply -auto-approve "$PLAN_OUTPUT"  
fi

echo "[*] ...Reading Terraform output..."
terraform output -json serverdata | jq -c '.' | tee $APP_PATH_TEMP/tf_output.json

echo "[*] ...Terraform output saved to tf_output.json and $APP_PATH_TEMP/tf_output.json"
