import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

interface NavLink {
  path: string;
  label: string;
}

@Component({
  selector: 'app-shell',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  readonly navLinks: NavLink[] = [
    { path: '/biblioteca', label: 'Biblioteca' },
    { path: '/buscar', label: 'Buscar' },
    { path: '/calendario', label: 'Calendario' },
    { path: '/dashboard', label: 'Dashboard' },
  ];
}
