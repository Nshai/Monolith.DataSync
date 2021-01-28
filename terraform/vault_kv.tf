resource "vault_generic_secret" "application_secrets" {

  # application secrets
  path = "kv/microservice-workflow"
  # file placeholder, to change the actual values used by Vault please push your changes to https://github.com/Intelliflo/values-intelligent-office
  data_json = file("./vault_kv.json")
}
