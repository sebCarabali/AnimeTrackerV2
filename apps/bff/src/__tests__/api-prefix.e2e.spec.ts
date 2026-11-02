import 'reflect-metadata';
import type { INestApplication } from '@nestjs/common';
import { Test } from '@nestjs/testing';
import request from 'supertest';
import { AppModule } from '../app.module';
import { configureApp } from '../bootstrap';

describe('API prefix (e2e)', () => {
  let app: INestApplication;

  beforeAll(async () => {
    process.env.CORE_BASE_URL = 'http://core-under-test.local';
    process.env.PORT = '3000';
    process.env.NODE_ENV = 'test';

    const moduleRef = await Test.createTestingModule({ imports: [AppModule] }).compile();

    app = moduleRef.createNestApplication();
    configureApp(app);
    await app.init();
  });

  afterAll(async () => {
    await app.close();
  });

  it('CA5: las rutas de negocio responden bajo /api/v1', async () => {
    const res = await request(app.getHttpServer()).get('/api/v1');

    expect(res.status).toBe(200);
  });

  it('CA5: /health queda fuera del prefijo /api/v1', async () => {
    const unprefixed = await request(app.getHttpServer()).get('/health');
    const prefixed = await request(app.getHttpServer()).get('/api/v1/health');

    expect(unprefixed.status).not.toBe(404);
    expect(prefixed.status).toBe(404);
  });

  it('CA5: una ruta de negocio sin el prefijo no se resuelve', async () => {
    const res = await request(app.getHttpServer()).get('/');

    expect(res.status).toBe(404);
  });
});
