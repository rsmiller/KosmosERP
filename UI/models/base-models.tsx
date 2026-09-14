export class BaseDto {
  id: number = 0;

  is_deleted?: boolean;

  created_on?: string; // or Date
  created_by?: string;
  created_on_timezone?: string;
  created_on_string?: string;

  updated_on?: string | null; // or Date
  updated_by?: string | null;
  updated_on_timezone?: string | null;
  updated_on_string?: string | null;

  deleted_on?: string | null; // or Date
  deleted_by?: string | null;
  deleted_on_timezone?: string | null;
  deleted_on_string?: string | null;

  guid?: string | null;
}

export class DataCommand
{

}

export interface ApiResponse<T>
{
  success: boolean;
  resultCode: number;
  exception: any;
  data?: T;
}

export interface PagedApiResponse<T>
{
  totalResultCount: number;
  totalPages: number; 
  currentPage: number;
  success: boolean;
  resultCode: number;
  exception: any;
  data?: T;
}

export class PagingSortingParameters
{
  start?: number;
  resultCount?: number;
  sortOrder?: string;
}