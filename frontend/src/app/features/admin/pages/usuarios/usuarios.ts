import { NgClass } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  LucideDynamicIcon,
  LucideUsers,
  LucideUserPlus,
  LucideSearch,
  LucideSlidersHorizontal,
  LucideShieldCheck,
  LucideGraduationCap,
  LucideClipboardCheck,
  LucidePenSquare,
  LucideTrash2,
  LucideMail,
  LucidePhone,
  LucideX,
  LucideSave,
  LucideAlertTriangle
} from '@lucide/angular';

type UserRole = 'ADMIN' | 'DOCENTE' | 'REVISOR';
type UserStatus = 'activo' | 'inactivo';
type ModalMode = 'create' | 'edit' | null;

interface AdminUser {
  id: number;
  nombre: string;
  email: string;
  telefono: string;
  rol: UserRole;
  estado: UserStatus;
  ultimoAcceso: string;
}

interface UserForm {
  id: number | null;
  nombre: string;
  email: string;
  telefono: string;
  rol: UserRole;
  estado: UserStatus;
  password: string;
}

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [
    NgClass,
    FormsModule,
    LucideDynamicIcon
  ],
  templateUrl: './usuarios.html',
  styleUrl: './usuarios.css'
})
export class Usuarios {
  search = signal('');
  roleFilter = signal<'todos' | UserRole>('todos');

  modalMode = signal<ModalMode>(null);
  deleteTarget = signal<AdminUser | null>(null);
  errorMessage = signal('');

  usersIcon = LucideUsers;
  addIcon = LucideUserPlus;
  searchIcon = LucideSearch;
  filterIcon = LucideSlidersHorizontal;
  adminIcon = LucideShieldCheck;
  docenteIcon = LucideGraduationCap;
  revisorIcon = LucideClipboardCheck;
  editIcon = LucidePenSquare;
  deleteIcon = LucideTrash2;
  mailIcon = LucideMail;
  phoneIcon = LucidePhone;
  closeIcon = LucideX;
  saveIcon = LucideSave;
  warningIcon = LucideAlertTriangle;

  users = signal<AdminUser[]>([
    {
      id: 1,
      nombre: 'Carlos Pérez',
      email: 'carlos.perez@uth.edu.mx',
      telefono: '7711234567',
      rol: 'DOCENTE',
      estado: 'activo',
      ultimoAcceso: '2026-07-10'
    },
    {
      id: 2,
      nombre: 'María González',
      email: 'maria.gonzalez@uth.edu.mx',
      telefono: '7714567890',
      rol: 'REVISOR',
      estado: 'activo',
      ultimoAcceso: '2026-07-09'
    },
    {
      id: 3,
      nombre: 'Administrador Sistema',
      email: 'admin@uth.edu.mx',
      telefono: '7710000000',
      rol: 'ADMIN',
      estado: 'activo',
      ultimoAcceso: '2026-07-11'
    },
    {
      id: 4,
      nombre: 'Ana López',
      email: 'ana.lopez@uth.edu.mx',
      telefono: '7719876543',
      rol: 'DOCENTE',
      estado: 'inactivo',
      ultimoAcceso: '2026-06-28'
    }
  ]);

  userForm = signal<UserForm>(this.getEmptyForm());

  filteredUsers = computed(() => {
    const query = this.search().trim().toLowerCase();
    const role = this.roleFilter();

    let items = [...this.users()];

    if (query) {
      items = items.filter(user =>
        user.nombre.toLowerCase().includes(query) ||
        user.email.toLowerCase().includes(query) ||
        user.telefono.includes(query)
      );
    }

    if (role !== 'todos') {
      items = items.filter(user => user.rol === role);
    }

    return items;
  });

  counters = computed(() => {
    const items = this.users();

    return {
      total: items.length,
      docentes: items.filter(item => item.rol === 'DOCENTE').length,
      revisores: items.filter(item => item.rol === 'REVISOR').length,
      administradores: items.filter(item => item.rol === 'ADMIN').length
    };
  });

