# API Documentation

## Authentication
All API endpoints require JWT Bearer token authentication. Include the token in the Authorization header:

```
Authorization: Bearer {your-jwt-token}
```

The token can also be sent as a cookie named `AccessToken`.

## Base URL
```
https://your-domain.com/api
```

---

## Group Expense API

### Get User Groups
Get all groups for the authenticated user.

**Endpoint:** `GET /api/GroupExpense`

**Response:**
```json
[
  {
    "id": 1,
    "name": "Travel Group",
    "description": "Trip to Da Nang",
    "createdByUserId": 1,
    "createdByUserName": "John Doe",
    "isPublic": true,
    "icon": "✈️",
    "color": "#4CAF50",
    "memberCount": 3,
    "totalExpenses": 1500000,
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-20T15:30:00Z",
    "members": [...]
  }
]
```

### Get Group by ID
Get details of a specific group.

**Endpoint:** `GET /api/GroupExpense/{id}`

**Parameters:**
- `id` (path) - Group ID

**Response:** Same as single group object above

### Create Group
Create a new expense group.

**Endpoint:** `POST /api/GroupExpense`

**Request Body:**
```json
{
  "name": "Travel Group",
  "description": "Trip to Da Nang",
  "isPublic": true,
  "icon": "✈️",
  "color": "#4CAF50",
  "memberUserIds": [2, 3, 4]
}
```

**Response:** Created group object with 201 status

### Update Group
Update an existing group.

**Endpoint:** `PUT /api/GroupExpense/{id}`

**Request Body:**
```json
{
  "id": 1,
  "name": "Updated Travel Group",
  "description": "Updated description",
  "isPublic": false,
  "icon": "🏖️",
  "color": "#2196F3"
}
```

**Response:** Updated group object

### Delete Group
Delete a group (owner only).

**Endpoint:** `DELETE /api/GroupExpense/{id}`

**Response:** 204 No Content

### Add Member
Add a member to a group.

**Endpoint:** `POST /api/GroupExpense/members`

**Request Body:**
```json
{
  "groupId": 1,
  "userId": 5,
  "role": "Member"
}
```

**Roles:** Owner, Admin, Member

**Response:**
```json
{
  "id": 10,
  "groupId": 1,
  "userId": 5,
  "userName": "Jane Smith",
  "userEmail": "jane@example.com",
  "role": "Member",
  "joinedAt": "2024-01-20T10:00:00Z"
}
```

### Remove Member
Remove a member from a group.

**Endpoint:** `DELETE /api/GroupExpense/groups/{groupId}/members/{memberId}`

**Response:** 204 No Content

### Create Group Transaction
Create a group expense with automatic splitting.

**Endpoint:** `POST /api/GroupExpense/transactions`

**Request Body:**
```json
{
  "groupId": 1,
  "amount": 300000,
  "currency": "VND",
  "description": "Hotel booking",
  "transactionDate": "2024-01-20",
  "category": "Accommodation",
  "splitMethod": 1,
  "customSplits": null
}
```

**Split Methods:**
- `1` - Equal split among all members
- `2` - Custom amounts (requires customSplits array)

**Custom Splits Example:**
```json
{
  "splitMethod": 2,
  "customSplits": [
    { "userId": 1, "amount": 150000 },
    { "userId": 2, "amount": 100000 },
    { "userId": 3, "amount": 50000 }
  ]
}
```

**Response:**
```json
{
  "id": 1,
  "groupId": 1,
  "groupName": "Travel Group",
  "paidByUserId": 1,
  "paidByUserName": "John Doe",
  "amount": 300000,
  "currency": "VND",
  "description": "Hotel booking",
  "transactionDate": "2024-01-20T00:00:00Z",
  "category": "Accommodation",
  "createdAt": "2024-01-20T10:00:00Z",
  "splits": [
    {
      "userId": 1,
      "amount": 100000,
      "isPaid": true
    },
    {
      "userId": 2,
      "amount": 100000,
      "isPaid": false
    },
    {
      "userId": 3,
      "amount": 100000,
      "isPaid": false
    }
  ]
}
```

