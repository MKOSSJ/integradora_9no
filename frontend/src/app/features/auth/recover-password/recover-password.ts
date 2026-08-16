import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import {
  LucideDynamicIcon,
  LucideMail,
  LucideArrowLeft,
  LucideShieldCheck,
  LucideSend
} from '@lucide/angular';

import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-recover-password',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    LucideDynamicIcon
  ],
  templateUrl: './recover-password.html',
  styleUrl: './recover-password.css'
})
export class RecoverPassword {

  private readonly authService = inject(AuthService);

  email = '';
  loading = false;
  success = false;
  errorMessage = '';

  mailIcon = LucideMail;
  backIcon = LucideArrowLeft;
  shieldIcon = LucideShieldCheck;
  sendIcon = LucideSend;

  submit(): void {

    if (this.loading) {
      return;
    }

    this.errorMessage = '';
    this.success = false;

    const email = this.email.trim().toLowerCase();

    if (!email) {
      this.errorMessage =
        'Ingresa tu correo electrónico.';
      return;
    }

    this.loading = true;

    this.authService
      .forgotPassword({
        email
      })
      .subscribe({

        next: (response: any) => {

          this.loading = false;

          this.success = true;

          console.log(
            'Recuperación solicitada:',
            response
          );
        },

        error: (error) => {

          this.loading = false;

          console.error(
            'Error al solicitar recuperación:',
            error
          );

          this.errorMessage =
            error?.error?.message ??
            'No fue posible procesar la solicitud. Intenta nuevamente.';
        }

      });
  }
}