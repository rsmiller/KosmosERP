export interface BaseDto {
  id: number;

  is_deleted?: boolean;

  created_on?: string; // or Date
  created_by?: number;
  created_on_timezone?: string;
  created_on_string?: string;

  updated_on?: string | null; // or Date
  updated_by?: number | null;
  updated_on_timezone?: string | null;
  updated_on_string?: string | null;

  deleted_on?: string | null; // or Date
  deleted_by?: number | null;
  deleted_on_timezone?: string | null;
  deleted_on_string?: string | null;

  guid?: string | null;
}
