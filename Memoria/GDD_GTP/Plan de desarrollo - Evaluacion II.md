---
title: "Plan de desarrollo — Evaluación II"
fecha: 2026-11-25
motor: Unity 6000.4.0f1 (Unity 6.4) / URP 17.4.0
repositorio: https://github.com/CE0S23/GTP-Qro-Game
tags: [GTP, plan, rubrica, evaluacion-II]
---

# Plan de desarrollo — Evaluación II

Volver a [[Bienvenido|Memoria del Juego]] · Resultado de la Parte 1: [[Hito 02 - Beta jugable]] · DE/AU: [[Hito 03 - Game feel, IA y optimizacion]] · Tareas de la Parte 2: [[Parte 2 - Orlando (pendientes)]]

## 1. Qué pide la rúbrica

Fuente: `Videojuegos - Lista de Cotejo para Evaluación II.pdf` (raíz del proyecto).

| Nivel | Requisito | Alcance de este plan |
| --- | --- | --- |
| **SA (mínimo)** | 1.1 Beta ≥ 5 min, no ciclado, con eventos, comportamientos, acciones, colisiones, puntaje y vidas | **Parte 1 — César (hecho)** |
| SA | 1.2 Manual de usuario | Parte 2 — Orlando |
| SA | 1.3 Diseño de empaque (medio de almacenamiento) | Parte 2 — Orlando |
| SA | 1.4 Reporte de pruebas: ≥ 5 errores **por cada severidad**, con descripción, pasos, información adicional, comportamiento esperado, frecuencia, severidad y prioridad | Parte 2 — Orlando |
| SA | 1.5 Publicar en cualquier store | Parte 2 — Orlando |
| DE | Cinemachine (Follow + Camera Shake), post-processing + partículas, Blend Trees | **Hito 03 — César (hecho)**; cuenta cuando SA esté al 100 % |
| AU | ScriptableObjects, NavMesh, reporte de Profiler | **Hito 03 — César (hecho)**: canales de eventos SO, NavMesh, [[Analisis de Optimizacion (AU 4.1)]] |

**Meta acordada: cumplir SA completo.** DE y AU solo si sobra tiempo, sin poner en riesgo la entrega. Actualización 2026-11-25: con la Parte 1 cerrada, César adelantó DE y AU en el [[Hito 03 - Game feel, IA y optimizacion]] sin tocar las tareas de Orlando. DE y AU solo se acreditan si SA queda al 100 %, así que **la Parte 2 sigue siendo lo prioritario**.

## 2. División del trabajo

| Parte | Responsable | Contenido | Estado |
| --- | --- | --- | --- |
| **Parte 1 — Desarrollo** | César Osvaldo Ramírez Pérez | Beta jugable completo de principio a fin (7 escenas), sistemas de puntaje, intentos, batería, sigilo, decisión y dos finales; pruebas automatizadas de humo; compilación de Windows verificada; repositorio. | ✅ Terminado el 2026-11-25 |
| **Hito 03 — DE/AU** | César Osvaldo Ramírez Pérez | Cinemachine con transiciones y sacudida, post-processing y partículas, Blend Tree, canales de eventos SO, NavMesh, análisis del Profiler, correcciones del beta, menú de compilación. | ✅ Terminado el 2026-11-25 |
| **Parte 2 — Pruebas y entrega** | Orlando Rubio Cabrera | Jugar y cronometrar, reporte de pruebas (SA 1.4), corrección de errores encontrados, manual (SA 1.2), empaque (SA 1.3), publicación (SA 1.5). | ⏳ Pendiente — ver [[Parte 2 - Orlando (pendientes)]] |

## 3. Cómo cubre el beta el punto SA 1.1

