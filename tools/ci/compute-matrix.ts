import { appendFileSync } from 'node:fs';
import { affectedJobs, computeMatrix } from './affected.ts';

const files = process.env.CHANGED_FILES?.split(' ').filter(Boolean) ?? [];
const jobs = affectedJobs(files);
const matrix = computeMatrix(jobs);

if (process.env.GITHUB_OUTPUT) {
  appendFileSync(process.env.GITHUB_OUTPUT, `matrix=${matrix}\n`);
}
console.log('Affected jobs:', matrix);
