export interface AdminImportColumn {
  key: string;
  label: string;
  required?: boolean;
}

export interface AdminImportConfig {
  title: string;
  subtitle: string;
  sectionLabel: string;
  importLabel: string;
  templateLabel: string;
  expectedColumns: string[];
  previewColumns: AdminImportColumn[];
  initialPreview: Record<string, any>[];
}
