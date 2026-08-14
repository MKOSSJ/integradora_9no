export interface LoginResponseDto {
  success: boolean;
  message: string;
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  requiresTwoFactor: boolean;
}