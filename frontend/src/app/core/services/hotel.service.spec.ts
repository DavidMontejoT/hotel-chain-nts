import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { HotelRequest } from '../models/hotel.model';
import { hotel } from '../../testing/hotel-fixtures';
import { HotelService } from './hotel.service';

describe('HotelService', () => {
  let service: HotelService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(HotelService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('envía solo los filtros con valor', () => {
    service.search({ name: 'aurora', rating: null, minRating: 4, page: 1 }).subscribe();

    const req = http.expectOne((r) => r.url === `${environment.apiUrl}/hotels`);
    expect(req.request.params.get('name')).toBe('aurora');
    expect(req.request.params.get('minRating')).toBe('4');
    expect(req.request.params.has('rating')).toBeFalse();
    req.flush({ items: [], total: 0, page: 1, pageSize: 20, totalPages: 0 });
  });

  it('envía la búsqueda sin parámetros cuando no hay filtros', () => {
    service.search().subscribe();

    const req = http.expectOne((r) => r.url === `${environment.apiUrl}/hotels`);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.keys()).toEqual([]);
    req.flush({ items: [], total: 0, page: 1, pageSize: 20, totalPages: 0 });
  });

  it('obtiene un hotel por id', () => {
    const expected = hotel({ id: 3 });
    let result: unknown;
    service.getById(3).subscribe((h) => (result = h));

    const req = http.expectOne(`${environment.apiUrl}/hotels/3`);
    expect(req.request.method).toBe('GET');
    req.flush(expected);
    expect(result).toEqual(expected);
  });

  it('crea un hotel con POST enviando el cuerpo', () => {
    const { id, createdAt, updatedAt, ...body } = hotel();
    const request: HotelRequest = body;
    service.create(request).subscribe();

    const req = http.expectOne(`${environment.apiUrl}/hotels`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush(hotel());
  });

  it('actualiza un hotel con PUT en su URL', () => {
    const { id, createdAt, updatedAt, ...body } = hotel({ id: 5, rating: 4 });
    service.update(5, body).subscribe();

    const req = http.expectOne(`${environment.apiUrl}/hotels/5`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(body);
    req.flush(hotel({ id: 5, rating: 4 }));
  });

  it('elimina un hotel por id', () => {
    service.delete(7).subscribe();
    const req = http.expectOne(`${environment.apiUrl}/hotels/7`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
