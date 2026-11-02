import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { AppController } from './app.controller';
import { AppService } from './app.service';
import { validateEnv } from './common/config';
import { HealthModule } from './health/health.module';

/** CA4: fails fast — throws naming any missing required variable. */
export function validateBffConfig(config: Record<string, unknown>): Record<string, unknown> {
  validateEnv(['CORE_BASE_URL', 'PORT', 'NODE_ENV']);
  return config;
}

@Module({
  imports: [
    ConfigModule.forRoot({
      isGlobal: true,
      validate: validateBffConfig,
    }),
    HealthModule,
  ],
  controllers: [AppController],
  providers: [AppService],
})
export class AppModule {}
