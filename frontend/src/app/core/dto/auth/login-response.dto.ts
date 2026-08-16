export interface LoginResponseDto {
  success: boolean;
  message: string;
  accessToken: string | null;
  accessTokenExpiresAt: string | null;
  refreshToken: string | null;
  requiresTwoFactor?: boolean;
  roles: string[];
}
