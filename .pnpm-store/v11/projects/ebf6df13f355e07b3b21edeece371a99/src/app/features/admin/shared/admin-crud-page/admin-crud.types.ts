import { Observable } from 'rxjs';

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
  min?: number;
  max?: number;
  step?: number;
  maxLength?: number;
  options?: AdminOption[];
  optionsFor?: (form: Record<string, any>) => AdminOption[];
  readonlyWhen?: (form: Record<string, any>) => boolean;
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
  unavailable?: boolean;
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
  dataSource?: AdminCrudDataSource;
  successMessages?: {
    create: string;
    update: string;
    delete: string;
  };
  blockedActionsMessage?: string;
  blockedCreateMessage?: string;
  blockedDeleteMessage?: string;
}

export type AdminCrudItem = AdminCrudConfig['initialItems'][number];

export interface AdminCrudDataSource {
  load(): Observable<AdminCrudItem[]>;
  create(item: AdminCrudItem): Observable<AdminCrudItem>;
  update(item: AdminCrudItem): Observable<AdminCrudItem>;
  delete(item: AdminCrudItem): Observable<boolean>;
}
