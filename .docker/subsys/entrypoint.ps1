#!/usr/bin/env pwsh

dotnet test -c Release --logger:"trx;LogFileName=apiTestsResults.xml" -r /results
Copy-Item bin/Release/netcoreapp3.1/reassure.yaml /results

if( $args[0] -match '^[-]{0,2}skip-results-upload$' ){
    Write-Host "skipping upload of test results..."
}  
else{
    Write-Host "upload test results to S3 bucket: $env:PIPELINE_S3_BUCKET"
    aws s3 cp /results/ "s3://$env:PIPELINE_S3_BUCKET" --recursive
}