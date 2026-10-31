export type Job = 'core' | 'bff' | 'spa' | 'extension' | 'contracts' | 'repo-checks';

const ALL_JOBS: Job[] = ['core', 'bff', 'spa', 'extension', 'contracts', 'repo-checks'];

/** Patterns that, when matched, trigger ALL jobs. */
const GLOBAL_PATTERNS = [
  /^Directory\.(Build|Packages)\.props$/,
  /^pnpm-lock\.yaml$/,
  /^\.github\/workflows\//,
  /^global\.json$/,
];

/** Per-path prefix → jobs. Evaluated in order; first match wins unless the
 *  path already triggered a global rebuild. */
const PATH_MAP: Array<{ prefix: RegExp; jobs: Job[] }> = [
  { prefix: /^services\/core\//, jobs: ['core'] },
  { prefix: /^apps\/bff\//, jobs: ['bff'] },
  { prefix: /^apps\/spa\//, jobs: ['spa'] },
  { prefix: /^apps\/extension\//, jobs: ['extension'] },
  { prefix: /^contracts\//, jobs: ['contracts', 'core', 'bff'] },
  { prefix: /^tools\/repo-checks\//, jobs: ['repo-checks'] },
  { prefix: /^tools\/ci\//, jobs: ['repo-checks'] },
];

/**
 * Given a list of changed file paths (relative to repo root), returns the
 * deduplicated set of CI job names that must run.
 */
export function affectedJobs(changedFiles: string[]): Job[] {
  const jobs = new Set<Job>();

  for (const file of changedFiles) {
    if (GLOBAL_PATTERNS.some((p) => p.test(file))) {
      return [...ALL_JOBS];
    }
    for (const { prefix, jobs: relatedJobs } of PATH_MAP) {
      if (prefix.test(file)) {
        relatedJobs.forEach((job) => jobs.add(job));
        break;
      }
    }
  }

  return [...jobs];
}

/**
 * Wraps a job list into the GitHub Actions matrix JSON string.
 * Falls back to ['repo-checks'] when no jobs are affected so the
 * pipeline always has at least one job to run.
 */
export function computeMatrix(jobs: Job[]): string {
  return JSON.stringify({ job: jobs.length ? jobs : ['repo-checks'] });
}

// ─── CA5 verification: intentionally uncovered multi-line code ───────────────
// These functions are NOT tested. Their purpose is to verify that the CI
// coverage threshold (80 % lines) fails when new code lacks test coverage.
// EXISTS ONLY IN verify/ca5-coverage — DELETE BEFORE MERGE.
export function _ca5_block1(x: number): string {
  if (x > 0) {
    return 'positive';
  }
  if (x < 0) {
    return 'negative';
  }
  return 'zero';
}
export function _ca5_block2(x: number, y: number): number {
  if (x === y) {
    return 0;
  }
  if (x > y) {
    return 1;
  }
  return -1;
}
export function _ca5_block3(items: string[]): string {
  const result: string[] = [];
  for (const item of items) {
    result.push(item.toUpperCase());
  }
  return result.join(',');
}
export function _ca5_block4(n: number): number[] {
  const arr: number[] = [];
  for (let i = 0; i < n; i++) {
    arr.push(i * i);
  }
  return arr;
}
export function _ca5_block5(map: Record<string, number>): number {
  let total = 0;
  for (const key of Object.keys(map)) {
    total += map[key];
  }
  return total;
}
// ─────────────────────────────────────────────────────────────────────────────
