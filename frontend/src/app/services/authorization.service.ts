import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, catchError, map } from 'rxjs';
import { NavigationService } from './navigation.service';
import { TokenDto } from '../dto/authenticateDto';
import { loginResponseDto } from '../dto/loginReponseDto';
import { AlertLevel } from '../Enums/enums';
@Injectable({
  providedIn: 'root'
})
export class AuthorizationService {

  constructor(
    private http: HttpClient,
    private navigation: NavigationService
  ) { }

  generateDeviceId(): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
      const r = (Math.random() * 16) | 0,
        v = c === 'x' ? r : (r & 0x3) | 0x8;
      return v.toString(16);
    });
  }

  login(data: TokenDto): Observable<loginResponseDto> {
    const url = this.navigation.apiBaseUrl + '/api/User/Validate';

    return this.http.post<any>(url, data).pipe(
      map(response => {
        this.navigation.setItemWithExpiry('isAuthenticated', 'true');

        return response;
      }),
      catchError(error => {
        this.clearStorageVariables();
        console.error('login(E) | ', error);
        throw error;
      })
    );
  }

  logout(userName: string): Observable<any> {
    const url = this.navigation.apiBaseUrl + '/api/User/LogOut';

    const setTokenData: TokenDto = { userName: userName, token: '', userId: 0 };
    return this.http.post<any>(url, setTokenData).pipe(
      map(response => {
        this.navigation.setItemWithExpiry('isAuthenticated', 'false');
        return response;
      }),
      catchError(error => {
        console.error('logout() | ', error);
        this.clearStorageVariables();
        throw error;
      })
    );
  }

  checkAuthentication(): boolean {
    let authenticationValue = this.navigation.getItemWithExpiry('isAuthenticated');
    let isAuthenticated = false;

    if (authenticationValue)
      isAuthenticated = this.navigation.getItemWithExpiry('isAuthenticated') === 'true';

    if (!isAuthenticated)
      this.navigation.showUIMessage("Sesión expirada, vuelve a iniciar sesión en la aplicación.", AlertLevel.Info);

    return isAuthenticated;
  }

  clearStorageVariables() {
    localStorage.removeItem('isAuthenticated');
    localStorage.removeItem('userState');
  }


  validate(data: TokenDto): Observable<loginResponseDto> {
    const url = this.navigation.apiBaseUrl + '/api/User/Validate';

    const deviceId = this.navigation.getItemWithExpiry('deviceId') || '';

    const headers = new HttpHeaders({
      'DeviceId': deviceId
    });

    return this.http.post<loginResponseDto>(url, data, { headers }).pipe(
      map(response => {
        const isValid = response.id && response.id > 0;
        if (isValid) {
          this.navigation.setItemWithExpiry('isAuthenticated', 'true');
        } else {
          this.navigation.setItemWithExpiry('isAuthenticated', 'false');
        }
        return { ...response, isValidate: isValid } as loginResponseDto & { isValidate: boolean };
      }),
      catchError(error => {
        this.clearStorageVariables();
        console.error('validate() | ', error);
        throw error;
      })
    );

  }

}