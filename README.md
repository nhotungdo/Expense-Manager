# 💰 MoneyTracker - Full-Stack Expense Management System

A modern, comprehensive expense tracking application built with ASP.NET Core 8, featuring glassmorphism UI design, AI-powered insights, and advanced reporting capabilities.

## 🚀 Features

### ✨ Core Functionality
- **User Authentication**: Google OAuth2 integration with JWT tokens
- **Expense Management**: Add, edit, delete, and categorize expenses
- **Income Tracking**: Comprehensive income management with categories
- **Dashboard**: Real-time financial overview with interactive charts
- **Category Management**: Customizable expense and income categories

### 🎨 Modern UI/UX
- **Glassmorphism Design**: Beautiful, modern interface with glass-like effects
- **Responsive Layout**: Optimized for desktop, tablet, and mobile devices
- **Dark/Light Theme**: Automatic theme switching based on user preference
- **Smooth Animations**: CSS animations and transitions for better UX
- **Interactive Charts**: Chart.js integration for data visualization

### 🤖 AI-Powered Insights
- **Smart Suggestions**: AI-generated financial recommendations
- **Spending Analysis**: Automatic analysis of spending patterns
- **Budget Recommendations**: Personalized budget advice based on spending habits
- **Trend Analysis**: Historical data analysis for better financial planning

### 📊 Advanced Reporting
- **PDF Export**: Professional PDF reports with charts and summaries
- **Excel Export**: Detailed Excel spreadsheets for further analysis
- **CSV Export**: Raw data export for external tools
- **Monthly Reports**: Automated monthly financial summaries
- **Custom Date Ranges**: Flexible reporting periods

### 📧 Email Notifications
- **Monthly Reports**: Automated monthly financial summaries via email
- **Budget Alerts**: Email notifications when budget limits are exceeded
- **Weekly Summaries**: Regular financial updates
- **SMTP Integration**: Gmail SMTP support for reliable email delivery

### 🔒 Security & Performance
- **JWT Authentication**: Secure token-based authentication
- **Role-Based Access**: Admin and user role management
- **Input Validation**: Comprehensive server-side validation
- **Audit Logging**: Complete audit trail of all user actions
- **Performance Optimization**: Database optimization and caching
- **Global Exception Handling**: Centralized error handling

## 🏗️ Architecture

### Backend (ASP.NET Core 8)
- **Clean Architecture**: Separation of concerns with Service Layer pattern
- **Repository Pattern**: Data access abstraction
- **Dependency Injection**: Built-in DI container
- **Entity Framework Core**: ORM with SQL Server
- **Serilog**: Structured logging with file and console outputs
- **Background Services**: Automated email scheduling

### Frontend (Razor Pages)
- **Modern CSS**: Glassmorphism design with CSS Grid and Flexbox
- **Chart.js**: Interactive data visualization
- **Font Awesome**: Comprehensive icon library
- **Responsive Design**: Mobile-first approach
- **Progressive Enhancement**: Works without JavaScript

### Database (SQL Server)
- **Normalized Schema**: Optimized database design
- **Indexes**: Performance-optimized queries
- **Audit Trail**: Complete user action logging
- **Data Integrity**: Foreign key constraints and validation

## 🛠️ Technology Stack

### Backend
- **.NET 8**: Latest .NET framework
- **ASP.NET Core**: Web application framework
- **Entity Framework Core**: Object-relational mapping
- **SQL Server**: Relational database
- **JWT Bearer**: Authentication tokens
- **Serilog**: Structured logging
- **MailKit**: Email functionality
- **iTextSharp**: PDF generation
- **ClosedXML**: Excel file generation

### Frontend
- **Razor Pages**: Server-side rendering
- **Bootstrap 5**: CSS framework
- **Chart.js**: Data visualization
- **Font Awesome**: Icons
- **jQuery**: JavaScript library
- **CSS Grid/Flexbox**: Modern layout

### DevOps & Tools
- **Git**: Version control
- **Visual Studio**: Development environment
- **SQL Server Management Studio**: Database management
- **Postman**: API testing

## 📁 Project Structure

```
Expense-Manager/
├── MoneyTracker/
│   ├── Controllers/          # API Controllers
│   ├── Services/            # Business Logic Services
│   ├── Models/              # Data Models & DTOs
│   ├── Pages/               # Razor Pages
│   ├── Middleware/          # Custom Middleware
│   ├── wwwroot/             # Static Files
│   │   ├── css/            # Stylesheets
│   │   ├── js/             # JavaScript
│   │   └── lib/            # Third-party Libraries
│   └── Program.cs           # Application Entry Point
└── README.md               # Project Documentation
```

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or Full)
- Visual Studio 2022 or VS Code
- Git

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Expense-Manager/MoneyTracker
   ```

2. **Update connection string**
   ```json
   // appsettings.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Your SQL Server connection string"
     }
   }
   ```

3. **Configure email settings**
   ```json
   // appsettings.json
   {
     "EmailSettings": {
       "SmtpHost": "smtp.gmail.com",
       "SmtpPort": 587,
       "SmtpUsername": "your-email@gmail.com",
       "SmtpPassword": "your-app-password"
     }
   }
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the application**
   - Navigate to `https://localhost:7249`
   - Register/Login with Google OAuth2
   - Start managing your finances!

