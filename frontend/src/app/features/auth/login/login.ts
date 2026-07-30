import { NgClass } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import {
  LucideDynamicIcon,
  LucideGraduationCap,
  LucideMail,
  LucideLockKeyhole,
  LucideLoaderCircle
} from '@lucide/angular';

import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    NgClass,
    FormsModule,
    RouterLink,
    LucideDynamicIcon
  ],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  appIcon = LucideGraduationCap;
  emailIcon = LucideMail;
  passwordIcon = LucideLockKeyhole;
  loaderIcon = LucideLoaderCircle;

  email = 'admin@email.com';
  password = '1234';

  loading = signal(false);
  errorMessage = signal('');

  ngOnInit(): void {
    if (this.authService.isLoggedIn()) {
      this.router.navigateByUrl('/dashboard', {
        replaceUrl: true
      });
    }
  }

  login(): void {
    if (this.loading()) return;

    this.errorMessage.set('');

    const email = this.email.trim();
    const password = this.password.trim();

    if (!email || !password) {
      this.errorMessage.set('Ingresa correo y contraseña.');
      return;
    }

    this.loading.set(true);

    setTimeout(() => {
      const success = this.authService.login(email, password);

      if (!success) {
        this.loading.set(false);
        this.errorMessage.set('Correo o contraseña incorrectos.');
        return;
      }

      this.loading.set(false);

      this.router.navigateByUrl('/dashboard', {
        replaceUrl: true
      });
    }, 600);
  }

  onSubmit(): void {
    this.login();
  }

  setDemoUser(email: string): void {
    this.email = email;
    this.password = '1234';
    this.errorMessage.set('');
  }
}