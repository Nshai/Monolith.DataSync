################
# certificates #
################
path "kv/certificates/signing"
{
  capabilities = ["read"]
}

path "kv/certificates/encryption"
{
  capabilities = ["read"]
}

################################
# application specific secrets #
################################
path "kv/${service_name}"
{
  capabilities = ["read"]
}

###################
# database access #
###################
path "database/creds/${service_name}"
{
  capabilities = ["read"]
}

# for subsystem only (testing environment)
path "kv/database/credentials"
{
  capabilities = ["read"]
}
