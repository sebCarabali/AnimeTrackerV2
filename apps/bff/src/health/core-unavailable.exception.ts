import { HttpException, HttpStatus } from '@nestjs/common';
import type { Problem } from './health-status.types';

export class CoreUnavailableException extends HttpException {
  constructor(coreStatus: string) {
    const problem: Problem = {
      type: 'https://tools.ietf.org/html/rfc9110#section-15.6.4',
      title: 'Service Unavailable',
      status: HttpStatus.SERVICE_UNAVAILABLE,
      detail: `Downstream core service is ${coreStatus}.`,
      instance: '/health/ready',
    };
    super(problem, HttpStatus.SERVICE_UNAVAILABLE);
  }
}
