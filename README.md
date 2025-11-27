# Expense Manager - Personal Finance Application

A comprehensive, production-ready personal finance management application built with ASP.NET Core 8.0 and React 18 with TypeScript.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18.3-61DAFB)](https://reactjs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.7-3178C6)](https://www.typescriptlang.org/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## ✨ Features

### Core Features

- ✅ **User Authentication** - JWT-based authentication with refresh tokens
- ✅ **Account Management** - Multiple accounts (Cash, Bank, E-Wallet, Credit Card, Savings)
- ✅ **Transaction Tracking** - Income, Expense, and Transfer transactions with automatic balance updates
- ✅ **Category Management** - Hierarchical categories with default and custom categories
- ✅ **Budget Tracking** - Set budgets by category or account with real-time progress tracking
- ✅ **Reports & Analytics** - Visual reports with interactive charts and trends
- ✅ **Multi-Currency Support** - Support for multiple currencies with exchange rates
- ✅ **Shared Accounts** - Share accounts with other users with permission levels
- ✅ **File Attachments** - Attach receipts and documents to transactions
- ✅ **Responsive Design** - Mobile-first design with Tailwind CSS

### Technical Highlights

- 🏗️ **Clean Architecture** - Separation of concerns with Repository Pattern and Unit of Work
- 🔐 **Security** - BCrypt password hashing, JWT tokens, input validation
- 📝 **Logging** - Structured logging with Serilog
- 🔄 **Background Jobs** - Hangfire for scheduled tasks
- 📚 **API Documentation** - Swagger/OpenAPI with interactive UI
- 🐳 **Docker Support** - Complete Docker Compose setup
- 🚀 **CI/CD Ready** - GitHub Actions workflow included

## 🏗️ Architecture

This application follows **Clean Architecture** (Onion Architecture) principles:

```
┌─────────────────────────────────────┐
│         API Layer (Controllers)      │
├─────────────────────────────────────┤
│      Application Layer (Services)    │
│  - DTOs, Validators, Mappings        │
├─────────────────────────────────────┤
│   Infrastructure Layer (Data Access) │
│  - Repositories, External Services   │
├─────────────────────────────────────┤
│      Core Layer (Domain)             │
│  - Entities, Interfaces, Enums       │
└─────────────────────────────────────┘
```

## 🛠️ Tech Stack

### Backend

- **Framework**: ASP.NET Core 8.0 (LTS)
- **ORM**: Entity Framework Core 8.0
- **Database**: SQL Server
- **Authentication**: JWT Bearer Tokens
- **Validation**: FluentValidation 11.11
- **Mapping**: AutoMapper 13.0
- **Logging**: Serilog 8.0
- **Background Jobs**: Hangfire 1.8
- **API Documentation**: Swagger/OpenAPI
- **Password Hashing**: BCrypt.Net

### Frontend

- **Framework**: React 18.3
- **Language**: TypeScript 5.7
- **Build Tool**: Vite 6.0
- **Styling**: Tailwind CSS 3.4
- **Routing**: React Router 6.28
- **State Management**: Zustand 5.0
- **Data Fetching**: TanStack Query (React Query) 5.62
- **HTTP Client**: Axios 1.7
- **Forms**: React Hook Form 7.54 + Zod 3.23
- **Charts**: Recharts 2.15
- **Icons**: Lucide React 0.468

## 📋 Prerequisites

- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 20+** - [Download](https://nodejs.org/)
- **SQL Server** - LocalDB, Express, or Docker
- **Docker** (optional) - For containerized deployment

## 🚀 Quick Start

### Option 1: Docker Compose (Recommended)

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd expense-manager
   ```

2. **Run with Docker Compose**

   ```bash
   docker-compose up -d
   ```

3. **Access the application**

   - Frontend: http://localhost:3000
   - Backend API: https://localhost:5000
   - Swagger UI: https://localhost:5000/swagger
   - Hangfire Dashboard: https://localhost:5000/hangfire

4. **Initialize the database**

   ```bash
   # Connect to SQL Server container
   docker exec -it expense-manager-db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd'

   # Run schema and seed data scripts
   # Copy and paste contents of Database_Schema.sql
   # Copy and paste contents of Database_SeedData.sql
   ```

### Option 2: Local Development

#### Backend Setup

1. **Navigate to backend directory**

   ```bash
   cd MoneyTracker/MoneyTracker
   ```

2. **Update connection string in `appsettings.json`**

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ExpenseManager;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```

3. **Create database**

   - Open SQL Server Management Studio or Azure Data Studio
   - Run `Database_Schema.sql`
   - Run `Database_SeedData.sql`

4. **Run the API**

   ```bash
   dotnet restore
   dotnet run
   ```

   API will be available at:

   - HTTPS: https://localhost:5000
   - Swagger: https://localhost:5000/swagger

#### Frontend Setup

1. **Navigate to frontend directory**

   ```bash
   cd frontend
   ```

2. **Install dependencies**

   ```bash
   npm install
   ```

3. **Create `.env` file**

   ```bash
   cp .env.example .env
   ```

   Edit `.env`:

   ```
   VITE_API_URL=https://localhost:7000/api
   ```

4. **Run development server**

   ```bash
   npm run dev
   ```

   Frontend will be available at http://localhost:3000

## 📝 API Documentation

### Authentication Endpoints

```
POST   /api/auth/register          - Register new user
POST   /api/auth/login             - Login user
POST   /api/auth/refresh           - Refresh access token
GET    /api/auth/me                - Get current user
PUT    /api/auth/me                - Update user profile
```

### Account Endpoints

```
GET    /api/accounts               - Get all user accounts
GET    /api/accounts/{id}          - Get account by ID
POST   /api/accounts               - Create new account
PUT    /api/accounts/{id}          - Update account
DELETE /api/accounts/{id}          - Delete account (soft delete)
POST   /api/accounts/{id}/share    - Share account with another user
```

### Transaction Endpoints

```
GET    /api/transactions           - Get transactions (with filtering & pagination)
GET    /api/transactions/{id}      - Get transaction by ID
POST   /api/transactions           - Create new transaction
PUT    /api/transactions/{id}      - Update transaction
DELETE /api/transactions/{id}      - Delete transaction
POST   /api/transactions/{id}/attachment - Upload attachment
```

### Category Endpoints

```
GET    /api/categories             - Get all categories (hierarchical)
POST   /api/categories             - Create new category
DELETE /api/categories/{id}        - Delete category
```

### Budget Endpoints

```
GET    /api/budgets                - Get all budgets
GET    /api/budgets/summary        - Get current month budget summary
POST   /api/budgets                - Create new budget
DELETE /api/budgets/{id}           - Delete budget
```

### Report Endpoints

```
GET    /api/reports/summary        - Get financial summary with charts
```

Full API documentation available at `/swagger` when running the application.

## 🧪 Testing

### Using Postman

1. Import `Expense_Manager_API.postman_collection.json` into Postman
2. Set the `baseUrl` variable to your API URL
3. Register a new user
4. Login and the token will be automatically saved
5. Test all endpoints

### Manual Testing

See `VERIFICATION_CHECKLIST.md` for comprehensive testing instructions.

## 📊 Database Schema

The application uses a comprehensive database schema with 20+ tables including:

- **Users** - User accounts and profiles
- **Accounts** - Financial accounts (bank, cash, credit card, etc.)
- **Transactions** - Income, expense, and transfer records
- **Categories** - Hierarchical transaction categories
- **Budgets** - Budget tracking by category/account
- **SavingsGoals**, **ScheduledTransactions**, **Debts**, **GroupExpenses**, **SharedAccounts**, **BankConnections**, **CurrencyRates**, **Notifications**, **Reports**, **AuditLogs**

See `Database_Schema.sql` for complete schema definition.

## 🔐 Security Features

- **Password Hashing**: BCrypt with salt rounds
- **JWT Authentication**: Secure token-based authentication
- **Refresh Tokens**: Long-lived refresh tokens for seamless UX
- **CORS**: Configured for frontend origin
- **Input Validation**: FluentValidation on all requests
- **SQL Injection Protection**: EF Core parameterized queries
- **XSS Protection**: React's built-in escaping

## 📚 Documentation

- **README.md** - This file (overview and quick start)
- **DEPLOYMENT_GUIDE.md** - Detailed deployment instructions
- **PROJECT_SUMMARY.md** - Complete project summary and implementation details
- **VERIFICATION_CHECKLIST.md** - Step-by-step verification guide
- **Swagger UI** - Interactive API documentation at `/swagger`
- **Postman Collection** - Complete API testing collection

## 🚢 Deployment

See `DEPLOYMENT_GUIDE.md` for detailed deployment instructions including:

- Docker Compose deployment
- Azure App Service deployment
- Manual deployment
- Environment configuration
- Security checklist

## 📁 Project Structure

```
expense-manager/
├── MoneyTracker/                    # Backend ASP.NET Core API
│   ├── MoneyTracker/
│   │   ├── Core/                    # Domain layer
│   │   ├── Application/             # Application layer
│   │   ├── Infrastructure/          # Infrastructure layer
│   │   ├── Controllers/             # API layer
│   │   ├── Models/                  # EF Core entities
│   │   └── Program.cs
│   └── Dockerfile
├── frontend/                        # React TypeScript SPA
│   ├── src/
│   │   ├── components/
│   │   ├── pages/
│   │   ├── lib/
│   │   ├── store/
│   │   └── types/
│   ├── package.json
│   └── Dockerfile
├── Database_Schema.sql              # Database schema
├── Database_SeedData.sql            # Seed data
├── docker-compose.yml
├── .github/workflows/ci-cd.yml      # CI/CD pipeline
└── README.md
```

## 🤝 Contributing

This is a demonstration project. For production use, consider:

1. Implementing real bank API integration (Plaid, Yodlee)
2. Adding comprehensive unit and integration tests
3. Implementing real email service (SendGrid, AWS SES)
4. Adding real-time notifications (SignalR)
5. Implementing advanced security features (2FA, rate limiting)

## 📄 License

This project is for demonstration purposes.

---

**Built with ❤️ using ASP.NET Core 8.0 and React 18**
