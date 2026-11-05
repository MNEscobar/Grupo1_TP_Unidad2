# Análisis de Depuración de Eventos y Pila de Llamadas (Call Stack)

## 1. Configuración de Breakpoints Utilizados

Para rastrear el ciclo de vida del evento `AllCollected` desde su suscripción hasta su disparo al recolectar el último ítem, se configuraron los siguientes puntos de interrupción en Visual Studio:

* **`ScoreManager.cs`**:
  * **Línea 52:** Evaluación de la condición `remaining <= 0` dentro del método `HandleCollected(Collectible)` para comprobar el momento en que se alcanza el total de coleccionables[cite: 2].
  * **Línea 53:** Invocación del evento `AllCollected?.Invoke()` para identificar el instante exacto en que se notifica a los suscriptores[cite: 3, 4, 5].
* **`GameplayHUD.cs`**:
  * **Línea 25:** Suscripción `scoreManager.AllCollected += ShowWinPanel` dentro de `OnEnable()`[cite: 6].
  * **Línea 52:** Entrada al método `ShowWinPanel()` y ejecución de `winPanel.SetActive(true)` para observar la activación de la interfaz de victoria[cite: 3].
* **`GameManager.cs`**:
  * **Línea 47:** Suscripción `scoreManager.AllCollected += HandleVictory` dentro de `OnEnable()`[cite: 7].
  * **Línea 100:** Entrada al método `HandleVictory()` y ejecución de `gameEnded = true` para registrar el cambio de estado de la partida[cite: 4].
* **`PlayerLockOnGameOver.cs`**:
  * **Línea 33:** Suscripción `scoreManager.AllCollected += LockPlayer` dentro de `OnEnable()`[cite: 1].
  * **Línea 44:** Entrada al método `LockPlayer()` y ejecución de `controller.enabled = false` para verificar el bloqueo del movimiento del jugador[cite: 5].

---

## 2. Seguimiento del Flujo en la Pila de Llamadas (Call Stack)

### A. Detección de Colisión y Condición de Victoria
Al recoger el último coleccionable en escena, el motor físico dispara `Collectible:OnTriggerEnter(Collider)` en `Collectible.cs` (línea 46), el cual invoca a `ScoreManager:HandleCollected(Collectible)`[cite: 2]. La ejecución se pausa en la **línea 52** de `ScoreManager.cs` para evaluar si quedan objetos restantes (`remaining <= 0`)[cite: 2].

> ![Comprobación de condición en ScoreManager](https://drive.google.com/uc?export=view&id=1w2KsOsE6UGoziquaKttrBqRvVTTKZrzl)

---

### B. Notificación al Primer Suscriptor: `GameplayHUD`
Al cumplirse la condición y ejecutarse `AllCollected?.Invoke()` en la **línea 53** de `ScoreManager.cs`, el avance paso a paso (**Step Into - F11**) redirige el flujo hacia el primer suscriptor registrado: el método `GameplayHUD:ShowWinPanel()` en la **línea 52** de `GameplayHUD.cs`, donde se activa el panel de victoria[cite: 3].


> ![Ejecución de ShowWinPanel en GameplayHUD](https://drive.google.com/uc?export=view&id=1PmiWDd-igZ9e9TsQ6ZTrEvChvjjH5b0_)

---

### C. Notificación al Segundo Suscriptor: `GameManager`
Una vez finalizada la tarea del HUD, el delegado continúa recorriendo su lista de invocación desde la **línea 53** de `ScoreManager.cs` y salta al segundo suscriptor: `GameManager:HandleVictory()` en la **línea 100** de `GameManager.cs`, actualizando el estado de la partida a finalizada (`gameEnded = true`)[cite: 4].

> ![Ejecución de HandleVictory en GameManager](https://drive.google.com/uc?export=view&id=1dUzeTLC5rcnLoFw08d20Xob3BEHEUMuM)

---

### D. Notificación al Tercer Suscriptor: `PlayerLockOnGameOver`
Por último, desde la misma invocación en la **línea 53** de `ScoreManager.cs`, el flujo ingresa al tercer y último suscriptor: `PlayerLockOnGameOver:LockPlayer()` en la **línea 44** de `PlayerLockOnGameOver.cs`, deshabilitando el componente controlador del jugador (`controller.enabled = false`)[cite: 5].

> ![Ejecución de LockPlayer en PlayerLockOnGameOver](https://drive.google.com/uc?export=view&id=1ev53wSQU-aVDDGBEW2OjH5ievMzZ_lPr)

---

## 3. Diagnóstico y Conclusiones del Comportamiento

### 1. ¿En qué orden se ejecutan los suscriptores?
Los suscriptores se ejecutan de manera secuencial respetando el orden FIFO (*First In, First Out*) en el que se registraron durante la fase de inicialización `OnEnable()`[cite: 1, 6, 7]:
1. **`GameplayHUD.ShowWinPanel()`** (`GameplayHUD.cs`, línea 52)[cite: 3].
2. **`GameManager.HandleVictory()`** (`GameManager.cs`, línea 100)[cite: 4].
3. **`PlayerLockOnGameOver.LockPlayer()`** (`PlayerLockOnGameOver.cs`, línea 44)[cite: 5].

En las tres capturas de los suscriptores se observa cómo la base de la pila de llamadas se mantiene intacta (`Collectible:OnTriggerEnter` $\rightarrow$ `ScoreManager:HandleCollected` en la línea 53), cambiando únicamente el marco superior activo según el listener que está recibiendo la notificación en ese instante[cite: 3, 4, 5].

### 2. ¿Algún listener se ejecuta más de una vez?
**No.** Cada uno de los tres métodos suscriptores se ejecuta exactamente una sola vez cuando `remaining <= 0` dispara el evento. Esto confirma que las suscripciones con el operador `+=` se realizaron correctamente una única vez en `OnEnable()`[cite: 1, 6, 7], sin generar duplicaciones en la lista de invocación del delegado.

### 3. ¿Alguno no responde?
**Todos responden correctamente.** Los tres componentes suscritos (`GameplayHUD`, `GameManager` y `PlayerLockOnGameOver`) reciben la llamada desde `ScoreManager:HandleCollected` y ejecutan su lógica interna esperada sin omisiones[cite: 3, 4, 5].

### 4. ¿Aparece alguna excepción o referencia nula?
**No se registra ninguna excepción ni referencia nula (`NullReferenceException`).** El uso del operador condicional nulo en `AllCollected?.Invoke()` asegura una llamada segura desde el emisor, todas las referencias internas de los receptores están correctamente asignadas en el Inspector y el depurador de Visual Studio reporta el estado *"No se encontraron problemas"* a lo largo de toda la cadena de ejecución[cite: 2, 3, 4, 5].
