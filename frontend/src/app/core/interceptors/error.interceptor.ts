import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { AuthService } from '../services/auth.service';
import { errorMessage } from '../services/error-message';
import { ToastService } from '../services/toast.service';

/**
 * Central handling of HTTP errors: network failures, expired sessions, missing resources and
 * server errors are shown as toasts. The error is re-thrown so callers can update their state.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse) {
        const isLogin = req.url.endsWith('/auth/login');
        if (err.status === 401 && !isLogin) {
          toast.error('Tu sesión expiró. Ingresa de nuevo.');
          auth.logout();
        } else if (err.status === 0 || err.status === 403 || err.status === 404 || err.status >= 500) {
          toast.error(errorMessage(err));
        }
      }
      return throwError(() => err);
    }),
  );
};
