import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { ToastService } from '../services/toast.service';
import { errorInterceptor } from './error.interceptor';

describe('errorInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;
  let toast: ToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
    toast = TestBed.inject(ToastService);
  });

  afterEach(() => controller.verify());

  it('muestra un toast para errores 500 y propaga el error', () => {
    let failed = false;
    http.get('/api/hotels').subscribe({ error: () => (failed = true) });

    controller.expectOne('/api/hotels').flush(null, { status: 500, statusText: 'Server Error' });

    expect(failed).toBeTrue();
    expect(toast.toasts()[0].message).toContain('error en el servidor');
  });

  it('muestra un toast cuando el recurso no existe (404)', () => {
    http.get('/api/hotels/99').subscribe({ error: () => undefined });
    controller.expectOne('/api/hotels/99').flush({ title: 'No existe un hotel con id 99.' }, { status: 404, statusText: 'Not Found' });

    expect(toast.toasts()[0].message).toBe('No existe un hotel con id 99.');
  });

  it('deja los errores de validación (400) al componente', () => {
    http.post('/api/hotels', {}).subscribe({ error: () => undefined });
    controller.expectOne('/api/hotels').flush({ title: 'Invalid' }, { status: 400, statusText: 'Bad Request' });

    expect(toast.toasts().length).toBe(0);
  });
});
