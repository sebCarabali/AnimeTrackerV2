import { Controller, Get, HttpStatus, Res } from '@nestjs/common';
import type { Response } from 'express';
import { CoreHealthIndicator } from './core-health.indicator';
import type { HealthStatus, Problem } from './health-status.types';

@Controller()
export class HealthController {
  constructor(private readonly coreHealthIndicator: CoreHealthIndicator) {}

  /** Liveness: always 200 — reports dependency status without failing the process. */
  @Get('health')
  async liveness(): Promise<HealthStatus> {
    const result = await this.coreHealthIndicator.check();
    return this.toHealthStatus(result.core.status);
  }

  /** Readiness: 503 when a downstream dependency is down. */
  @Get('health/ready')
  async readiness(@Res() res: Response): Promise<void> {
    const result = await this.coreHealthIndicator.check();

    if (result.core.status !== 'up') {
      // Nest's own response handling resets Content-Type to application/json for any
      // object body paired with a >=400 status (see ExpressAdapter.reply), so the problem+json
      // header must be set and the response sent manually — passthrough would be overwritten.
      res
        .status(HttpStatus.SERVICE_UNAVAILABLE)
        .setHeader('Content-Type', 'application/problem+json')
        .json(this.toProblem(result.core.status));
      return;
    }

    res.status(HttpStatus.OK).json(this.toHealthStatus(result.core.status));
  }

  private toHealthStatus(coreStatus: string): HealthStatus {
    return {
      status: 'ok',
      timestamp: new Date().toISOString(),
      dependencies: { core: coreStatus },
    };
  }

  private toProblem(coreStatus: string): Problem {
    return {
      type: 'https://tools.ietf.org/html/rfc9110#section-15.6.4',
      title: 'Service Unavailable',
      status: HttpStatus.SERVICE_UNAVAILABLE,
      detail: `Downstream core service is ${coreStatus}.`,
      instance: '/health/ready',
    };
  }
}
