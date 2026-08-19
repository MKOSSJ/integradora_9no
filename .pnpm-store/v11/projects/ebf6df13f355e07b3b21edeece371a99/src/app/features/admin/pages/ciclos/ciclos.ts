import { NgClass } from '@angular/common';
import { Component, inject, signal, ViewChild } from '@angular/core';
import { LucideDynamicIcon, LucidePlus } from '@lucide/angular';
import { CiclosService } from '../../../../core/services/ciclos.service';
import { PeriodosService } from '../../../../core/services/periodos.service';
import { AdminCrudPage } from '../../shared/admin-crud-page/admin-crud-page';
import { AdminCrudConfig } from '../../shared/admin-crud-page/admin-crud.types';

type CiclosTab = 'ciclos' | 'periodos';

@Component({
  selector: 'app-ciclos',
  standalone: true,
  imports: [NgClass, AdminCrudPage, LucideDynamicIcon],
  templateUrl: './ciclos.html',
  styleUrl: './ciclos.css'
})
export class Ciclos {
  private readonly ciclosService = inject(CiclosService);
  private readonly periodosService = inject(PeriodosService);

  activeTab = signal<CiclosTab>('ciclos');
  createIcon = LucidePlus;
  @ViewChild('ciclosCrud') private ciclosCrud?: AdminCrudPage;
  @ViewChild('periodosCrud') private periodosCrud?: AdminCrudPage;

  config: AdminCrudConfig = {
    title: 'Ciclos Escolares',
    subtitle: 'Administra los ciclos escolares utilizados en la gestión académica.',
    sectionLabel: 'Administración',
    addLabel: 'Nuevo ciclo escolar',
    searchPlaceholder: 'Buscar...',
    entityLabel: 'ciclos escolares',
    initialItems: [],
    dataSource: this.ciclosService,
    showHeader: false,
    showCreateAction: false,
    successMessages: {
      create: 'Ciclo escolar creado correctamente.',
      update: 'Ciclo escolar actualizado correctamente.',
      delete: 'Ciclo escolar dado de baja correctamente.'
    },
    columns: [{ key:'nombre', label:'Ciclo escolar' },{ key:'fechaInicio', label:'Fecha inicio', kind:'date' },{ key:'fechaFin', label:'Fecha fin', kind:'date' },{ key:'estado', label:'Estado', kind:'status' }],
    fields: [{ key:'nombre', label:'Nombre del ciclo escolar', type:'text', required:true, maxLength:50 },{ key:'fechaInicio', label:'Fecha inicio', type:'date', required:true },{ key:'fechaFin', label:'Fecha fin', type:'date', required:true }],
    counters: [{label:'Total',valueKey:'total'},{label:'Activos',valueKey:'activos',tone:'green'},{label:'Inactivos',valueKey:'inactivos'}],
    searchKeys: ['nombre']
  };

  periodosConfig: AdminCrudConfig = {
    title: 'Periodos',
    subtitle: 'Administra los periodos asociados a los ciclos escolares.',
    sectionLabel: 'Administración',
    addLabel: 'Nuevo periodo',
    searchPlaceholder: 'Buscar...',
    entityLabel: 'periodos',
    initialItems: [],
    dataSource: this.periodosService,
    showHeader: false,
    showCreateAction: false,
    successMessages: {
      create: 'Periodo creado correctamente.',
      update: 'Periodo actualizado correctamente.',
      delete: 'Periodo dado de baja correctamente.'
    },
    columns: [{ key:'nombre', label:'Periodo' },{ key:'cicloEscolarNombre', label:'Ciclo escolar' },{ key:'fechaInicio', label:'Fecha inicio', kind:'date' },{ key:'fechaFin', label:'Fecha fin', kind:'date' },{ key:'estado', label:'Estado', kind:'status' }],
    fields: [{ key:'cicloEscolarPublicId', label:'Ciclo escolar', type:'select', required:true, options:this.periodosService.cicloOptions },{ key:'nombre', label:'Nombre del periodo', type:'text', required:true, maxLength:100 },{ key:'fechaInicio', label:'Fecha inicio', type:'date', required:true },{ key:'fechaFin', label:'Fecha fin', type:'date', required:true }],
    counters: [{label:'Total',valueKey:'total'},{label:'Activos',valueKey:'activos',tone:'green'},{label:'Inactivos',valueKey:'inactivos'}],
    searchKeys: ['nombre','cicloEscolarNombre']
  };

  setTab(tab: CiclosTab): void {
    this.activeTab.set(tab);
  }

  openActiveCreateModal(): void {
    (this.activeTab() === 'ciclos' ? this.ciclosCrud : this.periodosCrud)?.openCreateModal();
  }
}
