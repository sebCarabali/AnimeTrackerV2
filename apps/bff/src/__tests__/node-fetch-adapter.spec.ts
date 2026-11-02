import nodeFetch from 'node-fetch';
import { nodeFetchAdapter } from '../core/node-fetch-adapter';

jest.mock('node-fetch', () => jest.fn());

const mockedNodeFetch = nodeFetch as unknown as jest.Mock;

function mockNodeFetchResponse(status: number, statusText: string): void {
  mockedNodeFetch.mockResolvedValue({
    status,
    statusText,
    headers: new Map(),
    arrayBuffer: async () => new ArrayBuffer(0),
  });
}

describe('nodeFetchAdapter', () => {
  afterEach(() => {
    mockedNodeFetch.mockReset();
  });

  it('no serializa body en una petición GET', async () => {
    mockNodeFetchResponse(200, 'OK');
    const request = new Request('http://core.local/health', { method: 'GET' });

    await nodeFetchAdapter(request);

    expect(mockedNodeFetch).toHaveBeenCalledWith(
      'http://core.local/health',
      expect.objectContaining({ method: 'GET', body: undefined }),
    );
  });

  it('serializa el body en una petición POST (rama method !== GET/HEAD)', async () => {
    mockNodeFetchResponse(201, 'Created');
    const payload = JSON.stringify({ episode: 1 });
    const request = new Request('http://core.local/track', {
      method: 'POST',
      body: payload,
      headers: { 'Content-Type': 'application/json' },
    });

    await nodeFetchAdapter(request);

    const [, options] = mockedNodeFetch.mock.calls[0] as [string, { method: string; body: ArrayBuffer }];
    expect(options.method).toBe('POST');
    expect(Buffer.from(options.body).toString('utf-8')).toBe(payload);
  });
});
