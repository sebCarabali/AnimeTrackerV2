import createClient, { type Client } from 'openapi-fetch';
import type { paths } from '../generated/core/schema';
import { nodeFetchAdapter } from './node-fetch-adapter';

export type CoreClient = Client<paths>;

export const CORE_CLIENT = Symbol('CORE_CLIENT');

export function createCoreClient(baseUrl: string): CoreClient {
  return createClient<paths>({ baseUrl, fetch: nodeFetchAdapter });
}
