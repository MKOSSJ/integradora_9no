import { PlaneacionDetail, PlaneacionStatus } from '../models/planeacion.model';

export type RevisionStatus = PlaneacionStatus;

export type ReviewTab = 'vista-previa' | 'programa';

export interface RevisionItem {
  id: string;
  titulo: string;
  autor: string;
  estado: RevisionStatus;
  fechaEnvio: string;
  carrera: string;
  grupo: string;
}

export interface RevisionCounters {
  pendientes: number;
  revision: number;
  aprobados: number;
  correcciones: number;
}

export interface RevisionDetail extends PlaneacionDetail<string> {
  reviewStatus: RevisionStatus;
  enviadoPor: string;
  fechaEnvio: string;
  carrera: string;
  grupo: string;
  comentariosRevision: string[];
}
