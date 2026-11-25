# GTP QroCITY stories

Aventura narrativa y de sigilo 2.5D (cámara cenital) hecha en Unity como Proyecto Integrador de **Creación de Videojuegos — UTEQ**.
Un joven recorre la ciudad durante una noche; una decisión en la Encrucijada lleva a la Ruta del Bien o a la Ruta del Mal, con finales opuestos. El juego tiene un mensaje explícito contra el uso de drogas.

Equipo: César Osvaldo Ramírez Pérez · Orlando Rubio Cabrera · Mauricio Gabriel Vázquez

## Estado

| Parte | Responsable | Estado |
| --- | --- | --- |
| Parte 1 — Desarrollo del beta (SA 1.1) | César | ✅ Terminada (2026-11-25) |
| Parte 2 — Pruebas, reporte, manual, empaque y publicación (SA 1.2–1.5) | Orlando | ⏳ Pendiente |

Toda la planeación está en la memoria (bóveda de Obsidian en `Memoria/GDD_GTP`):

- [`Bienvenido.md`](Memoria/GDD_GTP/Bienvenido.md) — memoria del juego
- [`Plan de desarrollo - Evaluacion II.md`](Memoria/GDD_GTP/Plan%20de%20desarrollo%20-%20Evaluacion%20II.md) — rúbrica, división del trabajo y decisiones
- [`Hito 02 - Beta jugable.md`](Memoria/GDD_GTP/Hito%2002%20-%20Beta%20jugable.md) — qué se construyó y cómo se validó
- [`Parte 2 - Orlando (pendientes).md`](Memoria/GDD_GTP/Parte%202%20-%20Orlando%20(pendientes).md) — lista de tareas pendientes

## Abrir el proyecto

1. Instalar **Unity 6000.4.0f1** (Unity 6.4) en Unity Hub — *Installs → Install Editor → Archive* o `unityhub://6000.4.0f1/8cf496087c8f`.
   **Usar exactamente esta versión.** No abrir con una versión anterior (6.3) ni más nueva (6.5).
2. Unity Hub → *Add → Add project from disk* → `Game/GTP Qro`.
3. Abrir `Assets/_Scenes/Escena_Menu.unity` y pulsar **Play**.

Controles: WASD/flechas mover · Shift correr · C agacharse · E interactuar · Espacio minijuegos · Esc pausa.

Pruebas automáticas: *Window → General → Test Runner* → EditMode y PlayMode → *Run All*.

Las escenas se generan con **GTP > Construir escenas greybox** (`Assets/Editor/GreyboxSceneBuilder.cs`). Ese comando **sobrescribe** las escenas `Escena_*`; si se editan a mano, no volver a ejecutarlo.

## Rúbrica

`Videojuegos - Lista de Cotejo para Evaluación II.pdf`
