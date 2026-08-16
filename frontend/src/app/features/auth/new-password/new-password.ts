import {
  Component,
  computed,
  inject,
  signal
} from '@angular/core';

import { FormsModule } from '@angular/forms';

import {
  ActivatedRoute,
  Router
} from '@angular/router';

import {
  LucideDynamicIcon,
  LucideLock,
  LucideEye,
  LucideEyeOff,
  LucideCheckCircle
} from '@lucide/angular';

import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-new-password',
  standalone: true,
  imports: [
    FormsModule,
    LucideDynamicIcon
  ],
  templateUrl: './new-password.html',
  styleUrl: './new-password.css'
})
export class NewPassword {

  private readonly router =
    inject(Router);

  private readonly route =
    inject(ActivatedRoute);

  private readonly authService =
    inject(AuthService);

  lockIcon = LucideLock;

  eyeIcon = LucideEye;

  eyeOffIcon = LucideEyeOff;

  successIcon = LucideCheckCircle;

  passwordResetToken =
    signal('');

  password =
    signal('');

  confirmPassword =
    signal('');

  showPassword =
    signal(false);

  showConfirm =
    signal(false);

  loading =
    signal(false);

  success =
    signal(false);

  errorMessage =
    signal('');

  hasUppercase =
    computed(() =>
      /[A-Z]/.test(this.password())
    );

  hasLowercase =
    computed(() =>
      /[a-z]/.test(this.password())
    );

  hasNumber =
    computed(() =>
      /\d/.test(this.password())
    );

  hasSpecial =
    computed(() =>
      /[!@#$%^&*(),.?":{}|<>]/.test(
        this.password()
      )
    );

  hasLength =
    computed(() =>
      this.password().length >= 8
    );

  strength =
    computed(() => {

      let score = 0;

      if (this.hasUppercase()) {
        score++;
      }

      if (this.hasLowercase()) {
        score++;
      }

      if (this.hasNumber()) {
        score++;
      }

      if (this.hasSpecial()) {
        score++;
      }

      if (this.hasLength()) {
        score++;
      }

      return score;

    });

  constructor() {

    this.route.queryParamMap
      .subscribe(params => {

        const token =
          params.get('token');

        this.passwordResetToken.set(
          token ?? ''
        );

        if (!token) {

          this.errorMessage.set(
            'El enlace de recuperación no es válido.'
          );

        }

      });

  }

  updatePassword(): void {

    if (this.loading()) {
      return;
    }

    this.errorMessage.set('');

    if (!this.passwordResetToken()) {

      this.errorMessage.set(
        'El enlace de recuperación no es válido o ha expirado.'
      );

      return;
    }

    if (!this.password()) {

      this.errorMessage.set(
        'Ingresa una nueva contraseña.'
      );

      return;
    }

    if (
      this.password() !==
      this.confirmPassword()
    ) {

      this.errorMessage.set(
        'Las contraseñas no coinciden.'
      );

      return;
    }

    if (this.strength() < 5) {

      this.errorMessage.set(
        'La contraseña no cumple con todos los requisitos.'
      );

      return;
    }

    this.loading.set(true);

    this.authService
      .resetPassword({

        passwordResetToken:
          this.passwordResetToken(),

        newPassword:
          this.password(),

        confirmPassword:
          this.confirmPassword()

      })
      .subscribe({

        next: () => {

          this.loading.set(false);

          this.success.set(true);

          setTimeout(() => {

            this.router.navigate(
              ['/auth/login'],
              {
                replaceUrl: true
              }
            );

          }, 2000);

        },

        error: (error) => {

          this.loading.set(false);

          console.error(
            'Error al actualizar contraseña:',
            error
          );

          this.errorMessage.set(
            error?.error?.message ??
            'No fue posible actualizar la contraseña. El enlace puede ser inválido o haber expirado.'
          );

        }

      });

  }

}
