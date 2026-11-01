import { Module } from '@nestjs/common';
import { TerminusModule } from '@nestjs/terminus';
import { CoreClientModule } from '../core/core-client.module';
import { CoreHealthIndicator } from './core-health.indicator';
import { HealthController } from './health.controller';

@Module({
  imports: [TerminusModule, CoreClientModule],
  controllers: [HealthController],
  providers: [CoreHealthIndicator],
})
export class HealthModule {}
