<div align="center">

# 📧 Money Tracker App - Email Testing Suite

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![xUnit](https://img.shields.io/badge/Testing-xUnit-512BD4?logo=xunit)](https://xunit.net/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**Comprehensive email testing framework for Money Tracker App**

[Features](#-features) • [Quick Start](#-quick-start) • [Test Structure](#-test-structure) • [Configuration](#-configuration) • [Troubleshooting](#-troubleshooting)

</div>

---

## 📋 Overview

This test suite provides comprehensive coverage for the Money Tracker App email functionality, including unit tests, integration tests, and manual tests for real email delivery to **donhotung2004@gmail.com**.

## ✨ Features

- 🧪 **Unit Tests** - Isolated testing of `EmailService` components
- 🔗 **Integration Tests** - End-to-end testing of `EmailSenderModel` page logic
- 📨 **Manual Tests** - Real email delivery verification
- 🌐 **Multi-language Support** - Vietnamese and special character handling
- 📊 **Performance Testing** - Bulk email sending capabilities
- 📝 **Detailed Documentation** - 8 comprehensive test cases

## 🚀 Quick Start

### Prerequisites

```bash
# Ensure .NET 8.0 SDK is installed
dotnet --version

# Restore dependencies
cd MoneyTrackerApp.Tests
dotnet restore
```

### Run All Tests

```bash
# Run unit and integration tests (excludes manual tests)
dotnet test --filter "Category!=ManualTest"

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

## 📁 Test Structure

### 📄 Test Files

| File | Type | Description |
|------|------|-------------|
| `EmailServiceTests.cs` | Unit | Tests for `EmailService` class |
| `EmailSenderModelTests.cs` | Integration | Tests for `EmailSenderModel` page |
| `EmailManualTests.cs` | Manual | Real email delivery tests |
| `EmailSendingTestCases.md` | Documentation | Detailed test case specifications |

### 🧪 Unit Tests (`EmailServiceTests.cs`)

Tests the core email service functionality:

- ✅ Email record creation in database
- ✅ User-email linking
- ✅ HTML content processing
- ✅ Special characters and Vietnamese text
- ✅ Bulk email sending
- ✅ Performance benchmarks

```bash
# Run unit tests only
dotnet test --filter "FullyQualifiedName~EmailServiceTests"
```

### 🔗 Integration Tests (`EmailSenderModelTests.cs`)

Tests the page model integration:

- ✅ `OnPostAsync` with valid input
- ✅ Input validation
- ✅ Multiple email parsing
- ✅ Scheduled email handling
- ✅ Email log retrieval

```bash
# Run integration tests only
dotnet test --filter "FullyQualifiedName~EmailSenderModelTests"
```

### 📨 Manual Tests (`EmailManualTests.cs`)

> ⚠️ **WARNING**: These tests send REAL emails to **donhotung2004@gmail.com**!

Available test cases:
- 📧 Basic email delivery
- 📧 Complex HTML content
- 📧 Long subject lines
- 📧 Performance test (10 consecutive emails)
- 📧 Multiple recipients
- 📧 Test report generation

## ⚙️ Configuration

### Step 1: Configure SMTP Settings

Edit `MoneyTrackerApp.Tests/EmailManualTests.cs`:

```csharp
var emailSettings = Options.Create(new EmailSettings
{
    Host = "smtp.gmail.com",
    Port = 587,
    Username = "YOUR_EMAIL@gmail.com",        // ← Update this
    Password = "YOUR_APP_PASSWORD",           // ← Update this (App Password)
    FromEmail = "YOUR_EMAIL@gmail.com",       // ← Update this
    FromName = "Money Tracker App - Test System",
    EnableSsl = true
});
```

### Step 2: Enable Manual Tests

Remove the `Skip` attribute from tests you want to run:

```diff
- [Fact(DisplayName = "TC-EMAIL-001: ...", Skip = "Manual test - Uncomment to run")]
+ [Fact(DisplayName = "TC-EMAIL-001: ...")]
```

### Step 3: Run Specific Tests

```bash
# Run a specific test case
dotnet test --filter "DisplayName~TC-EMAIL-001"

# Run all manual tests (if Skip removed)
dotnet test --filter "Category=ManualTest"

# Run with detailed logging
dotnet test --filter "DisplayName~TC-EMAIL-001" --logger "console;verbosity=detailed"
```

### Step 4: Verify Results

1. ✅ Check console output
2. ✅ Verify email in **donhotung2004@gmail.com** inbox
3. ✅ Document results in `EmailSendingTestCases.md`

## 🔐 Gmail App Password Setup

### Enable 2-Step Verification

1. Visit [Google Account Security](https://myaccount.google.com/security)
2. Find "2-Step Verification"
3. Enable the feature

### Generate App Password

1. Visit [App Passwords](https://myaccount.google.com/apppasswords)
2. Select "Mail" and "Other (Custom name)"
3. Enter name: "Money Tracker App Test"
4. Click "Generate"
5. Copy the 16-character password
6. Use this password in SMTP settings

### Update Configuration

**appsettings.json:**

```json
{
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-16-char-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "Money Tracker App",
    "EnableSsl": true
  }
}
```

## ✅ Pre-Test Checklist

Before running tests, ensure:

- [ ] SMTP settings configured correctly
- [ ] Gmail App Password generated
- [ ] Database migrated (`dotnet ef database update`)
- [ ] Application builds successfully (`dotnet build`)
- [ ] Access to **donhotung2004@gmail.com** for verification
- [ ] Test cases reviewed in `EmailSendingTestCases.md`

## 🐛 Troubleshooting

<details>
<summary><b>Authentication Failed</b></summary>

- ✅ Verify Username and Password
- ✅ Ensure using App Password, not regular password
- ✅ Confirm 2-Step Verification is enabled
</details>

<details>
<summary><b>Connection Timeout</b></summary>

- ✅ Check firewall settings
- ✅ Verify Port (587 or 465)
- ✅ Ensure `EnableSsl = true`
</details>

<details>
<summary><b>Mailbox Unavailable</b></summary>

- ✅ Verify recipient email address
- ✅ Confirm `FromEmail` is valid
</details>

<details>
<summary><b>Email Not Received</b></summary>

- ✅ Check Spam/Junk folder
- ✅ Wait 1-2 minutes for delivery
- ✅ Check email logs in database
</details>

## 📊 Useful SQL Queries

```sql
-- View all emails sent to donhotung2004@gmail.com
SELECT * FROM Emails 
WHERE RecipientEmail = 'donhotung2004@gmail.com'
ORDER BY CreatedAt DESC;

-- Count emails by status
SELECT Status, COUNT(*) as Count
FROM Emails
WHERE RecipientEmail = 'donhotung2004@gmail.com'
GROUP BY Status;

-- Calculate success rate
SELECT 
    COUNT(*) as Total,
    SUM(CASE WHEN Status = 'Sent' THEN 1 ELSE 0 END) as Sent,
    SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) as Failed,
    CAST(SUM(CASE WHEN Status = 'Sent' THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) as SuccessRate
FROM Emails
WHERE RecipientEmail = 'donhotung2004@gmail.com';

-- Delete all test emails
DELETE FROM Emails 
WHERE Subject LIKE '%[TEST]%';
```

## 📝 Test Case Documentation

### Available Test Cases

| ID | Description | Type |
|----|-------------|------|
| TC-EMAIL-001 | Successful email delivery | Manual |
| TC-EMAIL-002 | Complex HTML content | Manual |
| TC-EMAIL-003 | Long subject with special chars | Manual |
| TC-EMAIL-004 | Invalid email validation | Unit |
| TC-EMAIL-005 | Performance - Bulk sending | Manual |
| TC-EMAIL-006 | SMTP error handling | Unit |
| TC-EMAIL-007 | Scheduled email delivery | Integration |
| TC-EMAIL-008 | Multiple recipients | Manual |

### Recording Test Results

After running each test, update `EmailSendingTestCases.md`:

```markdown
### Test Results
- Test Date: 25/12/2025 18:45:00
- Status: [X] Pass / [ ] Fail
- Email Delivery Time: ~2 seconds
- Screenshot: screenshots/tc-email-001.png
- Notes: Email sent and received successfully
```

## 📈 Test Report Template

```markdown
# Email Testing Report
**Date:** 25/12/2025
**Tester:** [Your Name]
**Environment:** Development

## Summary
- Total Test Cases: 8
- Passed: X
- Failed: Y
- Pass Rate: Z%

## Test Results
[Copy from EmailSendingTestCases.md]

## Issues Found
1. [Bug description]
2. [Bug description]

## Recommendations
1. [Improvement suggestion]
2. [Improvement suggestion]
```

## 📞 Support

Need help or have questions?

- 📧 Email: donhotung2004@gmail.com
- 📝 Create an issue in the repository

---

<div align="center">

**Made with ❤️ for Money Tracker App**

[⬆ Back to Top](#-money-tracker-app---email-testing-suite)

</div>
