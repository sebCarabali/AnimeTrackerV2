import type { INestApplication } from '@nestjs/common';

/** CA5: business routes live under /api/v1; health endpoints stay unprefixed. */
export function configureApp(app: INestApplication): void {
  app.setGlobalPrefix('api/v1', { exclude: ['health', 'health/ready'] });
}
