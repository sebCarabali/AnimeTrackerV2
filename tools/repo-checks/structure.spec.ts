import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawnSync } from 'node:child_process';
import { describe, expect, it } from 'vitest';

const root = fileURLToPath(new URL('../..', import.meta.url));

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
  const readContent = () => readFileSync(join(root, 'README.md'), 'utf-8');

  it('README.md existe en la raíz', () => {
    expect(existsSync(join(root, 'README.md'))).toBe(true);
  });

  it('menciona Node LTS como requisito previo', () => {
    expect(readContent()).toMatch(/Node/i);
  });

  it('menciona .NET 10 como requisito previo', () => {
    expect(readContent()).toMatch(/\.NET\s*10/i);
  });

  it('menciona Docker como requisito previo', () => {
    expect(readContent()).toMatch(/Docker/i);
  });

  it('describe cómo arrancar el entorno', () => {
    expect(readContent()).toMatch(/arranc|setup|install/i);
  });

  it('menciona Conventional Commits', () => {
    expect(readContent()).toMatch(/Conventional Commits/i);
  });

  it('describe convención de ramas', () => {
    expect(readContent()).toMatch(/rama|branch/i);
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

// CA3b — .gitignore
describe('CA3b — .gitignore excluye artefactos de build y secretos', () => {
  it('gitignore_excluye_artefactos_de_build_y_secretos', () => {
    const gitignorePath = join(root, '.gitignore');
    expect(existsSync(gitignorePath), '.gitignore debe existir en la raíz').toBe(true);

    const content = readFileSync(gitignorePath, 'utf-8');

    for (const pattern of ['bin/', 'obj/', 'node_modules/', 'dist/', '.env']) {
      expect(content, `.gitignore debe excluir ${pattern}`).toContain(pattern);
    }

    const hasCoverageExclusion =
      content.includes('coverage/') ||
      content.includes('coverage-reports/') ||
      content.includes('lcov') ||
      content.includes('.nyc_output') ||
      content.includes('TestResults/');
    expect(hasCoverageExclusion, '.gitignore debe excluir artefactos de cobertura').toBe(true);
  });
});

// CA4 — commitlint valida el formato de los mensajes de commit
describe('CA4 — commitlint valida el formato de los mensajes de commit', () => {
  it('commitlint_rechaza_mensaje_sin_tipo', () => {
    const result = spawnSync('npx commitlint', {
      cwd: root,
      input: 'cambios varios',
      encoding: 'utf-8',
      shell: true,
    });
    expect(result.status, 'commitlint debe rechazar "cambios varios" con exit 1').toBe(1);
  });

  it('commitlint_acepta_mensaje_convencional', () => {
    const result = spawnSync('npx commitlint', {
      cwd: root,
      input: 'feat(bff): agrega health check',
      encoding: 'utf-8',
      shell: true,
    });
    expect(result.status, 'commitlint debe aceptar el mensaje convencional con exit 0').toBe(0);
  });
});