## 🔧 Configuration

### Database Setup
The application will automatically create the database and tables on first run. Ensure your SQL Server instance is running and accessible.

### Email Configuration
Configure SMTP settings in `appsettings.json` for email functionality:
- Gmail: Use App Passwords for authentication
- Other providers: Update SMTP host and port accordingly

### Google OAuth2 Setup
1. Create a project in Google Cloud Console
2. Enable Google+ API
3. Create OAuth2 credentials
4. Update `appsettings.json` with your credentials

## 📊 API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `GET /api/auth/me` - Get current user info

### Dashboard
- `GET /api/dashboard` - Get dashboard data
- `GET /api/dashboard/monthly-report` - Monthly report
- `GET /api/dashboard/budget-analysis` - Budget analysis
- `POST /api/dashboard/generate-ai-suggestion` - Generate AI insights

### Expenses
- `GET /api/expenses` - Get expenses
- `POST /api/expenses` - Create expense
- `PUT /api/expenses/{id}` - Update expense
- `DELETE /api/expenses/{id}` - Delete expense

### Incomes
- `GET /api/incomes` - Get incomes
- `POST /api/incomes` - Create income
- `PUT /api/incomes/{id}` - Update income
- `DELETE /api/incomes/{id}` - Delete income

### Reports
- `GET /api/report/export/pdf` - Export PDF report
- `GET /api/report/export/excel` - Export Excel report
- `GET /api/report/export/csv` - Export CSV report

### Admin (Admin role required)
- `GET /api/admin/database-stats` - Database statistics
- `POST /api/admin/optimize-database` - Optimize database
- `POST /api/admin/cleanup-data` - Clean up old data
- `GET /api/admin/system-health` - System health check

## 🎨 UI Components

### Dashboard
- **Stats Cards**: Income, expenses, savings, and balance overview
- **Interactive Charts**: Monthly trends and category breakdowns
- **Recent Transactions**: Latest financial activities
- **AI Insights**: Smart recommendations and alerts

### Navigation
- **Sidebar**: Collapsible navigation with icons
- **Header**: Search, notifications, and user profile
- **Breadcrumbs**: Clear navigation hierarchy

### Forms
- **Expense/Income Forms**: Intuitive data entry
- **Category Management**: Easy category creation and editing
- **User Profile**: Comprehensive profile management

## 🔒 Security Features

### Authentication & Authorization
- JWT token-based authentication
- Role-based access control (Admin/User)
- Google OAuth2 integration
- Secure password handling

### Data Protection
- Input validation and sanitization
- SQL injection prevention
- XSS protection
- CSRF protection

### Audit & Logging
- Complete audit trail
- User action logging
- System event logging
- Error tracking and monitoring

## 📈 Performance Optimizations

### Database
- Optimized queries with proper indexing
- Connection pooling
- Query result caching
- Database maintenance automation

### Application
- Memory caching for frequently accessed data
- Lazy loading for large datasets
- Background services for heavy operations
- Efficient data serialization

### Frontend
- Minified CSS and JavaScript
- Image optimization
- Lazy loading of components
- Responsive image delivery

## 🧪 Testing

### Manual Testing
- User registration and authentication
- CRUD operations for expenses and incomes
- Dashboard functionality
- Report generation
- Email notifications

### API Testing
Use Postman or similar tools to test API endpoints:
- Import the API collection
- Set up authentication headers
- Test all CRUD operations
- Verify response formats

## 🚀 Deployment

### Local Development
```bash
dotnet run --environment Development
```

### Production Deployment
1. **Build the application**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Configure production settings**
   - Update connection strings
   - Configure email settings
   - Set up SSL certificates
   - Configure logging

3. **Deploy to hosting provider**
   - Azure App Service
   - AWS Elastic Beanstalk
   - DigitalOcean App Platform
   - Self-hosted IIS

## 📝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the documentation

## 🎯 Future Enhancements

### Planned Features
- **Mobile App**: React Native or Flutter mobile application
- **Advanced Analytics**: Machine learning for spending predictions
- **Multi-Currency Support**: International currency handling
- **Budget Planning**: Advanced budget creation and tracking
- **Investment Tracking**: Portfolio management features
- **Bill Reminders**: Automated bill payment reminders
- **Receipt Scanning**: OCR for receipt processing
- **Social Features**: Family/group expense sharing

### Technical Improvements
- **Microservices**: Break down into smaller services
- **Docker**: Containerization for easier deployment
- **CI/CD**: Automated testing and deployment
- **Monitoring**: Application performance monitoring
- **Caching**: Redis for distributed caching
- **Message Queues**: Asynchronous processing

---

**Built with ❤️ using ASP.NET Core 8 and modern web technologies**