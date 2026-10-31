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
