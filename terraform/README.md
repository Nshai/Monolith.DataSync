# Terraform configuration

This Terraform config will create all the aws resources, e.g. IAM roles, S3 buckets, RDS etc required by the service.
Any shared resources, for example a CDN bucket will be provisioned when the environment is provisioned.

## backend.tf

This contains configuration for terraform cloud backend

## consul_kv.tf

This contains Consul root path where service configurations will be stored

## consul_kv.json

This contains service configurations to be stored in Consul

## main.tf

The required terraform resources and file references

## database_policy.hcl

Defines databases this service will be able to reach

## vault_policy.hcl

Hashicorp Vault policy, define permissions level this service will have

## vault_kv.tf

Defines key-value path where Vault secrets will be stored

## vault_kv.json

Placeholder file that will be used by Vault (this file will be replaced during deployment)

## provider.tf

the required provider credentials such as AWS keys, these variables are provided via pipeline

## variables.tf

Varibles described here must be aligned to variables supplied in pipeline. Variables not provided by pipeline must be inserted into an enviroment aligned auto.tfvars file within the values-intelligent-office repository

## iam_policy.tf

The required AWS IAM permissions for the service

## trust.json

Placeholder file used by terraform to write AWS role