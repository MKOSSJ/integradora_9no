import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { LucideArrowLeft, LucideDynamicIcon, LucideMail } from '@lucide/angular';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-verify-code',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    LucideDynamicIcon
  ],
  templateUrl: './verify-code.html',
  styleUrl: './verify-code.css'
})
export class VerifyCode {

  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  readonly mailIcon = LucideMail;
  readonly backIcon = LucideArrowLeft;

  email = signal('docente@utc.edu.mx');

  code = signal('');

  loading = signal(false);

  seconds = signal(60);

  canResend = computed(() => this.seconds() === 0);

  constructor() {

    const timer = setInterval(() => {

      if (this.seconds() > 0) {

        this.seconds.update(v => v - 1);

      } else {

        clearInterval(timer);

      }

    }, 1000);

  }

  verify(): void {

    if (this.code().length !== 6) {

      alert('Ingrese el código completo.');

      return;

    }

    this.loading.set(true);

    this.authService.verifyCode({

      email: this.email(),

      code: this.code()

    }).subscribe(() => {

      this.loading.set(false);

      this.router.navigate(['/auth/nueva-password']);

    });

  }

  resend(): void {

    this.seconds.set(60);

    alert('Se envió un nuevo código.');

  }

}