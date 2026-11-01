# Gestión de Secretos — AnimeTracker V2

## Local (Desarrollo)

- **Configuración:** `.env` (no versionado, en `.gitignore`)
- **Herramientas:** 
  - Para .NET: `dotnet user-secrets` (almacena en %APPDATA%\microsoft\UserSecrets)
  - Para BFF: `process.env` (lee `.env`)
- **Ejemplo:**
  ```bash
  # .NET local
  dotnet user-secrets init -p services/core/src/Host/AnimeTracker.Host
  dotnet user-secrets set "ConnectionStrings:Postgres" "postgres://user:pass@localhost/animetracker"

  # BFF local
  echo "CORE_BASE_URL=http://localhost:5000" >> .env
  echo "PORT=3000" >> .env
  echo "NODE_ENV=development" >> .env
  ```

## CI/CD (GitHub Actions)

- **Almacenamiento:** GitHub Secrets en github.com/sebCarabali/AnimeTrackerV2/settings/secrets
- **Niveles:** 
  - Repository secrets (accesibles en todos los workflows)
  - Environment secrets (accesibles solo en workflow del environment específico)
- **Inyección en CI:**
  ```yaml
  # .github/workflows/ci.yml
  env:
    POSTGRES_CONNECTION: ${{ secrets.DB_CONNECTION_CI }}
  ```
- **Validación:** Job `secrets` (gitleaks) escanea PRs para credenciales detectadas

## Staging (HashiCorp Vault Docker)

- **Gestor:** HashiCorp Vault (contenedor Docker)
- **Acceso:** Interno, NO expuesto públicamente
- **Autenticación:** AppRole token (almacenado en GitHub Secrets)
- **Flujo CD:**
  1. CD script autentica en Vault con AppRole token
  2. Extrae secretos (DB connection, API keys, etc.)
  3. Inyecta en variables de entorno del contenedor
- **Configuración Vault (ejemplo):**
  ```bash
  vault kv put secret/animetracker/staging \
    db_connection="postgres://..." \
    core_api_key="..."
  ```

## Producción

- **Fuera de scope MVP (sprint-1)**
- **Planeado para:** Q1 2026 (después de MVP)
- **Opciones evaluadas:**
  - AWS Secrets Manager
  - HashiCorp Vault (persistent)
  - Cloud-provider native (Azure Key Vault, Google Secret Manager)

---

## Pre-commit + CI — Defensa en Profundidad

### Local (Pre-commit)

```bash
# Hook: .husky/pre-commit
# Ejecuta: gitleaks detect
# Resultado: Rechaza commits con credenciales
```

### CI (Gitleaks Job)

```bash
# Job: .github/workflows/ci.yml → job: secrets
# Ejecuta: gitleaks/gitleaks-action@v2
# Config: .gitleaks.toml (ruleslist estándar + allowlist)
# Resultado: Bloquea PRs con credenciales
```

---

## Checklist de Seguridad

- [ ] `.env` NO está versionado (verificar `.gitignore`)
- [ ] `.env.example` documenta estructura (NO valores sensibles)
- [ ] gitleaks hook + job están activos
- [ ] Staging Vault está configurado (CD puede acceder)
- [ ] GitHub Secrets están configurados (para CD Vault auth)
- [ ] Team sabe dónde encontrar este doc (`docs/SECRETS.md`)

---

## Referencias

- **Twelve-Factor App:** Configuration should come from environment
- **gitleaks:** Previene credenciales en repo (CI + local)
- **HashiCorp Vault:** Gestor de secretos empresarial para staging
