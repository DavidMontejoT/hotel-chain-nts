import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';

import { environment } from '../../../../environments/environment';
import { HotelSearchComponent } from '../../../shared/components/hotel-search.component';
import { hotel, page } from '../../../testing/hotel-fixtures';
import { LandingComponent } from './landing.component';

describe('LandingComponent', () => {
  let fixture: ComponentFixture<LandingComponent>;
  let http: HttpTestingController;
  const url = `${environment.apiUrl}/hotels`;
  const cardTitles = () => Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('li.hotel h3')).map((h) => h.textContent);

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [LandingComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(LandingComponent);
    fixture.detectChanges();
  });

  afterEach(() => http.verify());

  it('lista los hoteles que devuelve la API', () => {
    http.expectOne((r) => r.url === url).flush(page([hotel({ id: 1, name: 'Aurora Uno' }), hotel({ id: 2, name: 'Aurora Dos' })]));
    fixture.detectChanges();

    expect(cardTitles()).toEqual(['Aurora Uno', 'Aurora Dos']);
  });

  it('envía los filtros a la API y muestra lo que responde, sin filtrar en el navegador', () => {
    http.expectOne((r) => r.url === url).flush(page([hotel()]));

    const search = fixture.debugElement.query(By.directive(HotelSearchComponent)).componentInstance as HotelSearchComponent;
    search.filterChange.emit({ name: 'test', rating: 5 });

    const req = http.expectOne((r) => r.url === url);
    expect(req.request.params.get('name')).toBe('test');
    expect(req.request.params.get('rating')).toBe('5');

    // The API is the source of truth: whatever it returns is rendered as-is.
    req.flush(page([hotel({ id: 9, name: 'Hotel Test 5 estrellas' })]));
    fixture.detectChanges();
    expect(cardTitles()).toEqual(['Hotel Test 5 estrellas']);
  });

  it('cancela la búsqueda anterior cuando llega una nueva', () => {
    http.expectOne((r) => r.url === url).flush(page([]));
    const search = fixture.debugElement.query(By.directive(HotelSearchComponent)).componentInstance as HotelSearchComponent;

    search.filterChange.emit({ name: 'aur' });
    const first = http.expectOne((r) => r.params.get('name') === 'aur');
    search.filterChange.emit({ name: 'aurora' });

    expect(first.cancelled).toBeTrue();
    http.expectOne((r) => r.params.get('name') === 'aurora').flush(page([]));
  });
});