### Get Group Transactions
Get all transactions for a group.

**Endpoint:** `GET /api/GroupExpense/{groupId}/transactions`

**Response:** Array of transaction objects

### Get Group Balances
Get member balances and settlements.

**Endpoint:** `GET /api/GroupExpense/{groupId}/balances`

**Response:**
```json
{
  "groupId": 1,
  "groupName": "Travel Group",
  "memberBalances": [
    {
      "userId": 1,
      "userName": "John Doe",
      "totalPaid": 500000,
      "totalOwed": 250000,
      "balance": 250000
    },
    {
      "userId": 2,
      "userName": "Jane Smith",
      "totalPaid": 0,
      "totalOwed": 250000,
      "balance": -250000
    }
  ],
  "settlements": [
    {
      "fromUserId": 2,
      "fromUserName": "Jane Smith",
      "toUserId": 1,
      "toUserName": "John Doe",
      "amount": 250000
    }
  ]
}
```

### Calculate Settlements
Get optimal debt settlements for a group.

**Endpoint:** `GET /api/GroupExpense/{groupId}/settlements`

**Response:** Array of settlement objects (same as in balances response)

---

## Report API

### Get Dashboard
Get dashboard overview with charts and stats.

**Endpoint:** `GET /api/Report/dashboard`

**Response:**
```json
{
  "currentBalance": 10000000,
  "monthlyIncome": 5000000,
  "monthlyExpense": 3000000,
  "monthlySavings": 2000000,
  "savingsRate": 40.0,
  "cashFlowChart": {
    "labels": ["01/15", "01/16", "01/17", "01/18", "01/19", "01/20", "01/21"],
    "incomeData": [100000, 200000, 150000, 300000, 250000, 180000, 220000],
    "expenseData": [80000, 120000, 100000, 150000, 130000, 90000, 110000]
  },
  "expensePieChart": [
    {
      "label": "Food",
      "value": 500000,
      "color": "#FF6384"
    },
    {
      "label": "Transport",
      "value": 300000,
      "color": "#36A2EB"
    }
  ],
  "recentTransactions": [...],
  "budgetAlerts": [...]
}
```

### Get Cash Flow Report
Generate cash flow report for a date range.

**Endpoint:** `GET /api/Report/cashflow`

**Query Parameters:**
- `startDate` (required) - Start date (YYYY-MM-DD)
- `endDate` (required) - End date (YYYY-MM-DD)

**Response:**
```json
{
  "startDate": "2024-01-01",
  "endDate": "2024-01-31",
  "totalIncome": 5000000,
  "totalExpense": 3000000,
  "netCashFlow": 2000000,
  "incomeItems": [
    {
      "categoryName": "Salary",
      "amount": 4500000,
      "percentage": 90.0
    }
  ],
  "expenseItems": [
    {
      "categoryName": "Food",
      "amount": 1000000,
      "percentage": 33.3
    }
  ],
  "dailyBreakdown": [
    {
      "date": "2024-01-01",
      "income": 0,
      "expense": 50000,
      "netFlow": -50000
    }
  ]
}
```

### Get Monthly Trends
Generate monthly trend report for a year.

**Endpoint:** `GET /api/Report/trends`

**Query Parameters:**
- `year` (required) - Year (e.g., 2024)

**Response:**
```json
{
  "year": 2024,
  "monthlyData": [
    {
      "month": 1,
      "monthName": "January",
      "income": 5000000,
      "expense": 3000000,
      "netIncome": 2000000,
      "savingsRate": 40.0
    }
  ],
  "averageIncome": 5000000,
  "averageExpense": 3000000,
  "trend": "Stable"
}
```

**Trend Values:** Increasing, Decreasing, Stable

### Get Category Breakdown
Generate category breakdown report.

**Endpoint:** `GET /api/Report/categories`

**Query Parameters:**
- `startDate` (required) - Start date (YYYY-MM-DD)
- `endDate` (required) - End date (YYYY-MM-DD)

