FROM intelliflo/monolith-datasync:windows
WORKDIR /app
COPY ./.docker/windows/network/fix-container-hosts.ps1 .
COPY ./.docker/windows/local-dev/run-local-dev.ps1 .
ENTRYPOINT [ "powershell", "./run-local-dev.ps1" ]