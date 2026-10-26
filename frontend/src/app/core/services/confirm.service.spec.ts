import { TestBed } from '@angular/core/testing';

import { ConfirmService } from './confirm.service';

describe('ConfirmService', () => {
  let service: ConfirmService;

  beforeEach(() => (service = TestBed.inject(ConfirmService)));

  it('resuelve true cuando el usuario confirma', async () => {
    const answer = service.ask({ title: 'Eliminar', message: '¿Seguro?' });
    expect(service.pending()?.title).toBe('Eliminar');

    service.close(true);

    expect(await answer).toBeTrue();
    expect(service.pending()).toBeNull();
  });

  it('resuelve false cuando el usuario cancela', async () => {
    const answer = service.ask({ title: 'Eliminar', message: '¿Seguro?' });
    service.close(false);
    expect(await answer).toBeFalse();
  });
});
