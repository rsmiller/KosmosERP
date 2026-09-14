import { BaseDto, DataCommand } from "./base-models";

export class ChartOfAccountDto extends BaseDto {
    account_number?: string;
    account_name?: string;
    account_type?: number;
    parent_account_id?: number | null;
    is_active?: boolean;
    normal_balance?: number;
    description?: string | null;

    account_type_name?: string;
    normal_balance_name?: string;
    parent_account_number?: string | null;
    parent_account_name?: string | null;
}

export class ChartOfAccountListDto extends BaseDto {
    account_number?: string;
    account_name?: string;
    account_type?: number;
    parent_account_id?: number | null;
    is_active?: boolean;
    normal_balance?: number;
    description?: string | null;

    account_type_name?: string;
    normal_balance_name?: string;
    parent_account_number?: string | null;
    parent_account_name?: string | null;
}

export class ChartOfAccountCreateCommand extends DataCommand {
    account_number?: string;
    account_name?: string;
    account_type?: number;
    parent_account_id?: number | null;
    is_active?: boolean;
    normal_balance?: number;
    description?: string | null;
}

export class ChartOfAccountEditCommand extends DataCommand {
    id?: number;
    account_number?: string;
    account_name?: string;
    account_type?: number;
    parent_account_id?: number | null;
    is_active?: boolean;
    normal_balance?: number;
    description?: string | null;
}

export class ChartOfAccountDeleteCommand extends DataCommand {
    id?: number;
}

export class ChartOfAccountFindCommand extends DataCommand {
    wildcard?: string;
    account_type?: number | null;
    is_active?: boolean | null;
    parent_account_id?: number | null;
}

// Account Type constants
export const AccountType = {
    Asset: 1,
    Liability: 2,
    Equity: 3,
    Revenue: 4,
    Expense: 5
} as const;

export const AccountTypeNames: { [key: number]: string } = {
    1: "Asset",
    2: "Liability",
    3: "Equity",
    4: "Revenue",
    5: "Expense"
};

// Normal Balance constants
export const NormalBalance = {
    Debit: 1,
    Credit: 2
} as const;

export const NormalBalanceNames: { [key: number]: string } = {
    1: "Debit",
    2: "Credit"
};
