resource "consul_key_prefix" "application_configuration" {

  path_prefix = "settings/applications/microservice-workflow/"

  subkeys = jsondecode(file("./consul_kv.json"))
}
