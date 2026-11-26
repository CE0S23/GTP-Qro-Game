---
title: "Análisis de Optimización (AU 4.1)"
fecha: 2026-11-25
responsable: César Osvaldo Ramírez Pérez
equipo_de_prueba: "Laptop · Intel UHD Graphics (iGPU, 4 GB compartidos) · Direct3D 11 · Windows 11"
compilacion: "Windows 64 bits, Development Build, Unity 6000.4.0f1"
tags: [GTP, profiler, optimizacion, AU]
---

# Análisis de Optimización (AU 4.1)

Volver a [[Bienvenido|Memoria del Juego]] · [[Hito 03 - Game feel, IA y optimizacion]] · [[Plan de desarrollo - Evaluacion II]]

## 1. Resumen

| Métrica (1600×900, misma máquina) | Antes | Después |
| --- | --- | --- |
| FPS promedio en la **redada** (3 policías + foco de helicóptero) | 43.5 | **91.9** |
| FPS mínimo en la redada (muestras de 0.5 s) | 35.9 | **75.1** |
| FPS promedio en la **prueba de estrés** (+20 policías extra, 23 amenazas con raycasts) | 41.1 | **86.9** |
| Tiempo del hilo principal por frame (redada) | 23.2 ms | **10.8 ms** |
| Memoria gráfica (Gfx Used Memory) | 76.7 MB | **45.9 MB** |
| Total Used Memory tras 6 recargas de Barrio ↔ Ruta del Mal | 182.8 MB (plana) | **151.7 MB (plana)** |
| Basura generada (GC Allocated In Frame) en juego | ~0.1 KB/frame | ~0.1 KB/frame |
| SetPass calls (redada) | 47 | 37 |
| Triángulos (estrés) | 82 849 | 45 233 |

> Durante la prueba con 23 amenazas en pantalla, el Profiler mostró que el juego estaba **limitado por la GPU integrada**, no por los scripts: el hilo principal pasaba ~23 ms por frame esperando a la tarjeta, y a 800×450 el mismo recorrido corría a ~130 FPS. Se bajó el costo de las sombras (de 4 cascadas a 2, distancia de 50 m a 35 m, mapa de 2048 a 1024, sombras suaves en calidad baja) y se apagó el SSAO del renderizador PC. Resultado: de ~42 a ~90 FPS estables, sin fugas de memoria en 2.5 minutos y 6 recargas de escena, y prácticamente cero basura para el recolector (GC).

## 2. Cómo se midió

La rúbrica pide medir durante el juego en momentos de mucha carga. Para que la medición se pueda repetir igual antes y después, se hizo una **sonda automática** (`Assets/Scripts/Core/PerfProbe.cs`) que solo se activa en la compilación de desarrollo con el argumento `-gtpPerf`:

1. **Barrio** (20 s): el jugador recorre la calle completa.
2. **Redada** (30 s): Ruta del Mal con la redada activa (3 policías con cono de visión + foco de helicóptero).
3. **Estrés** (25 s): la misma redada con **20 policías extra** (23 amenazas haciendo raycasts de línea de visión cada frame).
4. **Recargas** (6 ciclos): Barrio → Ruta del Mal → Barrio… para detectar fugas de memoria al cambiar de escena.

Cada 0.5 s se guardan contadores de `ProfilerRecorder` (los mismos que muestra la ventana Profiler): `Main Thread`, `GC Allocated In Frame`, `Total Used Memory`, `GC Used Memory`, `Gfx Used Memory`, `SetPass Calls Count` y `Triangles Count`. El jugador no pierde intentos durante la sonda (solo mide rendimiento). Resolución 1600×900 en ventana, sin VSync y sin límite de FPS, para ver el costo real.

```text
"GTP QroCITY stories.exe" -gtpPerf -gtpPerfOut C:\Medicion -gtpPerfNoLog -gtpPerfCycles 6
```

- Sin `-gtpPerfNoLog` también escribe `perf_gtp.raw`, que se abre en **Window → Analysis → Profiler → Load** para ver el Timeline y la jerarquía de CPU.
- Datos crudos en `Adjuntos/datos_profiler/` (`perf_antes.csv`, `perf_despues.csv`, `perf_con_registro_raw.csv`). Las gráficas se generan con `Game/GTP Qro/Tools/perf_charts.py`.

## 3. Estabilidad de FPS (CPU Usage)

![[Adjuntos/AU41_fps_antes_despues.png]]

- **Antes:** ~42 FPS en todas las fases, **aunque se agregaran 20 policías**. Que el número casi no cambie con 7 veces más amenazas fue la primera pista: el cuello de botella no eran los scripts.
- **Diagnóstico:** se repitió la prueba a **800×450** (¼ de los píxeles) y subió a **~130 FPS** con el hilo principal en ~8 ms. Si fuera CPU, bajar la resolución no habría cambiado casi nada. Por lo tanto el juego estaba **limitado por la GPU** (Intel UHD integrada) y el hilo principal solo esperaba a que terminara el frame.
- **Qué consumía la GPU** (configuración `PC_RPAsset` / `PC_Renderer`): sombras de la luz principal con 4 cascadas a 50 m y mapa de 2048 px con filtro suave de calidad alta, más **SSAO** (oclusión ambiental en pantalla, que se calcula por píxel). En una cámara cenital a ~17 m del jugador, 4 cascadas y 50 m de distancia no aportan nada visible.
- **Cambios** (`Assets/Settings/PC_RPAsset.asset`, `PC_Renderer.asset`):

