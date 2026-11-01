import { Inject, Injectable } from '@nestjs/common';
import { HealthIndicatorService } from '@nestjs/terminus';
import type { HealthIndicatorResult } from '@nestjs/terminus';
import { CORE_CLIENT, type CoreClient } from '../core/core-client';

@Injectable()
export class CoreHealthIndicator {
  constructor(
    private readonly healthIndicatorService: HealthIndicatorService,
    @Inject(CORE_CLIENT) private readonly coreClient: CoreClient,
  ) {}

  async check(): Promise<HealthIndicatorResult<'core'>> {
    const indicator = this.healthIndicatorService.check('core');

    try {
      const { data, error } = await this.coreClient.GET('/health');
      if (error || !data) {
        return indicator.down();
      }
      return indicator.up();
    } catch {
      return indicator.down();
    }
  }
}
