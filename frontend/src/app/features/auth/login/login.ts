import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import {
  LucideDynamicIcon,
  LucideMail,
  LucideLockKeyhole,
  LucideGraduationCap,
  LucideArrowRight
} from '@lucide/angular';

import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    LucideDynamicIcon
  ],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  private authService = inject(AuthService);
  private router = inject(Router);

  email = '';
  password = '';
  loading = false;
  errorMessage = '';

  mailIcon = LucideMail;
  lockIcon = LucideLockKeyhole;
  logoIcon = LucideGraduationCap;
  arrowIcon = LucideArrowRight;

  submit(): void {
    this.errorMessage = '';

    if (!this.email.trim() || !this.password.trim()) {
      this.errorMessage = 'Ingresa tu correo y contraseña.';
      return;
    }

    this.loading = true;

    setTimeout(() => {
      this.authService.login(this.email, this.password);
      this.loading = false;
    }, 600);
  }
}