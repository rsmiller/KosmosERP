import { BaseDto, DataCommand } from "./base-models";

export class JournalEntryHeaderDto extends BaseDto {
    entry_number?: string;
    entry_date?: string;
    description?: string | null;
    reference_type?: number | null;
    reference_id?: number | null;
    is_posted?: boolean;
    posted_on?: string | null;
    posted_by?: string | null;
    is_reversed?: boolean;
    reversed_by_entry_id?: number | null;
    fiscal_period?: string | null;

    reference_type_name?: string | null;
    posted_by_name?: string | null;
    lines?: JournalEntryLineDto[];
    total_debits?: number;
    total_credits?: number;
}

export class JournalEntryHeaderListDto extends BaseDto {
    entry_number?: string;
    entry_date?: string;
    description?: string | null;
    reference_type?: number | null;
    is_posted?: boolean;
    posted_on?: string | null;
    is_reversed?: boolean;
    fiscal_period?: string | null;

    reference_type_name?: string | null;
    total_debits?: number;
    total_credits?: number;
    line_count?: number;
}

export class JournalEntryLineDto extends BaseDto {
    journal_entry_header_id?: number;
    line_number?: number;
    chart_of_account_id?: number;
    debit_amount?: number;
    credit_amount?: number;
    description?: string | null;

    account_number?: string;
    account_name?: string;
}

export class JournalEntryHeaderCreateCommand extends DataCommand {
    entry_date?: string;
    description?: string | null;
    reference_type?: number | null;
    reference_id?: number | null;
    fiscal_period?: string | null;
    lines?: JournalEntryLineCreateCommand[];
}

export class JournalEntryLineCreateCommand extends DataCommand {
    journal_entry_header_id?: number;
    chart_of_account_id?: number;
    debit_amount?: number;
    credit_amount?: number;
    description?: string | null;
}

export class JournalEntryHeaderEditCommand extends DataCommand {
    id?: number;
    entry_date?: string;
    description?: string | null;
    reference_type?: number | null;
    reference_id?: number | null;
    fiscal_period?: string | null;
}

export class JournalEntryLineEditCommand extends DataCommand {
    id?: number;
    chart_of_account_id?: number;
    debit_amount?: number;
    credit_amount?: number;
    description?: string | null;
}

export class JournalEntryHeaderDeleteCommand extends DataCommand {
    id?: number;
}

export class JournalEntryLineDeleteCommand extends DataCommand {
    id?: number;
}

export class JournalEntryHeaderFindCommand extends DataCommand {
    wildcard?: string;
    is_posted?: boolean | null;
    is_reversed?: boolean | null;
    reference_type?: number | null;
    from_date?: string | null;
    to_date?: string | null;
    fiscal_period?: string | null;
}

export class JournalEntryPostCommand extends DataCommand {
    id?: number;
}

export class JournalEntryReverseCommand extends DataCommand {
    id?: number;
    reversal_date?: string;
    reversal_description?: string | null;
}

// Reference Type constants
export const JournalReferenceType = {
    Manual: 1,
    APInvoice: 2,
    ARInvoice: 3,
    CreditMemo: 4,
    Payment: 5
} as const;

export const JournalReferenceTypeNames: { [key: number]: string } = {
    1: "Manual Entry",
    2: "AP Invoice",
    3: "AR Invoice",
    4: "Credit Memo",
    5: "Payment"
};
