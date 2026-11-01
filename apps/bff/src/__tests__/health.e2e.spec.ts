import 'reflect-metadata';
import type { INestApplication } from '@nestjs/common';
import { Test } from '@nestjs/testing';
import nock from 'nock';
import request from 'supertest';
import { AppModule } from '../app.module';
import { configureApp } from '../bootstrap';

const CORE_BASE_URL = 'http://core-under-test.local';

describe('Health endpoints (e2e)', () => {
  let app: INestApplication;

  beforeAll(async () => {
    process.env.CORE_BASE_URL = CORE_BASE_URL;
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

  afterEach(() => {
    nock.cleanAll();
  });

  describe('GET /health', () => {
    it('CA1: responde 200 con dependencies.core "up" cuando el núcleo está disponible', async () => {
      nock(CORE_BASE_URL)
        .get('/health')
        .reply(200, { status: 'Healthy', timestamp: new Date().toISOString() });

      const res = await request(app.getHttpServer()).get('/health');

      expect(res.status).toBe(200);
      expect(res.body.status).toBe('ok');
      expect(res.body.dependencies.core).toBe('up');
      expect(res).toSatisfyApiSpec();
    });

    it('CA2: responde 200 con dependencies.core "down" cuando el núcleo no responde', async () => {
      nock(CORE_BASE_URL).get('/health').reply(503);

      const res = await request(app.getHttpServer()).get('/health');

      expect(res.status).toBe(200);
      expect(res.body.dependencies.core).toBe('down');
      expect(res).toSatisfyApiSpec();
    });
  });

  describe('GET /health/ready', () => {
    it('responde 200 cuando el núcleo está disponible', async () => {
      nock(CORE_BASE_URL)
        .get('/health')
        .reply(200, { status: 'Healthy', timestamp: new Date().toISOString() });

      const res = await request(app.getHttpServer()).get('/health/ready');

      expect(res.status).toBe(200);
      expect(res).toSatisfyApiSpec();
    });

    it('CA2: responde 503 cuando el núcleo no responde', async () => {
      nock(CORE_BASE_URL).get('/health').reply(503);

      const res = await request(app.getHttpServer()).get('/health/ready');

      expect(res.status).toBe(503);
      expect(res.body.title).toBeDefined();
      expect(res).toSatisfyApiSpec();
    });
  });
});
