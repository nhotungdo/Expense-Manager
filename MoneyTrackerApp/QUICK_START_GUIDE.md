# Quick Start Guide: New Features

## 🎯 Group & Split Bill Feature

### Creating a Group
1. Navigate to `/Groups` page
2. Click "Create New Group" button
3. Fill in:
   - **Group Name**: e.g., "Travel to Da Nang"
   - **Description**: Optional details about the group
   - **Icon**: Choose from preset emojis (🏠, ✈️, 🍕, etc.)
   - **Color**: Pick a color for easy identification
   - **Public/Private**: Toggle group visibility
4. Click "Save Group"

### Adding Group Members
1. Open a group by clicking on it
2. Go to "Members" tab
3. Click "Add Member"
4. Select user and assign role (Owner/Admin/Member)
5. Members can now participate in group expenses

### Recording Group Expenses
1. Open a group
2. Click "Add Expense" button
3. Enter:
   - **Description**: What was purchased
   - **Amount**: Total amount paid
   - **Currency**: VND, USD, or EUR
   - **Date**: When the expense occurred
   - **Category**: Optional categorization
   - **Split Method**: 
     - Equal: Divides equally among all members
     - Custom: Specify exact amounts per member
4. Click "Add Expense"

### Viewing Balances & Settlements
1. Open a group
2. Go to "Balances" tab
3. See:
   - Each member's total paid and owed amounts
   - Current balance (positive = owed money, negative = owes money)
   - **Suggested Settlements**: Optimized payment plan to settle all debts

**Example Settlement:**
```
John paid: 500,000 VND
John owes: 250,000 VND
Balance: +250,000 VND (John is owed 250k)

Jane paid: 0 VND
Jane owes: 250,000 VND
Balance: -250,000 VND (Jane owes 250k)

Settlement: Jane pays John 250,000 VND
```

## 📊 Reports & Analytics Feature

### Dashboard Overview
1. Navigate to `/Reports` page
2. View at a glance:
   - **Current Balance**: Total across all accounts
   - **Monthly Income**: This month's income
   - **Monthly Expense**: This month's expenses
   - **Monthly Savings**: Income - Expense
   - **Savings Rate**: Percentage saved
   - **Cash Flow Chart**: Last 7 days income vs expense
   - **Expense Pie Chart**: Top 5 expense categories

### Generating Reports

#### Cash Flow Report
1. Go to "Cash Flow" tab
2. Select date range (Start Date and End Date)
3. Click "Generate"
4. View:
   - Total income, expense, and net cash flow
   - Income breakdown by category
   - Expense breakdown by category
   - Daily cash flow analysis

#### Monthly Trends Report
1. Go to "Monthly Trends" tab
2. Select year (2024, 2023, 2022)
3. Click "Generate"
4. View:
   - 12-month income vs expense comparison
   - Average monthly income and expense
   - Trend indicator (Increasing/Decreasing/Stable)
   - Interactive bar chart

#### Category Breakdown Report
1. Go to "Category Breakdown" tab
2. Select date range
3. Click "Generate"
4. View:
   - Income categories with amounts and percentages
   - Expense categories with amounts and percentages
   - Transaction count per category
   - Visual category icons and colors

### AI Financial Advisor
1. Go to "AI Insights" tab
2. View automatic suggestions based on your spending
3. Click "Generate New Insights" to analyze latest data
4. Get advice on:
   - Savings rate improvements
   - High spending categories
   - Budget recommendations
   - Financial best practices

### Exporting Reports
1. Click "Export Report" button
2. Select:
   - **Report Type**: Cash Flow, Category Breakdown, or Monthly Trends
   - **Date Range**: Start and end dates
   - **File Format**: PDF, Excel (XLSX), CSV, or JSON
3. Click "Export"
4. File will be generated and path displayed

## 🔔 Notifications Feature

### Viewing Notifications
1. Navigate to `/Notifications` page
2. See unread count badge next to title
3. View all notifications in timeline format
4. Notifications include:
   - **Budget Alerts**: When you exceed budget limits
   - **Debt Reminders**: Upcoming debt payments
   - **Scheduled Transactions**: Automated transaction confirmations
   - **General Notifications**: System messages

