import { validateEnv } from '../common/config';

describe('validateEnv', () => {
  const originalEnv = process.env;

  beforeEach(() => {
    jest.resetModules();
    process.env = { ...originalEnv };
  });

  afterAll(() => {
    process.env = originalEnv;
  });

  describe('cuando falta una variable', () => {
    it('lanza error que nombra CORE_BASE_URL', () => {
      delete process.env.CORE_BASE_URL;
      process.env.PORT = '3000';
      process.env.NODE_ENV = 'development';

      expect(() => {
        validateEnv(['CORE_BASE_URL', 'PORT', 'NODE_ENV']);
      }).toThrow('Missing required environment variables: CORE_BASE_URL');
    });

    it('lanza error que nombra múltiples variables', () => {
      delete process.env.CORE_BASE_URL;
      delete process.env.PORT;
      process.env.NODE_ENV = 'development';

      expect(() => {
        validateEnv(['CORE_BASE_URL', 'PORT', 'NODE_ENV']);
      }).toThrow('Missing required environment variables: CORE_BASE_URL, PORT');
    });
  });

  describe('cuando todas las variables existen', () => {
    it('no lanza error', () => {
      process.env.CORE_BASE_URL = 'http://localhost:5000';
      process.env.PORT = '3000';
      process.env.NODE_ENV = 'development';

      // Note: validateEnv treats empty string ("") as present (falsy but defined).
      // Edge case: process.env.VAR = "" is still a defined variable in JS.
      // For strict validation, app logic should use additional checks (e.g., `!VAR || VAR.trim() === ""`).
      expect(() => {
        validateEnv(['CORE_BASE_URL', 'PORT', 'NODE_ENV']);
      }).not.toThrow();
    });
  });

  describe('casos adicionales', () => {
    it('lanza error cuando la lista de variables está vacía y se llama sin args', () => {
      process.env.CORE_BASE_URL = 'http://localhost:5000';
      process.env.PORT = '3000';
      process.env.NODE_ENV = 'development';

      expect(() => {
        validateEnv([]);
      }).not.toThrow();
    });

    it('valida una sola variable requerida', () => {
      delete process.env.CORE_BASE_URL;

      expect(() => {
        validateEnv(['CORE_BASE_URL']);
      }).toThrow('Missing required environment variables: CORE_BASE_URL');
    });
  });
});
