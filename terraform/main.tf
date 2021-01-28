locals {
  environment            = "${var.env_name}-${var.iso_code}-${var.env_instance}"

  global_tags = {
    cloud-platform-tenancy = var.tags["cloud-platform-tenancy"]
    service-group          = var.tags["service-group"] != "" ? var.tags["service-group"] : var.service_name
    service-name           = var.tags["service-name"] != "" ? var.tags["service-name"] : var.service_name
    project                = var.tags["project"]
    map-migrated           = var.map_migrated[var.tags["cloud-platform-tenancy"]]
  }
}

#Create IAM Role for service
resource "aws_iam_role" "service_role" {
  name = "${local.environment}-${var.service_name}"
  assume_role_policy = file("${path.module}/trust.json")

  tags = merge(
    map("Name", "${local.environment}-${var.service_name}"),
    local.global_tags
  )
}

#Upload the policy to corresponding role
resource "aws_iam_role_policy" "service_policy" {
  name = "${local.environment}-${var.service_name}"
  role = "${local.environment}-${var.service_name}"
  policy = data.aws_iam_policy_document.queues.json
}


#Create Vault Policy for Service
data "template_file" "policy_hcl" {
  template = file("${path.module}/vault_policy.hcl")
  vars = {
    service_name = var.service_name
  }
}

resource "vault_policy" "servicepolicy" {
  name   = var.service_name
  policy = data.template_file.policy_hcl.rendered
}

#Create backend role for database
resource "vault_database_secret_backend_role" "servicename" {
  backend             = "database"
  name                = var.service_name
  db_name             = var.database_instance
  creation_statements = [file("${path.module}/database_policy.hcl")]
  default_ttl         = 172800
  max_ttl             = 0
}

#Kubernetes Backend Role
resource "vault_kubernetes_auth_backend_role" "servicename" {
  //backend                        = vault_auth_backend.kubernetes.path
  role_name                        = var.service_name
  bound_service_account_names      = [var.service_name]
  bound_service_account_namespaces = [var.k8s_namespace]
  token_policies                   = [var.service_name]
}