/**
 * Valida que las variables de entorno requeridas existan.
 * Lanza error si alguna falta.
 *
 * Uso: validateEnv(['CORE_BASE_URL', 'PORT', 'NODE_ENV'])
 * Reutilizable por HU-00.03 en NestJS ConfigModule sin reescritura.
 *
 * @param requiredVars - Lista de nombres de variables de entorno requeridas
 * @throws Error si alguna variable falta
 */
export function validateEnv(requiredVars: string[]): void {
  const missing: string[] = [];

  requiredVars.forEach((varName) => {
    if (!process.env[varName]) {
      missing.push(varName);
    }
  });

  if (missing.length > 0) {
    throw new Error(
      `Missing required environment variables: ${missing.join(', ')}`
    );
  }
}
