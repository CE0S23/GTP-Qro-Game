---
title: "Hito 03 — Game feel, IA de navegación y optimización"
fecha: 2026-11-25
responsable: César Osvaldo Ramírez Pérez
estado: "Terminado — código, escenas y pruebas en el repositorio"
motor: Unity 6000.4.0f1 / URP 17.4.0 / Cinemachine 3.1.8 / AI Navigation 2.0.11
tags: [GTP, hito, cinemachine, navmesh, postprocessing, scriptableobjects, profiler]
---

# Hito 03 — Game feel, IA de navegación y optimización

Volver a [[Bienvenido|Memoria del Juego]] · Plan: [[Plan de desarrollo - Evaluacion II]] · Antes: [[Hito 02 - Beta jugable]] · Optimización: [[Analisis de Optimizacion (AU 4.1)]] · Pendientes: [[Parte 2 - Orlando (pendientes)]]

## Resumen

Con el beta SA 1.1 ya terminado ([[Hito 02 - Beta jugable]]), este hito avanza en lo que el plan dejó como opcional: los criterios **DE** (Cinemachine, post-processing con partículas, Blend Trees) y **AU** (ScriptableObjects, NavMesh, reporte del Profiler). También corrige dos limitaciones conocidas del beta. **No toca las tareas de la Parte 2** (reporte de pruebas, manual, empaque, publicación), que siguen siendo de Orlando; le deja herramientas para hacerlas más rápido (compilación con un clic, sonda de rendimiento).

| Rúbrica | Qué se hizo | Dónde |
| --- | --- | --- |
| **DE 2.1** Cinemachine: Follow/transiciones + Camera Shake | `CinemachineCamera` cenital con *Follow* amortiguado al jugador; un segundo plano cercano entra con **transición EaseInOut de 0.9 s** en diálogos, decisión y minijuegos; **Impulse (sacudida)** al ser detectado/alcanzado y al forzar la cerradura. Al reaparecer, la cámara corta directo al punto de control. | `CameraDirector`, `FeedbackFX`, constructor `AddCinemachineRig` |
| **DE 3.1** Post-processing + partículas | Volume global por escena con Bloom, Vignette, Color Adjustments, Tonemapping, Chromatic Aberration (y Film Grain en la Ruta del Mal), 5 perfiles de ánimo. En tiempo real: la viñeta se tiñe de rojo y late mientras un policía te ve, destello al perder un intento, amanecer cálido en el final del Bien y saturado → gris en el del Mal. Partículas: batería, grafiti, punto de control, sigilo perfecto, minijuego, detección, polvo al correr y la llama del encendedor. | `PostFXController`, `FeedbackFX`, `Assets/Settings/PostFX/*.asset` |
| **DE 4.1** Blend Trees | `Locomocion.controller` con **Blend Tree 1D** por el parámetro `Velocidad`: Reposo (0) → Caminar (4) → Correr (6.4). Lo usan el jugador y todos los NPC (policías, perseguidores, compañeros); la velocidad se mide del desplazamiento real. | `LocomotionAnimator`, `Assets/Animations/` |
| **AU 2.1** ScriptableObjects | Además de `GameConfig` (balance), 7 **canales de eventos** `GameEventChannel` en `Assets/Resources/Eventos/`. Quien emite (GameManager, BatteryPickup, Graffiti, Checkpoint, StealthSection, LockedGate, LevelExit) y quien escucha (FeedbackFX, PostFXController) solo conocen el asset, no se conocen entre sí. | `GameEventChannel`, `GameEvents` |
| **AU 3.1** NavMesh | NavMesh horneado en las 6 escenas jugables (`NavMeshSurface`). El asaltante, el policía del altercado, los policías de patrulla y los compañeros **rodean autos, edificios y contenedores** con rutas del NavMesh que se recalculan cada 0.25–0.5 s. El **portón** del callejón es un `NavMeshObstacle` con *carving*: al forzar la cerradura desaparece y el NavMesh se abre en tiempo real. | `NavSteering`, `ChaserNPC`, `PatrolRoute`, `CompanionFollower`, constructor `BakeNavMesh` |
| **AU 4.1** Profiler | Sonda de rendimiento en la compilación de desarrollo, medición real antes/después, optimización de la GPU y análisis de memoria. | [[Analisis de Optimizacion (AU 4.1)]] |

## Correcciones de errores del beta

| Limitación del Hito 02 | Corrección |
| --- | --- |
| Los perseguidores iban en línea recta y se atoraban en autos y esquinas. | Ahora siguen rutas del NavMesh (`NavSteering`). Si una escena no tiene NavMesh, vuelven a la línea recta (no se rompe nada). |
| Una salida que se desbloqueaba con el jugador ya dentro de la zona obligaba a salir y volver a entrar. | `LevelExit` recuerda si el jugador está dentro y completa el nivel en cuanto se desbloquea. |
| `RendererColor` creaba una copia de material por objeto (`renderer.material`). | Usa `MaterialPropertyBlock`: no crea materiales. La copia del perfil de post-proceso se destruye con la escena. |

Errores encontrados **durante este hito** y corregidos (pueden servir de ejemplo para el reporte de pruebas de Orlando):

