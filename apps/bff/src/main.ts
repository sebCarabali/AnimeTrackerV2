import 'reflect-metadata';
import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';
import { configureApp } from './bootstrap';

async function bootstrap(): Promise<void> {
  const app = await NestFactory.create(AppModule);
  configureApp(app);

  const port = process.env.PORT ?? 3000;
  await app.listen(port);
}

bootstrap().catch((error: unknown) => {
  // CA4: fail fast with a non-zero exit code naming the missing variable.
  const message = error instanceof Error ? error.message : String(error);
  console.error(`BFF failed to start: ${message}`);
  process.exit(1);
});
