# Chi tiêu nhóm - Hướng dẫn Developer

## 📁 Cấu trúc File

```
MoneyTrackerApp/
├── Pages/
│   └── Groups/
│       ├── Index.cshtml          # Main view
│       └── Index.cshtml.cs       # Page model
├── wwwroot/
│   ├── css/
│   │   └── groups.css           # Styles
│   ├── js/
│   │   └── groups.js            # Vue application
│   └── docs/
│       ├── groups-features.md   # Feature documentation
│       └── groups-developer-guide.md # This file
└── Controllers/
    └── GroupExpenseController.cs # API endpoints
```

## 🔌 API Endpoints

### Groups
```
GET    /api/GroupExpense              # Get all groups
POST   /api/GroupExpense              # Create group
GET    /api/GroupExpense/{id}         # Get group details
PUT    /api/GroupExpense/{id}         # Update group
DELETE /api/GroupExpense/{id}         # Delete group
```

### Balances
```
GET    /api/GroupExpense/{id}/balances    # Get group balances
```

### Transactions
```
GET    /api/GroupExpense/{id}/transactions    # Get group transactions
POST   /api/GroupExpense/transactions        # Add transaction
PUT    /api/GroupExpense/transactions/{id}   # Update transaction
DELETE /api/GroupExpense/transactions/{id}   # Delete transaction
```

### Members
```
POST   /api/GroupExpense/members             # Add member
DELETE /api/GroupExpense/members/{id}        # Remove member
```

### Friends
```
GET    /api/Friendship/friends               # Get friends list
```

## 🎨 Vue 3 Application Structure

### State Management

```javascript
// Core state
const groups = ref([]);
const loading = ref(true);
const searchQuery = ref('');
const groupBalances = ref({});
const recentActivities = ref([]);
const friends = ref([]);
const viewMode = ref('list');

// Modal states
const showCreateModal = ref(false);
const showFilterModal = ref(false);
const showQuickExpenseModal = ref(false);
const showExportModal = ref(false);
const showTemplateModal = ref(false);
const showShortcutsModal = ref(false);

// Selection
const selectionMode = ref(false);
const selectedGroups = ref([]);
```

### Computed Properties

```javascript
const filteredGroups = computed(() => {
    // Filter and sort logic
});

const totalBalance = computed(() => {
    // Calculate total balance
});

const totalReceivables = computed(() => {
    // Calculate receivables
});

const totalPayables = computed(() => {
    // Calculate payables
});
```

### Key Methods

```javascript
// Data loading
loadGroups()
loadGroupBalance(groupId)
loadRecentActivities()
loadFriends()

// CRUD operations
createGroup()
viewGroupDetails(group)
archiveGroup(group)
leaveGroup(group)

// Modals
openCreateGroupModal()
closeCreateGroupModal()
openFilterModal()
closeFilterModal()
openExportModal()
closeExportModal()

// Features
quickAddExpense(group)
shareGroup(group)
exportGroupData(group)
toggleSelectionMode()
bulkArchive()
bulkDelete()

// Utilities
formatCurrency(amount)
formatTimeAgo(dateStr)
showToast(message, type)
```

## 🎯 Event Handlers

### Keyboard Shortcuts

```javascript
// Ctrl/Cmd + N: Create new group
// Ctrl/Cmd + F: Focus search
// Ctrl/Cmd + E: Export data
// Ctrl/Cmd + K: Open filter
// ?: Show shortcuts help
// Esc: Close modals
```

### Click Outside

```javascript
// Close dropdowns when clicking outside
// Close context menus when clicking outside
```

## 🎨 CSS Architecture

### CSS Variables

```css
:root {
    --primary: #6366f1;
    --success: #10b981;
    --danger: #ef4444;
    --warning: #f59e0b;
    --info: #3b82f6;
    --bg-main: #f8fafc;
    --bg-card: #ffffff;
    --text-primary: #0f172a;
    --text-secondary: #64748b;
    --border: #e2e8f0;
    --shadow-sm: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
    --shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
    --shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
    --shadow-xl: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
    --radius-sm: 0.5rem;
    --radius: 0.75rem;
    --radius-lg: 1rem;
    --radius-xl: 1.5rem;
}
```

