import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed, fakeAsync, flushMicrotasks } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { environment } from '../../../../environments/environment';
import { ConfirmService } from '../../../core/services/confirm.service';
import { hotel, page } from '../../../testing/hotel-fixtures';
import { HotelListComponent } from './hotel-list.component';

describe('HotelListComponent', () => {
  let fixture: ComponentFixture<HotelListComponent>;
  let http: HttpTestingController;
  let confirm: ConfirmService;
  const url = `${environment.apiUrl}/hotels`;
  const rows = () => (fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr');
  const deleteButton = () => (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('tbody .btn-danger')!;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HotelListComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
    confirm = TestBed.inject(ConfirmService);
    fixture = TestBed.createComponent(HotelListComponent);
    fixture.detectChanges();
    http.expectOne((r) => r.url === url).flush(page([hotel({ id: 1, name: 'Aurora Uno' }), hotel({ id: 2, name: 'Aurora Dos' })]));
    fixture.detectChanges();
  });

  afterEach(() => http.verify());

  it('muestra una fila por hotel', () => {
    expect(rows().length).toBe(2);
  });

  it('pide confirmación, elimina y refresca el listado automáticamente', fakeAsync(() => {
    spyOn(confirm, 'ask').and.resolveTo(true);

    deleteButton().click();
    flushMicrotasks();

    const del = http.expectOne(`${url}/1`);
    expect(del.request.method).toBe('DELETE');
    del.flush(null);

    http.expectOne((r) => r.url === url && r.method === 'GET').flush(page([hotel({ id: 2, name: 'Aurora Dos' })]));
    fixture.detectChanges();
    expect(rows().length).toBe(1);
  }));

  it('no elimina nada si se cancela la confirmación', fakeAsync(() => {
    spyOn(confirm, 'ask').and.resolveTo(false);

    deleteButton().click();
    flushMicrotasks();

    http.expectNone(`${url}/1`);
    expect(rows().length).toBe(2);
  }));
});
