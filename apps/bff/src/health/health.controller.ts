import { Controller, Get, UseFilters } from '@nestjs/common';
import { CoreHealthIndicator } from './core-health.indicator';
import { CoreUnavailableException } from './core-unavailable.exception';
import type { HealthStatus } from './health-status.types';
import { ProblemJsonExceptionFilter } from './problem-json.filter';

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
  @UseFilters(ProblemJsonExceptionFilter)
  async readiness(): Promise<HealthStatus> {
    const result = await this.coreHealthIndicator.check();

    if (result.core.status !== 'up') {
      throw new CoreUnavailableException(result.core.status);
    }

    return this.toHealthStatus(result.core.status);
  }

  private toHealthStatus(coreStatus: string): HealthStatus {
    return {
      status: 'ok',
      timestamp: new Date().toISOString(),
      dependencies: { core: coreStatus },
    };
  }
}
