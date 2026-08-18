import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import {
  Academia,
  RolAcademia,
  SystemRole
} from '../models/admin-catalogos.model';
import { AcademiasService } from './academias.service';
import {
  UsuarioLocalImportInput,
  UsuariosLocalService
} from './usuarios-local.service';

type PreviewRow = Record<string, any>;

interface ImportStore {
  academiasPendientes: Array<{
    publicId: string;
    nombre: string;
    descripcion: string;
    estado: 'pendiente';
    fechaImportacion: string;
  }>;
  historial: Array<{
    publicId: string;
    tipo: 'academias' | 'profesores';
    registros: number;
    fechaImportacion: string;
  }>;
}

/** Importaciones frontend temporales; nunca sincroniza silenciosamente con API. */
@Injectable({ providedIn: 'root' })
export class ImportacionesLocalService {
  static readonly storageKey = 'plandi_local_importaciones';

  private readonly academiasService = inject(AcademiasService);
  private readonly usuariosLocalService = inject(UsuariosLocalService);

  readonly academiasDataSource = {
    validate: (file: File) => this.validateAcademias(file),
    import: (items: PreviewRow[]) => this.importAcademias(items),
    downloadTemplate: () => this.downloadCsvTemplate(
      ['nombre', 'descripcion'],
      'plantilla-importacion-academias.csv'
    )
  };

  readonly profesoresDataSource = {
    validate: (file: File) => this.validateProfesores(file),
    import: (items: PreviewRow[]) => this.importProfesores(items),
    downloadTemplate: () => this.downloadCsvTemplate(
      [
        'nombre', 'apellido_paterno', 'apellido_materno', 'email',
        'telefono', 'roles', 'academia', 'rol_academia'
      ],
      'plantilla-importacion-profesores.csv'
    )
  };

  private async validateAcademias(file: File): Promise<PreviewRow[]> {
    const rows = await this.readAcademiasFile(
      file,
      ['nombre', 'descripcion']
    );
    const academias = await firstValueFrom(this.academiasService.load());
    const store = this.readStore();
    const existingNames = new Set([
      ...academias.map(item => this.normalizeValue(item.nombre)),
      ...store.academiasPendientes.map(item => this.normalizeValue(item.nombre))
    ]);
    const namesInFile = new Set<string>();

    return rows.map((row, index) => {
      const nombre = row['nombre'].trim();
      const descripcion = row['descripcion'].trim();
      const normalizedName = this.normalizeValue(nombre);
      const errors: string[] = [];

      if (!nombre) errors.push('Falta nombre de academia.');
      if (!descripcion) errors.push('Falta descripción.');
      if (nombre.length > 150) errors.push('El nombre excede 150 caracteres.');
      if (descripcion.length > 300) errors.push('La descripción excede 300 caracteres.');
      if (normalizedName && existingNames.has(normalizedName)) {
        errors.push('La academia ya existe o ya está pendiente de sincronización.');
      }
      if (normalizedName && namesInFile.has(normalizedName)) {
        errors.push('Nombre duplicado dentro del archivo.');
      }
      if (normalizedName) namesInFile.add(normalizedName);

      return {
        id: index + 1,
        nombre,
        descripcion,
        estado: errors.length ? 'error' : 'validado',
        observacion: errors.join(' ') || 'Registro válido'
      };
    });
  }

  private async importAcademias(items: PreviewRow[]): Promise<void> {
    if (items.length === 0) throw new Error('No hay registros válidos para importar.');

    const store = this.readStore();
    const now = new Date().toISOString();
    const imported = items.map(item => ({
      publicId: globalThis.crypto.randomUUID(),
      nombre: String(item['nombre']).trim(),
      descripcion: String(item['descripcion']).trim(),
      estado: 'pendiente' as const,
      fechaImportacion: now
    }));

    store.academiasPendientes.push(...imported);
    store.historial.push({
      publicId: globalThis.crypto.randomUUID(),
      tipo: 'academias',
      registros: imported.length,
      fechaImportacion: now
    });
    this.writeStore(store);
  }

