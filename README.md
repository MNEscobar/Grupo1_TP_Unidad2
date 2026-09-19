# Grupo1_TP_Unidad2

## Interfaz responsiva

Configuración usada para que la UI se adapte a distintas resoluciones (issue #7):

- **Canvas Scaler** (en todas las escenas): `UI Scale Mode = Scale With Screen Size`, `Reference Resolution = 1920 x 1080`, `Screen Match Mode = Match Width Or Height`, `Match = 0.5`. Con 0.5 la UI escala a mitad de camino entre ancho y alto, así no se corta ni en 16:9 ni en 4:3.
- **Anchors**: los paneles de fondo van en *stretch* completo; el título se ancla al borde superior (0.5, 1); el bloque de botones al centro (0.5, 0.5); el botón *Continuar* de la Splashscreen al borde inferior (0.5, 0). Así cada elemento se mueve con el borde que le corresponde en vez de quedar fijo en píxeles.
- **Layout Groups**: los botones del menú principal viven dentro de `Container_Buttons` con un `Vertical Layout Group` (spacing 20, alineación Middle Center, sin control de tamaño de hijos) para que mantengan orden y separación sin posicionarlos a mano.
- **Prefab `Button_Base`** (`Assets/_Project/Prefabs/UI/`): botón de 320 x 64 con texto TMP; todas las instancias comparten tamaño y estados visuales (normal / highlighted / pressed / selected).

Para probar: en la Game view cambiar entre `1920x1080`, `1280x720` y una relación `4:3`; no debe quedar ningún elemento fuera del área visible.
