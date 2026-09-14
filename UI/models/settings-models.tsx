import { BaseDto, DataCommand } from "./base-models";

export class SettingsDto extends BaseDto {
    company_name?: string;
    company_address1?: string;
    company_address2?: string;
    company_city?: string;
    company_state?: string;
    company_zip?: string;
    company_country?: string;
    company_phone?: string;
    company_ar_email?: string;
    company_ap_email?: string;
    company_general_email?: string;
    company_website?: string;
    tax_id?: string;
    fiscal_year_start?: string;
    guid?: string;
}

export class SettingsListDto extends BaseDto {
    company_name?: string;
    company_address1?: string;
    company_address2?: string;
    company_city?: string;
    company_state?: string;
    company_zip?: string;
    company_country?: string;
    company_phone?: string;
    company_ar_email?: string;
    company_ap_email?: string;
    company_general_email?: string;
    company_website?: string;
    tax_id?: string;
    fiscal_year_start?: string;
    guid?: string;
}

export class SettingsCreateCommand extends DataCommand {
    company_name?: string;
    company_address1?: string;
    company_address2?: string;
    company_city?: string;
    company_state?: string;
    company_zip?: string;
    company_country?: string;
    company_phone?: string;
    company_ar_email?: string;
    company_ap_email?: string;
    company_general_email?: string;
    company_website?: string;
    tax_id?: string;
    fiscal_year_start?: string;
}

export class SettingsEditCommand extends DataCommand {
    id?: number;
    company_name?: string;
    company_address1?: string;
    company_address2?: string;
    company_city?: string;
    company_state?: string;
    company_zip?: string;
    company_country?: string;
    company_phone?: string;
    company_ar_email?: string;
    company_ap_email?: string;
    company_general_email?: string;
    company_website?: string;
    tax_id?: string;
    fiscal_year_start?: string;
}

export class SettingsDeleteCommand extends DataCommand {
    id?: number;
}

export class SettingsFindCommand extends DataCommand {
    wildcard?: string;
}
