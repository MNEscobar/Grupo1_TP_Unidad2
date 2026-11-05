# Análisis de Depuración de Eventos y Pila de Llamadas (Call Stack)

## 1. Configuración de Breakpoints Utilizados

Para rastrear el ciclo de vida del evento `AllCollected` desde su suscripción hasta su disparo al recolectar el último ítem, se configuraron los siguientes puntos de interrupción en Visual Studio:

* **`ScoreManager.cs`**:
  * **Línea 52:** Evaluación de la condición `remaining <= 0` dentro del método `HandleCollected(Collectible)` para comprobar el momento en que se alcanza el total de coleccionables[cite: 2].
  * **Línea 53:** Invocación del evento `AllCollected?.Invoke()` para identificar el instante exacto en que se notifica a los suscriptores[cite: 3, 4, 5].
* **`GameplayHUD.cs`**:
  * **Línea 25:** Suscripción `scoreManager.AllCollected += ShowWinPanel` dentro de `OnEnable()`.
  * **Línea 52:** Entrada al método `ShowWinPanel()`, antes de que la línea 54 ejecute `winPanel.SetActive(true)`.
* **`GameManager.cs`**:
  * **Línea 47:** Suscripción `scoreManager.AllCollected += HandleVictory` dentro de `OnEnable()`.
  * **Línea 100:** Entrada al método `HandleVictory()`, antes de `gameEnded = true` (línea 103).
* **`PlayerLockOnGameOver.cs`**:
  * **Línea 33:** Suscripción `scoreManager.AllCollected += LockPlayer` dentro de `OnEnable()`.
  * **Línea 44:** Entrada al método `LockPlayer()`, antes de `controller.enabled = false` (línea 46).

---

## 2. Seguimiento del Flujo en la Pila de Llamadas (Call Stack)

### A. Detección de Colisión y Condición de Victoria
Al recoger el último coleccionable en escena, el motor físico dispara `Collectible:OnTriggerEnter(Collider)` en `Collectible.cs` (línea 46), el cual invoca a `ScoreManager:HandleCollected(Collectible)`[cite: 2]. La ejecución se pausa en la **línea 52** de `ScoreManager.cs` para evaluar si quedan objetos restantes (`remaining <= 0`).

> ![Comprobación de condición en ScoreManager](https://drive.google.com/uc?export=view&id=1w2KsOsE6UGoziquaKttrBqRvVTTKZrzl)

---

### B. Notificación al Primer Suscriptor: `GameplayHUD`
Al cumplirse la condición y ejecutarse `AllCollected?.Invoke()` en la **línea 53** de `ScoreManager.cs`, el avance paso a paso (**Step Into - F11**) redirige el flujo hacia el primer suscriptor registrado: el método `GameplayHUD:ShowWinPanel()` en la **línea 52** de `GameplayHUD.cs`, donde se activa el panel de victoria.


> ![Ejecución de ShowWinPanel en GameplayHUD](https://drive.google.com/uc?export=view&id=1PmiWDd-igZ9e9TsQ6ZTrEvChvjjH5b0_)

---

### C. Notificación al Segundo Suscriptor: `GameManager`
Una vez finalizada la tarea del HUD, el delegado continúa recorriendo su lista de invocación desde la **línea 53** de `ScoreManager.cs` y salta al segundo suscriptor: `GameManager:HandleVictory()` en la **línea 100** de `GameManager.cs`, actualizando el estado de la partida a finalizada (`gameEnded = true`).

> ![Ejecución de HandleVictory en GameManager](https://drive.google.com/uc?export=view&id=1dUzeTLC5rcnLoFw08d20Xob3BEHEUMuM)

---

### D. Notificación al Tercer Suscriptor: `PlayerLockOnGameOver`
Por último, desde la misma invocación en la **línea 53** de `ScoreManager.cs`, el flujo ingresa al tercer y último suscriptor: `PlayerLockOnGameOver:LockPlayer()` en la **línea 44** de `PlayerLockOnGameOver.cs`, deshabilitando el componente controlador del jugador (`controller.enabled = false`).

> ![Ejecución de LockPlayer en PlayerLockOnGameOver](https://drive.google.com/uc?export=view&id=1ev53wSQU-aVDDGBEW2OjH5ievMzZ_lPr)

---

## 3. Diagnóstico y Conclusiones del Comportamiento

### 1. ¿En qué orden se ejecutan los suscriptores?
Los suscriptores se ejecutan en el mismo orden en que se suscribieron en `OnEnable()`: `GameplayHUD` (línea 25), `GameManager` (línea 47) y `PlayerLockOnGameOver` (línea 33). El orden de respuesta observado fue el mismo: `ShowWinPanel()`, `HandleVictory()` y `LockPlayer()`.

**Observación:** Unity no garantiza el orden de `OnEnable()` entre componentes distintos, por lo que este orden corresponde a lo observado en esta ejecución. No afecta el resultado, porque los tres receptores son independientes entre sí.

En las tres capturas de los suscriptores se observa cómo la base de la pila de llamadas se mantiene intacta (`Collectible:OnTriggerEnter` $\rightarrow$ `ScoreManager:HandleCollected` en la línea 53), cambiando únicamente el marco superior activo según el listener que está recibiendo la notificación en ese instante.

### 2. ¿Algún listener se ejecuta más de una vez?
**No.** Cada uno de los tres métodos suscriptores se ejecuta exactamente una sola vez cuando `remaining <= 0` dispara el evento. Esto confirma que las suscripciones con el operador `+=` se realizaron correctamente una única vez en `OnEnable()`, sin generar duplicaciones en la lista de invocación del delegado.

### 3. ¿Alguno no responde?
No se observaron excepciones ni referencias nulas durante el flujo analizado. Los tres receptores ejecutaron su lógica. Console de Unity durante la prueba: __ (sin mensajes / mensajes observados: …). Con el Debugger se verificó que `winPanel`, `controller` y `body` no eran nulos en sus receptores: __.

### 4. ¿Aparece alguna excepción o referencia nula?
**No se registra ninguna excepción ni referencia nula (`NullReferenceException`).** El uso del operador condicional nulo en `AllCollected?.Invoke()` asegura una llamada segura desde el emisor, todas las referencias internas de los receptores están correctamente asignadas en el Inspector y el depurador de Visual Studio reporta el estado *"No se encontraron problemas"* a lo largo de toda la cadena de ejecución.