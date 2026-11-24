# 🏥 Gestión de Citas Médicas API

Web API REST desarrollada en **C# / .NET 10** bajo una arquitectura por capas desacopladas (`Controllers`, `Services`, `Models`, `Middlewares`), persistencia en memoria sincronizada (`Singleton`), manejo global de errores bajo el estándar **RFC 7807 (`ProblemDetails`)** y documentación interactiva con **OpenAPI + Scalar**.

---

## 1. Arquitectura y Estructura del Proyecto

```text
GestionCitasAPI/
├── Controllers/
│   ├── CitasController.cs            # CRUD de citas, PATCH de cancelación y simulación HTTP 500
│   ├── EspecialidadesController.cs   # CRUD del catálogo de especialidades médicas
│   ├── MedicosController.cs          # CRUD de médicos y filtro por especialidad
│   ├── PacientesController.cs        # CRUD de pacientes
│   └── ProgramacionesController.cs   # CRUD de turnos médicos y cálculo de bloques de 20 min
├── Middlewares/
│   └── ManejadorExcepcionesMiddleware.cs # Interceptor global RFC 7807 (400, 404, 409, 500)
├── Models/
│   ├── CitaDtos.cs                   # DTOs de creación, reprogramación, cancelación y respuesta
│   ├── EspecialidadDtos.cs           # DTOs de especialidades
│   ├── ExcepcionNegocio.cs           # Excepción tipada para reglas de negocio con código HTTP
│   ├── MedicoDtos.cs                 # DTOs de médicos (con relación a múltiples especialidades)
│   ├── PacienteDtos.cs               # DTOs de pacientes
│   └── ProgramacionDtos.cs           # DTOs de turnos médicos y disponibilidad horaria
├── Services/
│   ├── AlmacenEnMemoria.cs           # Contenedor en memoria thread-safe (lock) compartido
│   ├── CitaServicio.cs               # Lógica de reserva, reprogramación, cancelación y validaciones
│   ├── DatosPrueba.cs                # Semilla inicial de datos cargada al arranque
│   ├── EspecialidadServicio.cs       # Lógica de negocio de especialidades
│   ├── MedicoServicio.cs             # Lógica de negocio de médicos
│   ├── PacienteServicio.cs           # Lógica de negocio de pacientes
│   ├── ProgramacionServicio.cs       # Lógica de turnos y generación de bloques de 20 minutos
│   └── ReglasComunes.cs              # Utilidades de validación, zona horaria (UTC-5) y cruces
├── Properties/
│   └── launchSettings.json           # Configuración de ejecución Kestrel (Puerto 5026)
├── Program.cs                        # Configuración IoC, pipeline HTTP, Scalar y carga de semilla
├── REGLAS_NEGOCIO.md                 # Detalle técnico de las 8 reglas de negocio implementadas
└── GestionCitasAPI.slnx              # Solución del proyecto
```

---

## 2. Requisitos Previos

- **.NET 10 SDK** instalado.
- **Visual Studio 2026** (o VS Code / CLI de .NET).

---

## 3. Ejecución del Proyecto

### Opción A: Desde Terminal (.NET CLI)

Ubicado en la carpeta `GestionCitasAPI`:

```powershell
dotnet restore .\GestionCitasAPI.slnx
dotnet build .\GestionCitasAPI.slnx
dotnet run --project .\GestionCitasAPI.csproj --launch-profile http
```

### Opción B: Desde Visual Studio

1. Abrir la solución **`GestionCitasAPI.slnx`**.
2. Seleccionar el perfil de ejecución **`http`**.
3. Iniciar sin depurar (**`Ctrl + F5`**).

### Acceso a la Documentación Interactiva