| Criterio | Dónde está en el juego | Código |
| --- | --- | --- |
| **1.1.1 Eventos** | Zonas que disparan acciones al entrar (asalto, redada, revelación, puntos de control, salidas); al terminar diálogos se desbloquean salidas, se activa la decisión o la persecución. Se ven y editan en el Inspector como `UnityEvent`. Además, eventos C# de sesión (puntaje, intentos, batería, ruta). | `TriggerZone`, `NarrativeInteractable`, `NarrativeSequence`, `LockedGate.OnOpened`, `GameSession` (eventos), `GameManager.PlayerRespawned` |
| **1.1.2 Comportamientos** | Policías que patrullan y se detienen a mirar al jugador; cono de visión bloqueado por obstáculos; foco del helicóptero que barre la zona; asaltante y policía que persiguen y se rinden; compañeros que siguen al jugador. | `PatrolRoute`, `PoliceVisionCone`, `SearchlightDetector`, `ChaserNPC`, `CompanionFollower` (base común `ThreatDetector`) |
| **1.1.3 Acciones** | Caminar, correr (Shift), agacharse (C), interactuar (E): leer grafitis, hablar, revisar el celular, esconderse en contenedores, forzar cerradura (minijuego), encendedor (minijuego), decidir la ruta, pausar (Esc). | `PlayerMovement`, `PlayerInteractor`, `HidingSpot`, `MinigameManager`, `DecisionPanel` |
| **1.1.4 Colisiones** | Física 3D con `Rigidbody`: paredes, autos y edificios bloquean; triggers para zonas, baterías, puntos de control y salidas; raycasts de línea de visión; contacto de perseguidores. | Colliders en escenas + `OnTriggerEnter` |
| **1.1.5 Puntaje** | +100 grafiti leído, +50 batería, +200 tramo de sigilo sin ser detectado, +150 minijuego de cerradura, +100 por nivel completado, −50 por detección, bono final por batería e intentos restantes. Récord guardado (`PlayerPrefs`). | `GameSession`, `GameConfig`, `EndingController` |
| **1.1.6 Oportunidades/vidas** | 3 intentos por nivel. Ser visto 2 s o alcanzado resta un intento y regresa al último punto de control. Sin intentos (o batería en 0 %) = Game Over con “Reintentar nivel” o “Menú”. | `GameManager.ReportPlayerCaught`, `PlayerRespawn`, `Checkpoint`, `GameOverPanel` |
| **≥ 5 min, no ciclado** | Historia lineal Menú → Cuarto → Barrio → Encrucijada → Ruta del Bien **o** Ruta del Mal → Spot → final. Estimación de diseño: 7–10 min por ruta. **Orlando debe cronometrarlo** (ver Parte 2). | `SceneNames.BuildOrder` |

## 4. Decisiones técnicas

- **Versión de Unity: `6000.4.0f1` para ambos.** El proyecto se creó y validó con esa versión. Abrirlo en una versión **anterior** (6.3) no está soportado: Unity advierte del *downgrade* y los paquetes (URP 17.4, Input System 1.19) son de 6.4, por lo que puede haber errores de paquetes o de serialización. Abrirlo en una **más nueva** (6.5) actualiza el proyecto y obliga al otro a subir también. Instalar `6000.4.0f1` desde Unity Hub → *Installs* → *Install Editor* → *Archive*, o con el enlace `unityhub://6000.4.0f1/8cf496087c8f`.
- **Greybox generado por código.** `Assets/Editor/GreyboxSceneBuilder.cs` (menú **GTP > Construir escenas greybox**) crea las 7 escenas con primitivas y deja todos los eventos conectados. Si alguien edita una escena a mano, **no** debe volver a ejecutar el constructor (sobrescribe las escenas) o debe pasar el cambio al script.
- **Datos fuera de la escena (base para AU 2.1).** El balance (velocidades, intentos, batería, tiempos de detección y puntajes) vive en `Assets/Resources/GameConfig.asset` (ScriptableObject). El estado de la partida vive en `GameSession`, una clase C# sin MonoBehaviour, probada con pruebas unitarias.
- **UI por código** (uGUI con fuente integrada) para no depender de importar TextMeshPro; persistente entre escenas junto al `GameManager`.
- **Sin armas** (GDD §4.6): el altercado es una huida; ser alcanzado resta un intento.
- **Movimiento en cuatro direcciones**, como se decidió en el Hito 01, para coincidir con las animaciones arriba/abajo/izquierda/derecha del GDD.

## 5. Calendario

| Fecha | Actividad | Responsable |
| --- | --- | --- |
| 2026-11-25 | Parte 1 terminada y subida al repositorio | César |
| Semana siguiente | Clonar, abrir con 6000.4.0f1, jugar ambas rutas y cronometrar | Orlando |
| Después | Reporte de pruebas (≥ 5 errores por severidad) y correcciones | Orlando |
| Después | Manual, empaque, compilación final y publicación en itch.io | Orlando |
| Antes de la entrega | Revisión conjunta del portafolio contra la lista de cotejo | Ambos |

## 6. Control de cambios del plan

Registrar aquí cualquier cambio de alcance: fecha, qué cambió, por qué y quién lo decidió.

| Fecha | Cambio | Motivo | Quién |
| --- | --- | --- | --- |
| 2026-11-25 | Se implementa el recorrido completo en greybox (antes: solo movimiento) | Requisito SA 1.1 de la Evaluación II | César |
| 2026-11-25 | Hito 03: se agregan Cinemachine 3.1.8, post-processing, partículas, Blend Tree, canales de eventos SO y NavMesh; se bajan sombras y se apaga SSAO del perfil PC | Adelantar DE/AU con la Parte 1 cerrada; la GPU integrada no llegaba a 60 FPS | César |
