---
title: "Hito 02 — Beta jugable (Parte 1)"
fecha: 2026-11-25
responsable: César Osvaldo Ramírez Pérez
estado: "Terminado — pendiente de pruebas de la Parte 2"
motor: Unity 6000.4.0f1 / URP 17.4.0 / Input System 1.19.0
tags: [GTP, hito, beta, unity]
---

# Hito 02 — Beta jugable (Parte 1)

Volver a [[Bienvenido|Memoria del Juego]] · Plan: [[Plan de desarrollo - Evaluacion II]] · Siguiente: [[Parte 2 - Orlando (pendientes)]]

Fecha real de trabajo: **25 de noviembre de 2026**. Se pasó del greybox de movimiento (Hito 01) a un **beta jugable de principio a fin**, con arte provisional de primitivas (cápsulas y cubos de colores, como en el storyboard: Orlando verde, novia morado, amigo turquesa, policía claro, asaltante rojo).

## Cómo jugar en el editor

1. Abrir `Game/GTP Qro` con **Unity 6000.4.0f1**.
2. Abrir `Assets/_Scenes/Escena_Menu.unity` y pulsar **Play** (también se puede dar Play en cualquier `Escena_*` para probar un nivel suelto).
3. Controles: **WASD/flechas** mover · **Shift** correr · **C** agacharse · **E** interactuar · **Espacio** minijuegos · **Esc** pausa · **1/2** o clic en la decisión.

## Recorrido implementado

| Escena | Nivel GDD | Qué pasa | Sale cuando |
| --- | --- | --- | --- |
| `Escena_Menu` | — | Jugar, Controles, Salir, récord | Jugar |
| `Escena_Cuarto` | 0 · Tutorial | Diálogo de inicio, revisar el celular (12 %), audífonos, foto en la pared | Tras revisar el celular, cruzar la puerta |
| `Escena_Barrio` | 1A–1D | Grafitis y baterías; **asalto** (perseguidor que se rinde en zona segura); **sigilo** con 2 patrullas, autos de cobertura y contenedores; **Punto cerrado** | Tras tocar la puerta del Punto, salida a la plaza |
| `Escena_Encrucijada` | 2 · Decisión | Diálogo con Orlando y **decisión de dos botones**; la ruta se guarda una sola vez | Elegir ruta |
| `Escena_RutaBien` | 3A | Encontrar a la novia (lo sigue), luego al amigo (lo sigue); grafitis y banca reflexiva; sin policía | Llegar juntos a la salida |
| `Escena_RutaMal` | 3B I–III | Orlando acompaña; **redada** (3 policías + foco de helicóptero, contenedores); **minijuego de cerradura** para el callejón; **revelación** en casa de la novia; Orlando se va y **un policía persigue** hasta la salida | Llegar a la salida |
| `Escena_Spot` | 4 · Final | Bien: diálogo y amanecer cálido. Mal: **minijuego del encendedor**, luz saturada que se apaga al amanecer. Resumen de puntaje y récord | Volver al menú |

## Arquitectura (sin “God Object”)

```text
Assets/
├── _Scenes/            Escena_Menu … Escena_Spot (+ Demo_Movimiento_01 del Hito 01)
├── Editor/             GreyboxSceneBuilder.cs  (menú GTP > Construir escenas greybox)
├── Materials/Greybox/  materiales URP generados
├── Resources/          GameConfig.asset  (balance editable, ScriptableObject)
├── Scripts/            ensamblado GTP.Runtime
│   ├── Core/           GameManager, GameSession, GameConfig, GameTypes, RendererColor
│   ├── Player/         PlayerMovement, PlayerStealth, PlayerInteractor, PlayerRespawn
│   ├── NPC/            ThreatDetector, PoliceVisionCone, SearchlightDetector, PatrolRoute, ChaserNPC, CompanionFollower
│   ├── World/          TriggerZone, Checkpoint, LevelExit, Graffiti, BatteryPickup, HidingSpot, LockedGate, StealthSection, ObjectiveStep, RouteOnly, Billboard
│   ├── Narrative/      NarrativeInteractable, NarrativeSequence, BranchingManager, EndingController
│   ├── UI/             GameUI, PhoneUIController (HUD celular), DialogueBox, DecisionPanel, MinigameManager, PausePanel, GameOverPanel, SummaryPanel, MainMenu, UIFactory
│   └── CameraRig/      CameraFollow
└── Tests/              EditMode (GameSession) y PlayMode (humo por escena)
```

