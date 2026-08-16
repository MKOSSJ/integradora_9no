export type AdminFieldType =
  | 'text'
  | 'email'
  | 'tel'
  | 'number'
  | 'date'
  | 'textarea'
  | 'select'
  | 'multiselect';

export interface AdminOption {
  label: string;
  value: string | number;
}

export interface AdminField {
  key: string;
  label: string;
  type: AdminFieldType;
  placeholder?: string;
  required?: boolean;
  options?: AdminOption[];
  span?: 'full' | 'half';
}

export interface AdminColumn {
  key: string;
  label: string;
  kind?: 'text' | 'status' | 'chips' | 'date';
}

export interface AdminCounter {
  label: string;
  valueKey: string;
  tone?: 'slate' | 'green' | 'cyan' | 'amber' | 'purple' | 'red';
}

export interface AdminCrudConfig {
  title: string;
  subtitle: string;
  sectionLabel: string;
  addLabel: string;
  searchPlaceholder: string;
  entityLabel: string;
  columns: AdminColumn[];
  fields: AdminField[];
  initialItems: Record<string, any>[];
  counters: AdminCounter[];
  searchKeys: string[];
}
