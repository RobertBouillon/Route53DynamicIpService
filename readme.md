# Route 53 Dynamic IP Updater
This windows service will periodically update DNS name records in AWS Route 53 with the public IP address of the current machine.

This service is useful if your ISP periodically changes your IP address and you want to keep a DNS record up-to-date.

# Installation and Use

## AWS Setup
You'll need an AWS API key which allows the service to make updates to Route 53. It's important that this key has limited access to your account.

1. Locate the Hosted Zone ID for the domain you want to update.
2. Create a new Policy in IAM
```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Sid": "VisualEditor0",
            "Effect": "Allow",
            "Action": "route53:ChangeResourceRecordSets",
            "Resource": "arn:aws:route53:::hostedzone/<YOUR HOSTED ZONE ID>"
        }
    ]
}
```
3. Create a new user in IAM with the above policy as its permission set
4. Generate an API key for the new user

## Windows Service Setup

1. Build the binaries (`dotnet publish -r win-x64 -c Release`)
2. Edit the `appsettings.json` file with your configuration settings
3. Open a Powershell prompt with elevated privileges
4. Initialize the event log `New-EventLog -LogName Application -Source "Route 53 Dynamic IP"`
5. Make a new directory for the service (e.g. `mkdir "$env:ProgramFiles\Route 53 Dynamic IP"`)
6. Copy the binaries to the installation folder 
7. Install the service using the command `sc.exe create "Route 53 Dynamic IP" binpath="$env:ProgramFiles\Route 53 Dynamic IP\Route53DynamicIpService.exe" start=auto`
8. Start the service `sc.exe start "Route 53 Dynamic IP"`


## Debugging
1.  Add an `appsettings.secrets.json` to your solution. This file will not be checked into GIT