| Componente | Responsabilidad única |
| --- | --- |
| `GameManager` | Estados (Menú, Exploración, Narrativa, Decisión, Minijuego, Pausa, Game Over, Final), carga de escenas, pausa, intentos y Game Over. Se crea solo al iniciar (`RuntimeInitializeOnLoadMethod`). |
| `GameSession` | Datos de la partida: puntaje, intentos, batería, ruta, objetos ya recogidos y “foto” al inicio del nivel para reintentar. Publica eventos. |
| `GameConfig` | Valores de balance (ScriptableObject). |
| `PhoneUIController` | HUD del celular: batería, puntos, intentos, mapa con flecha y distancia al objetivo, pensamientos, avisos y barra “¡Te están viendo!”. |
| `ThreatDetector` | Base de amenazas: acumula sospecha y, a los 2 s, resta un intento. `PoliceVisionCone` y `SearchlightDetector` solo definen cómo ven. |
| `BranchingManager` | Decisión de la Encrucijada y carga de la ruta. |
| `MinigameManager` | Minijuegos de cerradura (aguja en zona verde, 3 aciertos) y encendedor (presionar Espacio). |

Los componentes `PhoneUIController`, `MinigameManager`, `BranchingManager` y `PoliceVisionCone`, planificados en el GDD §4.9 y en el Hito 01, **ya están implementados**. `CompanionFollower` cubre el seguimiento de Orlando; su ayuda activa (distraer policías) sigue pendiente.

## Validación realizada (Parte 1)

- **Compilación** sin errores en Unity 6000.4.0f1 (modo batch).
- **Constructor de escenas** ejecutado: 7 escenas generadas y añadidas a *Build Settings* en orden.
- **6 pruebas EditMode** de `GameSession`: puntaje no negativo, intentos, batería con límite y evento único, ruta guardada una vez, objetos únicos, reintento de nivel. **6/6 aprobadas.**
- **8 pruebas PlayMode**: el menú y las 6 escenas jugables cargan y corren 2 s sin errores en consola, con jugador y cámara; 3 detecciones llevan a Game Over y “Reintentar” restaura los intentos. **8/8 aprobadas.**
- **Compilación de Windows 64 bits** generada sin errores y ejecutable iniciado sin errores en el log.
- **Capturas renderizadas** por Unity para revisar el trazado de los niveles (tomadas antes de un ajuste menor de luz ambiental):

![[Adjuntos/Hito02_Cuarto_camara.png]]
![[Adjuntos/Hito02_Barrio_planta.png]]
![[Adjuntos/Hito02_Encrucijada_camara.png]]
![[Adjuntos/Hito02_RutaMal_planta.png]]

**Lo que NO se ha verificado** (pasa a la Parte 2): una partida manual completa con teclado de cada ruta, la duración real (≥ 5 min), la dificultad del sigilo y la persecución, legibilidad de la UI en distintas resoluciones y el comportamiento de la compilación con gráficos reales.

## Limitaciones conocidas

- Arte, audio y animaciones son provisionales o inexistentes (sin música ni efectos de sonido).
- Los perseguidores van en línea recta hacia el jugador (sin NavMesh): pueden atorarse en esquinas.
- Los textos 3D y la UI usan la fuente integrada de Unity.
- El balance (velocidades, batería 12 % a 1.2 %/min, 2 s de detección) es inicial y se ajusta en `GameConfig.asset`.
- La salida bloqueada requiere salir y volver a entrar a su zona después de desbloquearse.
