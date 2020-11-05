CREATE LOGIN [{{name}}] WITH PASSWORD = '{{password}}', CHECK_EXPIRATION = OFF, CHECK_POLICY = OFF;
    USE workflow CREATE USER [{{name}}] FOR LOGIN [{{name}}];
      ALTER ROLE [db_owner] ADD MEMBER [{{name}}];
    USE afper CREATE USER [{{name}}] FOR LOGIN [{{name}}];
      ALTER ROLE [db_owner] ADD MEMBER [{{name}}];