**Response:**
```json
{
  "startDate": "2024-01-01",
  "endDate": "2024-01-31",
  "incomeCategories": [
    {
      "categoryName": "Salary",
      "categoryIcon": "💰",
      "categoryColor": "#4CAF50",
      "amount": 5000000,
      "percentage": 100.0,
      "transactionCount": 1
    }
  ],
  "expenseCategories": [
    {
      "categoryName": "Food",
      "categoryIcon": "🍕",
      "categoryColor": "#FF5722",
      "amount": 1000000,
      "percentage": 33.3,
      "transactionCount": 15
    }
  ],
  "totalIncome": 5000000,
  "totalExpense": 3000000
}
```

### Export Report
Export report to file.

**Endpoint:** `POST /api/Report/export`

**Request Body:**
```json
{
  "reportType": 1,
  "startDate": "2024-01-01",
  "endDate": "2024-01-31",
  "fileFormat": 2
}
```

**Report Types:**
- `1` - Cash Flow Report
- `3` - Category Breakdown
- `4` - Monthly Trends

**File Formats:**
- `1` - PDF
- `2` - Excel (XLSX)
- `3` - CSV
- `4` - JSON

**Response:**
```json
{
  "filePath": "/reports/report_1_20240120153045.xlsx",
  "message": "Report exported successfully"
}
```

---

## Notification API

### Get Notifications
Get user notifications.

**Endpoint:** `GET /api/Notification`

**Query Parameters:**
- `unreadOnly` (optional) - Filter unread only (true/false)

**Response:**
```json
[
  {
    "id": 1,
    "userId": 1,
    "title": "Budget Alert: Food",
    "message": "You have exceeded 80% of your Food budget",
    "type": "BudgetAlert",
    "actionUrl": "/budgets/1",
    "isRead": false,
    "createdAt": "2024-01-20T10:00:00Z"
  }
]
```

**Notification Types:**
- BudgetAlert
- DebtReminder
- ScheduledTransaction
- General

### Get Unread Count
Get count of unread notifications.

**Endpoint:** `GET /api/Notification/unread-count`

**Response:**
```json
{
  "count": 5
}
```

### Create Notification
Create a new notification.

**Endpoint:** `POST /api/Notification`

**Request Body:**
```json
{
  "userId": 1,
  "title": "Custom Notification",
  "message": "This is a custom message",
  "type": "General",
  "actionUrl": "/custom-page"
}
```

**Response:** Created notification object

### Mark as Read
Mark a notification as read.

**Endpoint:** `PUT /api/Notification/{id}/read`

**Response:** 204 No Content

### Mark All as Read
Mark all notifications as read.

**Endpoint:** `PUT /api/Notification/read-all`

**Response:** 204 No Content

---

## Currency API

### Get All Rates
Get all available exchange rates.

**Endpoint:** `GET /api/Currency/rates`

**Response:**
```json
[
  {
    "fromCurrency": "USD",
    "toCurrency": "VND",
    "rate": 24000.0,
    "lastUpdated": "2024-01-20T10:00:00Z"
  },
  {
    "fromCurrency": "EUR",
    "toCurrency": "VND",
    "rate": 26000.0,
    "lastUpdated": "2024-01-20T10:00:00Z"
  }
]
```

### Get Exchange Rate
Get exchange rate between two currencies.

**Endpoint:** `GET /api/Currency/rates/{fromCurrency}/{toCurrency}`

**Parameters:**
- `fromCurrency` (path) - Source currency code (e.g., USD)
- `toCurrency` (path) - Target currency code (e.g., VND)

**Response:**
```json
{
  "fromCurrency": "USD",
  "toCurrency": "VND",
  "rate": 24000.0,
  "lastUpdated": "2024-01-20T10:00:00Z"
}
```

### Convert Currency
Convert amount from one currency to another.

**Endpoint:** `POST /api/Currency/convert`

**Request Body:**
```json
{
  "fromCurrency": "USD",
  "toCurrency": "VND",
  "amount": 100
}
```

**Response:**
```json
{
  "fromCurrency": "USD",
  "toCurrency": "VND",
  "originalAmount": 100,
  "convertedAmount": 2400000,
  "exchangeRate": 24000.0,
  "rateDate": "2024-01-20T10:00:00Z"
}
```

