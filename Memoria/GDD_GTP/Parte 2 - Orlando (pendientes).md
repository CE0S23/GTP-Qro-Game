---
title: "Parte 2 — Pruebas y entrega (Orlando)"
responsable: Orlando Rubio Cabrera
estado: "Pendiente"
tags: [GTP, pendientes, pruebas, entrega]
---

# Parte 2 — Pruebas y entrega (Orlando)

Volver a [[Bienvenido|Memoria del Juego]] · Plan: [[Plan de desarrollo - Evaluacion II]] · Lo que ya está hecho: [[Hito 02 - Beta jugable]]

Marca cada casilla al terminarla y registra los cambios en la sección 6 del plan.

## 0. Preparar el proyecto

- [ ] Instalar **Unity 6000.4.0f1** en Unity Hub (*Installs → Install Editor → Archive*, o abrir `unityhub://6000.4.0f1/8cf496087c8f`). **No abrir el proyecto con 6.3** (es una versión anterior y no está soportado) ni con 6.5 (actualizaría el proyecto para los dos).
- [ ] `git clone https://github.com/CE0S23/GTP-Qro-Game.git`
- [ ] En Unity Hub: *Add → Add project from disk* → carpeta `Game/GTP Qro`. La primera apertura tarda (regenera `Library`).
- [ ] Abrir `Assets/_Scenes/Escena_Menu.unity` → **Play**.
- [ ] Correr las pruebas automáticas: *Window → General → Test Runner* → EditMode y PlayMode → *Run All* (deben salir 14/14 en verde).

## 1. Jugar y cronometrar (SA 1.1)

- [ ] Partida completa por la **Ruta del Bien**: anotar duración.
- [ ] Partida completa por la **Ruta del Mal**: anotar duración.
- [ ] Confirmar que cada ruta dura **≥ 5 minutos** sin repetir contenido. Si alguna queda corta, subir distancias o bajar velocidades en `Assets/Resources/GameConfig.asset`, o agregar un grafiti/tramo.
- [ ] Probar cada elemento de la rúbrica: eventos, comportamientos, acciones, colisiones, puntaje y vidas (ver la tabla del plan, §3).

| Ruta | Duración 1 | Duración 2 | Duración 3 | Notas |
| --- | --- | --- | --- | --- |
| Bien | | | | |
| Mal | | | | |

## 2. Reporte de pruebas (SA 1.4)

La rúbrica pide **al menos 5 errores por cada severidad**. Con las 4 severidades siguientes son **mínimo 20 errores**. Cada error lleva los 7 campos de la rúbrica.

| Severidad | Significado |
| --- | --- |
| Crítica | Cierra el juego, bloquea el avance o hace imposible terminar. |
| Alta | Una función principal falla (detección, intentos, puntaje, decisión), pero hay forma de seguir. |
| Media | Comportamiento incorrecto que no impide jugar (cámara, colisiones raras, textos fuera de lugar). |
| Baja | Detalles visuales, ortografía, colores, pulido. |

Plantilla (copiar una fila por error):

| ID | Descripción del error | Pasos para reproducir | Información adicional (escena, versión, captura) | Comportamiento esperado | Frecuencia (Siempre / A veces / Rara vez) | Severidad | Prioridad (Alta / Media / Baja) | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| E-01 | | 1. … 2. … 3. … | | | | | | Abierto |

Zonas donde conviene buscar errores (sin confirmar; son hipótesis para probar):

- Perseguidores (asaltante, policía del altercado) que se atoran contra autos o esquinas porque van en línea recta.
- Entrar/salir de un contenedor justo cuando un policía detecta; reaparecer escondido.
- Interactuar con E en el mismo instante en que se abre o cierra un diálogo.
- Salida bloqueada: después de desbloquearla hay que salir y volver a entrar a la zona verde.
- Batería en 0 % durante un diálogo o un minijuego.
- Pausa (Esc) durante minijuego, decisión o resumen final.
- “Reintentar nivel” desde Game Over y desde Pausa: puntaje, batería y objetos recogidos.
- UI en resoluciones distintas a 1920×1080 (ventana pequeña, 4:3, ultrapanorámica).
- Textos 3D que se encimen o queden tapados por edificios.
- Dar Play directo a una escena (sin pasar por el menú) y luego terminar el juego.