| Ajuste | Antes | Después |
| --- | --- | --- |
| Cascadas de sombra | 4 | 2 |
| Distancia de sombras | 50 m | 35 m |
| Resolución del mapa de sombras | 2048 | 1024 |
| Calidad de sombras suaves | Alta | Baja |
| SSAO (Renderer Feature) | Activo | Desactivado |

- **Después:** ~90 FPS promedio y mínimo de 67–75 FPS en las fases de más carga: por encima de 60 FPS todo el tiempo de juego. Los picos aislados de la línea (bajadas a ~70–80) coinciden con **cargas de escena**, no con el gameplay.

![[Adjuntos/AU41_cpu.png]]

**Qué scripts consumen CPU.** Con 23 amenazas, el tiempo del hilo principal solo sube de 10.8 ms a 11.5 ms (+0.7 ms, ~0.03 ms por policía). Lo más caro por amenaza es `PoliceVisionCone` (un raycast de línea de visión y la reconstrucción de la malla del cono cada frame) y `ThreatDetector.Update`. La navegación ya está acotada: `NavSteering` recalcula la ruta cada 0.25–0.5 s en lugar de cada frame. Con este margen no hizo falta pasar los conos a una corrutina; si se agregaran muchos más enemigos, ese sería el siguiente paso (actualizar el cono cada 0.1 s).

## 4. Gestión de memoria (Memory)

![[Adjuntos/AU41_memoria.png]]

- **Sin fugas.** Con la sonda sin registro binario, `Total Used Memory` se queda en **151.6–152.8 MB** durante toda la prueba, incluidas 6 recargas de escena (antes de optimizar: 182.8 MB, también plana). Si hubiera una fuga, la línea subiría en cada recarga.
- **Falsa alarma encontrada:** en la primera medición la memoria subía de 229 MB a 432 MB. Se repitió sin el registro `.raw` y quedó plana: **el crecimiento era el búfer del propio Profiler** escribiendo el archivo (600 MB en 2 minutos), no el juego. Por eso las mediciones de memoria se hacen con `-gtpPerfNoLog`.
- **Memoria gráfica:** de 76.7 MB a 45.9 MB, por el mapa de sombras más pequeño y sin las texturas del SSAO.
- **Garbage Collector:** ~0.1 KB por frame en juego (solo las cadenas de la propia sonda). La causa está en el diseño: las partículas son **un ParticleSystem por efecto que se reutiliza** con `Emit()` (no se instancian ni destruyen objetos), las rutas del NavMesh reutilizan el mismo arreglo (`GetCornersNonAlloc`) y los canales de eventos no generan basura.
- **Limpieza preventiva:** `RendererColor` dejó de crear copias de materiales (`renderer.material`) y ahora usa `MaterialPropertyBlock`; la copia del perfil de post-proceso y la malla del cono se destruyen con la escena.

## 5. Renderizado (Rendering / Batches)

- **SetPass calls:** 47 → 37 en la redada; 44 → 34 en el Barrio. Es un número bajo para una escena 3D: todo el escenario usa 28 materiales compartidos (`Assets/Materials/Greybox`) y el SRP Batcher está activo en `PC_RPAsset`.
- **Triángulos:** 10 000–17 000 en juego normal; 45 000 en la prueba de estrés (cada policía es una cápsula con su cono). Bajaron al reducir las cascadas de sombra, porque cada cascada vuelve a dibujar la geometría.
- El contador `Batches Count` sale en 0 en la compilación de Windows (el contador no se publica fuera del editor en esta versión). Para ver los batches exactos usar el **Frame Debugger** o la pestaña *Rendering* del Profiler conectado al editor (§6).

## 6. Pendiente para el portafolio (capturas de la ventana del Profiler)

La rúbrica pide **capturas de pantalla del Profiler**. Los datos de este análisis son los mismos contadores, pero falta la imagen de la ventana:

1. *GTP > Compilar Windows (desarrollo + Profiler)*.
2. Ejecutar `Builds/Windows_Dev/GTP QroCITY stories.exe -gtpPerf -gtpPerfOut C:\Medicion` (con registro).
3. En el editor: *Window → Analysis → Profiler → Load* → `C:\Medicion\perf_gtp.raw`.
4. Capturar: CPU Usage durante la redada y el estrés (vista *Timeline*), Memory (*Total Used Memory*) y Rendering.
5. Pegarlas aquí debajo.

## 7. Conclusión

Durante la prueba de estrés con 23 amenazas en pantalla, el Profiler mostró que el juego estaba limitado por la GPU integrada (~42 FPS, 23 ms por frame) y que el costo de los scripts era menor a 1 ms. Se redujeron las cascadas, la distancia y la resolución de las sombras y se desactivó el SSAO. El juego quedó en **~90 FPS promedio (mínimo 67 FPS en la prueba de estrés)**, con **152 MB de memoria total sin crecer en 6 recargas de escena** y sin basura para el recolector durante el juego.
