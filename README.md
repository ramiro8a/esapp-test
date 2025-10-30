# Payment & Risk Evaluation Microservices

Sistema de procesamiento de pagos con evaluación automática de riesgos en tiempo real basado en arquitectura de microservicios con comunicación asincrónica mediante Kafka.

---

## 🚀 ⚡ QUICK START - DOCKER COMPOSE

### Ejecuta esto y listo:

```bash
docker compose -p esapp-test up -d
```

**¡Eso es todo!** Los servicios estarán disponibles en:
- 🌐 PaymentService: http://localhost:8080/swagger
- 🔍 RiskEvaluationService: Sólo por Kafka
- 📊 Kafdrop (Kafka): http://localhost:9000
- 🗄️ PostgreSQL: localhost:5433(username:postgres, password:postgres)


---

## 📋 Tabla de Contenidos

- [Descripción General](#descripción-general)
- [Arquitectura](#arquitectura)
- [Stack Tecnológico](#stack-tecnológico)
- [Instalación y Ejecución](#instalación-y-ejecución)
- [Endpoints](#endpoints)
- [Flujos de Datos](#flujos-de-datos)
- [Base de Datos](#base-de-datos)

---

## 📖 Descripción General

Sistema de procesamiento de pagos con evaluación automática de fraudes en tiempo real.

**Características:**
- ✅ Creación y consulta de pagos
- ✅ Evaluación de riesgos en tiempo real
- ✅ Comunicación asincrónica mediante Kafka
- ✅ Persistencia en PostgreSQL
- ✅ Documentación automática con Swagger/OpenAPI
- ✅ CORS habilitado
- ✅ Docker Compose ready

---

## 🏗️ Arquitectura

```
CLIENTE
  │
  ├─→ POST /api/v1/payments ────────→ PaymentService (Puerto 5000/8080)
  │                                         │
  │                                    ┌────┴─────────┐
  │                                    │               │
  │                            Kafka Topic      PostgreSQL
  │                   (risk-evaluation-request)
  │                            │
  │                            ▼
  │                    RiskEvaluationService (5001)
  │                            │
  │                       Kafka Topic
  │                  (risk-evaluation-response)
  │                            │
  │                            ▼
  │                    PaymentService (Consume)
  │                            │
  │                       Update BD
  │
  └─→ GET /api/v1/payments/{id} ────→ Retorna estado final
```

---

## ⚙️ Configuración

### Variables de Entorno

#### 🐳 Docker (Producción)

Archivos: `appsettings.json`

**PaymentService** (`PaymentService/src/API/appsettings.json`):
```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=postgres;Port=5432;Database=payment_service;Username=postgres;Password=postgres"
  },
  "Kafka": {
    "BootstrapServers": "kafka:29092"
  }
}
```

**RiskEvaluationService** (`RiskEvaluationService/src/API/appsettings.json`):
```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=postgres;Port=5432;Database=payment_service;Username=postgres;Password=postgres"
  },
  "Kafka": {
    "BootstrapServers": "kafka:29092"
  }
}
```

#### 💻 Local (Desarrollo)

Archivos: `appsettings.Development.json`

**PaymentService** (`PaymentService/src/API/appsettings.Development.json`):
```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5433;Database=payment_service;Username=postgres;Password=postgres"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  }
}
```

**RiskEvaluationService** (`RiskEvaluationService/src/API/appsettings.Development.json`):
```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5433;Database=payment_service;Username=postgres;Password=postgres"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  }
}
```

### Credenciales por Default

| Servicio | Usuario | Contraseña | Puerto |
|----------|---------|------------|--------|
| PostgreSQL | postgres | postgres | 5433 (local) / 5432 (docker) |
| Kafka | N/A | N/A | 9092 (local) / 29092 (docker) |

### Variables Clave

- `ConnectionStrings:PostgreSQL` - Conexión a base de datos
- `Kafka:BootstrapServers` - Servidor Kafka para mensajería
- `Logging:LogLevel:Default` - Nivel de logging

---

## 💻 Stack Tecnológico

| Componente | Versión |
|-----------|----------|
| .NET | 8.0 |
| PostgreSQL | 16 |
| Kafka | 7.5.0 |
| Docker | Latest |

---

## 🚀 Instalación y Ejecución

### ⭐ Opción 1: Docker Compose (Recomendado)

```bash
cd D:\esapp\test
docker-compose up
```

**Servicios disponibles:**
- PaymentService Swagger: http://localhost:8080/swagger
- RiskEvaluationService: solo por kafka
- Kafdrop (Kafka UI): http://localhost:9000
- PostgreSQL: localhost:5433

**Comandos útiles:**
```bash
docker-compose down              # Detener
docker-compose down -v           # Detener y limpiar datos
docker-compose logs -f           # Ver logs
docker-compose up --build        # Reconstruir
```

---

### Opción 2: Ejecución Local

#### 1. Restaurar dependencias

```bash
cd PaymentService
dotnet restore

cd ..\RiskEvaluationService
dotnet restore
```

#### 2. Configurar PostgreSQL

```sql
CREATE DATABASE payment_service;
```

#### 3. Ejecutar PaymentService

```bash
cd PaymentService\src\API
dotnet run --environment Development
```

URL: http://localhost:5000/swagger

#### 4. Ejecutar RiskEvaluationService (otra terminal)

```bash
cd RiskEvaluationService\src\API
dotnet run --environment Development
```

URL: http://localhost:5001/swagger

---

## 🔌 Endpoints

### PaymentService

#### POST `/api/v1/payments` - Crear Pago

```json
{
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "serviceProviderId": "6ba7b810-9dad-11d1-80b4-00c04fd430c8",
  "paymentMethodId": 1,
  "amount": 1500.50
}
```

**Response (201):**
```json
{
  "externalOperationId": "d2a6dd74-6ac4-41b3-bab2-e15ab4d380d2",
  "createdAt": "2025-10-30T14:20:06Z",
  "status": "evaluating",
  "amount": 1500.50
}
```

---

#### GET `/api/v1/payments/{externalOperationId}` - Consultar Pago

**Response (200):**
```json
{
  "externalOperationId": "d2a6dd74-6ac4-41b3-bab2-e15ab4d380d2",
  "createdAt": "2025-10-30T14:20:06Z",
  "status": "accepted",
  "amount": 1500.50
}
```

**Estados:**
- `evaluating` - En evaluación
- `accepted` - Aprobado
- `denied` - Rechazado

---

### RiskEvaluationService

#### POST `/api/RiskEvaluation/evaluate` - Evaluar Riesgo

```json
{
  "externalOperationId": "d2a6dd74-6ac4-41b3-bab2-e15ab4d380d2",
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "amount": 1500.50
}
```

**Response (200):**
```json
{
  "externalOperationId": "d2a6dd74-6ac4-41b3-bab2-e15ab4d380d2",
  "status": "accepted"
}
```

---

## 📊 Flujos de Datos

### Reglas de Evaluación

- **Límite por transacción:** Máximo 2,000 Bs
- **Límite diario:** Máximo 5,000 Bs por cliente

**Lógica:**
```
if (monto > 2000) → DENIED
else if (acumulado_diario + monto > 5000) → DENIED
else → ACCEPTED
```

---

## 🧪 Testing

### Crear Pago

```bash
curl -X POST http://localhost:8080/api/v1/payments \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440000",
    "serviceProviderId": "6ba7b810-9dad-11d1-80b4-00c04fd430c8",
    "paymentMethodId": 1,
    "amount": 1500.50
  }'
```

### Consultar Pago

```bash
curl http://localhost:8080/api/v1/payments/d2a6dd74-6ac4-41b3-bab2-e15ab4d380d2
```

**Nota:** Esperar 2-3 segundos antes de consultar para obtener estado final (la evaluación es asincrónica)

---

## 🗄️ Base de Datos

### Tabla `payments`

```sql
CREATE TABLE payments (
    id UUID PRIMARY KEY,
    external_operation_id UUID UNIQUE NOT NULL,
    customer_id UUID NOT NULL,
    service_provider_id UUID NOT NULL,
    payment_method_id INT NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    status VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

CREATE INDEX idx_external_operation_id ON payments(external_operation_id);
CREATE INDEX idx_customer_id ON payments(customer_id);
CREATE INDEX idx_created_at ON payments(created_at);
```

Las migraciones se aplican automáticamente al iniciar.

---

