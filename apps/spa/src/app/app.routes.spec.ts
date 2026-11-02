import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { routes } from './app.routes';

describe('app.routes', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideRouter(routes)],
    });
  });

  it('CA1: "/" redirige a "/dashboard"', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/');

    expect(harness.routeNativeElement?.querySelector('h1')?.textContent).toContain('Dashboard');
  });

  it.each([
    ['/biblioteca', 'Biblioteca'],
    ['/buscar', 'Buscar'],
    ['/calendario', 'Calendario'],
    ['/dashboard', 'Dashboard'],
  ])('CA2: navega a %s y muestra "%s"', async (url, title) => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl(url);

    expect(harness.routeNativeElement?.querySelector('h1')?.textContent).toContain(title);
  });

  it('CA3: una ruta inexistente muestra "No encontrado" con enlace al dashboard', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/no-existe');

    const nativeElement = harness.routeNativeElement as HTMLElement;
    expect(nativeElement.querySelector('h1')?.textContent).toContain('No encontrado');

    const link = nativeElement.querySelector('a[href="/dashboard"]');
    expect(link).toBeTruthy();
  });
});
