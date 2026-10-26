import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { RouterTestingHarness } from '@angular/router/testing';

import { routes } from './app.routes';
import { LandingComponent } from './features/public/landing/landing.component';
import { LoginComponent } from './features/admin/login/login.component';

describe('app routes', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideRouter(routes), provideHttpClient(), provideHttpClientTesting()],
    });
  });

  it('lazy loads each feature area with loadChildren', () => {
    const areas = routes.filter((r) => r.loadChildren).map((r) => r.path);
    expect(areas).toEqual(['', 'admin']);
  });

  it('renders the public landing at the root', async () => {
    const harness = await RouterTestingHarness.create();
    expect(await harness.navigateByUrl('/', LandingComponent)).toBeTruthy();
  });

  it('redirects anonymous users from the admin area to the login page', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/admin/hotels', LoginComponent);
    expect(TestBed.inject(Router).url).toContain('/admin/login');
  });
});
