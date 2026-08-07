import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

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
    RouterLink,
    LucideDynamicIcon
  ],
  templateUrl: './new-password.html',
  styleUrl: './new-password.css'
})
export class NewPassword {

  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  lockIcon = LucideLock;
  eyeIcon = LucideEye;
  eyeOffIcon = LucideEyeOff;
  successIcon = LucideCheckCircle;

  email = signal('docente@utc.edu.mx');

  code = signal('123456');

  password = signal('');

  confirmPassword = signal('');

  showPassword = signal(false);

  showConfirm = signal(false);

  loading = signal(false);

  success = signal(false);

  hasUppercase = computed(() =>
    /[A-Z]/.test(this.password())
  );

  hasLowercase = computed(() =>
    /[a-z]/.test(this.password())
  );

  hasNumber = computed(() =>
    /\d/.test(this.password())
  );

  hasSpecial = computed(() =>
    /[!@#$%^&*(),.?":{}|<>]/.test(this.password())
  );

  hasLength = computed(() =>
    this.password().length >= 8
  );

  strength = computed(() => {

    let score = 0;

    if (this.hasUppercase()) score++;
    if (this.hasLowercase()) score++;
    if (this.hasNumber()) score++;
    if (this.hasSpecial()) score++;
    if (this.hasLength()) score++;

    return score;

  });

  updatePassword(): void {

    if (this.password() !== this.confirmPassword()) {

      alert('Las contraseñas no coinciden.');

      return;

    }

    if (this.strength() < 5) {

      alert('La contraseña es demasiado débil.');

      return;

    }

    this.loading.set(true);

    this.authService.resetPassword({

      email: this.email(),

      code: this.code(),

      password: this.password()

    }).subscribe(() => {

      this.loading.set(false);

      this.success.set(true);

      setTimeout(() => {

        this.router.navigate(['/auth/login']);

      }, 2000);

    });

  }

}