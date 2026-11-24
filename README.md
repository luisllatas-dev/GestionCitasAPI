# 🏥 Gestión de Citas Médicas API

Backend académico en **C# / .NET 10**, adaptado a las **ocho reglas esenciales**. Conserva la estructura del proyecto GestionCitasAPI y los DTOs agrupados en Models. Usa listas/diccionarios en memoria y **Scalar** para las pruebas, sin base de datos, Docker ni microservicios.

## 1. Qué cambió respecto a la versión inicial

El original tenía CRUD de pacientes, médicos y citas, pero aceptaba referencias inexistentes y no tenía agenda de turnos. Esta versión agrega especialidades, programación, disponibilidad, cancelación, validación de cruces y precarga separada.

Se conservan el nombre GestionCitasAPI, los contratos de servicios, los rangos de ID (pacientes 1, médicos 101, citas 1001), fechas de creación/actualización y puerto **5026**.

Cambios necesarios de los cuerpos JSON:

| Antes | Ahora | Por qué |
|---|---|---|
| Médico con `especialidad` como texto | `especialidadIds: [3, 1]` | Referenciar un catálogo y permitir varias especialidades. |
| CMP `CMP-45890` | `45890` | Validación numérica consistente. |
| Cita con `medicoId` y `fechaHora` | `programacionId` y `horaInicio` | Reservar dentro de un turno real, sin contradicciones. |
| PUT de cita podía cambiar paciente | DTO de actualización sin pacienteId | Conserva quién reservó la cita. |
| Datos repartidos en servicios | `Services/DatosPrueba.cs` | Editar toda la simulación en un solo archivo. |

## 2. Estructura y propósito de los archivos

| Ubicación | Para qué sirve y por qué existe |
|---|---|
| `Controllers/*Controller.cs` | Reciben GET/POST/PUT/DELETE y llaman a servicios. El de citas añade PATCH de cancelación. No guardan datos ni duplican reglas. |
| `Models/PacienteDtos.cs` | Entrada y salida de pacientes; protege ID y auditoría del formulario editable. |
| `Models/MedicoDtos.cs` | Entrada y salida de médicos con sus IDs de especialidad. |
| `Models/EspecialidadDtos.cs` | Entrada y salida del catálogo de especialidades. |
| `Models/ProgramacionDtos.cs` | Solicitud del turno, respuesta y bloques disponibles. |
| `Models/CitaDtos.cs` | Entrada de creación, actualización, cancelación y salida de cita. |
| `Models/ExcepcionNegocio.cs` | Representa rechazos esperados para que el middleware distinga 400/404/409. |
| `Services/*Servicio.cs` | Cada archivo mantiene su interfaz y su implementación en memoria, como en la clase. |
| `Services/AlmacenEnMemoria.cs` | Diccionarios, contadores y compuerta compartida. No es una base de datos. |
| `Services/ReglasComunes.cs` | Fecha de Lima, validación, normalización y comparación de horarios reutilizables. |
| **`Services/DatosPrueba.cs`** | **Archivo separado para ingresar la lista de datos de prueba.** |
| `Middlewares/ManejadorExcepcionesMiddleware.cs` | Convierte errores en JSON ProblemDetails sin exponer detalles internos de un 500. |
| `Program.cs` | Registra servicios, carga datos y habilita OpenAPI/Scalar. |
| `Properties/launchSettings.json` | Perfil http, puerto 5026 y apertura de Scalar. |
| `GestionCitasAPI.csproj` | Versión net10 y paquetes NuGet. |
| `GestionCitasAPI.slnx` | Solución que abres en Visual Studio. |
| `GestionCitasAPI.http` | Ejemplo auxiliar actualizado; las pruebas de la guía se hacen en Scalar. |
| `REGLAS_NEGOCIO.md` | Las ocho reglas con ubicación en código y decisiones del ejercicio. |

## 3. Requisitos

- SDK **.NET 10** y **Visual Studio 2026 18.0 o posterior** con la carga Desarrollo de ASP.NET y web.
- Internet para restaurar NuGet y cargar los recursos de Scalar.
- Navegador actualizado. No se requiere Postman.

