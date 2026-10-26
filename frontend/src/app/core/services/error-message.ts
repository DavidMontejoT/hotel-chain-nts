import { HttpErrorResponse } from '@angular/common/http';

import { ProblemDetails } from '../models/hotel.model';

/** Convierte un error HTTP de la API en un mensaje legible para el usuario. */
export function errorMessage(err: unknown): string {
  if (err instanceof HttpErrorResponse) {
    if (err.status === 0) return 'No se pudo conectar con la API. Verifica que esté en ejecución.';
    const problem = err.error as ProblemDetails | null;
    if (problem?.errors) return Object.values(problem.errors).flat().join(' ');
    if (problem?.title) return problem.title;
  }
  return 'Ocurrió un error inesperado.';
}
