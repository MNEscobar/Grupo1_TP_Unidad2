# Grupo1_TP_Unidad2

Proyecto desarrollado para la **Tarea N.º 1 de la Unidad 2 — Diseño de flujo de pantallas inicial (menúes y Splashscreen)** de la materia **Desarrollo de Videojuegos**.

## Integrantes

* **Matias Escobar**
* **Luca Depetris**
* **Lucas Fischer**

## Descripción

El objetivo de la tarea es diseñar e implementar la estructura inicial de navegación de un proyecto desarrollado en **Unity**, incluyendo una Splashscreen, un menú principal, una pantalla de opciones, una sección de créditos y una escena de Gameplay navegable.

El proyecto utiliza una arquitectura basada en **escenas independientes**, junto con un sistema de navegación centralizado mediante `UIManager`. También se aplican criterios de reutilización de componentes mediante prefabs y técnicas de diseño de UI responsiva utilizando `Canvas Scaler`, anchors y `Layout Groups`.

Para el desarrollo colaborativo se utiliza **GitHub**, mediante ramas, Issues, Pull Requests y una rama `develop` destinada a la integración de los cambios.

## Tecnología

* **Unity:** 6.3.17f
* **Lenguaje:** C#
* **Interfaz:** Unity UI / TextMeshPro
* **Control de versiones:** Git / GitHub

## Estructura de navegación

Las principales escenas del proyecto son:

* `Bootstrap` — Splashscreen inicial.
* `MainMenu` — Menú principal.
* `Options` — Pantalla de opciones.
* `Credits` — Créditos del proyecto.
* `Gameplay` — Escena de juego 3D.

## Documentación

Toda la documentación correspondiente a la Tarea N.º 1 se encuentra en [`Docs`](Docs).

* [Informe de la Tarea N.º 1](Docs/Informe.md)
* [Diagramas de navegación](Docs/Diagramas.md)
* [Capturas del proyecto](Docs/Capturas.md)

## Estructura del repositorio

```text
Grupo1_TP_Unidad2/
├── Assets/
│   └── _Project/
│       ├── Prefabs/
│       ├── Scenes/
│       └── Scripts/
├── Docs/
│    ├── Informe/
│    ├── Diagramas/
│    └── Capturas/
├── .gitignore
└── README.md
```

## Flujo de trabajo

El desarrollo colaborativo se organiza mediante tres niveles principales de ramas:

```text
main
  ↓
develop
  ↓
feature/*
```

* `main`: versión estable del proyecto.
* `develop`: rama de integración.
* `feature/*`: ramas utilizadas para desarrollar tareas específicas.

Los cambios se incorporan mediante **Pull Requests**, permitiendo mantener organizada la integración del trabajo de los integrantes.
