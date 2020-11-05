# list kv secrets, from kv root folder
path "kv/database/credentials"
{
  capabilities = ["read"]
}

path "kv/certificates/identity_signing_certificate"
{
  capabilities = ["read"]
}

path "kv/certificates/client_certificate_default"
{
  capabilities = ["read"]
}

path "kv/certificates/encryption_certificate_default"
{
  capabilities = ["read"]
}

path "kv/certificates/idsrv3test/*"
{
  capabilities = ["read"]
}

path "kv/certificates/idsrv3test"
{
  capabilities = ["read"]
}

path "kv/dataprotectionsecret"
{
  capabilities = ["read"]
}

# read database
path "database/creds/${service_name}"
{
  capabilities = ["read"]
}
