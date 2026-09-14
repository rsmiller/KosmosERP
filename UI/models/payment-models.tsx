import { BaseDto, DataCommand } from "./base-models";

export class PaymentDto extends BaseDto {
    amount?: number;
    currency?: string;
    status?: string;
    payment_method?: string;
    ar_header_guid?: string;
    guid?: string;
}

export class PaymentListDto extends BaseDto {
    amount?: number;
    currency?: string;
    status?: string;
    payment_method?: string;
    ar_header_guid?: string;
    guid?: string;
}

export class PaymentCreateCommand extends DataCommand {
    amount?: number;
    currency?: string;
    payment_method?: string;
    ar_header_guid?: string;
}

export class PaymentEditCommand extends DataCommand {
    id?: number;
    amount?: number;
    currency?: string;
    payment_method?: string;
    status?: string;
    ar_header_guid?: string;
}

export class PaymentDeleteCommand extends DataCommand {
    id?: number;
}

export class PaymentFindCommand extends DataCommand {
    wildcard?: string;
}

export class PaymentProviderGetDto
{
    status?: string;
    payment_id?: string;
    charge_id?: string;
    auth_code?: string;
    receipt_number?: string;
    amount_received?: string;
}

export class PaymentProviderCreateDto
{
    clientSecret: string = "";
    id: string = "";
    amount: number = 0;
    ar_invoice_number: string = "";
}

export class CreateStripePaymentIntent extends DataCommand
{
    ar_header_guid: string = "";
    payment_method_id?: string;
}

export class StripePaymentIntentStatusCommand extends DataCommand
{
    payment_intent_id: string = "";
    payment_method_id?: string;
}

export class SavedBankDto
{
    id: string = "";
    bank: string = "";
    last4: string = "";
}

export class SavedCreditCardDto
{
    id: string = "";
    brand: string = "";
    last4: string = "";
    exp_month: string = "";
    exp_year: string = "";
}

export class SavedPaymentMethodsDto
{
    credit_cards: SavedCreditCardDto[] = [];
    banks: SavedBankDto[] = [];
}

export class GetSavedPaymentMethodsCommand extends DataCommand
{
    customer_id?: number;
    ar_invoice_header_guid?: string;
}

export class PaymentCardGetDto
{
    clientSecret: string = "";
    identifier: string = "";
    success: boolean = true;
}

export class CreateStripeNewCardIntentCommand extends DataCommand
{
    ar_header_guid: string = "";
}