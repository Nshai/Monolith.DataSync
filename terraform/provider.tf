#AWS Provider Credentials
provider "aws" {
  region     = var.aws_region
  access_key = var.aws_key
  secret_key = var.aws_secret
}

#Vault Provider Credentials
provider "vault" {
  address = var.vault_url
  token   = var.vault_token
}