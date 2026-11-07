# Informe de depuración del evento `AllCollected`

**Evento:** `ScoreManager.AllCollected` se emite en `HandleCollected` (línea 53) cuando `remaining <= 0`, al recoger el último coleccionable.

**Scripts involucrados:** `Collectible` detecta el contacto, `ScoreManager` emite el evento, y `GameplayHUD`, `GameManager` y `PlayerLockOnGameOver` lo reciben.

**Método:** se usaron breakpoints, Step Into (F11) y el Call Stack de Visual Studio conectado al Editor de Unity.

**Orden observado:**

1. `Collectible.OnTriggerEnter` dispara el evento `Collected`.
2. `ScoreManager.HandleCollected` evalúa la condición y emite `AllCollected`.
3. `GameplayHUD.ShowWinPanel` activa el panel de victoria.
4. `GameManager.HandleVictory` marca la partida como finalizada.
5. `PlayerLockOnGameOver.LockPlayer` bloquea al jugador.

**Call Stack:** la base `OnTriggerEnter` → `HandleCollected` se mantuvo en los tres receptores; solo cambió el marco superior. Cada receptor se ejecutó una vez.

**Problemas detectados:** no se observaron fallas: la Console quedó vacía, `winPanel`, `controller` y `body` tenían valor, el panel de victoria se mostró y el jugador quedó bloqueado. Como riesgo, el orden de los receptores depende de `OnEnable()`, que Unity no garantiza entre componentes; hoy no afecta porque son independientes.

**Mejoras propuestas (surgen de revisar el código y no se implementaron):** registrar un error si `GameplayHUD` no tiene `winPanel`, porque hoy no muestra el panel y no avisa; y contemplar una escena sin coleccionables, donde `AllCollected` nunca se emite.

**Video:** [Ver video de la depuración](https://drive.google.com/file/d/1xjrju8MTMrix9IRPWlR5aYkY6DT95Oiv/view?usp=sharing) | **Detalle:** [Test_Depuracion.md](Test_Depuracion.md)