### Key Classes

```css
/* Layout */
.groups-container
.page-header
.content-grid
.groups-section
.sidebar

/* Components */
.stat-card
.group-card
.group-card-content
.group-card-actions
.sidebar-card
.activity-item

/* Modals */
.modal-backdrop
.modal
.modal-header
.modal-body
.modal-footer

/* Utilities */
.btn
.btn-primary
.btn-secondary
.form-input
.form-select
.toast
```

## 🔄 Data Flow

```
User Action
    ↓
Vue Event Handler
    ↓
API Call (fetch)
    ↓
Update State (ref)
    ↓
Computed Properties Update
    ↓
DOM Re-render
    ↓
Show Toast Notification
```

## 🧪 Testing Checklist

### Functional Tests
- [ ] Create group
- [ ] Edit group
- [ ] Delete group
- [ ] Add expense
- [ ] Filter groups
- [ ] Sort groups
- [ ] Search groups
- [ ] Export data
- [ ] Share group
- [ ] Bulk actions

### UI Tests
- [ ] Responsive on mobile
- [ ] Responsive on tablet
- [ ] Responsive on desktop
- [ ] Animations work
- [ ] Modals open/close
- [ ] Toast notifications
- [ ] Loading states
- [ ] Empty states

### Accessibility Tests
- [ ] Keyboard navigation
- [ ] Screen reader support
- [ ] Focus indicators
- [ ] Color contrast
- [ ] ARIA labels

## 🚀 Performance Optimization

### Implemented
- ✅ Lazy loading for activities
- ✅ Debounced search
- ✅ Memoized computed properties
- ✅ Efficient DOM updates with Vue
- ✅ CSS animations (GPU accelerated)
- ✅ LocalStorage for preferences

### Future Improvements
- [ ] Virtual scrolling for large lists
- [ ] Image lazy loading
- [ ] Code splitting
- [ ] Service worker for offline
- [ ] IndexedDB for caching

## 🔒 Security Considerations

- ✅ CSRF protection
- ✅ XSS prevention (Vue escaping)
- ✅ Authorization checks
- ✅ Input validation
- ✅ Secure API calls

## 📦 Dependencies

```json
{
  "vue": "^3.0.0",
  "chart.js": "^4.0.0",
  "font-awesome": "^6.0.0"
}
```

## 🐛 Common Issues & Solutions

### Issue: Chart not rendering
**Solution**: Ensure Chart.js is loaded before groups.js

### Issue: Toast not showing
**Solution**: Check z-index and toast state

### Issue: Modal not closing
**Solution**: Verify click.self directive on backdrop

### Issue: Keyboard shortcuts not working
**Solution**: Check event listener is attached in onMounted

## 📝 Code Style Guide

### JavaScript
```javascript
// Use const/let, not var
const myVariable = 'value';

// Use arrow functions
const myFunction = () => {};

// Use async/await
const loadData = async () => {
    const response = await fetch(url);
    const data = await response.json();
};

// Use template literals
const message = `Hello ${name}`;
```

### CSS
```css
/* Use BEM naming */
.block__element--modifier {}

/* Use CSS variables */
color: var(--primary);

/* Mobile-first media queries */
@media (min-width: 768px) {}
```

### Vue
```javascript
// Use Composition API
setup() {
    const state = ref(value);
    const computed = computed(() => {});
    const method = () => {};
    
    return { state, computed, method };
}
```

## 🔄 Git Workflow

```bash
# Feature branch
git checkout -b feature/new-feature

# Commit with meaningful message
git commit -m "feat: add export to PDF feature"

# Push and create PR
git push origin feature/new-feature
```

## 📚 Additional Resources

- [Vue 3 Documentation](https://vuejs.org/)
- [Chart.js Documentation](https://www.chartjs.org/)
- [Font Awesome Icons](https://fontawesome.com/)
- [MDN Web Docs](https://developer.mozilla.org/)

## 🤝 Contributing

1. Fork the repository
2. Create feature branch
3. Make changes
4. Write tests
5. Submit pull request

## 📞 Support

- Technical Lead: [email]
- Slack Channel: #moneytracker-dev
- Documentation: /docs

---

**Last Updated**: December 2024
**Version**: 2.0.0
