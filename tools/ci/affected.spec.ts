import { describe, expect, it } from 'vitest';
import { affectedJobs, type Job } from './affected.js';

const sorted = (jobs: Job[]) => [...jobs].sort();

// CA1 — only the affected component runs
describe('CA1 — single-component changes trigger only that job', () => {
  it.each<[string[], Job[]]>([
    [['apps/spa/src/app/app.ts'], ['spa']],
    [['apps/bff/src/health.ts'], ['bff']],
    [['apps/extension/src/popup.ts'], ['extension']],
    [['services/core/src/Host/Program.cs'], ['core']],
    [['tools/repo-checks/structure.spec.ts'], ['repo-checks']],
    [['tools/ci/affected.ts'], ['repo-checks']],
  ])('changed %j → jobs %j', (files, expected) => {
    expect(sorted(affectedJobs(files))).toEqual(sorted(expected));
  });
});

// CA2 — contract changes propagate to consumers
describe('CA2 — contract changes trigger contracts + core + bff', () => {
  it('contracts/internal/core.v1.yaml triggers contracts, core and bff', () => {
    expect(sorted(affectedJobs(['contracts/internal/core.v1.yaml']))).toEqual(
      sorted(['contracts', 'core', 'bff']),
    );
  });
});

// CA3 — transversal files trigger all jobs
describe('CA3 — transversal file changes trigger all jobs', () => {
  const ALL = sorted(['core', 'bff', 'spa', 'extension', 'contracts', 'repo-checks']);

  it.each([
    ['Directory.Packages.props'],
    ['Directory.Build.props'],
    ['pnpm-lock.yaml'],
    ['.github/workflows/ci.yml'],
    ['global.json'],
  ])('%s triggers all jobs', (file) => {
    expect(sorted(affectedJobs([file]))).toEqual(ALL);
  });
});

// Multiple independent changes are unioned
describe('union of multiple changed paths', () => {
  it('spa + bff changes → spa and bff jobs', () => {
    expect(sorted(affectedJobs(['apps/spa/src/a.ts', 'apps/bff/src/b.ts']))).toEqual(
      sorted(['spa', 'bff']),
    );
  });

  it('spa + contracts changes → spa, contracts, core, bff', () => {
    expect(sorted(affectedJobs(['apps/spa/src/a.ts', 'contracts/public/bff.v1.yaml']))).toEqual(
      sorted(['spa', 'contracts', 'core', 'bff']),
    );
  });
});

// Empty input → no jobs
describe('edge cases', () => {
  it('no changed files → no jobs', () => {
    expect(affectedJobs([])).toEqual([]);
  });

  it('unknown path → no jobs', () => {
    expect(affectedJobs(['infra/docker-compose.yml'])).toEqual([]);
  });
});