| Error | Causa | Corrección |
| --- | --- | --- |
| Excepción al cargar escenas: *InitializeNavMeshPath is not allowed to be called from a MonoBehaviour constructor*. | Se creaba un `NavMeshPath` en un inicializador de campo. | Se crea al primer uso (`NavSteering.Repath`). |
| La prueba de Game Over se congelaba si corría después de otras. | La sesión (intentos) persiste entre escenas; con un intento menos, el segundo "atrapado" ya era Game Over y el juego quedaba en pausa. | Las pruebas restauran la sesión al terminar. |
| La sonda de rendimiento se detenía al perder el foco la ventana. | Unity pausa el juego en segundo plano. | La sonda activa `Application.runInBackground`. |

## Cómo verlo en el editor

1. Abrir el proyecto con **Unity 6000.4.0f1**. La primera vez Unity descarga **Cinemachine 3.1.8** (ya está en `Packages/manifest.json`).
2. Abrir `Assets/_Scenes/Escena_Barrio.unity` → **Play**. Observar: la cámara se acerca durante el diálogo inicial y regresa al terminar; correr (Shift) levanta polvo y el personaje rebota más rápido; recoger una batería suelta partículas azules.
3. En la Ruta del Mal: dejarse ver por un policía → viñeta roja que late; al perder el intento → sacudida de cámara y destello. Forzar el portón → el policía del altercado podrá pasar por ahí.
4. Para ver el NavMesh: en la escena, seleccionar el objeto **NavMesh** (vista *Scene*, con *Gizmos* activos).

### Si alguien cambia una escena

Las escenas se siguen generando con **GTP > Construir escenas greybox**. El constructor ahora también crea los canales de eventos, el material de partículas, el Animator Controller, los perfiles de post-proceso y hornea el NavMesh de cada escena (`Assets/_Scenes/NavMesh/`). El código nuevo del constructor está aparte en `Assets/Editor/GreyboxSceneBuilder.Hito03.cs`.

### Compilar

Nuevo menú **GTP > Compilar Windows (release)** → `Builds/Windows/GTP QroCITY stories.exe` (la que se publica). **GTP > Compilar Windows (desarrollo + Profiler)** → `Builds/Windows_Dev/`. También por línea de comandos: `-executeMethod GTP.EditorTools.BuildTools.BuildReleaseBatch`.

## Arquitectura nueva

```text
Assets/
├── Animations/            Reposo, Caminar, Correr (.anim) + Locomocion.controller (Blend Tree)
├── Editor/                GreyboxSceneBuilder.Hito03.cs (cámaras, post, NavMesh, Animator, eventos) · BuildTools.cs
├── Resources/Eventos/     7 canales GameEventChannel (.asset)
├── Resources/FX/          Particula.mat (URP Particles/Unlit, aditivo)
├── Settings/PostFX/       PostFX_Noche / Cuarto / Parque / Peligro / Spot
├── _Scenes/NavMesh/       NavMesh horneado de cada escena
└── Scripts/
    ├── CameraRig/CameraDirector.cs     cámara de seguimiento ↔ plano cercano
    ├── Core/GameEventChannel.cs        canal SO + acceso GameEvents
    ├── Core/PerfProbe.cs               sonda de rendimiento (solo con -gtpPerf)
    ├── FX/FeedbackFX.cs                partículas reutilizadas + Impulse
    ├── FX/PostFXController.cs          viñeta de peligro, destello y ánimo de los finales
    ├── NPC/NavSteering.cs              rutas del NavMesh para NPC con Rigidbody
    └── Player/LocomotionAnimator.cs    alimenta el Blend Tree
```

Decisiones:

- **NavMesh sin `NavMeshAgent`.** Los NPC ya se mueven con `Rigidbody` y triggers (atrapar, zonas). `NavSteering` solo calcula la ruta (`NavMesh.CalculatePath`) y entrega la siguiente esquina; el Rigidbody sigue moviendo al NPC. Así no se duplicó la física ni cambió el comportamiento probado del beta.
- **Partículas sin `Instantiate`/`Destroy`.** Hay un `ParticleSystem` persistente por tipo de efecto y se emite en la posición del evento (`Emit`). Cero objetos nuevos durante el juego (confirmado en el Profiler: ~0.1 KB de GC por frame).
- **Animación procedural** (rebote y estiramiento sobre un hijo `Rig`), porque los personajes siguen siendo cápsulas. Cuando haya sprites o modelos animados basta con reemplazar los 3 clips del Blend Tree; el parámetro `Velocidad` y `LocomotionAnimator` se quedan igual.

## Validación

- Compilación sin errores ni advertencias del proyecto en Unity 6000.4.0f1.
- **Pruebas automáticas: 10/10 EditMode y 21/21 PlayMode aprobadas** (antes 6 + 8). Nuevas: canales de eventos, respaldo sin NavMesh, NavMesh + Cinemachine + Volume en las 6 escenas, ruta que rodea un auto, portón como obstáculo dinámico, perseguidor usando NavMesh, transición de cámara en diálogo, sacudida + partículas + viñeta al ser detectado, Blend Tree siguiendo la velocidad, salida desbloqueada con el jugador dentro.
- **Compilación de Windows de desarrollo** generada y ejecutada (recorrido automático de 2.5 min sin errores en el log). Datos en [[Analisis de Optimizacion (AU 4.1)]].
- Capturas tomadas por la sonda en la compilación (1600×900):

![[Adjuntos/Hito03_redada.png]]
![[Adjuntos/Hito03_estres_20_policias.png]]

**Lo que NO se ha verificado:** una partida manual completa con teclado después de estos cambios (la sacudida, la viñeta y la cámara cercana se probaron con pruebas automáticas y capturas, no jugando), y si la sacudida o la viñeta resultan molestas. Eso entra en las pruebas de la Parte 2.
