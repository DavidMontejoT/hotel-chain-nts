import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';

import { environment } from '../../../../environments/environment';
import { hotel } from '../../../testing/hotel-fixtures';
import { HotelFormComponent } from './hotel-form.component';

describe('HotelFormComponent', () => {
  let fixture: ComponentFixture<HotelFormComponent>;
  let http: HttpTestingController;
  let router: Router;
  const el = () => fixture.nativeElement as HTMLElement;
  const submitButton = () => el().querySelector<HTMLButtonElement>('button[type=submit]')!;

  function type(selector: string, value: string): void {
    const input = el().querySelector<HTMLInputElement>(selector)!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
  }

  function selectRating(index: number): void {
    const select = el().querySelector<HTMLSelectElement>('select[formcontrolname=rating]')!;
    select.selectedIndex = index; // 0 is the "Selecciona" placeholder, 1..5 are the stars
    select.dispatchEvent(new Event('change'));
  }

  function fillValidForm(): void {
    type('input[formcontrolname=name]', 'Hotel Test');
    type('input[formcontrolname=city]', 'Bogotá');
    type('input[formcontrolname=address]', 'Calle 100');
    selectRating(5);
    type('input[formcontrolname=pricePerNight]', '250000');
    fixture.detectChanges();
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HotelFormComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    spyOn(router, 'navigate').and.resolveTo(true);
    fixture = TestBed.createComponent(HotelFormComponent);
  });

  afterEach(() => http.verify());

  it('deshabilita Guardar mientras faltan campos obligatorios o la calificación', () => {
    fixture.detectChanges();
    expect(submitButton().disabled).toBeTrue();

    fillValidForm();
    expect(submitButton().disabled).toBeFalse();

    selectRating(0);
    fixture.detectChanges();
    expect(submitButton().disabled).toBeTrue();
  });

  it('muestra los mensajes de error al tocar un campo vacío', () => {
    fixture.detectChanges();
    const name = el().querySelector<HTMLInputElement>('input[formcontrolname=name]')!;
    name.dispatchEvent(new Event('blur'));
    fixture.detectChanges();

    expect(el().textContent).toContain('El nombre es obligatorio');
  });

  it('crea el hotel con POST a la API y vuelve al listado', () => {
    fixture.detectChanges();
    fillValidForm();

    submitButton().click();

    const req = http.expectOne(`${environment.apiUrl}/hotels`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(jasmine.objectContaining({ name: 'Hotel Test', rating: 5, pricePerNight: 250000, imageUrl: null }));
    req.flush(hotel({ name: 'Hotel Test' }));

    expect(router.navigate).toHaveBeenCalledWith(['/admin/hotels']);
  });

  it('en modo edición carga el hotel y guarda con PUT', () => {
    fixture.componentRef.setInput('id', '7');
    fixture.detectChanges();
    http.expectOne(`${environment.apiUrl}/hotels/7`).flush(hotel({ id: 7, name: 'Aurora Cali', rating: 3 }));
    fixture.detectChanges();

    expect(el().querySelector<HTMLInputElement>('input[formcontrolname=name]')!.value).toBe('Aurora Cali');

    type('input[formcontrolname=name]', 'Aurora Cali Plus');
    fixture.detectChanges();
    submitButton().click();

    const req = http.expectOne(`${environment.apiUrl}/hotels/7`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body.name).toBe('Aurora Cali Plus');
    req.flush(hotel({ id: 7, name: 'Aurora Cali Plus' }));
  });
});
