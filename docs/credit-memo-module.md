# Credit Memo Module

## Overview
The Credit Memo Module is a comprehensive solution for managing customer credit memos in the KosmosERP system. It follows the established architectural patterns used throughout the ERP system and provides full CRUD operations for credit memo headers and lines.

## Purpose
Credit memos are used to:
- Issue credits to customers for returns, discounts, or adjustments
- Track credit amounts and reasons
- Associate credits with specific invoices or orders
- Manage approval workflows
- Track application of credits against outstanding balances

## Architecture

### Database Models
- **CreditMemoHeader**: Main credit memo record containing customer, amounts, dates, and status
- **CreditMemoLine**: Individual line items within a credit memo with product details and GL accounts

### Business Layer
- **CreditMemoModule**: Core business logic implementing IERPModule interface
- **DTOs**: Data transfer objects for API communication
- **Commands**: Command objects for create, edit, delete, and find operations

### API Layer
- **CreditMemoController**: RESTful API endpoints for all credit memo operations
- **Authentication**: JWT-based authentication required for all endpoints
- **Authorization**: Role-based permissions for read, create, edit, and delete operations

## Key Features

### Credit Memo Header
- Customer association
- Credit memo number (unique identifier)
- Credit memo date and due date
- Total credit amount
- Memo/description field
- Credit reason (e.g., Return, Discount, Adjustment)
- Approval status
- Application status
- Association with AR invoices or orders

### Credit Memo Lines
- Line number and total
- Quantity credited
- GL account for accounting
- Product association
- Description
- Links to original invoice or order lines

### Business Rules
- Credit memo numbers must be unique per customer
- Soft delete functionality
- Audit trail with created/updated/deleted timestamps
- Permission-based access control

## API Endpoints

### Header Operations
- `GET /api/v1/CreditMemo/GetCreditMemo?id={id}` - Retrieve credit memo by ID
- `GET /api/v1/CreditMemo/GetCreditMemoByGuid?guid={guid}` - Retrieve credit memo by GUID
- `POST /api/v1/CreditMemo/CreateCreditMemo` - Create new credit memo
- `PUT /api/v1/CreditMemo/UpdateCreditMemo` - Update existing credit memo
- `POST /api/v1/CreditMemo/DeleteCreditMemo` - Soft delete credit memo
- `POST /api/v1/CreditMemo/FindCreditMemo` - Search and list credit memos

### Line Operations
- `POST /api/v1/CreditMemo/CreateCreditMemoLine` - Create new credit memo line
- `PUT /api/v1/CreditMemo/UpdateCreditMemoLine` - Update existing credit memo line
- `POST /api/v1/CreditMemo/DeleteCreditMemoLine` - Soft delete credit memo line

## Permissions
The module implements a role-based permission system:
- **Credit Memo Users**: Role with access to credit memo operations
- **read_creditmemo**: View credit memos
- **create_creditmemo**: Create new credit memos
- **edit_creditmemo**: Modify existing credit memos
- **delete_creditmemo**: Remove credit memos

## Integration Points
- **Customers**: Links credit memos to customer accounts
- **AR Invoices**: Associates credits with specific invoices
- **Orders**: Links credits to sales orders
- **Products**: References products in credit memo lines
- **GL Accounts**: Accounting integration for financial reporting

## Usage Examples

### Creating a Credit Memo
```json
POST /api/v1/CreditMemo/CreateCreditMemo
{
  "calling_user_id": 1,
  "token": "user-token",
  "customer_id": 123,
  "credit_memo_number": "CM-2024-001",
  "credit_memo_date": "2024-01-15T00:00:00Z",
  "credit_memo_due_date": "2024-02-15T00:00:00Z",
  "credit_memo_total": 150.00,
  "memo": "Customer return - damaged goods",
  "credit_reason": "Return",
  "is_approved": false,
  "is_applied": false,
  "credit_memo_lines": [
    {
      "line_number": 1,
      "line_total": 150.00,
      "qty_credited": 1,
      "gl_account": "4000",
      "description": "Product return - damaged during shipping"
    }
  ]
}
```

### Finding Credit Memos
```json
POST /api/v1/CreditMemo/FindCreditMemo
{
  "calling_user_id": 1,
  "token": "user-token",
  "wildcard": "CM-2024"
}
```

## Testing
The module includes comprehensive unit tests in `Tests/KosmosERP.Tests.Modules/CreditMemoModuleTests.cs` covering:
- Create operations
- Read operations
- Update operations
- Delete operations
- Search functionality
- Permission validation

## Future Enhancements
Potential improvements could include:
- Credit memo approval workflows
- Integration with accounting systems
- Credit memo templates
- Bulk operations
- Advanced reporting and analytics
- Email notifications for approvals
- Credit memo numbering sequences

## Dependencies
- Entity Framework Core for data access
- JWT authentication for API security
- Role-based permissions system
- Common data helpers for audit fields
- Model validation helpers 