  private async validateProfesores(file: File): Promise<PreviewRow[]> {
    const csvRows = await this.readCsv(file, [
      'nombre', 'apellido_paterno', 'apellido_materno', 'email',
      'telefono', 'roles', 'academia', 'rol_academia'
    ]);
    const [usuarios, academias] = await Promise.all([
      firstValueFrom(this.usuariosLocalService.load()),
      firstValueFrom(this.academiasService.load())
    ]);
    const existingEmails = new Set(
      usuarios.map(user => user.email.trim().toLocaleLowerCase())
    );
    const emailsInFile = new Set<string>();
    const academyByName = new Map(
      academias
        .filter(academia => academia.estado === 'activo')
        .map(academia => [this.normalizeValue(academia.nombre), academia])
    );

    return csvRows.map((row, index) => {
      const nombre = row['nombre'].trim();
      const apellidoPaterno = row['apellido_paterno'].trim();
      const apellidoMaterno = row['apellido_materno'].trim();
      const email = row['email'].trim();
      const telefono = row['telefono'].trim();
      const rolesText = row['roles'].trim();
      const academiaNombre = row['academia'].trim();
      const rolAcademiaText = row['rol_academia'].trim();
      const emailKey = email.toLocaleLowerCase();
      const roles = this.parseRoles(rolesText);
      const academia = academiaNombre
        ? academyByName.get(this.normalizeValue(academiaNombre))
        : undefined;
      const rolAcademia = this.parseRolAcademia(rolAcademiaText);
      const errors: string[] = [];

      if (!nombre) errors.push('Falta nombre.');
      if (!apellidoPaterno) errors.push('Falta apellido paterno.');
      if (!email) errors.push('Falta correo electrónico.');
      if (!rolesText) errors.push('Faltan roles.');
      if (nombre.length > 64) errors.push('El nombre excede 64 caracteres.');
      if (apellidoPaterno.length > 64) errors.push('El apellido paterno excede 64 caracteres.');
      if (apellidoMaterno.length > 64) errors.push('El apellido materno excede 64 caracteres.');
      if (email.length > 128) errors.push('El correo excede 128 caracteres.');
      if (telefono.length > 20) errors.push('El teléfono excede 20 caracteres.');
      if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
        errors.push('El correo no tiene un formato válido.');
      }
      if (emailKey && existingEmails.has(emailKey)) {
        errors.push('El correo ya existe en Usuarios.');
      }
      if (emailKey && emailsInFile.has(emailKey)) {
        errors.push('Correo duplicado dentro del archivo.');
      }
      if (emailKey) emailsInFile.add(emailKey);
      if (rolesText && roles.length === 0) {
        errors.push('Los roles no son reconocidos.');
      }
      if (this.hasUnknownRoles(rolesText)) {
        errors.push('Uno o más roles no son reconocidos.');
      }
      if (academiaNombre && !academia) {
        errors.push('La academia no existe o está inactiva.');
      }
      if (rolAcademiaText && !rolAcademia) {
        errors.push('El rol dentro de academia no es reconocido.');
      }
      if (rolAcademia && !academiaNombre) {
        errors.push('Debe indicar una academia para asignar un rol dentro de ella.');
      }

      return {
        id: index + 1,
        nombre,
        apellidoPaterno,
        apellidoMaterno,
        email,
        telefono,
        roles: roles.join(','),
        academia: academiaNombre,
        rolAcademia: rolAcademiaText,
        estado: errors.length ? 'error' : 'validado',
        observacion: errors.join(' ') || 'Registro válido',
        __roles: roles,
        __academiaPublicId: academia?.publicId ?? '',
        __academiaNombre: academia?.nombre ?? '',
        __rolAcademia: rolAcademia ?? ''
      };
    });
  }

  private async importProfesores(items: PreviewRow[]): Promise<void> {
    if (items.length === 0) throw new Error('No hay registros válidos para importar.');

    const inputs: UsuarioLocalImportInput[] = items.map(item => ({
      nombre: String(item['nombre']).trim(),
      apellidoPaterno: String(item['apellidoPaterno']).trim(),
      apellidoMaterno: String(item['apellidoMaterno'] ?? '').trim(),
      email: String(item['email']).trim(),
      telefono: String(item['telefono'] ?? '').trim(),
      roles: [...item['__roles']] as SystemRole[],
      academiaPublicId: String(item['__academiaPublicId'] ?? ''),
      academiaNombre: String(item['__academiaNombre'] ?? ''),
      rolEnAcademia: item['__rolAcademia'] as RolAcademia | ''
    }));

    this.usuariosLocalService.importUsers(inputs);
    const store = this.readStore();
    store.historial.push({
      publicId: globalThis.crypto.randomUUID(),
      tipo: 'profesores',
      registros: inputs.length,
      fechaImportacion: new Date().toISOString()
    });
    this.writeStore(store);
  }

  private async readCsv(file: File, expectedHeaders: string[]): Promise<Record<string, string>[]> {
    const extension = file.name.split('.').pop()?.toLocaleLowerCase();
    if (extension !== 'csv') {
      throw new Error('Esta importación local sólo admite archivos CSV.');
    }

    const rows = this.parseCsv((await file.text()).replace(/^\uFEFF/, ''));
    return this.rowsToRecords(rows, expectedHeaders, 'CSV');
  }

  private async readAcademiasFile(
    file: File,
    expectedHeaders: string[]
  ): Promise<Record<string, string>[]> {
    const extension = file.name.split('.').pop()?.toLocaleLowerCase();

    if (extension === 'csv') {
      return this.readCsv(file, expectedHeaders);
    }

    if (extension === 'xls' || extension === 'xlsx') {
      return this.readExcel(file, expectedHeaders);
    }

    throw new Error('Formato de archivo no válido.');
  }

  private async readExcel(
    file: File,
    expectedHeaders: string[]
  ): Promise<Record<string, string>[]> {
    let rows: string[][];

    try {
      const { read, utils } = await import('xlsx');
      const workbook = read(await file.arrayBuffer(), { type: 'array' });
      const firstSheetName = workbook.SheetNames[0];

      if (!firstSheetName) {
        throw new Error('El archivo Excel no contiene hojas.');
      }

      const sheet = workbook.Sheets[firstSheetName];
      const rawRows = utils.sheet_to_json<unknown[]>(sheet, {
        header: 1,
        defval: '',
        raw: false,
        blankrows: false
      });

      rows = rawRows.map(row =>
        row.map(value => this.cellToString(value))
      );
    } catch {
      throw new Error('No fue posible leer el archivo Excel.');
    }

    return this.rowsToRecords(rows, expectedHeaders, 'Excel');
  }

  private rowsToRecords(
    rows: string[][],
    expectedHeaders: string[],
    sourceLabel: 'CSV' | 'Excel'
  ): Record<string, string>[] {
    if (rows.length === 0) {
      throw new Error(`El archivo ${sourceLabel} está vacío.`);
    }

    const headers = rows[0].map(value => this.normalizeHeader(value));
    const missing = expectedHeaders.filter(header => !headers.includes(header));
    if (missing.length) {
      throw new Error(`Faltan las columnas: ${missing.join(', ')}.`);
    }

    const dataRows = rows.slice(1)
      .filter(row => row.some(value => value.trim() !== ''))
      .map(row => Object.fromEntries(
        expectedHeaders.map(header => [
          header,
          row[headers.indexOf(header)] ?? ''
        ])
      ));

    if (dataRows.length === 0) {
      throw new Error(
        `El archivo ${sourceLabel} no contiene registros para validar.`
      );
    }

    return dataRows;
  }

  private cellToString(value: unknown): string {
    if (value === null || value === undefined) return '';
    if (typeof value === 'number' && Number.isNaN(value)) return '';
    return String(value);
  }

  private parseCsv(text: string): string[][] {
    const rows: string[][] = [];
    let row: string[] = [];
    let value = '';
    let quoted = false;

    for (let index = 0; index < text.length; index += 1) {
      const character = text[index];

      if (character === '"') {
        if (quoted && text[index + 1] === '"') {
          value += '"';
          index += 1;
        } else {
          quoted = !quoted;
        }
      } else if (character === ',' && !quoted) {
        row.push(value);
        value = '';
      } else if ((character === '\n' || character === '\r') && !quoted) {
        if (character === '\r' && text[index + 1] === '\n') index += 1;
        row.push(value);
        rows.push(row);
        row = [];
        value = '';
      } else {
        value += character;
      }
    }

    if (quoted) throw new Error('El archivo CSV contiene comillas sin cerrar.');
    if (value.length > 0 || row.length > 0) {
      row.push(value);
      rows.push(row);
    }
    return rows;
  }

  private parseRoles(value: string): SystemRole[] {
    const allowed: SystemRole[] = ['DIRECTIVO', 'REVISOR', 'DOCENTE'];
    return [...new Set(value
      .split(/[;,|]/)
      .map(role => role.trim().toLocaleUpperCase())
      .filter((role): role is SystemRole => allowed.includes(role as SystemRole))
    )];
  }

  private hasUnknownRoles(value: string): boolean {
    const allowed = new Set(['DIRECTIVO', 'REVISOR', 'DOCENTE']);
    return value
      .split(/[;,|]/)
      .map(role => role.trim().toLocaleUpperCase())
      .filter(Boolean)
      .some(role => !allowed.has(role));
  }

  private parseRolAcademia(value: string): RolAcademia | '' {
    const normalized = this.normalizeValue(value);
    if (normalized === 'docente') return 'Docente';
    if (normalized === 'revisor') return 'Revisor';
    return '';
  }

  private normalizeHeader(value: string): string {
    return this.normalizeValue(value).replace(/[\s-]+/g, '_');
  }

  private normalizeValue(value: string): string {
    return value
      .trim()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLocaleLowerCase();
  }

  private readStore(): ImportStore {
    const stored = localStorage.getItem(ImportacionesLocalService.storageKey);
    if (stored === null) return { academiasPendientes: [], historial: [] };

    try {
      const parsed = JSON.parse(stored) as Partial<ImportStore>;
      return {
        academiasPendientes: Array.isArray(parsed.academiasPendientes)
          ? parsed.academiasPendientes
          : [],
        historial: Array.isArray(parsed.historial) ? parsed.historial : []
      };
    } catch {
      throw new Error('No fue posible leer las importaciones guardadas localmente.');
    }
  }

  private writeStore(store: ImportStore): void {
    localStorage.setItem(
      ImportacionesLocalService.storageKey,
      JSON.stringify(store)
    );
  }

  private downloadCsvTemplate(headers: string[], fileName: string): void {
    const blob = new Blob([`\uFEFF${headers.join(',')}\r\n`], {
      type: 'text/csv;charset=utf-8'
    });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(url);
  }
}
