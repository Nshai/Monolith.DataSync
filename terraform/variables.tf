######Provided by Pipeline##########
variable "aws_key" {}

variable "aws_secret" {}

variable "env_name" {
  type = string
}

variable "iso_code" {
  type = string
}

variable "env_instance" {
  type = string
}

variable "message_resource" {
  type = string
}

variable "aws_region" {
  type = string
}

variable "aws_account_id" {
  type = string
}

variable "service_name" {
  type = string
}

variable "dns_domain" {
  type = string
}

variable "vault_url" {
  type = string
}

variable "vault_token" {
  type = string
}

variable "k8s_namespace" {
  type = string
}

variable "database_instance" {
  type = string
}

####any additional variables added below must be provided in values-intellifloplatform repo under each enviroment

variable "tags" {
  type = map
  default = {
    cloud-platform-tenancy = "IntellifloPlatform"
    service-group           = ""
    service-name            = ""
    project                 = "main"
    map-migrated            = ""
  }
}

variable "map_migrated" {
  type = map
  default = {
    AdviserPro = "d-server-02o148b1q9af4j"
    DataWarehouse = "d-server-00c2n8t6rgm312"
    Infrastructure = ""
    IntellifloPlatform = "d-server-017s7ptq4zblkl"
    Observability = "d-server-02cj4omp5dtr3n"
    Planner = "d-server-02835zuhefi5bt"
    PortfolioPathway = "d-server-00tnky5p4lxkof"
    rbCloud = "d-server-03iqjut611j071"
    Vision = "d-server-02xmuhj131gi2c"
  }
}