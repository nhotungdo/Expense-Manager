# Subscription Notification System

## Overview
This document describes the implementation of email and in-app notifications for subscription purchase success and failure events.

## Features

### 1. Email Notifications
When a user attempts to purchase a service package:
- **Success**: User receives a beautifully formatted email with subscription details
- **Failure**: User receives an email explaining the error and how to retry

### 2. In-App Notifications
- **Success**: Green notification banner with success message
- **Failure**: Red notification banner with error details
- Both notifications appear in the user's notification center

## Implementation Details

### Email Templates

#### 1. SubscriptionSuccess.html
**Location:** `Templates/Email/SubscriptionSuccess.html`

**Template Variables:**
- `{{UserName}}` - User's full name or email
- `{{PackageName}}` - Name of the subscribed package
- `{{PackagePrice}}` - Price or "Đã thanh toán"
- `{{StartDate}}` - Subscription start date (dd/MM/yyyy HH:mm)
- `{{EndDate}}` - Subscription end date (dd/MM/yyyy HH:mm)
- `{{Features}}` - HTML list of package features
- `{{DashboardLink}}` - Link to subscription dashboard

**Design:**
- Green header (#10B981) with checkmark
- Clean, professional layout
- Feature list in highlighted box
- Call-to-action button to view subscription

#### 2. SubscriptionFailed.html
**Location:** `Templates/Email/SubscriptionFailed.html`

**Template Variables:**
- `{{UserName}}` - User's full name or email
- `{{PackageName}}` - Name of the attempted package
- `{{ErrorMessage}}` - Detailed error message
- `{{RetryLink}}` - Link to retry the purchase

**Design:**
- Red header (#EF4444) with X mark
- Error details in highlighted box
- Helpful troubleshooting steps
- Call-to-action button to retry

### Backend Changes

#### PaymentController.cs

**New Dependencies:**
```csharp
private readonly IEmailService _emailService;
private readonly IWebHostEnvironment _environment;
```

**New Methods:**

1. **SendSubscriptionSuccessNotification**
   ```csharp
   private async Task SendSubscriptionSuccessNotification(
       long userId, 
       string packageName, 
       DateTime startDate, 
       DateTime endDate, 
       List<string> features)
   ```
   
   **Functionality:**
   - Creates in-app notification with type "success"
   - Loads email template from file system
   - Replaces template variables with actual data
   - Sends email to user's registered email address
   - Logs any errors without failing the main operation

2. **SendSubscriptionFailureNotification**
   ```csharp
   private async Task SendSubscriptionFailureNotification(
       long userId, 
       string packageName, 
       string errorMessage)
   ```
   
   **Functionality:**
   - Creates in-app notification with type "error"
   - Loads failure email template
   - Replaces template variables
   - Sends failure notification email
   - Logs errors gracefully

**Updated ConfirmPayment Method:**
- Calls `SendSubscriptionSuccessNotification` after successful activation
- Calls `SendSubscriptionFailureNotification` in catch block on errors
- Notifications are sent asynchronously without blocking the response

## Database Changes

### Notification Table
New records are created with the following structure:

**Success Notification:**
```json
{
  "UserId": 123,
  "Title": "Gói dịch vụ đã được kích hoạt!",
  "Message": "Gói Pro của bạn đã được kích hoạt thành công...",
  "Type": "success",
  "IsRead": false,
  "IsImportant": true,
  "ActionUrl": "/Subscription",
  "CreatedAt": "2025-12-25T16:00:00Z"
}
```

**Failure Notification:**
```json
{
  "UserId": 123,
  "Title": "Kích hoạt gói dịch vụ thất bại",
  "Message": "Không thể kích hoạt gói Pro. Lý do: ...",
  "Type": "error",
  "IsRead": false,
  "IsImportant": true,
  "ActionUrl": "/Subscription/Checkout",
  "CreatedAt": "2025-12-25T16:00:00Z"
}
```

### Email Table
Email records are automatically created by the `EmailService` with:
- RecipientEmail
- Subject
- Body (HTML)
- Status (Pending → Sent/Failed)
- SentAt timestamp
- UserId (if found)

## User Experience Flow

### Success Flow
1. User clicks "Tôi đã thanh toán"
2. System activates subscription
3. **In-app notification appears** (green banner)
4. **Email is sent** to user's email address
5. User is redirected to `/Subscription` page
6. User can view notification in notification center
7. User receives email with full subscription details

### Failure Flow
1. User clicks "Tôi đã thanh toán"
2. System encounters an error
3. **In-app notification appears** (red banner)
4. **Email is sent** explaining the error
5. Error message is displayed
6. User can click notification to retry
7. User receives email with troubleshooting steps

## Notification Types

### Success Notification Features
- **Title**: "Gói dịch vụ đã được kích hoạt!"
- **Type**: "success"
- **Importance**: High (IsImportant = true)
- **Action**: Links to `/Subscription` to view details
- **Email Subject**: "Gói dịch vụ đã được kích hoạt"

### Failure Notification Features
- **Title**: "Kích hoạt gói dịch vụ thất bại"
- **Type**: "error"
- **Importance**: High (IsImportant = true)
- **Action**: Links to `/Subscription/Checkout` to retry
- **Email Subject**: "Kích hoạt gói dịch vụ thất bại"

## Email Content

### Success Email Includes:
1. Personalized greeting
2. Package name and confirmation
3. Subscription details box:
   - Package name
   - Price status
   - Start date
   - End date
   - List of features
4. Call-to-action button
5. Professional footer

### Failure Email Includes:
1. Personalized greeting
2. Error explanation
3. Error details box with specific message
4. Troubleshooting steps:
   - Check payment information
   - Ensure payment completion
   - Contact bank if declined
   - Retry after a few minutes
5. Retry button
6. Support contact information

## Error Handling

### Graceful Degradation
- If email sending fails, the subscription still activates (for success case)
- If notification creation fails, it's logged but doesn't block the operation
- Template file errors are caught and logged
- User lookup failures are handled gracefully

### Logging
All notification operations are logged:
```csharp
_logger.LogError(ex, "Failed to send subscription success notification for user {UserId}", userId);
_logger.LogError(ex, "Failed to send subscription failure notification for user {UserId}", userId);
```

## Testing

### Test Success Notification
1. Complete a subscription purchase successfully
2. Check in-app notifications (should show green success notification)
3. Check email inbox for success email
4. Verify all template variables are replaced correctly
5. Click email button to verify link works

### Test Failure Notification
1. Trigger a subscription error (e.g., duplicate subscription)
2. Check in-app notifications (should show red error notification)
3. Check email inbox for failure email
4. Verify error message is clear and helpful
5. Click retry button to verify link works

## Configuration

### Email Settings
Ensure `appsettings.json` has proper email configuration:
```json
{
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@moneytracker.com",
    "FromName": "Money Tracker App",
    "EnableSsl": true
  }
}
```

### Template Location
Templates must be in: `Templates/Email/`
- SubscriptionSuccess.html
- SubscriptionFailed.html

## Future Enhancements

1. **SMS Notifications**: Add SMS alerts for critical events
2. **Push Notifications**: Browser/mobile push notifications
3. **Notification Preferences**: Let users choose notification channels
4. **Email Scheduling**: Reminder emails before subscription expires
5. **Notification History**: View all past notifications
6. **Rich Notifications**: Add images and interactive elements
7. **Multi-language Support**: Translate templates based on user preference
8. **Notification Batching**: Group multiple notifications

## Build Status
✅ **0 Errors**
⚠️ **125 Warnings** (nullable reference warnings - non-critical)
✅ **All Features Implemented**

## Summary

The notification system provides comprehensive feedback to users about their subscription status through both email and in-app notifications. The system is designed to be:
- **Reliable**: Errors don't block main operations
- **User-friendly**: Clear, helpful messages
- **Professional**: Beautiful email templates
- **Maintainable**: Clean, well-documented code
- **Extensible**: Easy to add new notification types
