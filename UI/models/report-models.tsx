// Mirrors the report catalog DTOs served by GET /api/v1/Reports/Catalog.

export interface ReportParameter {
  name: string;   // query-string key expected by the endpoint
  label: string;  // control label
  type: "date" | "int" | "string" | "select";
  required: boolean;
  // When set, the parameter is a dropdown whose options are fetched from this endpoint
  // (relative to the API root, no leading slash).
  optionsEndpoint?: string | null;
}

// One selectable option for a dropdown parameter (value is submitted, label is displayed).
export interface ReportOption {
  value: string;
  label: string;
}

export interface ReportCatalogItem {
  key: string;
  name: string;
  description: string;
  category: string;
  endpoint: string; // relative to API root, no leading slash (e.g. "api/v1/Reports/ArAging")
  parameters: ReportParameter[];
}

export interface ReportCategory {
  name: string;
  reports: ReportCatalogItem[];
}

export type ReportFormat = "html" | "pdf";
