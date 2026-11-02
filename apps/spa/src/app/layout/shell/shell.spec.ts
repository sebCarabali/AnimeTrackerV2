import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Shell } from './shell';

describe('Shell', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Shell],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('muestra los 4 enlaces de navegación a Biblioteca, Buscar, Calendario y Dashboard', () => {
    const fixture = TestBed.createComponent(Shell);
    fixture.detectChanges();

    const links = fixture.nativeElement.querySelectorAll('nav a') as NodeListOf<HTMLAnchorElement>;
    const labels = Array.from(links).map((link) => link.textContent?.trim());

    expect(links.length).toBe(4);
    expect(labels).toEqual(['Biblioteca', 'Buscar', 'Calendario', 'Dashboard']);
  });

  it('expone un nav accesible con aria-label', () => {
    const fixture = TestBed.createComponent(Shell);
    fixture.detectChanges();

    const nav = fixture.nativeElement.querySelector('nav');
    expect(nav?.getAttribute('aria-label')).toBe('Navegación principal');
  });
});
