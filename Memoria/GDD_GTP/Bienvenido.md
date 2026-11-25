---
title: "Memoria del Juego — GTP QroCITY stories"
aliases: [Memoria del Juego]
desarrolladores:
  - César Osvaldo Ramírez Pérez
  - Orlando Rubio Cabrera
  - Mauricio Gabriel Vazquez
institucion: Universidad Tecnológica de Querétaro (UTEQ)
materia: Creación de Videojuegos
estado: "Beta jugable (Parte 1 terminada) — pendiente Parte 2: pruebas y entrega"
fecha: 2026-11-25
motor: Unity 6000.4.0f1 / URP 17.4.0
lenguaje: C#
tags: [GTP, memoria, unity, prototipo, clean-code]
---

# Memoria del Juego

> **Estado al 2026-11-25:** la Parte 1 (desarrollo del beta, César) está terminada y en el repositorio <https://github.com/CE0S23/GTP-Qro-Game>. La Parte 2 (pruebas, manual, empaque y publicación, Orlando) está pendiente.
>
> - Plan y rúbrica: [[Plan de desarrollo - Evaluacion II]]
> - Qué se construyó: [[Hito 02 - Beta jugable]]
> - Qué falta y cómo hacerlo: [[Parte 2 - Orlando (pendientes)]]
> - Versión de Unity obligatoria para ambos: **6000.4.0f1**.

## Core Concept

**GTP QroCITY stories** es una aventura narrativa y de sigilo 2.5D con cámara cenital. Un joven recorre una ciudad hostil durante una noche, evade amenazas y toma una decisión entre buscar apoyo en sus seres queridos o continuar buscando sustancias como una falsa salida a sus problemas. Las rutas convergen en el Spot al amanecer, con consecuencias diferentes. La batería limitada del celular está planteada como fuente de tensión.

El ciclo previsto es **explorar → evadir → decidir → experimentar consecuencias**. La meta del GDD es una demo de aproximadamente 15 minutos; este primer avance se limita a comprobar el desplazamiento y el encuadre con primitivas.

## Fuentes y decisiones de diseño

- [GDD principal](../../Archivos%20e%20Imagenes/GDD_GTP_QroCITY_stories%20%281%29.docx): secciones 3, 5, 7 y 9; fuente principal para autores completos, concepto, controles direccionales y arquitectura modular.
- [GDD de impacto social, versión 3.0](../../Archivos%20e%20Imagenes/GDD_GTP_QroCITY_Social_Impact%20%282%29.docx): confirma las dos rutas y el propósito social.
- [Borrador inicial](../../Archivos%20e%20Imagenes/GDD_Borrador_Videojuego_Unity%20V2%20%282%29.pdf): secciones 3–5; propone componentes independientes y empezar por físicas, movimiento y cámara. Su desenlace lineal se considera antecedente frente a las dos rutas de los documentos posteriores.
- [Referencias visuales](../../Archivos%20e%20Imagenes/Notes_260906_174729%20%283%29.pdf): dos páginas de personaje, indumentaria y ciclos de animación. Se revisaron visualmente; el arte se reserva para otra etapa.

El GDD define vista cenital y animaciones arriba/abajo/izquierda/derecha, pero no fija teclas, velocidad ni API física. Para esta entrega se decide usar **WASD o flechas**, velocidad inicial de **4 unidades/segundo**, plano **X/Z** y altura fija **Y = 1**. Los cuerpos y colisiones son 3D; “2.5D” describe la presentación. La cámara será fija, en perspectiva e inclinada hacia el jugador al inicio.

## Separación de responsabilidades

