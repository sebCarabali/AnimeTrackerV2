import nodeFetch from 'node-fetch';

/**
 * openapi-fetch builds requests with the global (undici) `Request`/`Response`.
 * nock only patches Node's `http`/`https` modules, which undici's native
 * `fetch` bypasses — so requests made with the global fetch are invisible to
 * nock in tests. This adapter re-dispatches through `node-fetch` (which does
 * use `http`/`https`) so the same client code is interceptable in tests and
 * behaves identically in production.
 */
export async function nodeFetchAdapter(input: Request): Promise<Response> {
  const hasBody = input.method !== 'GET' && input.method !== 'HEAD';

  const nodeResponse = await nodeFetch(input.url, {
    method: input.method,
    headers: Object.fromEntries(input.headers.entries()),
    body: hasBody ? await input.arrayBuffer() : undefined,
  });

  const responseBody = await nodeResponse.arrayBuffer();

  return new Response(responseBody, {
    status: nodeResponse.status,
    statusText: nodeResponse.statusText,
    headers: Object.fromEntries(nodeResponse.headers.entries()),
  });
}
