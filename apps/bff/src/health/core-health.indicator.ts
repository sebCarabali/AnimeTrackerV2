import { Inject, Injectable, Logger } from '@nestjs/common';
import { HealthIndicatorService } from '@nestjs/terminus';
import type { HealthIndicatorResult } from '@nestjs/terminus';
import { CORE_CLIENT, type CoreClient } from '../core/core-client';

@Injectable()
export class CoreHealthIndicator {
  private readonly logger = new Logger(CoreHealthIndicator.name);

  constructor(
    private readonly healthIndicatorService: HealthIndicatorService,
    @Inject(CORE_CLIENT) private readonly coreClient: CoreClient,
  ) {}

  async check(): Promise<HealthIndicatorResult<'core'>> {
    const indicator = this.healthIndicatorService.check('core');

    try {
      const { data, error } = await this.coreClient.GET('/health');
      if (error || !data) {
        this.logger.warn(`Core health responded without data: ${JSON.stringify(error)}`);
        return indicator.down();
      }
      return indicator.up();
    } catch (err) {
      const error = err instanceof Error ? err : new Error(String(err));
      this.logger.error(`Core health check failed: ${error.message}`, error.stack);
      return indicator.down();
    }
  }
}
