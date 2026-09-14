import { BaseDto, DataCommand } from "./base-models";

export class CountryDto extends BaseDto {
  country_name?: string;
  iso3?: string;
  phonecode?: string;
  currency?: string;
  currency_symbol?: string;
  region?: string;
  states?: StateDto[];
}

export class CountryListDto extends BaseDto {
  country_name?: string;
  iso3?: string;
  phonecode?: string;
  currency?: string;
  currency_symbol?: string;
  region?: string;
}

export class StateDto extends BaseDto {
  country_id?: number;
  state_name?: string;
  iso2?: string;
}

export class StateListDto extends BaseDto {
  country_id?: number;
  state_name?: string;
  iso2?: string;
}

export class CountryCreateCommand extends DataCommand {
  country_name?: string;
  iso3?: string;
  phonecode?: string;
  currency?: string;
  currency_symbol?: string;
  region?: string;
}

export class CountryEditCommand extends DataCommand {
  id?: number;
  country_name?: string;
  iso3?: string;
  phonecode?: string;
  currency?: string;
  currency_symbol?: string;
  region?: string;
}

export class StateCreateCommand extends DataCommand {
  country_id?: number;
  state_name?: string;
  iso2?: string;
}

export class StateEditCommand extends DataCommand {
  id?: number;
  country_id?: number;
  state_name?: string;
  iso2?: string;
}

export class CountryDeleteCommand extends DataCommand {
  id?: number;
}

export class CountryFindCommand extends DataCommand {
  wildcard?: string;
}

export class StateDeleteCommand extends DataCommand {
  id?: number;
}

export class StateFindCommand extends DataCommand {
  country_id?: number;
  wildcard?: string;
} 