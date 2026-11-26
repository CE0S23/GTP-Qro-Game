"""Gráficas del Análisis de Optimización (AU 4.1) a partir de los CSV de PerfProbe.

Uso:
    python perf_charts.py <antes.csv> <despues.csv> <con_registro.csv> <carpeta_salida>

- antes.csv / despues.csv: corridas con -gtpPerfNoLog antes y después de optimizar.
- con_registro.csv: corrida con el registro binario del Profiler activo (para mostrar su costo en memoria).
"""
import csv
import sys

import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt  # noqa: E402

SURFACE = "#fcfcfb"
TEXT = "#0b0b0b"
TEXT2 = "#52514e"
GRID = "#e4e3df"
BLUE = "#2a78d6"    # serie 1
ORANGE = "#eb6834"  # serie 2


def load(path):
    rows = list(csv.DictReader(open(path, encoding="utf-8")))
    # Se descarta la primera muestra (carga del menú, sin datos de juego).
    return [r for r in rows if float(r["total_used_mb"]) > 0]


def style(ax, title, ylabel):
    ax.set_facecolor(SURFACE)
    ax.figure.set_facecolor(SURFACE)
    ax.set_title(title, loc="left", color=TEXT, fontsize=13, fontweight="bold", pad=12)
    ax.set_ylabel(ylabel, color=TEXT2)
    ax.set_xlabel("Tiempo de la prueba (s)", color=TEXT2)
    ax.grid(axis="y", color=GRID, linewidth=0.8)
    ax.tick_params(colors=TEXT2, length=0)
    for side in ("top", "right", "left"):
        ax.spines[side].set_visible(False)
    ax.spines["bottom"].set_color(GRID)


def phase_bands(ax, rows, ymax):
    """Sombrea las fases de la prueba (redada, estrés) para dar contexto."""
    spans = {}
    for r in rows:
        t = float(r["t_s"])
        f = r["fase"]
        if f.startswith("recarga"):
            f = "recargas"
        a, b = spans.get(f, (t, t))
        spans[f] = (min(a, t), max(b, t))
    labels = {"barrio": "Barrio", "redada": "Redada", "estres_20_policias": "Estrés +20 policías", "recargas": "Recargas de escena"}
    for i, (f, (a, b)) in enumerate(spans.items()):
        if i % 2 == 1:
            ax.axvspan(a, b, color="#f0efec", zorder=0)
        ax.text((a + b) / 2, ymax * 0.98, labels.get(f, f), ha="center", va="top", fontsize=8, color=TEXT2)


def line(ax, rows, key, color, label):
    xs = [float(r["t_s"]) for r in rows]
    ys = [float(r[key]) for r in rows]
    ax.plot(xs, ys, color=color, linewidth=2, label=label, solid_capstyle="round")
    ax.annotate(label, (xs[-1], ys[-1]), xytext=(6, 0), textcoords="offset points", va="center", fontsize=9, color=TEXT)
    return ys


def main(before_path, after_path, logged_path, out_dir):
    before, after, logged = load(before_path), load(after_path), load(logged_path)

    fig, ax = plt.subplots(figsize=(10, 4.2), dpi=130)
    ymax = max(max(float(r["fps"]) for r in after), 60) * 1.15
    phase_bands(ax, after, ymax)
    line(ax, before, "fps", ORANGE, "Antes")
    line(ax, after, "fps", BLUE, "Después")
    ax.axhline(60, color=TEXT2, linewidth=1, linestyle=(0, (4, 3)))
    ax.text(1, 61, "60 FPS", color=TEXT2, fontsize=8)
    ax.set_ylim(0, ymax)
    style(ax, "FPS durante la prueba (1600×900, Intel UHD)", "FPS")
    ax.legend(loc="lower right", frameon=False, labelcolor=TEXT)
    fig.tight_layout()
    fig.savefig(out_dir + "/AU41_fps_antes_despues.png", facecolor=SURFACE)

    fig, ax = plt.subplots(figsize=(10, 4.2), dpi=130)
    ymax = max(float(r["total_used_mb"]) for r in logged) * 1.2
    phase_bands(ax, after, ymax)
    line(ax, logged, "total_used_mb", ORANGE, "Con registro .raw")
    line(ax, after, "total_used_mb", BLUE, "Sin registro")
    ax.set_ylim(0, ymax)
    style(ax, "Total Used Memory: plana sin el registro del Profiler (sin fugas)", "MB")
    ax.legend(loc="lower right", frameon=False, labelcolor=TEXT)
    fig.tight_layout()
    fig.savefig(out_dir + "/AU41_memoria.png", facecolor=SURFACE)

    fig, ax = plt.subplots(figsize=(10, 4.2), dpi=130)
    ymax = max(float(r["cpu_main_ms"]) for r in before) * 1.3
    phase_bands(ax, after, ymax)
    line(ax, before, "cpu_main_ms", ORANGE, "Antes")
    line(ax, after, "cpu_main_ms", BLUE, "Después")
    ax.axhline(16.7, color=TEXT2, linewidth=1, linestyle=(0, (4, 3)))
    ax.text(1, 17.2, "16.7 ms = 60 FPS", color=TEXT2, fontsize=8)
    ax.set_ylim(0, ymax)
    style(ax, "Tiempo del hilo principal por frame", "ms")
    ax.legend(loc="upper right", frameon=False, labelcolor=TEXT)
    fig.tight_layout()
    fig.savefig(out_dir + "/AU41_cpu.png", facecolor=SURFACE)


if __name__ == "__main__":
    main(*sys.argv[1:5])
