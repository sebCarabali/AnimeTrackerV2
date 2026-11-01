import jestOpenAPI from 'jest-openapi';
import path from 'node:path';

jestOpenAPI(path.resolve(__dirname, '../../contracts/public/bff.v1.yaml'));