Al corregir un error: anotar en la columna Estado “Corregido (commit xxxxxxx)” y volver a probarlo.

## 3. Manual de usuario (SA 1.2)

- [ ] Portada, requisitos (Windows 10/11 64 bits, teclado y mouse), instalación y cómo iniciar.
- [ ] Historia breve y objetivo (llegar al Spot al amanecer; dos rutas).
- [ ] Controles (WASD/flechas, Shift, C, E, Espacio, Esc, 1/2).
- [ ] HUD del celular: batería, puntos, intentos, mapa con flecha, objetivo, barra “¡Te están viendo!”.
- [ ] Reglas: 3 intentos por nivel, detección de 2 s, Game Over por intentos o batería, puntos de control.
- [ ] Puntaje: tabla de puntos (ver plan §3) y récord.
- [ ] Capturas de cada escena y de los dos finales.
- [ ] Créditos y aviso del mensaje contra el uso de drogas.

## 4. Diseño de empaque (SA 1.3)

- [ ] Elegir el medio (USB o disco) y diseñar portada, contraportada, lomo y etiqueta del medio.
- [ ] Incluir título, clasificación sugerida (público 16+ por temas maduros), requisitos, capturas, sinopsis, logos UTEQ y créditos.
- [ ] Usar como base `Archivos e Imagenes/Vision_GTP_QroCITY_stories.png` y los storyboards SD.

## 5. Compilar y publicar (SA 1.5)

- [ ] *File → Build Profiles → Windows* → escenas ya están en orden (Escena_Menu primero) → *Build* en una carpeta `Builds/` (está ignorada por git).
- [ ] Probar el `.exe` fuera del editor (la entrega se valida en la compilación, no en el editor).
- [ ] Comprimir la carpeta del build en `.zip`.
- [ ] Publicar en **itch.io** (gratis): crear cuenta → *Upload new project* → tipo *Downloadable* → subir el `.zip` marcado como Windows → capturas, descripción y clasificación → *Public*.
- [ ] Guardar el enlace público en el plan y en el portafolio.

## 6. Opcional (solo si SA ya está completo)

| Rúbrica | Idea concreta sobre el código actual |
| --- | --- |
| DE 2.1 Cinemachine | Instalar `com.unity.cinemachine`; cambiar `CameraFollow` por una CinemachineCamera con *Follow* al jugador y un *Impulse* (shake) al llamar `GameManager.ReportPlayerCaught`. |
| DE 3.1 Post-processing + partículas | Volumen global (Bloom, Vignette, Color Adjustments) y partículas al recoger batería, al ser detectado y en el encendedor. |
| DE 4.1 Blend Trees | Requiere modelo/sprites animados; Blend Tree 2D con la velocidad de `PlayerMovement`. |
| AU 2.1 ScriptableObjects | Ya existe `GameConfig`; se puede pasar `GameSession` a un SO y usar canales de eventos SO. |
| AU 3.1 NavMesh | El paquete AI Navigation ya está instalado: NavMeshSurface en cada escena y `NavMeshAgent` en `ChaserNPC`/`PatrolRoute`. |
| AU 4.1 Profiler | Capturas del Profiler en la redada (más amenazas activas) con FPS, CPU (cono de visión hace raycasts cada frame) y memoria. |

## 7. Al terminar

- [ ] Actualizar [[Bienvenido|Memoria del Juego]] con un “Hito 03 — Pruebas y publicación”.
- [ ] `git add`, `git commit` y `git push` de los cambios.
