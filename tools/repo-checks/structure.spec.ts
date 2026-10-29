import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { describe, expect, it } from 'vitest';

const root = join(__dirname, '..', '..');

const REQUIRED_DIRS = [
  'apps/spa',
  'apps/bff',
  'apps/extension',
  'services/core',
  'contracts',
  'infra',
];

// CA1 — carpetas raíz obligatorias
describe('CA1 — Estructura raíz del monorepo', () => {
  it.each(REQUIRED_DIRS)('contiene la carpeta %s', (dir) => {
    expect(existsSync(join(root, dir))).toBe(true);
  });

  it.each(REQUIRED_DIRS)('la carpeta %s tiene README.md', (dir) => {
    expect(existsSync(join(root, dir, 'README.md'))).toBe(true);
  });
});

// CA2 — README raíz
describe('CA2 — README raíz', () => {
  it('readme_raiz_contiene_secciones_requisitos_arranque_y_convenciones', () => {
    const readmePath = join(root, 'README.md');
    expect(existsSync(readmePath), 'README.md debe existir en la raíz').toBe(true);

    const content = readFileSync(readmePath, 'utf-8');

    expect(content, 'debe mencionar Node LTS').toMatch(/Node/i);
    expect(content, 'debe mencionar .NET 8').toMatch(/\.NET\s*8/i);
    expect(content, 'debe mencionar Docker').toMatch(/Docker/i);
    expect(content, 'debe describir cómo arrancar el entorno').toMatch(/arranc|setup|install/i);
    expect(content, 'debe mencionar Conventional Commits').toMatch(/Conventional Commits/i);
    expect(content, 'debe describir convención de ramas').toMatch(/rama|branch/i);
  });
});

// CA3b — .gitignore
describe('CA3b — .gitignore excluye artefactos de build y secretos', () => {
  it('gitignore_excluye_artefactos_de_build_y_secretos', () => {
    const gitignorePath = join(root, '.gitignore');
    expect(existsSync(gitignorePath), '.gitignore debe existir en la raíz').toBe(true);

    const content = readFileSync(gitignorePath, 'utf-8');

    for (const pattern of ['bin/', 'obj/', 'node_modules/', 'dist/', '.env']) {
      expect(content, `.gitignore debe excluir ${pattern}`).toContain(pattern);
    }

    // artefactos de cobertura
    const hasCoverageExclusion =
      content.includes('coverage/') ||
      content.includes('coverage-reports/') ||
      content.includes('lcov') ||
      content.includes('.nyc_output') ||
      content.includes('TestResults/');
    expect(hasCoverageExclusion, '.gitignore debe excluir artefactos de cobertura').toBe(true);
  });
});

// CA3a — .editorconfig
describe('CA3a — .editorconfig define codificación UTF-8, LF y sangría por tipo', () => {
  it('editorconfig_define_codificacion_utf8_lf_y_sangria_por_tipo', () => {
    const editorconfigPath = join(root, '.editorconfig');
    expect(existsSync(editorconfigPath), '.editorconfig debe existir en la raíz').toBe(true);

    const content = readFileSync(editorconfigPath, 'utf-8');

    expect(content, 'debe definir charset = utf-8').toMatch(/charset\s*=\s*utf-8/i);
    expect(content, 'debe definir end_of_line = lf').toMatch(/end_of_line\s*=\s*lf/i);
    expect(content, 'debe definir indent_size = 2 para *.ts / *.json / *.yml').toMatch(
      /indent_size\s*=\s*2/,
    );
    expect(content, 'debe definir indent_size = 4 para *.cs').toMatch(/indent_size\s*=\s*4/);
  });
});
