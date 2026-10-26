import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { ImagePickerComponent } from './image-picker.component';

describe('ImagePickerComponent', () => {
  let fixture: ComponentFixture<ImagePickerComponent>;
  let http: HttpTestingController;
  const element = () => fixture.nativeElement as HTMLElement;

  function choose(file: File): void {
    const input = element().querySelector<HTMLInputElement>('input[type=file]')!;
    const files = new DataTransfer();
    files.items.add(file);
    input.files = files.files;
    input.dispatchEvent(new Event('change'));
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ImagePickerComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(ImagePickerComponent);
    fixture.detectChanges();
  });

  afterEach(() => http.verify());

  it('sube la foto y muestra el tamaño final optimizado por la API', () => {
    const changes: string[] = [];
    fixture.componentInstance.registerOnChange((value) => changes.push(value));

    choose(new File([new Uint8Array(2048)], 'foto.jpg', { type: 'image/jpeg' }));

    const req = http.expectOne(`${environment.apiUrl}/images`);
    expect(req.request.method).toBe('POST');
    expect((req.request.body as FormData).get('file')).toBeTruthy();
    req.flush({ url: '/uploads/abc.webp', width: 1920, height: 1280, sizeBytes: 398 * 1024 });
    fixture.detectChanges();

    expect(changes).toEqual(['/uploads/abc.webp']);
    expect(element().querySelector('.info')?.textContent).toContain('1920 × 1280 px');
    expect(element().querySelector('.info')?.textContent).toContain('398 KB');
  });

  it('rechaza archivos de más de 10 MB sin llamar a la API', () => {
    choose(new File([new Uint8Array(10 * 1024 * 1024 + 1)], 'enorme.jpg', { type: 'image/jpeg' }));
    fixture.detectChanges();

    http.expectNone(`${environment.apiUrl}/images`);
    expect(element().querySelector('.error-text')?.textContent).toContain('10 MB');
  });
});