.NET 10 necesita una versión compatible del editor; instalar solo el SDK no actualiza Visual Studio 2022. Referencia: https://learn.microsoft.com/dotnet/core/install/windows

## 4. Instalar y ejecutar, paso a paso

1. Entra a **GestionCitasAPI** y abre **GestionCitasAPI.slnx**.
2. Espera a que Visual Studio restaure los paquetes. Si no lo hace, clic derecho en la solución → Restaurar paquetes NuGet.
3. Clic derecho en el proyecto → Establecer como proyecto de inicio.
4. Selecciona el perfil **http** en la barra superior.
5. Compilar → Compilar solución. Soluciona cualquier error antes de ejecutar.
6. Usa **Ctrl + F5 (Iniciar sin depurar)**. Así las excepciones esperadas de las pruebas no pausan Visual Studio.
7. Abre **http://localhost:5026/scalar/v1**. Este proyecto usa **5026**.
8. Conserva la consola abierta. Cerrar la pestaña no detiene la API.
9. Para detener, presiona Ctrl + C en la consola. Para cambiar código, detén, edita, guarda y vuelve a ejecutar.

Alternativa: terminal ubicada donde está `GestionCitasAPI.slnx`:

```powershell
dotnet restore .\GestionCitasAPI.slnx
dotnet build .\GestionCitasAPI.slnx
dotnet run --project .\GestionCitasAPI.csproj --launch-profile http
```

La raíz `/` redirige a Scalar. OpenAPI se genera en `/openapi/v1.json`. Scalar solo está habilitado en Development.

## 5. Dónde ingresar o cambiar los datos de prueba

Abre **Services → DatosPrueba.cs**. Contiene listas explícitas y comentarios:

1. Especialidades: se crean primero.
2. Pacientes: nombres completos ficticios; se generan documentos/teléfonos de prueba.
3. Médicos: incluye los IDs de sus especialidades.
4. Programaciones: asigna médico, especialidad, fecha, inicio y fin.
5. Citas: elige paciente, programación, hora y motivo.

Ejemplo para agregar un paciente después de los existentes:

```csharp
pacientes.Registrar(new()
{
    Nombres = "Andrés Felipe",
    Apellidos = "Molina Rivas",
    Dni = "87654321",
    Telefono = "987654321",
    Correo = "andres@example.com",
    Activo = true
});
```

No repitas DNI/CMP. No ejecutes manualmente Cargar varias veces: `Program.cs` ya lo hace una vez. Los datos pasan por las mismas reglas que Scalar; si escribes un turno inválido en esta precarga, la aplicación no arrancará y debes corregir esa fila. El middleware controla solicitudes HTTP, no errores previos al arranque.

**No cambies el orden de las filas sin revisar las referencias**, porque los IDs se asignan en orden. Añadir filas al final evita mover los IDs de las existentes.

## 6. Listado inicial para verificar

| Recurso | Cantidad | IDs / notas |
|---|---|---|
| Especialidades | 4 | 1 General, 2 Pediatría, 3 Cardiología, 4 Dermatología. |
| Pacientes | 12 | 1–12; 12 inactivo. |
| Médicos | 6 | 101–106; 101 Roberto tiene especialidades 3 y 1. |
| Turnos | 10 | 1–10; 5 inactivo; 9 dura 70 minutos. |
| Citas | 8 | 1001–1008; 1008 cancelada. |

Fechas: mañana según Lima al arrancar; turno 5 en dos días y 9/10 en tres días. Consulta GET programaciones para conocer la fecha exacta.

| Turno | Médico | Especialidad | Horario |
|---|---|---|---|
| 1 | 101 Roberto | 3 Cardiología | 09:00–11:00 |
| 2 | 102 Ana Lucía | 2 Pediatría | 09:00–11:00 |
| 3 | 103 Elena | 3 Cardiología | 14:00–16:10 |
| 4 | 101 Roberto | 1 General | 14:00–16:00 |
| 5 | 101 Roberto | 3 Cardiología | 09:00–11:00, inactivo |
| 6 | 104 Marco | 4 Dermatología | 09:00–11:00 |
| 7 | 105 Lucía | 1 General | 09:00–11:00 |
| 8 | 106 Daniel | 3 Cardiología | 09:00–11:00 |
| 9 | 101 Roberto | 3 Cardiología | 09:00–10:10 |
| 10 | 102 Ana Lucía | 2 Pediatría | 14:00–16:00 |

