#!/bin/bash
set -euo pipefail
cd "$(dirname "$0")/../deploy/terraform"

echo "[*] ...Generating main.tf from template"
envsubst < main.template.tf > main.tf
rm -f main.template.tf
cat main.tf

echo "[*] ...Running terraform...INIT"
terraform init

echo "[*] ...Running terraform...PLAN"
terraform plan -var-file="vars-${WORKSPACE}.tfvars" -input=false

echo "[*] ...Running terraform...APPLY"
terraform apply -auto-approve -var-file="vars-${WORKSPACE}.tfvars" -input=false

echo "[*] ...Reading Terraform output..."
terraform output -json serverdata | jq -c '.' | tee /tmp/tf_output.json

echo "[*] ...Terraform output saved to tf_output.json and /tmp/tf_output.json"