  isFormValid = computed(() => {
    const form = this.userForm();

    return (
      form.nombre.trim().length > 0 &&
      form.email.trim().length > 0 &&
      form.telefono.trim().length > 0 &&
      form.rol.trim().length > 0 &&
      form.estado.trim().length > 0
    );
  });

  setRoleFilter(value: string): void {
    this.roleFilter.set(value as 'todos' | UserRole);
  }

  openCreateModal(): void {
    this.errorMessage.set('');
    this.userForm.set(this.getEmptyForm());
    this.modalMode.set('create');
  }

  openEditModal(user: AdminUser): void {
    this.errorMessage.set('');

    this.userForm.set({
      id: user.id,
      nombre: user.nombre,
      email: user.email,
      telefono: user.telefono,
      rol: user.rol,
      estado: user.estado,
      password: ''
    });

    this.modalMode.set('edit');
  }

  closeUserModal(): void {
    this.modalMode.set(null);
    this.errorMessage.set('');
    this.userForm.set(this.getEmptyForm());
  }

  updateFormField(field: keyof UserForm, value: string): void {
    this.userForm.update(current => ({
      ...current,
      [field]: value
    }));
  }

  saveUser(): void {
    if (!this.isFormValid()) {
      this.errorMessage.set('Completa todos los campos obligatorios.');
      return;
    }

    const form = this.userForm();
    const mode = this.modalMode();

    if (mode === 'create') {
      const nextId = this.getNextId();

      const newUser: AdminUser = {
        id: nextId,
        nombre: form.nombre.trim(),
        email: form.email.trim(),
        telefono: form.telefono.trim(),
        rol: form.rol,
        estado: form.estado,
        ultimoAcceso: 'Sin acceso'
      };

      this.users.update(current => [newUser, ...current]);
      this.closeUserModal();
      return;
    }

    if (mode === 'edit' && form.id !== null) {
      this.users.update(current =>
        current.map(user =>
          user.id === form.id
            ? {
                ...user,
                nombre: form.nombre.trim(),
                email: form.email.trim(),
                telefono: form.telefono.trim(),
                rol: form.rol,
                estado: form.estado
              }
            : user
        )
      );

      this.closeUserModal();
    }
  }

  openDeleteModal(user: AdminUser): void {
    this.deleteTarget.set(user);
  }

  closeDeleteModal(): void {
    this.deleteTarget.set(null);
  }

  confirmDelete(): void {
    const target = this.deleteTarget();

    if (!target) return;

    this.users.update(current =>
      current.filter(user => user.id !== target.id)
    );

    this.closeDeleteModal();
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map(part => part.charAt(0).toUpperCase())
      .join('');
  }

  getRoleLabel(role: UserRole): string {
    if (role === 'ADMIN') return 'Administrador';
    if (role === 'REVISOR') return 'Revisor';
    return 'Docente';
  }

  getRoleClasses(role: UserRole): string {
    if (role === 'ADMIN') return 'bg-purple-100 text-purple-700 ring-purple-200';
    if (role === 'REVISOR') return 'bg-cyan-100 text-cyan-700 ring-cyan-200';
    return 'bg-teal-100 text-teal-700 ring-teal-200';
  }

  getStatusClasses(status: UserStatus): string {
    if (status === 'activo') return 'bg-green-100 text-green-700 ring-green-200';
    return 'bg-slate-100 text-slate-600 ring-slate-200';
  }

  private getNextId(): number {
    const ids = this.users().map(user => user.id);
    return ids.length === 0 ? 1 : Math.max(...ids) + 1;
  }

  private getEmptyForm(): UserForm {
    return {
      id: null,
      nombre: '',
      email: '',
      telefono: '',
      rol: 'DOCENTE',
      estado: 'activo',
      password: ''
    };
  }
}