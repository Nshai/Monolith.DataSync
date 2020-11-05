#Terraform Cloud Backend Configuration workspace_name is replaced with sed in pipeline
terraform {
  backend "remote" {
    hostname     = "app.terraform.io"
    organization = "intelliflo"

    workspaces {
      name = "workspace_name"
    }
  }
}
