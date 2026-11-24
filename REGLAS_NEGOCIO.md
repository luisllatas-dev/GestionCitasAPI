# Las 8 reglas de negocio esenciales

Este documento relaciona cada regla con el código y una prueba. Los formatos de entrada (DNI de 8 dígitos, teléfono de 9, correo válido, campos obligatorios) son validaciones de datos que acompañan estas ocho reglas.

| Regla | Qué exige | Dónde se aplica | Prueba |
|---|---|---|---|
| RN01 — Evitar duplicados | DNI, CMP y nombres de especialidad únicos. En PUT se excluye el mismo registro. | PacienteServicio, MedicoServicio, EspecialidadServicio | Repetir DNI → 409. |
| RN02 — Médico y especialidad | Un turno solo puede usar una especialidad asignada al médico. | ProgramacionServicio.Validar | Médico 101 con especialidad 2 → 409. |
| RN03 — Programación válida | Inicio futuro, fin posterior, duración mínima de 20 minutos en el mismo día; sin cruces de turnos activos del médico. | ProgramacionServicio.Validar | Repetir turno 1 → 409; fin anterior → 400. |
| RN04 — Registros habilitados | Paciente, médico, especialidad y turno deben existir y estar activos al reservar. | CitaServicio.ValidarReserva | Paciente 12 inactivo → 409; paciente inexistente → 404. |
| RN05 — Duración y bloques | Cita de 20 minutos dentro del turno, sobre un bloque válido y futuro. | CitaServicio.ValidarReserva y ProgramacionServicio.Disponibilidad | 09:10 en turno 1 → 400; turno 9 ofrece solo 3 bloques. |
| RN06 — Sin citas simultáneas | No cruzar citas programadas del médico ni del paciente. La validación y escritura se protegen con una compuerta. | CitaServicio.ValidarReserva / AlmacenEnMemoria | Dos reservas del mismo horario → 201 y 409. |
| RN07 — Cambios controlados | Reprogramar/cancelar solo antes del inicio y estando Programada. Validar destino antes de sustituir; cancelar exige motivo y libera espacio. | CitaServicio.Actualizar / Cancelar | Reprogramación fallida conserva original; cancelación libera bloque. |
| RN08 — Proteger relaciones | No borrar paciente con citas, médico con turnos o especialidad asignada; no modificar/borrar turnos con citas, incluso canceladas. No quitar del médico una especialidad usada por turnos. | Eliminar de cada servicio y ProgramacionServicio.SinCitas | DELETE paciente 1 o turno 1 → 409. |

## Decisiones para mantener el ejercicio sencillo

- PUT de cita conserva el paciente; si se eligió otra persona por error, se cancela y se crea una cita nueva.
- La cancelación repetida devuelve la cancelación existente sin sobrescribir el motivo.
- DELETE de cita borra físicamente de la memoria para demostrar CRUD. Cancelar conserva historia.
- Los cambios de Activo en catálogos impiden nuevas reservas y reprogramaciones hacia registros inactivos; no cancelan automáticamente citas ya existentes. Una cancelación existente sigue siendo posible antes del inicio. No se agrega una novena regla de bloqueo de desactivación.
- Los nombres que devuelve una cita se conservan como fotografía de cuando se registró/reprogramó el profesional. Cambiar un catálogo no reescribe los nombres de las citas anteriores.
- No hay restricciones clínicas por edad ni validación real de CMP: se trabaja con datos ficticios.
- Las fechas y auditoría usan hora de Lima. Reiniciar recarga los datos y elimina los cambios de la sesión.
