import { HttpErrorResponse } from '@angular/common/http';

import { ProblemDetails } from '../models/hotel.model';

/**
 * Errors already reported to the user by errorInterceptor (network, auth, not found, server).
 * Components only need to render the remaining ones (validation 400 and conflict 409).
 */
export function isHandledGlobally(err: unknown): boolean {
  return err instanceof HttpErrorResponse && (err.status === 0 || err.status === 401 || err.status === 403 || err.status === 404 || err.status >= 500);
}

/** Turns an API error into a user-facing message. */
export function errorMessage(err: unknown): string {
  if (err instanceof HttpErrorResponse) {
    if (err.status === 0) return 'No se pudo conectar con la API. Verifica que esté en ejecución.';
    if (err.status >= 500) return 'Ocurrió un error en el servidor. Intenta de nuevo en unos minutos.';
    const problem = err.error as ProblemDetails | null;
    if (problem?.errors) return Object.values(problem.errors).flat().join(' ');
    if (problem?.title) return problem.title;
    if (err.status === 404) return 'El recurso solicitado no existe.';
    if (err.status === 403) return 'No tienes permisos para realizar esta acción.';
  }
  return 'Ocurrió un error inesperado.';
}
