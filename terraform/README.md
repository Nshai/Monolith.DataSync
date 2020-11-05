# Terraform Cloud Config

This Terraform config will create all the aws resources, e.g. IAM roles, S3 buckets, RDS etc required by the service.
Any shared resources, for example a CDN bucket will be provisioned when the environment is provisioned.

## backend.tf
This contains configuration for terraform cloud backend

## main.tf
The required terraform resources and file references

## provider.tf
the required provider credentials such as AWS keys, these variables are provided via pipeline

## Variables.tf
Varibles described here must be aligned to variables supplied in pipeline. Variables not provided by pipeline must be inserted into an enviroment aligned auto.tfvars file within the values-intelligent-office repository

## policy.tf
The required AWS IAM permissions for the service