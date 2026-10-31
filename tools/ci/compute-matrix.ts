import { appendFileSync } from 'node:fs';
import { affectedJobs } from './affected.ts';

const files = process.env.CHANGED_FILES?.split(' ').filter(Boolean) ?? [];
const jobs = affectedJobs(files);
const matrix = JSON.stringify({ job: jobs.length ? jobs : ['repo-checks'] });

appendFileSync(process.env.GITHUB_OUTPUT!, `matrix=${matrix}\n`);
console.log('Affected jobs:', matrix);
