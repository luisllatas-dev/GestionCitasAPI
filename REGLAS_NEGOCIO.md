# 📋 Reglas de Negocio - Gestión de Citas API

Este documento define las reglas de negocio y validaciones que se implementarán progresivamente en la capa de servicios (`Services/`) y controladores (`Controllers/`) del proyecto **GestionCitasAPI**.

> **Nota arquitectónica:** Siguiendo el patrón de `Sesion_07`, las violaciones de reglas de negocio en la capa de Servicios lanzarán `InvalidOperationException`, las cuales serán capturadas por `ManejadorExcepcionesMiddleware` para devolver una respuesta estándar **HTTP 400 Bad Request** bajo el formato **RFC 7807 (`ProblemDetails`)**.

---

## 🏥 0. Módulo de Especialidades (`EspecialidadServicio`)

- **RN-ESP-01 (Datos obligatorios):** El `Nombre` de la especialidad es obligatorio y no puede estar vacío.
- **RN-ESP-02 (Unicidad de Nombre):** No pueden existir dos especialidades con el mismo `Nombre` (ignorando mayúsculas/minúsculas).
- **RN-ESP-03 (Estado Activo):** Toda nueva especialidad se crea con `Activo = true` por defecto. Si una especialidad está inactiva (`Activo = false`), no se le pueden asignar nuevos médicos.
- **RN-ESP-04 (Integridad al eliminar):** No se puede eliminar una especialidad si tiene médicos asociados a ella.
- **RN-ESP-05 (Auditoría):** Registro automático de `FechaCreacion` al crear y `FechaActualizacion` al modificar datos o cambiar el estado `Activo`.

---

## 👤 1. Módulo de Pacientes (`PacienteServicio`)

- **RN-PAC-01 (Datos obligatorios):** Los `Nombres`, `Apellidos` y el `Dni` del paciente no pueden estar vacíos (`NombreCompleto` se calcula automáticamente como `$"{Nombres} {Apellidos}"`).
- **RN-PAC-02 (Formato de DNI):** El DNI debe contener exactamente **8 dígitos numéricos**.
- **RN-PAC-03 (Unicidad de DNI):** No se puede registrar ni actualizar un paciente con un DNI que ya esté registrado en otro paciente activo.
- **RN-PAC-04 (Formato de Correo):** Si se proporciona un correo electrónico, debe contener un formato válido (incluir `@` y dominio).
- **RN-PAC-05 (Integridad al eliminar):** No se puede eliminar un paciente si tiene citas en estado `"Programada"` (pendientes de atención).
- **RN-PAC-06 (Auditoría):** Al crear un paciente, `FechaCreacion` se asigna con la fecha/hora actual (`DateTime.Now`) y `FechaActualizacion` queda en `null` (o igual a `FechaCreacion`). Al modificarlo, se actualiza `FechaActualizacion`.

---

## 🩺 2. Módulo de Médicos (`MedicoServicio`)

- **RN-MED-01 (Datos obligatorios):** Los `Nombres`, `Apellidos`, el `EspecialidadId` y el número de colegiatura (`Cmp`) son obligatorios (`NombreCompleto` se calcula automáticamente como `$"{Nombres} {Apellidos}"`).
- **RN-MED-02 (Existencia y Estado de Especialidad):** El `EspecialidadId` debe corresponder a una especialidad existente y cuyo estado sea `Activo == true`.
- **RN-MED-03 (Unicidad de Colegiatura):** No pueden existir dos médicos con el mismo número de colegiatura (`Cmp`).
- **RN-MED-04 (Integridad al eliminar):** No se puede eliminar un médico si tiene citas en estado `"Programada"`.
- **RN-MED-05 (Auditoría):** Registro automático de `FechaCreacion` al dar de alta y `FechaActualizacion` al modificar sus datos.

---

## 📅 3. Módulo de Citas (`CitaServicio`)

- **RN-CIT-01 (Existencia de Paciente y Médico):** Para agendar una cita, tanto el `PacienteId` como el `MedicoId` deben corresponder a registros existentes en el sistema.
- **RN-CIT-02 (Prohibición de fechas pasadas):** La `FechaHora` de la cita debe ser estrictamente futura (`FechaHora > DateTime.Now`). No se pueden agendar citas en el pasado.
- **RN-CIT-03 (Horario de atención de la clínica):** Las citas solo pueden agendarse dentro del horario laboral, por ejemplo entre las **08:00 y las 20:00 horas**.
- **RN-CIT-04 (Disponibilidad del Médico / Sin cruce de horario):** Un médico no puede tener dos citas en estado `"Programada"` en la misma fecha y hora (o dentro de una ventana de 30 minutos de diferencia).
- **RN-CIT-05 (Disponibilidad del Paciente / Sin cruce de horario):** Un paciente no puede agendar dos citas en estado `"Programada"` a la misma fecha y hora con diferentes médicos.
- **RN-CIT-06 (Ciclo de vida / Estados de la Cita):**
  - Toda cita nueva nace con el estado `"Programada"`.
  - Los estados válidos son: `"Programada"`, `"Atendida"`, `"Cancelada"`.
  - No se puede cancelar ni reprogramar una cita cuyo estado ya sea `"Atendida"` o `"Cancelada"`.
- **RN-CIT-07 (Auditoría):** Registro automático de `FechaCreacion` al agendar la cita, y actualización automática de `FechaActualizacion` cuando se reprograma o cambia de estado (`"Atendida"` / `"Cancelada"`).
