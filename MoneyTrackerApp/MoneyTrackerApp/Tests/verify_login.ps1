# verify_login.ps1 - Test Simultaneous Login
$base = "http://localhost:5000" # Change port if needed, usually 5000 or 5001 (https)
$registerUrl = "$base/api/Auth/register"
$loginUrl = "$base/api/Auth/login"

# Ignore SSL errors for localhost
if (-not ([System.Management.Automation.PSTypeName]'ServerCertificateValidationCallback').Type)
{
$certCallback = @"
    using System;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    public class ServerCertificateValidationCallback
    {
        public static void Ignore()
        {
            if(ServicePointManager.ServerCertificateValidationCallback ==null)
            {
                ServicePointManager.ServerCertificateValidationCallback += 
                    delegate
                    (
                        Object obj, 
                        X509Certificate certificate, 
                        X509Chain chain, 
                        SslPolicyErrors errors
                    )
                    {
                        return true;
                    };
            }
        }
    }
"@
    Add-Type $certCallback
 }
[ServerCertificateValidationCallback]::Ignore()

function Register-User ($email, $pass) {
    echo "Registering $email..."
    try {
        $body = @{
            email = $email
            password = $pass
            fullName = "Test User"
        } | ConvertTo-Json
        
        $res = Invoke-RestMethod -Uri $registerUrl -Method Post -Body $body -ContentType "application/json" -ErrorAction Stop
        return $res
    } catch {
        # Valid if already exists
        echo "User $email might already exist."
    }
}

function Login-User ($email, $pass, $sessionName) {
    echo "Logging in $email ($sessionName)..."
    $body = @{
        email = $email
        password = $pass
        rememberMe = $false
    } | ConvertTo-Json

    $res = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $body -ContentType "application/json" -SessionVariable $sessionName
    return $res
}

# 1. Register User A and User B
Register-User "usera@test.com" "Password123!"
Register-User "userb@test.com" "Password123!"

# 2. Login User A (Session A)
$tokenA = Login-User "usera@test.com" "Password123!" "SessionA"
echo "User A Token: $($tokenA.accessToken.Substring(0, 10))..."
$cookieA = $SessionA.Cookies.GetCookies($base) | Where-Object { $_.Name -eq "AccessToken" }
echo "User A Cookie: $($cookieA.Value.Substring(0, 10))..."

# 3. Login User B (Session B) - Independent Session
$tokenB = Login-User "userb@test.com" "Password123!" "SessionB"
echo "User B Token: $($tokenB.accessToken.Substring(0, 10))..."
$cookieB = $SessionB.Cookies.GetCookies($base) | Where-Object { $_.Name -eq "AccessToken" }
echo "User B Cookie: $($cookieB.Value.Substring(0, 10))..."

# 4. Verify Independence
if ($tokenA.accessToken -ne $tokenB.accessToken) {
    echo "SUCCESS: Tokens are different."
} else {
    echo "FAIL: Tokens are same."
}

if ($cookieA.Value -ne $cookieB.Value) {
    echo "SUCCESS: Cookies are different (Independent Sessions)."
} else {
    echo "FAIL: Cookies are same."
}
