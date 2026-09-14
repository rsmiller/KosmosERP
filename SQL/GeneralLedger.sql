-- =============================================================================
-- General Ledger Tables for KosmosERP
-- Created: April 5, 2026
-- Description: Creates Chart of Accounts, Journal Entry, and Financial Transaction tables
-- =============================================================================

-- =============================================================================
-- DROP TABLES (if needed for clean re-creation)
-- =============================================================================
-- DROP TABLE IF EXISTS financial_transactions;
-- DROP TABLE IF EXISTS journal_entry_lines;
-- DROP TABLE IF EXISTS journal_entry_headers;
-- DROP TABLE IF EXISTS chart_of_accounts;

-- =============================================================================
-- CREATE TABLES
-- =============================================================================

-- -----------------------------------------------------------------------------
-- Chart of Accounts
-- Account Types: 1=Asset, 2=Liability, 3=Equity, 4=Revenue, 5=Expense
-- Normal Balance: 1=Debit, 2=Credit
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS chart_of_accounts (
    id INT AUTO_INCREMENT PRIMARY KEY,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    created_on DATETIME NOT NULL,
    created_on_timezone VARCHAR(20) NOT NULL,
    created_on_string VARCHAR(50) NOT NULL,
    created_by VARCHAR(100) NOT NULL,
    updated_on DATETIME NULL,
    updated_by VARCHAR(100) NULL,
    updated_on_timezone VARCHAR(20) NULL,
    updated_on_string VARCHAR(50) NULL,
    account_number VARCHAR(20) NOT NULL,
    account_name VARCHAR(200) NOT NULL,
    account_type INT NOT NULL,
    parent_account_id INT NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    normal_balance INT NOT NULL,
    description VARCHAR(1000) NULL,
    guid VARCHAR(50) NOT NULL,
    UNIQUE INDEX idx_chart_of_accounts_account_number (account_number),
    INDEX idx_chart_of_accounts_account_type (account_type),
    INDEX idx_chart_of_accounts_parent_account_id (parent_account_id),
    INDEX idx_chart_of_accounts_guid (guid),
    INDEX idx_chart_of_accounts_is_active (is_active, is_deleted)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Journal Entry Headers
-- Reference Types: 1=Manual, 2=APInvoice, 3=ARInvoice, 4=CreditMemo, 5=Payment
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS journal_entry_headers (
    id INT AUTO_INCREMENT PRIMARY KEY,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    created_on DATETIME NOT NULL,
    created_on_timezone VARCHAR(20) NOT NULL,
    created_on_string VARCHAR(50) NOT NULL,
    created_by VARCHAR(100) NOT NULL,
    updated_on DATETIME NULL,
    updated_by VARCHAR(100) NULL,
    updated_on_timezone VARCHAR(20) NULL,
    updated_on_string VARCHAR(50) NULL,
    entry_number INT NOT NULL,
    entry_date DATETIME NOT NULL,
    description VARCHAR(1000) NULL,
    reference_type INT NULL,
    reference_id INT NULL,
    is_posted TINYINT(1) NOT NULL DEFAULT 0,
    posted_on DATETIME NULL,
    posted_by VARCHAR(100) NULL,
    is_reversed TINYINT(1) NOT NULL DEFAULT 0,
    reversed_by_entry_id INT NULL,
    fiscal_period VARCHAR(10) NULL,
    guid VARCHAR(50) NOT NULL,
    UNIQUE INDEX idx_journal_entry_headers_entry_number (entry_number),
    INDEX idx_journal_entry_headers_entry_date (entry_date),
    INDEX idx_journal_entry_headers_is_posted (is_posted),
    INDEX idx_journal_entry_headers_fiscal_period (fiscal_period),
    INDEX idx_journal_entry_headers_reference (reference_type, reference_id),
    INDEX idx_journal_entry_headers_guid (guid)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Journal Entry Lines
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS journal_entry_lines (
    id INT AUTO_INCREMENT PRIMARY KEY,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    created_on DATETIME NOT NULL,
    created_on_timezone VARCHAR(20) NOT NULL,
    created_on_string VARCHAR(50) NOT NULL,
    created_by VARCHAR(100) NOT NULL,
    updated_on DATETIME NULL,
    updated_by VARCHAR(100) NULL,
    updated_on_timezone VARCHAR(20) NULL,
    updated_on_string VARCHAR(50) NULL,
    journal_entry_header_id INT NOT NULL,
    line_number INT NOT NULL,
    chart_of_account_id INT NOT NULL,
    debit_amount DECIMAL(14,3) NOT NULL DEFAULT 0.000,
    credit_amount DECIMAL(14,3) NOT NULL DEFAULT 0.000,
    description VARCHAR(500) NULL,
    guid VARCHAR(50) NOT NULL,
    INDEX idx_journal_entry_lines_header_id (journal_entry_header_id),
    INDEX idx_journal_entry_lines_account_id (chart_of_account_id),
    INDEX idx_journal_entry_lines_guid (guid),
    CONSTRAINT fk_journal_entry_lines_header FOREIGN KEY (journal_entry_header_id)
        REFERENCES journal_entry_headers(id) ON DELETE CASCADE,
    CONSTRAINT fk_journal_entry_lines_account FOREIGN KEY (chart_of_account_id)
        REFERENCES chart_of_accounts(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Financial Transactions (Ledger)
-- Transaction Types: 1=JournalEntry, 2=APPost, 3=ARPost, 4=PaymentReceived, 5=PaymentSent, 6=CreditMemoApplied
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS financial_transactions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    created_on DATETIME NOT NULL,
    created_on_timezone VARCHAR(20) NOT NULL,
    created_on_string VARCHAR(50) NOT NULL,
    created_by VARCHAR(100) NOT NULL,
    updated_on DATETIME NULL,
    updated_by VARCHAR(100) NULL,
    updated_on_timezone VARCHAR(20) NULL,
    updated_on_string VARCHAR(50) NULL,
    transaction_date DATETIME NOT NULL,
    transaction_type INT NOT NULL,
    source_module VARCHAR(50) NOT NULL,
    source_id INT NOT NULL,
    source_guid VARCHAR(50) NOT NULL,
    chart_of_account_id INT NOT NULL,
    debit_amount DECIMAL(14,3) NOT NULL DEFAULT 0.000,
    credit_amount DECIMAL(14,3) NOT NULL DEFAULT 0.000,
    running_balance DECIMAL(14,3) NOT NULL DEFAULT 0.000,
    description VARCHAR(1000) NULL,
    fiscal_period VARCHAR(10) NULL,
    journal_entry_id INT NULL,
    is_reversal TINYINT(1) NOT NULL DEFAULT 0,
    guid VARCHAR(50) NOT NULL,
    INDEX idx_financial_transactions_date (transaction_date),
    INDEX idx_financial_transactions_type (transaction_type),
    INDEX idx_financial_transactions_account_id (chart_of_account_id),
    INDEX idx_financial_transactions_source (source_module, source_id),
    INDEX idx_financial_transactions_journal_entry (journal_entry_id),
    INDEX idx_financial_transactions_fiscal_period (fiscal_period),
    INDEX idx_financial_transactions_guid (guid),
    CONSTRAINT fk_financial_transactions_account FOREIGN KEY (chart_of_account_id)
        REFERENCES chart_of_accounts(id) ON DELETE RESTRICT,
    CONSTRAINT fk_financial_transactions_journal_entry FOREIGN KEY (journal_entry_id)
        REFERENCES journal_entry_headers(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================================================
-- SAMPLE DATA: Standard Chart of Accounts
-- =============================================================================

-- Account Types: 1=Asset, 2=Liability, 3=Equity, 4=Revenue, 5=Expense
-- Normal Balance: 1=Debit, 2=Credit

INSERT INTO chart_of_accounts (id, is_deleted, created_on, created_on_timezone, created_on_string, created_by, account_number, account_name, account_type, parent_account_id, is_active, normal_balance, description, guid) VALUES
-- ASSETS (1xxx) - Normal Balance: Debit
(1, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '1000', 'Assets', 1, NULL, 1, 1, 'Parent account for all assets', UUID()),
(2, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '1010', 'Cash', 1, 1, 1, 1, 'Cash on hand', UUID()),
(3, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '1020', 'Checking Account', 1, 1, 1, 1, 'Main checking account', UUID()),
(4, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '1030', 'Savings Account', 1, 1, 1, 1, 'Savings account', UUID()),
(5, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '1100', 'Accounts Receivable', 1, 1, 1, 1, 'Money owed by customers', UUID()),
(6, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '1200', 'Inventory', 1, 1, 1, 1, 'Merchandise inventory', UUID()),
(7, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '1300', 'Prepaid Expenses', 1, 1, 1, 1, 'Expenses paid in advance', UUID()),
(8, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '1500', 'Fixed Assets', 1, 1, 1, 1, 'Long-term physical assets', UUID()),
(9, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '1510', 'Equipment', 1, 8, 1, 1, 'Office and production equipment', UUID()),
(10, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '1520', 'Furniture & Fixtures', 1, 8, 1, 1, 'Office furniture and fixtures', UUID()),
(11, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '1530', 'Vehicles', 1, 8, 1, 1, 'Company vehicles', UUID()),
(12, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '1600', 'Accumulated Depreciation', 1, 1, 1, 2, 'Contra asset account for depreciation', UUID()),

-- LIABILITIES (2xxx) - Normal Balance: Credit
(13, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '2000', 'Liabilities', 2, NULL, 1, 2, 'Parent account for all liabilities', UUID()),
(14, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '2010', 'Accounts Payable', 2, 13, 1, 2, 'Money owed to vendors', UUID()),
(15, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '2020', 'Accrued Expenses', 2, 13, 1, 2, 'Expenses incurred but not yet paid', UUID()),
(16, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '2030', 'Sales Tax Payable', 2, 13, 1, 2, 'Sales tax collected and owed', UUID()),
(17, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '2040', 'Payroll Liabilities', 2, 13, 1, 2, 'Wages and taxes owed', UUID()),
(18, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '2100', 'Credit Cards Payable', 2, 13, 1, 2, 'Credit card balances', UUID()),
(19, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '2200', 'Short-Term Loans', 2, 13, 1, 2, 'Loans due within one year', UUID()),
(20, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '2500', 'Long-Term Liabilities', 2, 13, 1, 2, 'Obligations due beyond one year', UUID()),
(21, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '2510', 'Long-Term Loans', 2, 20, 1, 2, 'Loans due beyond one year', UUID()),
(22, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '2520', 'Mortgage Payable', 2, 20, 1, 2, 'Mortgage on property', UUID()),

-- EQUITY (3xxx) - Normal Balance: Credit
(23, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '3000', 'Equity', 3, NULL, 1, 2, 'Parent account for equity', UUID()),
(24, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '3010', 'Owner''s Capital', 3, 23, 1, 2, 'Owner investment', UUID()),
(25, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '3020', 'Owner''s Draws', 3, 23, 1, 1, 'Owner withdrawals', UUID()),
(26, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '3100', 'Retained Earnings', 3, 23, 1, 2, 'Accumulated profits', UUID()),
(27, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '3200', 'Common Stock', 3, 23, 1, 2, 'Issued common stock', UUID()),
(28, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '3210', 'Additional Paid-In Capital', 3, 23, 1, 2, 'Capital above par value', UUID()),

-- REVENUE (4xxx) - Normal Balance: Credit
(29, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '4000', 'Revenue', 4, NULL, 1, 2, 'Parent account for revenue', UUID()),
(30, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '4010', 'Sales Revenue', 4, 29, 1, 2, 'Revenue from product sales', UUID()),
(31, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '4020', 'Service Revenue', 4, 29, 1, 2, 'Revenue from services', UUID()),
(32, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '4030', 'Interest Income', 4, 29, 1, 2, 'Interest earned', UUID()),
(33, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '4040', 'Other Income', 4, 29, 1, 2, 'Miscellaneous income', UUID()),
(34, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '4100', 'Sales Returns & Allowances', 4, 29, 1, 1, 'Contra revenue account', UUID()),
(35, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '4110', 'Sales Discounts', 4, 29, 1, 1, 'Discounts given to customers', UUID()),

-- EXPENSES (5xxx, 6xxx) - Normal Balance: Debit
(36, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '5000', 'Cost of Goods Sold', 5, NULL, 1, 1, 'Direct costs of products sold', UUID()),
(37, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '5010', 'Purchases', 5, 36, 1, 1, 'Merchandise purchases', UUID()),
(38, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '5020', 'Purchase Returns', 5, 36, 1, 2, 'Returns to vendors', UUID()),
(39, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '5030', 'Freight In', 5, 36, 1, 1, 'Shipping costs for purchases', UUID()),
(40, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '5040', 'Direct Labor', 5, 36, 1, 1, 'Direct manufacturing labor', UUID()),

(41, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6000', 'Operating Expenses', 5, NULL, 1, 1, 'Parent account for operating expenses', UUID()),
(42, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6010', 'Advertising Expense', 5, 41, 1, 1, 'Marketing and advertising costs', UUID()),
(43, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6020', 'Bank Charges', 5, 41, 1, 1, 'Bank service fees', UUID()),
(44, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6030', 'Depreciation Expense', 5, 41, 1, 1, 'Asset depreciation', UUID()),
(45, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6040', 'Insurance Expense', 5, 41, 1, 1, 'Insurance premiums', UUID()),
(46, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6050', 'Interest Expense', 5, 41, 1, 1, 'Interest paid on loans', UUID()),
(47, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6060', 'Office Supplies', 5, 41, 1, 1, 'Office supply expenses', UUID()),
(48, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6070', 'Professional Fees', 5, 41, 1, 1, 'Legal, accounting, consulting fees', UUID()),
(49, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6080', 'Rent Expense', 5, 41, 1, 1, 'Building and equipment rent', UUID()),
(50, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6090', 'Repairs & Maintenance', 5, 41, 1, 1, 'Repair and maintenance costs', UUID()),
(51, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6100', 'Salaries & Wages', 5, 41, 1, 1, 'Employee compensation', UUID()),
(52, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6110', 'Payroll Taxes', 5, 41, 1, 1, 'Employer payroll taxes', UUID()),
(53, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6120', 'Employee Benefits', 5, 41, 1, 1, 'Health, retirement benefits', UUID()),
(54, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6130', 'Telephone Expense', 5, 41, 1, 1, 'Phone and internet services', UUID()),
(55, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6140', 'Travel Expense', 5, 41, 1, 1, 'Business travel costs', UUID()),
(56, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6150', 'Utilities Expense', 5, 41, 1, 1, 'Electric, gas, water', UUID()),
(57, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6160', 'Shipping Expense', 5, 41, 1, 1, 'Outbound shipping costs', UUID()),
(58, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6170', 'Meals & Entertainment', 5, 41, 1, 1, 'Business meals and entertainment', UUID()),
(59, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6180', 'Software & Subscriptions', 5, 41, 1, 1, 'Software licenses and SaaS subscriptions', UUID()),
(60, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6190', 'Bad Debt Expense', 5, 41, 1, 1, 'Uncollectible accounts', UUID()),
(61, 0, NOW(), '-05:00', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'), 'system', '6200', 'Miscellaneous Expense', 5, 41, 1, 1, 'Other operating expenses', UUID());

-- =============================================================================
-- ALTER EXISTING TABLES: Add GL Posting Fields
-- Run these statements to add posting fields to existing AP/AR invoice tables
-- =============================================================================

-- Add GL posting fields to AP Invoice Headers
ALTER TABLE ap_invoice_headers
    ADD COLUMN IF NOT EXISTS is_posted TINYINT(1) NOT NULL DEFAULT 0 AFTER is_paid,
    ADD COLUMN IF NOT EXISTS posted_on DATETIME NULL AFTER is_posted,
    ADD COLUMN IF NOT EXISTS posted_by VARCHAR(100) NULL AFTER posted_on;

-- Add GL posting fields to AR Invoice Headers
ALTER TABLE ar_invoice_headers
    ADD COLUMN IF NOT EXISTS is_posted TINYINT(1) NOT NULL DEFAULT 0 AFTER payment_invoice_url,
    ADD COLUMN IF NOT EXISTS posted_on DATETIME NULL AFTER is_posted,
    ADD COLUMN IF NOT EXISTS posted_by VARCHAR(100) NULL AFTER posted_on;

-- =============================================================================
-- END OF SCRIPT
-- =============================================================================
