export interface ResetPasswordDto {
  passwordResetToken: string;
  newPassword: string;
  confirmPassword: string;
}