### Update Exchange Rates
Update all exchange rates (Admin only).

**Endpoint:** `POST /api/Currency/rates/update`

**Authorization:** Requires Admin role

**Response:**
```json
{
  "message": "Exchange rates updated successfully"
}
```

---

## AI Advisor API

### Get AI Suggestions
Get AI financial suggestions for the user.

**Endpoint:** `GET /api/AiAdvisor/suggestions`

**Response:**
```json
[
  {
    "id": 1,
    "suggestionType": "Savings",
    "suggestion": "Your savings rate is 15.5%. Consider saving at least 20% of your income for financial security.",
    "createdAt": "2024-01-20T10:00:00Z"
  },
  {
    "id": 2,
    "suggestionType": "Spending Pattern",
    "suggestion": "You're spending 35.2% of your budget on Food. Consider reviewing this category for potential savings.",
    "createdAt": "2024-01-20T10:00:00Z"
  }
]
```

**Suggestion Types:**
- Savings
- Spending Pattern
- Budget
- Financial Advice

### Generate AI Suggestions
Generate new AI suggestions based on spending patterns.

**Endpoint:** `POST /api/AiAdvisor/generate`

**Request Body (Optional):**
```json
{
  "startDate": "2024-01-01",
  "endDate": "2024-01-31"
}
```

If not provided, defaults to last 3 months.

**Response:**
```json
{
  "message": "AI suggestions generated successfully"
}
```

---

## Error Responses

All endpoints may return the following error responses:

### 400 Bad Request
```json
{
  "message": "Validation error message"
}
```

### 401 Unauthorized
```json
{
  "message": "Unauthorized access"
}
```

### 403 Forbidden
```json
{
  "message": "You don't have permission to perform this action"
}
```

### 404 Not Found
```json
{
  "message": "Resource not found"
}
```

### 500 Internal Server Error
```json
{
  "message": "An error occurred while processing your request"
}
```

---

## Rate Limiting
Currently no rate limiting is implemented. Consider implementing rate limiting in production.

## Pagination
Currently no pagination is implemented. All list endpoints return all results. Consider implementing pagination for large datasets.

## Versioning
API version: v1 (implicit)

Future versions may use URL versioning: `/api/v2/...`

---

## Example Usage (JavaScript)

### Fetch Groups
```javascript
async function getGroups() {
  const response = await fetch('/api/GroupExpense', {
    headers: {
      'Authorization': 'Bearer ' + getAccessToken()
    }
  });
  
  if (!response.ok) {
    throw new Error('Failed to fetch groups');
  }
  
  return await response.json();
}
```

### Create Group Transaction
```javascript
async function createTransaction(data) {
  const response = await fetch('/api/GroupExpense/transactions', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + getAccessToken()
    },
    body: JSON.stringify(data)
  });
  
  if (!response.ok) {
    throw new Error('Failed to create transaction');
  }
  
  return await response.json();
}
```

### Get Dashboard
```javascript
async function getDashboard() {
  const response = await fetch('/api/Report/dashboard', {
    headers: {
      'Authorization': 'Bearer ' + getAccessToken()
    }
  });
  
  if (!response.ok) {
    throw new Error('Failed to fetch dashboard');
  }
  
  return await response.json();
}
```

---

## Testing with Postman

1. **Set up authentication:**
   - Add Authorization header: `Bearer {token}`
   - Or use Cookie: `AccessToken={token}`

2. **Import collection:**
   - Create a new collection
   - Add requests for each endpoint
   - Use environment variables for base URL and token

3. **Test scenarios:**
   - Create a group
   - Add members
   - Create transactions
   - View balances
   - Generate reports
   - Check notifications

---

## Support

For API support:
- Check this documentation
- Review error messages
- Check server logs
- Contact development team

## Changelog

### Version 1.0 (2024-01-20)
- Initial API release
- Group Expense endpoints
- Report endpoints
- Notification endpoints
- Currency endpoints
- AI Advisor endpoints