### Managing Notifications
- **Filter**: Click "All Notifications" or "Unread Only"
- **Mark as Read**: Click on any notification
- **Mark All as Read**: Click "Mark All as Read" button
- **Auto-refresh**: Page updates every 30 seconds

### Notification Types
- 🔵 **Budget Alert**: Overspending warning
- 🔴 **Debt Reminder**: Payment due soon
- 🟢 **Scheduled Transaction**: Automatic transaction executed
- 🟣 **General**: Other system notifications

## 💱 Currency Exchange (API Only)

### Get Exchange Rates
```
GET /api/Currency/rates
```
Returns all available exchange rates

### Convert Currency
```
POST /api/Currency/convert
{
  "fromCurrency": "USD",
  "toCurrency": "VND",
  "amount": 100
}
```
Returns converted amount with current exchange rate

### Supported Currencies
- VND (Vietnamese Dong)
- USD (US Dollar)
- EUR (Euro)
- GBP (British Pound)
- JPY (Japanese Yen)

## 🤖 AI Financial Advisor (API Only)

### Get AI Suggestions
```
GET /api/AiAdvisor/suggestions
```
Returns personalized financial advice

### Generate New Suggestions
```
POST /api/AiAdvisor/generate
{
  "startDate": "2024-01-01",
  "endDate": "2024-12-31"
}
```
Analyzes spending patterns and generates new insights

### Types of AI Suggestions
1. **Savings Rate Analysis**
   - Compares your savings rate to recommended 20%
   - Suggests improvements

2. **Spending Pattern Alerts**
   - Identifies categories consuming >30% of budget
   - Recommends review and optimization

3. **Budget Recommendations**
   - Suggests creating budgets if none exist
   - Helps with financial planning

## 🔐 API Authentication

All API endpoints require JWT authentication:

```javascript
fetch('/api/GroupExpense', {
  headers: {
    'Authorization': 'Bearer ' + getCookie('AccessToken')
  }
})
```

## 📱 Mobile Responsive

All pages are fully responsive and work on:
- Desktop computers
- Tablets
- Mobile phones

## 🎨 UI Features

### Modern Design Elements
- **Gradient Icons**: Beautiful color gradients for visual appeal
- **Card Layouts**: Clean, organized information display
- **Interactive Charts**: Hover for detailed information
- **Smooth Animations**: Transitions and hover effects
- **Color Coding**: Visual indicators for different types
- **Icons**: Font Awesome icons throughout

### Accessibility
- Clear labels and descriptions
- Keyboard navigation support
- High contrast colors
- Readable font sizes

## 💡 Tips & Best Practices

### For Group Expenses
1. Set clear group names and descriptions
2. Add all members before recording expenses
3. Use equal split for simple shared costs
4. Use custom split for unequal contributions
5. Settle debts regularly to keep balances clear

### For Reports
1. Review dashboard weekly for financial health
2. Generate monthly trend reports to track progress
3. Use category breakdown to identify spending patterns
4. Export reports for tax preparation or budgeting
5. Check AI insights regularly for optimization tips

### For Notifications
1. Enable browser notifications for real-time alerts
2. Check notifications daily
3. Act on budget alerts promptly
4. Set up debt reminders in advance
5. Mark notifications as read to stay organized

## 🆘 Troubleshooting

### Groups Not Loading
- Check internet connection
- Verify you're logged in
- Refresh the page
- Clear browser cache

### Reports Not Generating
- Ensure date range is valid (start < end)
- Check you have transactions in selected period
- Try a different date range

### Notifications Not Appearing
- Check notification permissions
- Verify you have enabled notifications in settings
- Refresh the page
- Check unread count badge

## 📞 Support

For issues or questions:
1. Check this guide first
2. Review the Implementation Summary document
3. Check API documentation
4. Contact system administrator

## 🚀 Next Steps

1. **Create your first group** for shared expenses
2. **Generate a cash flow report** to see your financial overview
3. **Check notifications** for important alerts
4. **Get AI insights** for personalized financial advice
5. **Export reports** for record keeping

Enjoy your enhanced MoneyTrackerApp experience! 🎉