Una vez iniciado el servidor en el puerto **5026**:
- **Portal Interactivo Scalar**: [http://localhost:5026/scalar/v1](http://localhost:5026/scalar/v1) *(la ruta raíz `/` redirige automáticamente a Scalar)*.
- **Especificación OpenAPI (JSON)**: [http://localhost:5026/openapi/v1.json](http://localhost:5026/openapi/v1.json)

---

## 4. Endpoints Principales y Semántica HTTP

Todos los módulos (`especialidades`, `pacientes`, `medicos`, `programaciones`, `citas`) implementan las operaciones estándar REST:

| Método | Ruta | Descripción | Códigos de Respuesta |
|---|---|---|---|
| `GET` | `/api/{recurso}` | Lista todos los registros del módulo | `200 OK` |
| `GET` | `/api/{recurso}/{id}` | Obtiene un registro por su ID | `200 OK`, `404 Not Found` |
| `POST` | `/api/{recurso}` | Registra una nueva entidad | `201 Created`, `400 Bad Request`, `409 Conflict` |
| `PUT` | `/api/{recurso}/{id}` | Actualiza una entidad existente y registra `FechaActualizacion` | `200 OK`, `400`, `404`, `409` |
| `DELETE` | `/api/{recurso}/{id}` | Elimina físicamente el registro validando integridad referencial | `204 No Content`, `404`, `409 Conflict` |

### Endpoints Especializados del Dominio

- `GET /api/medicos/por-especialidad/{especialidadId}`: Filtra médicos habilitados en una especialidad específica.
- `GET /api/programaciones?medicoId={id}&especialidadId={id}`: Filtra turnos médicos por profesional y/o especialidad.
- `GET /api/programaciones/{id}/disponibilidad`: Calcula dinámicamente los **bloques de 20 minutos** del turno e indica si cada bloque está libre (`disponible: true/false`).
- `PATCH /api/citas/{id}/cancelacion`: Cancela lógicamente una cita programada registrando el motivo de cancelación y liberando el bloque horario.
- `GET /api/citas/simular-error-500`: Endpoint de prueba para verificar la respuesta estandarizada **HTTP 500** del middleware global.

---

## 5. Reglas de Negocio y Manejo de Excepciones (RFC 7807)

El sistema implementa **8 reglas de negocio esenciales** detalladas en [`REGLAS_NEGOCIO.md`](./REGLAS_NEGOCIO.md):

1. **RN01 — Unicidad**: Validación de duplicados en DNI (Pacientes), CMP (Médicos) y Nombre (Especialidades) → `409 Conflict`.
2. **RN02 — Coherencia Médico-Especialidad**: Un turno solo puede crearse con una especialidad asignada al médico → `409 Conflict`.
3. **RN03 — Validez de Programación**: Turnos futuros, duración mínima de 20 minutos y sin solapamiento de horarios para un mismo médico → `400` / `409`.
4. **RN04 — Estado Activo**: Solo se permiten reservas cuando Paciente, Médico, Especialidad y Programación existen (`404`) y están activos (`409`).
5. **RN05 — Bloques de 20 Minutos**: Toda cita dura exactamente 20 minutos y debe coincidir con un bloque válido dentro del turno → `400 Bad Request`.
6. **RN06 — Prevención de Cruces**: Ningún médico ni paciente puede tener dos citas `"Programada"` simultáneas (sincronizado mediante `lock`) → `409 Conflict`.
7. **RN07 — Reprogramación y Cancelación Controlada**: Solo aplicable a citas `"Programada"` futuras; la cancelación exige motivo y libera el bloque horario → `409 Conflict`.
8. **RN08 — Integridad Referencial**: Impide eliminar o desasociar registros que posean turnos o citas vinculadas → `409 Conflict`.

### Middleware Global (`ManejadorExcepcionesMiddleware`)
Todas las respuestas de error (incluyendo validaciones de `DataAnnotations`, `ExcepcionNegocio`, rutas inexistentes y errores no controlados `500`) retornan bajo el estándar **`application/problem+json` (RFC 7807)** incluyendo `type`, `title`, `status`, `detail`, `instance` y `traceId`, ocultando cualquier *Stack Trace* en fallos internos.

---

## 6. Datos de Prueba Precargados (`Services/DatosPrueba.cs`)

Al iniciar la aplicación, `DatosPrueba.Cargar(app.Services)` inicializa el entorno en memoria utilizando los propios servicios del dominio (con fechas dinámicas calculadas a partir del día siguiente en hora de Lima `UTC-5`):

| Entidad | Cantidad | Rango de IDs | Observaciones |
|---|---|---|---|
| **Especialidades** | 4 | `1` al `4` | `1`: Medicina General, `2`: Pediatría, `3`: Cardiología, `4`: Dermatología |
| **Pacientes** | 12 | `1` al `12` | Paciente `12` precargado como inactivo (`Activo = false`) para pruebas de `RN04` |
| **Médicos** | 6 | `101` al `106` | Médico `101` asociado a múltiples especialidades (`[3, 1]`) |
| **Programaciones** | 10 | `1` al `10` | Turno `5` inactivo; Turno `9` de 70 min (genera 3 bloques exactos de 20 min) |
| **Citas** | 8 | `1001` al `1008` | Cita `1008` precargada en estado `"Cancelada"` |
