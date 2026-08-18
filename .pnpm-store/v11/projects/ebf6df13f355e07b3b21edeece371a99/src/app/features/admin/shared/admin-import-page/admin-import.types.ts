export interface AdminImportColumn {
  key: string;
  label: string;
  required?: boolean;
}

export interface AdminImportDataSource {
  validate(file: File): Promise<Record<string, any>[]>;
  import(items: Record<string, any>[]): Promise<void | AdminImportOutcome>;
  downloadTemplate(): void;
}

export interface AdminImportOutcome {
  message?: string;
  type?: 'success' | 'error';
  items?: Record<string, any>[];
}

export interface AdminImportConfig {
  title: string;
  subtitle: string;
  sectionLabel: string;
  importLabel: string;
  templateLabel: string;
  expectedColumns: string[];
  previewColumns: AdminImportColumn[];
  dataSource: AdminImportDataSource;
  successMessage: string;
  acceptedFileTypes?: string;
  formatHint?: string;
  showHeader?: boolean;
  showTemplateAction?: boolean;
}
