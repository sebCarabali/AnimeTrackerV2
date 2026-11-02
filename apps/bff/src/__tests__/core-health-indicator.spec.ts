import { Test } from '@nestjs/testing';
import { HealthIndicatorService } from '@nestjs/terminus';
import { CORE_CLIENT } from '../core/core-client';
import { CoreHealthIndicator } from '../health/core-health.indicator';

describe('CoreHealthIndicator', () => {
  async function buildIndicator(coreClient: { GET: jest.Mock }) {
    const moduleRef = await Test.createTestingModule({
      providers: [
        CoreHealthIndicator,
        HealthIndicatorService,
        { provide: CORE_CLIENT, useValue: coreClient },
      ],
    }).compile();

    return moduleRef.get(CoreHealthIndicator);
  }

  it('CA3: usa el cliente generado y reporta "up" cuando el núcleo responde sin error', async () => {
    const coreClient = {
      GET: jest.fn().mockResolvedValue({ data: { status: 'Healthy' }, error: undefined }),
    };
    const indicator = await buildIndicator(coreClient);

    const result = await indicator.check();

    expect(coreClient.GET).toHaveBeenCalledWith('/health');
    expect(result.core.status).toBe('up');
  });

  it('reporta "down" cuando el cliente generado devuelve un error', async () => {
    const coreClient = {
      GET: jest.fn().mockResolvedValue({ data: undefined, error: { title: 'down' } }),
    };
    const indicator = await buildIndicator(coreClient);

    const result = await indicator.check();

    expect(result.core.status).toBe('down');
  });

  it('reporta "down" cuando el cliente generado lanza (red caída)', async () => {
    const coreClient = { GET: jest.fn().mockRejectedValue(new Error('ECONNREFUSED')) };
    const indicator = await buildIndicator(coreClient);

    const result = await indicator.check();

    expect(result.core.status).toBe('down');
  });

  it('reporta "down" cuando el cliente generado lanza un valor que no es Error', async () => {
    const coreClient = { GET: jest.fn().mockRejectedValue('timeout') };
    const indicator = await buildIndicator(coreClient);

    const result = await indicator.check();

    expect(result.core.status).toBe('down');
  });
});
