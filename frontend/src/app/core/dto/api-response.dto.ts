export interface ApiResponseDto<T> {
  success: boolean;
  data: T | null;
  message: string | null;
  errors: string[] | null;
}