> Esta sección describe el estado del Hito 01. La arquitectura actual está en [[Hito 02 - Beta jugable#Arquitectura (sin “God Object”)]].

Scripts de juego que se crean en este avance:

| Script | Responsabilidad actual | Límite y evolución prevista |
| --- | --- | --- |
| `PlayerMovement.cs` | Leer dirección en `Update` y aplicar velocidad al `Rigidbody` en `FixedUpdate`. | No administra batería, escenas, UI, narrativa ni cámara. |
| `GameManager.cs` | Mantener una única instancia persistente, eliminando duplicados. | Solo estructura; los `TODO` describen futura coordinación del estado de batería y carga de niveles. |

El GDD propone además `PhoneUIController` (batería/mapa), `MinigameManager` (transiciones a minijuegos) y `BranchingManager` (rutas, obstáculos, escenas y diálogos). **Son planificación; todavía no están implementados.** `GameManager` se añade por el alcance de esta práctica y coordinará estos componentes, sin absorber sus responsabilidades. La lógica de consumo/presentación del celular deberá permanecer en su componente específico, y las decisiones narrativas en `BranchingManager`.

## Hito 01 — Greybox de movimiento

Fecha real de trabajo: **8 de octubre de 2026**. Este registro distingue trabajo realizado, validación y pendientes; no representa sesiones previas de desarrollo.

Proyecto: `Game/GTP Qro`.

```text
Assets/
├── _Scenes/Demo_Movimiento_01.unity
├── Scripts/
│   ├── PlayerMovement.cs
│   └── GameManager.cs
├── Prefabs/
└── Materials/
    ├── Suelo_Gris.mat
    └── Jugador_Azul.mat
```

`Prefabs` queda preparada para futuros objetos reutilizables. Se conserva la escena original `SampleScene`.

### Montaje creado

- Plane `Suelo`: posición `(0, 0, 0)`, escala `(2, 1, 2)`; superficie de 20 × 20 unidades, collider y material URP gris oscuro.
- Capsule `Jugador`: posición `(0, 1, 0)`, collider y material azul sólido. `Rigidbody` dinámico con interpolación, gravedad desactivada, altura y rotación bloqueadas para desplazamiento plano sin vuelcos.
- `Main Camera`: posición `(0, 14, -10)`, perspectiva y orientación hacia el centro del jugador. Con este encuadre, X corresponde a izquierda/derecha y Z a arriba/abajo.
- Una luz direccional y un objeto raíz `GameManager`.

### Controles y límites de la prueba

1. Abrir `Assets/_Scenes/Demo_Movimiento_01.unity` y pulsar **Play**.
2. Hacer clic en la vista **Game**; mover con WASD o flechas.
3. Comprobar las cuatro direcciones y parada al soltar las teclas. Las teclas opuestas se cancelan; si se pulsa horizontal y vertical a la vez, se prioriza horizontal para mantener exactamente cuatro direcciones.
4. Confirmar que la cápsula mantiene altura y no vuelca. Detener **Play** para volver al origen.

El proyecto ya utiliza el nuevo **Input System 1.19.0**. El script se adapta a esa configuración sin cambiar al sistema de entrada antiguo. La cámara no sigue al personaje y el suelo no tiene límites de nivel: alejarse lo suficiente puede sacar la cápsula del encuadre. No hay salto, carrera, sigilo, enemigos, texturas, batería funcional, narrativa, UI ni carga real de niveles.

### Validación

- **Compilación correcta en Unity 6000.4.0f1**, usando una copia temporal de `Assets`, `Packages` y `ProjectSettings` para conservar la sesión abierta del proyecto original.
- Escena generada con la API del editor, guardada y reabierta: sin scripts faltantes, colliders presentes y jugador centrado en la cámara.
- **12 casos automatizados de movimiento** con eventos de teclado del Input System y simulación física de 50 pasos de 0.02 segundos por caso: WASD, flechas, reposo, teclas opuestas y prioridad horizontal. Los desplazamientos de un segundo corresponden a 4 unidades cuando hay entrada.
- Comprobadas altura fija, rotación estable y parada al desactivar el componente.
- Revisada una captura renderizada por Unity: suelo gris oscuro, cápsula azul, iluminación y encuadre correctos.
- Incorporadas escena y materiales al proyecto original; verificados los GUID de sus dos scripts para conservar las referencias generadas por la sesión abierta.

Las pruebas de movimiento invocaron los métodos reales del componente en **Edit Mode** con `Physics.Simulate`; no equivalen a una sesión manual de juego. Se abrió después `Demo_Movimiento_01` en la sesión original de Unity y se comprobó su entrada en **Play**: suelo gris y cápsula azul visibles, consola sin errores y objeto persistente `DontDestroyOnLoad` presente. La sesión había conservado abierta `SampleScene`, lo que explicaba que Play mostrara una escena vacía. Quedan pendientes la prueba manual sostenida de movimiento y la comprobación del Singleton durante cambios reales de escena. La batería y la carga de niveles siguen siendo `TODO`.

![[Adjuntos/Demo_Movimiento_01.png]]

Evidencia técnica local: `tmp/unity-validation/result.txt`, `unity-isolated.log` y `capture.log`, en la raíz de trabajo. La herramienta de generación fue temporal; el juego conserva únicamente los dos scripts nuevos solicitados.

## Hito 02 — Beta jugable

Ver [[Hito 02 - Beta jugable]] (25 de noviembre de 2026). Recorrido completo Menú → Cuarto → Barrio → Encrucijada → Ruta del Bien o del Mal → Spot, con puntaje, intentos, batería, sigilo, decisión, minijuegos y dos finales.

## Próximos hitos (pendientes)

Los pendientes del Hito 01 (cono de visión, componente del celular, decisión narrativa) se cumplieron en el Hito 02. Lo que sigue está en [[Parte 2 - Orlando (pendientes)]]:

- Pruebas manuales, duración real ≥ 5 min y reporte de pruebas (≥ 5 errores por severidad).
- Manual de usuario, diseño de empaque y publicación en itch.io.
- Opcional: Cinemachine, post-processing, NavMesh y Profiler (niveles DE/AU).

## Referencia técnica

- [Rigidbody en Unity 6](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/rigidbody): actualización de físicas mediante `FixedUpdate`.
- [Keyboard en Input System 1.19](https://docs.unity.cn/Packages/com.unity.inputsystem%401.19/api/UnityEngine.InputSystem.Keyboard.html): lectura del teclado activo.
