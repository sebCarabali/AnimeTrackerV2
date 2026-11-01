import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawnSync } from 'node:child_process';
import { describe, expect, it } from 'vitest';
import YAML from 'yaml';

const root = fileURLToPath(new URL('../..', import.meta.url));

describe('HU-00.07 · CA1 — Dos especificaciones versionadas', () => {
  const publicPath = join(root, 'contracts/public/bff.v1.yaml');
  const internalPath = join(root, 'contracts/internal/core.v1.yaml');

  it('existe contracts/public/bff.v1.yaml', () => {
    expect(existsSync(publicPath), 'bff.v1.yaml debe existir').toBe(true);
  });

  it('existe contracts/internal/core.v1.yaml', () => {
    expect(existsSync(internalPath), 'core.v1.yaml debe existir').toBe(true);
  });

  it.each([
    ['public/bff.v1.yaml', publicPath],
    ['internal/core.v1.yaml', internalPath],
  ])('%s declara openapi 3.1.x y versionado semántico', (_, filePath) => {
    if (!existsSync(filePath)) {
      throw new Error(`Archivo no encontrado: ${filePath}`);
    }
    const doc = YAML.parse(readFileSync(filePath, 'utf-8'));
    expect(doc.openapi).toMatch(/^3\.1\.\d+$/);
    expect(doc.info?.version).toMatch(/^\d+\.\d+\.\d+$/);
    expect(doc.paths?.['/health']).toBeDefined();
    expect(doc.components?.schemas?.Problem).toBeDefined();
  });
});

describe('HU-00.07 · CA2 — Lint de estilo con Spectral', () => {
  const spectralConfig = join(root, '.spectral.yaml');
  const fixtureInvalid = join(root, 'contracts/__fixtures__/invalid.yaml');

  it('.spectral.yaml existe en la raíz', () => {
    expect(existsSync(spectralConfig), '.spectral.yaml debe existir').toBe(true);
  });

  it('Spectral detecta errores en fixture inválido (rojo primero)', () => {
    expect(existsSync(fixtureInvalid), 'fixture inválido debe existir').toBe(true);

    const spectralBin = join(root, 'node_modules/.bin/spectral' + (process.platform === 'win32' ? '.cmd' : ''));
    const result = spawnSync(
      spectralBin,
      ['lint', fixtureInvalid, '--ruleset', spectralConfig],
      {
        cwd: root,
        encoding: 'utf-8',
        shell: true,
      },
    );

    expect(result.status, 'Spectral debe fallar ante especificación inválida con exit code != 0').not.toBe(0);
    const output = (result.stdout || '') + (result.stderr || '');
    expect(output).toMatch(/operationId|path|casing|Problem/i);
  });

  it.each([
    ['contracts/public/bff.v1.yaml', 'contracts/public/bff.v1.yaml'],
    ['contracts/internal/core.v1.yaml', 'contracts/internal/core.v1.yaml'],
  ])('Spectral valida con éxito %s', (_, relativePath) => {
    const specPath = join(root, relativePath);
    expect(existsSync(specPath), `${relativePath} debe existir`).toBe(true);

    const spectralBin = join(root, 'node_modules/.bin/spectral' + (process.platform === 'win32' ? '.cmd' : ''));
    const result = spawnSync(
      spectralBin,
      ['lint', specPath, '--ruleset', spectralConfig],
      {
        cwd: root,
        encoding: 'utf-8',
        shell: true,
      },
    );

    expect(result.status, `Spectral reportó errores:\n${result.stdout}\n${result.stderr}`).toBe(0);
  });
});


describe('HU-00.07 · CA6 — Documentación navegable', () => {
  it('pnpm contracts:docs genera documentación HTML para ambos contratos', () => {
    const bffDoc = join(root, 'dist/docs/bff.html');
    const coreDoc = join(root, 'dist/docs/core.html');

    const result = spawnSync('pnpm', ['contracts:docs'], {
      cwd: root,
      encoding: 'utf-8',
      shell: true,
    });

    expect(result.status, 'pnpm contracts:docs debe finalizar con exit 0').toBe(0);
    expect(existsSync(bffDoc), 'dist/docs/bff.html debe existir').toBe(true);
    expect(existsSync(coreDoc), 'dist/docs/core.html debe existir').toBe(true);
  });
});