## 7. Rutas y códigos

Cada recurso (`pacientes`, `medicos`, `especialidades`, `programaciones`, `citas`) tiene:

- GET `/api/recurso`: listar.
- GET `/api/recurso/{id}`: buscar.
- POST `/api/recurso`: crear.
- PUT `/api/recurso/{id}`: actualizar con todos los campos editables.
- DELETE `/api/recurso/{id}`: eliminar.

Adicionales:

- GET `/api/medicos/por-especialidad/{especialidadId}`.
- GET `/api/programaciones?medicoId=101&especialidadId=3`.
- GET `/api/programaciones/{id}/disponibilidad`.
- PATCH `/api/citas/{id}/cancelacion`.
- GET `/api/citas/simular-error-500`: demostración solo en Development.

Respuestas: **200** consulta/cambio; **201** creación con Location; **204** borrado sin cuerpo; **400** entrada inválida; **404** no existe; **409** duplicidad, cruce, inactivo o referencia; **500** fallo inesperado.

## 8. Qué se mejoró del middleware

El original tenía una base válida: try/catch y ProblemDetails. Sin embargo, clasificaba todo `InvalidOperationException` como 400 y registraba cualquier rechazo como error del servidor.

Ahora:

- `ExcepcionNegocio` distingue 400/404/409 sin confundir errores de programación.
- Los errores inesperados se registran con `LogError`; el detalle interno no se envía al cliente.
- Cada respuesta incluye `traceId` y `instance`.
- Se unifican también las validaciones automáticas de DTOs y rutas inexistentes en `Program.cs`.
- No intenta reemplazar una respuesta que ya comenzó.
- Trata la desconexión del cliente por separado.
- Mantiene `application/problem+json` y `Type: about:blank`, con semántica estándar de ProblemDetails.

Prueba el 500 desde Scalar. No hay una base de datos desconectada: es una excepción intencional de demostración. Si Visual Studio se detiene al probar errores con F5, continúa o ejecuta con Ctrl+F5. No borres las validaciones para evitar la pausa.

## 9. Orden recomendado para entender el código

Lee primero `Models/PacienteDtos.cs`; luego `PacientesController.cs`; después `PacienteServicio.cs` y `AlmacenEnMemoria.cs`. Sigue con `ProgramacionDtos`/`ProgramacionServicio` y `CitaDtos`/`CitaServicio`. Finalmente lee `ManejadorExcepcionesMiddleware.cs` y `Program.cs`.

Un POST recorre: **Scalar → controlador → servicio → almacenamiento → respuesta**. Si el servicio rechaza la operación, el middleware transforma la excepción en una respuesta HTTP. `DataAnnotations` puede rechazar un cuerpo antes de llegar al servicio.

## 10. Límites y entrega al docente

Es una simulación local sin autenticación ni base de datos. No usar datos reales de pacientes. Los cambios desaparecen al reiniciar. El bloqueo sirve dentro de una sola instancia, no entre servidores diferentes.

## 11. Problemas frecuentes

- **NETSDK1004**: ejecutar restore y build; no continuar depuración si falla la compilación.
- **NETSDK1045**: instalar SDK 10 y editor compatible.
- **No abre Scalar**: revisar consola, usar perfil http y puerto 5026.
- **400**: revisar `errors` o `detail`; Body no debe tener campos obligatorios vacíos.
- **404**: un CMP no es un ID. Médicos empiezan en 101, especialidades en 1, citas en 1001.
- **409 al repetir**: la primera solicitud ya modificó los datos. Consulta el recurso o reinicia para repetir toda la guía.
- **Datos no cambian tras editar el archivo**: guarda, detén y ejecuta otra vez. En Scalar vuelve a pulsar Send.
