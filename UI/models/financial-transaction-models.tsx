import { BaseDto, DataCommand } from "./base-models";

export class FinancialTransactionDto extends BaseDto {
    transaction_date?: string;
    transaction_type?: number;
    source_module?: string;
    source_id?: number;
    source_guid?: string;
    chart_of_account_id?: number;
    debit_amount?: number;
    credit_amount?: number;
    running_balance?: number;
    description?: string | null;
    fiscal_period?: string | null;
    journal_entry_id?: number | null;
    is_reversal?: boolean;

    transaction_type_name?: string;
    account_number?: string;
    account_name?: string;
}

export class FinancialTransactionListDto extends BaseDto {
    transaction_date?: string;
    transaction_type?: number;
    source_module?: string;
    source_id?: number;
    chart_of_account_id?: number;
    debit_amount?: number;
    credit_amount?: number;
    running_balance?: number;
    description?: string | null;
    fiscal_period?: string | null;
    is_reversal?: boolean;

    transaction_type_name?: string;
    account_number?: string;
    account_name?: string;
}

export class AccountBalanceDto {
    chart_of_account_id?: number;
    account_number?: string;
    account_name?: string;
    account_type?: number;
    balance?: number;
    total_debits?: number;
    total_credits?: number;
    as_of_date?: string | null;
}

export class FinancialTransactionFindCommand extends DataCommand {
    wildcard?: string;
    chart_of_account_id?: number | null;
    transaction_type?: number | null;
    source_module?: string | null;
    from_date?: string | null;
    to_date?: string | null;
    fiscal_period?: string | null;
    is_reversal?: boolean | null;
}

export class AccountBalanceFindCommand extends DataCommand {
    chart_of_account_id?: number;
    as_of_date?: string | null;
}

// Transaction Type constants
export const FinancialTransactionType = {
    JournalEntry: 1,
    APPost: 2,
    ARPost: 3,
    PaymentReceived: 4,
    PaymentSent: 5,
    CreditMemoApplied: 6
} as const;

export const FinancialTransactionTypeNames: { [key: number]: string } = {
    1: "Journal Entry",
    2: "AP Post",
    3: "AR Post",
    4: "Payment Received",
    5: "Payment Sent",
    6: "Credit Memo Applied"
};
