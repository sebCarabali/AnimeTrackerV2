// @nestjs/config's ConfigModule.forRoot({ validate }) runs validate() eagerly
// when the module is imported (inside the AppModule class decorator), before
// any beforeAll hook runs. These must be set before test files are loaded.
process.env.CORE_BASE_URL ??= 'http://core-under-test.local';
process.env.PORT ??= '3000';
process.env.NODE_ENV ??= 'test';
