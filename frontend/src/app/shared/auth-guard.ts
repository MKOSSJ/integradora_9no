import { inject } from "@angular/core"
import { AuthorizationService } from "../services/authorization.service";
import { Router, RouterStateSnapshot } from "@angular/router";


export const AuthGuard = (state: RouterStateSnapshot) => {

   
    const publicRoutes = ['policy', 'agreements','register','verification','register-company','questions','forms-navigator'];
    const currentUrl = state.url.toString();
    const normalizedUrl = currentUrl.startsWith("/") ? state.url.substring(1) : state.url;
    if (publicRoutes.includes(normalizedUrl.toString()))  {
      return true;
    }
    
    const authService = inject(AuthorizationService);
    const router = inject(Router);

    let isloggedIn = authService.checkAuthentication();
    return !isloggedIn ? router.navigate(['/login']) : true;
}