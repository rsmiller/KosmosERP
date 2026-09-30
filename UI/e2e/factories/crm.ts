import type { ActivityDto } from '@/models/activity-models';
import type { AddressDto } from '@/models/address-models';
import type { CommentDto } from '@/models/comment-models';
import type { ContactDto } from '@/models/contact-models';
import type { LeadDto } from '@/models/lead-models';
import type { OpportunityDto, OpportunityLineDto } from '@/models/opportunity-models';
import { LeadStageKeys, OpportunityStageKeys } from './lookups';
import { baseDto, DUE_DATE_ONLY, FIXED_DATE, nextId } from './sequence';

export function buildAddress(overrides: Partial<AddressDto> = {}): AddressDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('addr', id),
    street_address1: `${100 + id} Fabrication Way`,
    street_address2: '',
    city: 'Austin',
    state: 'TX',
    postal_code: '78701',
    country: 'USA',
    ...overrides,
  } as AddressDto;
}

export function buildContact(overrides: Partial<ContactDto> = {}): ContactDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('cont', id),
    customer_id: 1,
    first_name: 'Casey',
    last_name: `Contact${id}`,
    title: 'Purchasing Manager',
    email: `casey${id}@customer.example.test`,
    phone: '555-0110',
    cell_phone: '555-0111',
    customer_name: 'Customer',
    ...overrides,
  } as ContactDto;
}

export function buildLead(overrides: Partial<LeadDto> = {}): LeadDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('lead', id),
    first_name: 'Logan',
    last_name: `Lead${id}`,
    title: 'CTO',
    email: `logan${id}@prospect.example.test`,
    phone: '555-0120',
    cell_phone: '555-0121',
    company_name: `Prospect ${id} Inc`,
    lead_stage: LeadStageKeys.New,
    stage_name: 'New',
    time_zone: 'America/Chicago',
    address_line1: '1 Prospect Plaza',
    address_line2: '',
    city: 'Austin',
    state: 'TX',
    zip: '78701',
    country: 'USA',
    is_converted: false,
    owner_id: 9001,
    owner_name: 'Ada Admin',
    ...overrides,
  } as LeadDto;
}

export function buildOpportunityLine(overrides: Partial<OpportunityLineDto> = {}): OpportunityLineDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('opln', id),
    opportunity_id: 1,
    product_id: 1,
    description: 'Workstation build',
    line_number: 1,
    quantity: 5,
    unit_price: 1800,
    product_name: 'Product',
    identifier1: 'SKU-1',
    ...overrides,
  } as OpportunityLineDto;
}

export function buildOpportunity(overrides: Partial<OpportunityDto> = {}): OpportunityDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('oppt', id),
    opportunity_name: `Opportunity ${id}`,
    customer_id: 1,
    contact_id: 1,
    amount: 9000,
    stage: OpportunityStageKeys.Proposal,
    stage_name: 'Proposal',
    win_chance: 50,
    expected_close: DUE_DATE_ONLY,
    owner_id: 9001,
    owner_name: 'Ada Admin',
    customer_name: 'Customer',
    contact_name: 'Casey Contact',
    opportunity_lines: [],
    ...overrides,
  } as OpportunityDto;
}

export function buildActivity(overrides: Partial<ActivityDto> = {}): ActivityDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('actv', id),
    subject: `Follow-up call ${id}`,
    description: 'Discuss the quote',
    activity_type: 'call',
    status: 'open',
    owner_id: 9001,
    owner_name: 'Ada Admin',
    start_date: FIXED_DATE,
    end_date: FIXED_DATE,
    priority: 'normal',
    is_all_day: false,
    ...overrides,
  } as ActivityDto;
}

export function buildComment(overrides: Partial<CommentDto> = {}): CommentDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('cmnt', id),
    object_guid: '',
    comment_text: `Comment ${id}`,
    comment_by_name: 'Ada Admin',
    ...overrides,
  } as CommentDto;
}
