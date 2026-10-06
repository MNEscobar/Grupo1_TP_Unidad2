# Grupo1_DesarrolloDeVidejuegos_II

Proyecto desarrollado para la la materia **Diseño y Desarrollo de Videojuegos 2**.

## Integrantes

* **Matias Escobar**
* **Luca Depetris**

## Descripción

El proyecto implementa la estructura de navegación de un juego desarrollado en **Unity**, incluyendo una Splashscreen, un menú principal, una pantalla de opciones, una sección de créditos y una escena de Gameplay jugable.
 
En el Gameplay, el jugador se mueve por el escenario y recoge objetos coleccionables que suman puntos. Al juntarlos todos aparece una pantalla de victoria con las opciones de reiniciar la partida o volver al menú.
 
El proyecto utiliza una arquitectura basada en **escenas independientes**, junto con un sistema de navegación centralizado mediante `UIManager`. También se aplican criterios de reutilización de componentes mediante **prefabs y variantes**, y técnicas de diseño de UI responsiva utilizando `Canvas Scaler`, anchors y `Layout Groups`, con soporte para resoluciones horizontales (16:9) y verticales (9:16).
 
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

## Gameplay
 
* **Movimiento:** el jugador se mueve sobre el plano XZ mediante un `Rigidbody`.
* **Controles:** `W` `A` `S` `D` o flechas del teclado.
* **Cámara:** sigue al jugador, por lo que el escenario es recorrible en cualquier relación de aspecto.
* **Puntaje:** cada coleccionable suma puntos al contador del HUD. Hay coleccionables normales (1 punto) y dorados (variante de prefab).
* **Final de partida:** al recoger todos los coleccionables se muestra el panel de victoria y se bloquea el movimiento del jugador.

## Herramientas de Editor
 
* **Validar UI** (`Tools > Grupo1 > Validar UI`): recorre las escenas del Build Profile, verifica la configuración del `Canvas Scaler` y detecta elementos de la interfaz que se salen de su contenedor en 16:9 y 9:16.

## Documentación

Toda la documentación correspondiente al Proyecto se encuentra en [`Docs`](Docs).

* [Informe de la Tarea N.º 1](Docs/Informe.md)
* [Diagramas de navegación](Docs/Diagramas.md)
* [Capturas del proyecto](Docs/Capturas.md)
* [Gameplay](Docs/Gameplay.md)
* [Diseño Responsivo](Docs/ui_responsive.md)

## Estructura del repositorio

```text
Grupo1_TP_Unidad2/
├── Assets/
│    └── _Project/
│        ├── Materials/
│        ├── Prefabs/
│        │   ├── Gameplay/
│        │   │   ├── Collectible.prefab
│        │   │   ├── Player.prefab
│        │   │   └── Variants/
│        │   │       └── Collectible_Gold.prefab
│        │   └── UI/
│        │       ├── Button_Base.prefab
│        │       └── Variants/
│        │           ├── Button_Back.prefab
│        │           ├── Button_Cancel.prefab
│        │           ├── Button_Credits.prefab
│        │           ├── Button_Options.prefab
│        │           ├── Button_Play.prefab
│        │           └── Button_Quit.prefab
│        ├── Scenes/
│        │   ├── Bootstrap.unity
│        │   ├── Credits.unity
│        │   ├── Gameplay.unity
│        │   ├── MainMenu.unity
│        │   └── Options.unity
│        └── Scripts/
│            ├── Editor/
│            ├── Gameplay/
│            └── UI/
├── Docs/
│    ├── Informe.md
│    ├── Diagramas.md
│    └── Capturas.md
│    └── Gameplay.md
│    └── ui_responsive.md
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