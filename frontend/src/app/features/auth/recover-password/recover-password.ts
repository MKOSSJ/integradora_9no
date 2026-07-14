import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import {
  LucideDynamicIcon,
  LucideMail,
  LucideArrowLeft,
  LucideShieldCheck,
  LucideSend
} from '@lucide/angular';

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
  email = '';
  loading = false;
  success = false;
  errorMessage = '';

  mailIcon = LucideMail;
  backIcon = LucideArrowLeft;
  shieldIcon = LucideShieldCheck;
  sendIcon = LucideSend;

  submit(): void {
    this.errorMessage = '';
    this.success = false;

    if (!this.email.trim()) {
      this.errorMessage = 'Ingresa tu correo electrónico.';
      return;
    }

    this.loading = true;

    setTimeout(() => {
      this.loading = false;
      this.success = true;
    }, 850);
  }
}