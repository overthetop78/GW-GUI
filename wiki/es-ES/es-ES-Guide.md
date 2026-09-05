[🌐 Languages / Langues](../Home.md)

# GW GUI Guía del usuario

GW GUI es una aplicación de Windows para leer, escribir, convertir, inspeccionar y emular imágenes de disquete-disk. Puede controlar Greaseweazle hardware, trabajar con archivos de imagen de disco a través de su motor interno, y ejecutar configuraciones de emulado-máquina guardadas.

Esta guía describe la interfaz de Inglés mostrada en la versión actual de la aplicación. Está escrito como la fuente del manual de usuario imprimible: capturas de pantalla ilustran los controles, mientras que el texto circundante explica qué elegir, por qué elegir, y cómo verificar el resultado.

> **Importante:** Leer un disco no es destructivo. Escribir, borrar, actualizar firmware y algunas herramientas de hardware pueden modificar medios o hardware. Lea la advertencia adjunta al procedimiento pertinente antes de hacer clic ** Ejecutar**.

### Cómo utilizar esta guía

Si esta es tu primera vez GW GUI, complete [Getting started](#getting-started), luego seguir [Leer un disco](#reading-a-disk). Si la aplicación ya está configurada, vaya directamente al capítulo para la operación que desee realizar. Los capítulos de opciones sirven como referencia cuando un procedimiento le pide que cambie una unidad, motor, perfil o ajuste de máquina emulada.

Los nombres de la interfaz se muestran en **negrita**. Los nombres de archivos, caminos, comandos y valores literales se muestran como `code`. Las notas explican el comportamiento normal; las advertencias identifican las operaciones que pueden alterar un disco, un controlador o una configuración almacenada.

## Índice

1. [Apoyo al flujo de trabajo](#understanding-the-workflow)
2. [Comenzando]](#getting-started)
3. [Main ventana](#main-window)
4. [Leer un disco](#reading-a-disk)
5. [Escribe un disco](#writing-a-disk)
6. [Convertir imágenes de disco](#converting-disk-images)
7. [Visualizar una imagen de disco](#visualizing-a-disk-image)
8. [Explorar contenido de disco](#exploring-disk-contents)
9. [Usando las herramientas](#using-the-tools)
10. [Emulación](#emulation)
11. [ Opciones de aplicación](#application-options)
12. [Opciones de aislamiento](#emulation-options)
13. [Amiga configuración](#amiga-configuration)
14. [Diagnóstico y mantenimiento del hardware](#hardware-diagnostics-and-maintenance)
15. [Logs and operation history](#logs-and-operation-history)
16. [ Datos de aplicación y uso portátil](#application-data-and-portable-use)
17. [Afluencias de trabajo recomendadas](#recommended-workflows)
18. [lista de seguridad](#safety-checklist)
19. [Solución de problemas](#troubleshooting)
20. [Glosario](#glossary)
21. [Referencia rápida](#quick-reference)

## Comprender el flujo de trabajo

GW GUI separa las operaciones de disco físico de las operaciones de archivo de imagen:

| Objetivo | Input | Producto | Página recomendada |
|---|---|---|---|
| Preserve un disquete | Disco físico | Archivo de imagen | **Leer** |
| Recrear un disquete | Archivo de imagen | Disco físico | **Escriba** |
| Cambiar formato de imagen | Archivo de imagen | Uno o más archivos de imagen | **Conversión** |
| Inspeccionar pistas y anomalías | Archivo de imagen | Análisis visual | **Visualización** |
| Buscar archivos almacenados en una imagen | Sistema de imagen y archivo compatibles | Archivos y directorios | **Disk Explorer** |
| Diagnostique una unidad o controlador | Greaseweazle hardware | Medidas o estado | **Herramientas** |
| Ejecutar una máquina virtual guardada | Configuración de la máquina guardada | Período de sesiones de establecimiento | **Emulación** |

Para la preservación, primero haz una captura cruda y mantenla inalterada como un maestro. Cree copias de trabajo convertidas o reparadas de ese maestro. Esto evita repetir una lectura física y preserva información que un formato basado en el sector no puede retener.

## Comienzo

### Necesidades

- Windows con Microsoft .NET Tiempo de ejecución de escritorio requerido por la aplicación.
- A Greaseweazle Controlador para operaciones de disk de disquete físico.
- Un camino configurado `gw.exe` cuando utiliza el Greaseweazle Host Tools motor.
- Legalmente obtenido ROM archivos cuando una máquina emulada los requiere.

La aplicación revisa su tiempo de ejecución .NET requerido al inicio. Si falta, siga el impulso de instalación y reinicie GW GUI.

### Antes de conectar hardware

Compruebe lo siguiente antes de ejecutar una operación de disco físico:

1. Conectar el Greaseweazle controlador a un estable USB Puerto.
2. Conecta el cable floppy con la orientación correcta.
3. Conecte la fuente de alimentación antes de insertar medios valiosos.
4. Confirme que el tamaño de la unidad y la densidad coinciden con el disco.
5. Protege el disco fuente cuando sea posible.

GW GUI no puede evitar daños causados por cableado incorrecto, poder inadecuado, o una unidad mecánicamente insegura. Prueba el hardware desconocido con un disco fungible primero.

### Primer lanzamiento

1. Abierto `gwgui.exe`.
2. Abierto **Opciones**.
3. In **Controladores y unidades**, escanear para el controlador y configurar la unidad.
4. Verificar o seleccionar el camino a `gw.exe`.
5. In **Motores**, elegir qué motor debe realizar cada operación.
6. Regrese a la ventana principal y seleccione la pestaña de operación requerida.

### Confirmando que la configuración está lista

Una configuración de trabajo debe mostrar el controlador y la unidad en la barra de estado, por ejemplo un número de unidad, tamaño, densidad y COM Puerto. In **Opciones &quot; Controladores y unidades **, el controlador debe ser marcado ** Disponible ** y la unidad ** Configurado **. Corre ** Información de control** antes de leer medios valiosos si desea verificar la comunicación sin alterar un disco.

### Elegir un motor

GW GUI puede exponer más de una implementación para algunas operaciones. El **Greaseweazle Host Tools** motor invoca el configurado `gw.exe`; el interior GW GUI motor maneja operaciones compatibles dentro de la aplicación. La selección del motor es explícita e independiente para la lectura, escritura, conversión y Disk Explorer. Si una operación no es compatible con el motor seleccionado, GW GUI informa que la condición en lugar de cambiar motores automáticamente.

## Ventana principal

La ventana principal agrupa las operaciones principales en siete pestañas:

- **Leer** crea una imagen de un disco físico.
- **Escriba** escribe una imagen a un disco físico.
- **Conversión** convierte un formato de imagen de disco en uno o más formatos de salida.
- **Visualización** muestra pistas y datos de flujo o decodificados.
- **Disk Explorer** navega sistemas de archivos soportados y contenidos de disco.
- **Herramientas** proporciona mantenimiento de hardware y comandos de diagnóstico.
- **Emulación** gestiona y ejecuta máquinas emuladas guardadas.

La consola en la parte inferior muestra el comando que se ejecuta y su salida. La barra de estado informa de la unidad seleccionada, perfil y estado actual.

### Leyendo la interfaz

La mayoría de las páginas de operación siguen el mismo patrón:

1. **Fuente o destino** controles identifican el disco, la imagen o la carpeta.
2. **Controles de formato** seleccionar la detección automática o una máquina y formato explícitos.
3. **Controles de perfil** aplicar ajustes reutilizables.
4. **Ajustes avanzados** exponer parámetros que normalmente son opcionales.
5. **Ejecutar** comienza la operación.
6. El **consola** muestra el comando generado, progreso, advertencias y errores.

El **Ejecutar** botón no implica que todos los valores sean seguros para el disco insertado. Siempre revise el destino y seleccione la unidad antes de una operación de escritura o mantenimiento.

### Barra de estado y consola

El lado izquierdo de la barra de estado identifica la unidad física activa. El centro muestra el perfil activo cuando se selecciona. El indicador del estado informa si la aplicación está lista o ocupada. La consola no es simplemente diagnóstico: es el registro autorizado del comando enviado al motor seleccionado. Utilice su control de copia cuando necesite preservar o compartir ese comando.

## Leyendo un disco

Abrir el **Leer** pestaña para capturar un disquete físico como imagen.

<p align="center"><img src="../images/main-read-en.png" alt="Leer ficha" width="78%"></p>

### Procedimiento básico

1. Inserte el disco fuente en la unidad configurada.
2. Elija el tipo de imagen:
   - **Imagen crudaSCP)** preserva información de nivel de flujo.
   - **Formato de disco conocido** crea una imagen usando una máquina y formato seleccionados.
3. Elija la carpeta de destino.
4. Introduzca el nombre de archivo de salida.
5. Seleccione un perfil si es necesario.
6. Haga clic **Ejecutar**.

La consola muestra el comando exacto y el progreso. No retire el disco ni desconecte el controlador hasta que la operación haya terminado.

### Elegir el tipo de salida

Uso **Imagen crudaSCP)** cuando el objetivo es archivar captura, análisis, recuperación o conversión posterior. Una imagen cruda registra información de tiempo y múltiples revoluciones, que es útil para formatos inusuales, sectores débiles, esquemas de protección y medios dañados.

Uso **Formato de disco conocido** cuando ya conoce la familia del disco y necesita una imagen sectorial directamente utilizable. Esta opción puede ser más pequeña y más fácil de abrir en otro software, pero representa el resultado decodificado en lugar de cada detalle observado por la unidad.

Cuando sea incierto, primero crea la imagen cruda. Puedes convertirlo más tarde sin leer el disco de nuevo.

### Carpeta, nombre de archivo y perfil

El **Folder ** es el directorio de destino. El ** Nombre de archivo** debe identificar el disco sin confiar sólo en su etiqueta física. Un nombre de archivo útil contiene el título, número de disco o lado, y una nota de condición cuando sea aplicable. No agregue una extensión de formato que entra en conflicto con el formato de salida seleccionado.

A **Perfil ** aplica un conjunto guardado de parámetros de lectura. Seleccione uno solo cuando sepa lo que contiene. El ** Default** El perfil es adecuado para un primer intento normal; un perfil de recuperación especializado puede leer deliberadamente más revoluciones o un rango diferente y por lo tanto tomar más tiempo.

### Ajustes avanzados

Ampliación **Ajustes avanzados** para acceder a parámetros de formato o de expertos. Deje estos valores sin cambios a menos que el disco requiera un rango de pista particular, cuenta de revolución o opción de controlador.

Los valores avanzados comunes incluyen:

| Ajuste | Propósito | Cuándo cambiarlo |
|---|---|---|
| Rango de seguimiento | Limita los cilindros y cabezas para leer | Medios unilaterales, geometría inusual, o un pase de recuperación apuntado |
| Revolucións | Controla cuántas rotaciones se muestren | Aumento de las vías inestables o protegidas; reducir sólo para la velocidad cuando proceda |
| Discusiones de expertos | Pasa parámetros adicionales del motor | Sólo cuando sigue documentado Greaseweazle Orientación |

### Verificar una lectura exitosa

No confíe sólo en la ausencia de un diálogo de error. Después de que el comando complete:

1. Confirme que el archivo de salida existe y no está vacío.
2. Lea las líneas de consola final para pistas fallidas o perdidas.
3. Abra la imagen en **Visualización** para comprobar que ambos lados y el rango de pistas esperado contienen datos.
4. Ábrelo. **Disk Explorer** cuando el sistema de archivos es compatible.
5. Mantenga el registro de operaciones con importantes capturas de archivo.

Si repetidas lecturas difieren, preservar cada captura cruda en lugar de sobreescribir el primero. Las diferencias pueden ser útiles durante la recuperación.

## Escribir un disco

Abrir el **Escriba** ficha para escribir una imagen existente a un disco de disquete físico.

<p align="center"><img src="../images/main-write-en.png" alt="Escribe la ficha" width="78%"></p>

### Procedimiento básico

1. Inserte el disco de destino.
2. Seleccione la imagen fuente con **Navega**.
3. Confirme el formato detectado.
4. Seleccione un perfil si es necesario.
5. Haga clic **Ejecutar**.

La escritura reemplaza los datos en el disco de destino. Verifique la unidad seleccionada y la imagen antes de comenzar.

> **Advertencia:** Escribir es destructivo. Sustituye los datos magnéticos en el disco de destino. Utilice un archivo fuente protegido por escrito y un disco de destino separado siempre que sea posible.

### Antes de escribir

Compruebe cuatro elementos antes de hacer clic **Ejecutar**:

1. **Imagen:** el camino seleccionado es la imagen de origen prevista.
2. **Disk:** el disco en la unidad puede ser sobrescrito con seguridad.
3. **Conducir:** el tamaño y densidad configurados se adaptan al medio de destino.
4. **Formato:** La detección automática o el formato seleccionado manualmente coincide con la imagen.

Si la imagen fuente no ha sido probada, ábrela **Visualización ** o ** Disk Explorer** primero. Una escritura exitosa no puede reparar una imagen fuente incompleta.

### Inspección de seguimiento y modificación

Después de seleccionar una imagen, **Visualizar pistas ** abre su representación de pista. ** Modificar** expone las modificaciones de imagen soportadas antes de escribir. Las acciones disponibles dependen del formato y el motor seleccionados.

### Verificar un disco escrito

Cuando el motor soporta la verificación, utilícela para medios importantes. De lo contrario, leer el disco escrito de nuevo a una nueva imagen y comparar su contenido decodificado o inspeccionarlo en **Visualización** Mantenga la captura de verificación separada de la imagen original para que el original nunca sea sobrescrito.

Si la escritura falla en pistas consistentes, comprobar la condición del disco, densidad, limpieza de la unidad y configuración de la unidad. Si los fallos ocurren aleatoriamente, compruebe USB estabilidad y comunicación del controlador.

## Convertir imágenes de disco

El **Conversión** pestaña convierte una imagen fuente en uno o varios formatos de destino.

<p align="center"><img src="../images/main-conversion-en.png" alt="Ficha de conversión" width="78%"></p>

### Procedimiento básico

1. Seleccione la imagen fuente.
2. Opcionalmente proporcionar nombres de salida.
3. Elige una familia de máquinas.
4. Seleccione uno o más formatos de salida y extensiones.
5. Habilitación **Añadir etiquetas** si los nombres de archivo deben utilizar el patrón de etiqueta configurado.
6. Haga clic **Ejecutar**.

El **Seleccionado ** El panel enumera los productos solicitados. ** Migración de archivos** proporciona el flujo de trabajo dedicado para la migración de archivos soportados en lugar de realizar una conversión de imagen estándar.

### Seleccionar formatos

El **Máquina ** lista filtra los formatos mostrados en el ** Formato** panel. Un nombre de formato describe el diseño lógico del disco; la extensión describe el contenedor de salida. Algunos formatos pueden ser representados por más de una extensión, y algunos contenedores no pueden preservar todas las características de una fuente cruda.

Seleccione sólo las salidas que realmente necesita. Múltiples formatos son útiles al crear un maestro de archivos, una copia compatible con emulador y una copia para otra herramienta de análisis en una operación.

### Nombres de salida y etiquetas

**Nombres de los productos ** le permite controlar los nombres de base generados para formatos seleccionados. ** Añadir etiquetas ** aplica el patrón de nombre de archivo configurado en ** Opciones &quot; General**. Tags puede codificar la familia, formato, extensión, fecha o hora. Avance el ejemplo en Opciones antes de convertir un lote grande para que los archivos se nombren consistentemente.

### Resultados de conversión

Para cada producto solicitado:

1. Confirme que se creó un archivo.
2. Revise la consola para pistas o sectores que no podrían decodificarse.
3. Abrir el resultado en **Disk Explorer** si contiene un sistema de archivos compatible.
4. Compare la capacidad y el contenido de disco esperados con la fuente.

Una conversión puede completar mientras se informa de la pérdida de información que es inherente al formato de destino. Retener la imagen original cruda incluso cuando la imagen convertida aparece correcta.

## Visualizar una imagen de disco

El **Visualización** pestaña muestra la estructura y distribución de datos de una imagen.

<p align="center"><img src="../images/main-visualization-en.png" alt="Ficha de visualización" width="78%"></p>

1. Haga clic **Abra una imagen de disco**.
2. Manténganse. **Detección automática** activado, o selecciona manualmente la máquina y el formato.
3. Uso **Enlace zoom** para mantener ambos lados en el mismo nivel de zoom.
4. Uso **Reset** para restaurar la vista inicial.
5. Abierto **Inspector** para información detallada sobre la región seleccionada.

La leyenda distingue flujo normal, transiciones cortas y largas, encabezados, datos decodificados y anomalías detectadas. Una imagen cruda puede contener datos que no pueden descifrarse en un sistema de archivos conocido pero todavía se puede inspeccionar aquí.

### Interpretación de la opinión

Cada panel circular grande representa un lado del disco. El centro identifica el lado y su estado actual de datos; las posiciones concéntricos corresponden a las pistas. Los colores clasifican las regiones detectadas según la leyenda. El visualizador está destinado a responder preguntas tales como:

- ¿La imagen contiene datos de un lado o ambos?
- ¿Están presentes las pistas esperadas?
- ¿Son las anomalías aisladas o repetidas a través del disco?
- ¿La detección automática identificó una máquina y formato plausible?

Un color de anomalía es una razón para inspeccionar la región, no prueba de que el disco es inutilizable. Protección de copia, formato no estándar, una grabación débil y un sector dañado pueden producir diferentes estructuras que requieren interpretación contextual.

### Secuencia de inspección recomendada

Comience con zoom conectado habilitado para comparar ambos lados a la misma escala. Seleccione una región sospechosa, abierta **Inspector**, y compararlo con las vías vecinas. Si el resultado parece ser un problema de detección, deshabilita la detección automática y elige una máquina y formato conocidos. Volver a la detección automática después de la prueba para que un ajuste forzado no se utilice accidentalmente para otra imagen.

## Explorar el contenido del disco

El **Disk Explorer** pestaña navega imágenes de disco compatibles como una jerarquía de archivos.

<p align="center"><img src="../images/main-disk-explorer-en.png" alt="Disk Explorer pestaña" width="78%"></p>

1. Abra una imagen existente o lea un disco.
2. Manténganse. **Detección automática** habilitado a menos que necesite forzar una máquina o formato.
3. Revise la información de volumen: sistema, protección, sistema de archivos, capacidad, espacio libre y recuento de elementos.
4. Examine directorios en el panel izquierdo.
5. Seleccione un elemento para ver sus detalles en el panel derecho.

Si el formato de imagen o sistema de archivos no está soportado, use **Visualización** para inspeccionar la estructura cruda.

### Comprender los paneles

El resumen superior describe la imagen montada y el volumen detectado. El panel inferior-izquierda contiene la jerarquía del directorio. La tabla central lista los elementos en el directorio seleccionado con nombre, fecha de modificación, tipo y tamaño. El panel derecho muestra detalles para el artículo seleccionado.

Disk Explorer no implica que cada pista cruda fue decodificada perfectamente. Utilice el resumen de volumen y el recuento de elementos como un cheque de plausibilidad rápida, luego abrir archivos representativos o compararlos con un listado de directorio conocido cuando la exactitud de la preservación importa.

### Cuando nada aparece

Primero confirma que el camino de la imagen es correcto. Luego revise la máquina y el formato detectados. Una imagen válida puede contener un sistema de archivos no compatible o dañado, en cuyo caso el explorador puede permanecer vacío a pesar de **Visualización** muestra datos registrados. No sobreescribir o descartar la imagen de origen basado sólo en un explorador vacío.

## Utilizando las herramientas

El **Herramientas** grupos Greaseweazle operaciones de mantenimiento.

<p align="center"><img src="../images/main-tools-en.png" alt="Ficha de herramientas" width="78%"></p>

Seleccione un comando de la lista de la izquierda, revise sus parámetros y haga clic en **Ejecutar**. Los comandos destructivos o intercambiadores de hardware sólo deben utilizarse después de verificar el controlador y la unidad seleccionados.

La mayoría de los diálogos de herramientas contienen tres áreas: parámetros en la parte superior, un estado y área de salida en el centro, y el comando generado en la parte inferior. El comando previsualiza cambios como opciones están habilitadas. Un parámetro no comprobado normalmente significa “no modificar este valor”, mientras que un parámetro comprobado incluye ese valor en el comando.

Los diálogos de diagnóstico individual se describen en [Hardware diagnóstico y mantenimiento](#hardware-diagnostics-and-maintenance).

## Emulación

### Abrir una máquina guardada

El **Emulación ** listas de fichas guardadas configuraciones. Seleccione uno y haga clic ** Abierto**Cada máquina de correr aparece en su propia pestaña.

<p align="center"><img src="../images/main-emulation-welcome-en.png" alt="Pantalla de bienvenida Emulation" width="78%"></p>

Crear y editar máquinas en **Opciones &quot; Emulación Configuraciones ** y ** Opciones &quot; Emulation &quot; Amiga**.

Si no aparece ninguna configuración, cree uno en Opciones primero. Una configuración guardada combina el modelo de máquina, versión emulador, ROM, memoria, vídeo, audio, almacenamiento y mapas de entrada. Ahorrar una configuración no lo inicia; volver a la principal **Emulación ** pestaña y haga clic ** Abierto**.

### Controles de máquina de correr

<p align="center"><img src="../images/main-emulation-running-en.png" alt="Máquina emulada" width="78%"></p>

La barra de herramientas de máquina de correr proporciona potencia, pausa, restablecimiento, estado de ahorro, estado de carga, captura y controles de visualización. También muestra:

- los atajos configurados de rápida y rápida carga;
- el renderizador activo, como Direct3D 11;
- los atajos de pantalla completa y liberación del ratón;
- audio, controlador y estado del ratón;
- la resolución actual, tasa de actualización y tasa de marco.

La tira de disco en la parte inferior de la pantalla de emulación administra medios extraíbles para cada unidad emulada. Las asignaciones de teclado se pueden cambiar en **Opciones &quot; Emulación Atajos**, mientras que las asignaciones de teclado, ratón y controlador emulados se configuran en el correspondiente Amiga fichas.

### Referencia de la barra de herramientas

| Grupo de control | Propósito |
|---|---|
| Poder y pausa | Comienza, detiene, pausa o retoma la máquina emulada |
| Controles de reiniciar | Realiza la acción de reajuste suave o duro configurada |
| Controles estatales | Guarda o carga un estado emulador para una rápida continuación |
| Captura | Guarda una imagen de la pantalla emulada |
| Visualización | Cambia la presentación de la pantalla o entra en pantalla completa |
| Recordatorio de estado rápido | Muestra los atajos activos de ahorro/carga |
| Renderer | Reporta el backend de vídeo activo |
| Input reminder | Muestra atajos de pantalla completa y liberación del ratón |
| Indicadores de dispositivos | Informes audio, controlador y estado del ratón |
| Ejecución | Informes tamaño de salida, frecuencia de actualización y velocidad de marco |

### Dejando la pantalla completa o liberando el ratón

La barra de herramientas muestra las teclas asignadas actualmente. En la configuración ilustrada, **Alt+ Regreso ** toggles pantalla completa y ** F12** libera el ratón. Tratar los valores mostrados como autorizados porque los atajos pueden ser reasignados.

### Usando los medios floppy

La tira de la unidad identifica cada unidad emulada, como `DF0:`. Utilice sus controles multimedia para insertar, reemplazar o expulsar una imagen. Reemplazar los medios cambia sólo el disco insertado de la máquina de correr; no cambia la definición de dispositivo de almacenamiento en la máquina guardada a menos que esa acción se guarde explícitamente.

## Opciones de aplicación

Abierto **Opciones** desde la ventana principal para configurar la aplicación.

### General

<p align="center"><img src="../images/options-general-en.png" alt="Opciones generales" width="72%"></p>

El **General** La pestaña contiene:

- la carpeta de imagen de disco predeterminada;
- lenguaje de interfaz y tema;
- generación de nombre de archivo para conversiones;
- patrones predefinidos y recientes de etiquetas personalizadas;
- un ejemplo de nombre de archivo en vivo.

Las variables de la etiqueta incluyen el nombre de la fuente, la familia, el formato, la extensión, la fecha y el tiempo. Utilice el botón de reinicio para restaurar el patrón predeterminado.

El nombre de archivo previsualiza actualizaciones antes de crear cualquier archivo. Úsalo para detectar separadores duplicados, extensiones perdidas o nombres ambiguos. Los patrones personalizados recientes proporcionan acceso rápido a los esquemas de nombres anteriores sin reemplazar el preset actual.

### Logs

<p align="center"><img src="../images/options-logs-en.png" alt="Opciones de registro" width="72%"></p>

El registro se puede configurar independientemente para cada operación. Para cada categoría, elija si guardar registros, establecer un tamaño máximo de archivo, y decidir si los registros anteriores deben ser retenidos. Un tamaño de `0` significa ilimitado. **Carpeta abierta** abre el directorio de registro actual.

Habilitación **Mantener registros anteriores** para la conservación y el trabajo de diagnóstico donde la historia de varios intentos importa. Desactivarlo cuando sólo el resultado más reciente es útil. Los límites de tamaño máximo se aplican al almacenamiento de registros, no a las imágenes de disco capturadas.

### Controladores y unidades

<p align="center"><img src="../images/options-controllers-and-drives-en.png" alt="Controladores y unidades" width="72%"></p>

Utilice esta pestaña para:

- escaneado para controladores conectados;
- añadir y eliminar configuraciones de la unidad;
- seleccionar el tamaño de la unidad, densidad y velocidad;
- guardar la configuración del hardware;
- elegir o encontrar automáticamente `gw.exe`;
- comprobar y descargar Greaseweazle Host Tools actualizaciones;
- restaurar una ruta ejecutable previamente configurada.

Los ajustes de hardware guardados permanecen disponibles cuando una unidad se desconecta temporalmente.

#### Añadiendo una unidad

1. Haga clic **Scan** y esperar a que aparezcan controladores conectados.
2. Haga clic **Añadir una unidad** si la unidad requerida no está lista.
3. Seleccione su número de unidad lógico, tamaño físico, densidad de grabación y velocidad de rotación.
4. Guarda la fila.
5. Confirme que muestra **Disponible ** y ** Configurado**.

Utilice el control de basura sólo para eliminar la configuración guardada; no desconecta hardware. Si el mismo controlador aparece en otro COM port later, Scan again before assumed that the stored port is still valid.

#### Gestión Greaseweazle Host Tools

**Encontrar gw.exe ** busca lugares conocidos. ** Elija ** selecciona un ejecutable específico. ** Revisar las actualizaciones ** consultas disponibles versiones sin reemplazar el instalado. ** Descargar la última versión ** instala el paquete actual seleccionado, y ** Usar el camino anterior ** restaura la ubicación configurada anterior. Después de cambiar el ejecutable, corre ** Información de control** para confirmar que la versión seleccionada puede comunicarse con el controlador.

### Motores

<p align="center"><img src="../images/options-engines-en.png" alt="Selección de motores" width="72%"></p>

Elija el motor independientemente para la lectura, escritura, conversión y Disk Explorer. El motor seleccionado se utiliza estrictamente: si no puede realizar la operación solicitada, GW GUI reporta la limitación en lugar de los motores de conmutación silenciosa.

Esta independencia es intencional. Por ejemplo, las lecturas físicas pueden usar Greaseweazle Host Tools mientras la conversión de imagen y exploración utilizan el motor interno. Grabar las opciones del motor en una nota de perfil o proyecto cuando importa la reproducibilidad.

### Perfiles

<p align="center"><img src="../images/options-profiles-en.png" alt="Perfiles" width="72%"></p>

Los perfiles almacenan ajustes reutilizables para operaciones de lectura, escritura y conversión. Seleccione la categoría relevante para gestionar sus perfiles. Un perfil seleccionado se muestra en la barra de estado de la ventana principal y en las pantallas de operación.

Utilice perfiles para flujos de trabajo repetibles en lugar de como colecciones no explicadas de banderas de expertos. Dar a cada perfil un nombre específico, como una unidad particular, una familia de disco o un método de recuperación. Revise un perfil después de actualizar el motor subyacente porque las opciones soportadas pueden cambiar.

## Opciones de emulación

El **Emulación** opciones contienen configuraciones de almacenamiento general, atajos globales, configuraciones guardadas y configuraciones específicas de la máquina.

### Carpetas de emulación general

<p align="center"><img src="../images/options-emulation-general-en.png" alt="Opciones de emulación general" width="72%"></p>

Establecer la carpeta de almacenamiento de emulación compartida y las carpetas predeterminadas para capturas y estados guardados. **Carpeta abierta** abre la ubicación compartida en File Explorer.

Mantenga capturas y estados guardados en carpetas separadas. Una captura es una imagen ordinaria; un estado guardado contiene un estado de máquina específico emulador y puede depender de la versión y configuración del emulador que la creó. Retrocede la configuración y los medios junto a importantes estados guardados.

### Atajos mundiales

<p align="center"><img src="../images/options-emulation-shortcuts-en.png" alt="Atajos de emulación" width="72%"></p>

Busque una acción o asignación clave, asigne o elimine atajos, restablezca los defectos y conflictos claros. La columna identifica asignaciones válidas y conflictivas.

Para cambiar un atajo, encontrar la acción, haga clic **Assign **, y presionar la combinación de clave deseada. Compruebe el estado antes de cerrar Opciones. ** Conflictos claros ** elimina las asignaciones conflictivas; no restaura la asignación predeterminada. Uso ** Restaurar defectos** cuando desea reemplazar las asignaciones personalizadas con el conjunto estándar.

### Configuraciones guardadas

<p align="center"><img src="../images/options-emulation-configurations-en.png" alt="Configuraciones de emulación guardadas" width="72%"></p>

Esta página enumera máquinas guardadas. Seleccione una configuración para editarla en el **Amiga** tab. Puede actualizar la lista o eliminar la configuración seleccionada.

Eliminar una configuración elimina la definición de máquina guardada. No debe ser utilizado como una manera de expulsar medios o cerrar una máquina de funcionamiento. Antes de la eliminación, note cualquier ROM, imagen de disco duro y archivos de estado asociados con la configuración.

## Amiga configuración

La interfaz actual proporciona detalles Amiga páginas de configuración. La misma estructura de configuración se puede ampliar para otros sistemas emulados sin cambiar el flujo de trabajo principal.

### General

<p align="center"><img src="../images/options-amiga-general-en.png" alt="Amiga Ajustes generales" width="72%"></p>

Elija el Amiga modelo, guardar la configuración, instalar o reemplazar la versión del emulador, y definir carpetas predeterminadas para discos duros y otros medios. **Versiones de búsqueda** consulta la fuente oficial de emulador-versión.

Empieza con el modelo porque limita páginas posteriores. Cambiarlo puede alterar el disponible CPU, memoria, ROM, chipset y opciones de almacenamiento. Después de seleccionar una versión emulador, guarde la configuración antes de lanzarla desde la ventana principal. Instalar otra versión del emulador reemplaza la versión utilizada por esa configuración; no crea una segunda copia de la máquina.

### CPU

<p align="center"><img src="../images/options-amiga-cpu-en.png" alt="Amiga CPU Ajustes" width="72%"></p>

El CPU página muestra el procesador seleccionado por el modelo de máquina y proporciona precisión compatible, FPU, y opciones de velocidad. Las opciones que no se aplican al modelo seleccionado siguen siendo deshabilitadas.

- **CPU modelo** identifica al procesador emulado.
- **Precisión** controla el modelo de tiempo. Los modos exactos del ciclo favorecen la compatibilidad del hardware pero requieren más procesamiento del host.
- **FPU** permite una unidad flotante compatible cuando se admite.
- **CPU velocidad** selecciona el tiempo original o un modo acelerado.

Para una configuración de referencia, mantenga el modelo-derived CPU y velocidad original. Cambiar la aceleración sólo después de las botas de la máquina correctamente en su configuración estándar.

### RAM

<p align="center"><img src="../images/options-amiga-ram-en.png" alt="Amiga RAM Ajustes" width="72%"></p>

Configurar Chip RAM, Despacio RAM, rápido RAM, y soporte la memoria de expansión. Los mensajes de compatibilidad explican restricciones para la máquina seleccionada, y la memoria configurada total se muestra en la parte inferior.

**Chip RAM ** es accesible a los chips personalizados y es requerido por la plataforma. ** Despacio. RAM ** representa la memoria de expansión compatible utilizada por configuraciones comunes. ** Rápido RAM ** es memoria de expansión orientada al procesador. ** Zorro III RAM** sólo se aplica a modelos que apoyan esa arquitectura de expansión. Los mensajes de compatibilidad y los controles de discapacidad evitan combinaciones que el modelo seleccionado no puede representar.

### ROM

<p align="center"><img src="../images/options-amiga-rom-en.png" alt="Amiga ROM Ajustes" width="72%"></p>

Seleccione el sistema Kickstart ROM, ampliación opcional ROM, y ROM llave. El detectado...ROM lista muestra nombres, revisiones y compatibilidad con el modelo seleccionado. Seleccione una detectada ROM y haga clic **Uso**, o busque un archivo manualmente.

ROM Los archivos no son suministrados por GW GUI. Utilice ROMs que está legalmente permitido utilizar.

La lista detectada es preferible a adivinar desde un nombre de archivo: reporta la ROM identidad y revisión y evaluación de compatibilidad con el modelo seleccionado. **Compatible ** es la elección normal; ** Parcialmente compatible ** indica que ROM puede arrancar pero no coincide precisamente con la máquina. ** Refresh ** reescanda el configurado ROM localizaciones. ** Uso** asigna el seleccionado detectado ROM a la configuración.

### Video

<p align="center"><img src="../images/options-amiga-video-en.png" alt="Amiga configuración de vídeo" width="72%"></p>

Configurar estándar de vídeo, relación de aspecto, resolución, modo de línea, corte de frontera, renderizado, profundidad de color, corte de marco, gamma y fijación de flicker. Los ajustes adicionales de chipset están disponibles más abajo en la página cuando están soportados por el modelo seleccionado.

| Ajuste | Efecto práctico |
|---|---|
| estándar de vídeo | Selecciones PAL o NTSC tiempo y comportamiento de actualización esperado |
| Relación entre los aspectos | Controla cómo se escala la imagen emulada |
| Resolución | Seleccione el detalle de salida automático o explícito |
| Modo de línea | Controla el tratamiento de salida interrelacionada o duplicada en línea |
| Crop borders | Elimina el overscan no utilizado sólo cuando está habilitado |
| Rendering | Elige el backend gráfico |
| Profundidad de color | Selecciona precisión de color de salida |
| Frame skip | Reduce los marcos renderizados cuando estén habilitados |
| Gamma | Ajuste la respuesta del brillo |
| Flicker fixer | Procesos modos que de otra manera visiblemente flicker |

Cambie un ajuste de pantalla a la vez. Si la ventana de emulación se vuelve en blanco o inestable, vuelva a la resolución automática, patrón de marco deshabilitado, gamma neutral, y el renderizador de trabajo anterior.

### Audio

<p align="center"><img src="../images/options-amiga-audio-en.png" alt="Amiga Ajustes de audio" width="72%"></p>

Activar o deshabilitar el audio, elegir el dispositivo de salida y latencia, luego configurar la interpolación, Amiga filtrado, tipo de filtro, separación estéreo, sonido de disquete y volumen de CD-audio.

La menor latencia reduce el retraso, pero puede causar deserciones en un equipo ocupado. Aumenta si el audio se rompe. Interpolación y la Amiga El filtro de audio cambia la reproducción del sonido en lugar de la lógica del programa emulado. El volumen de sonido de la unidad controla el sonido mecánico simulado por separado de lo normal Amiga audio.

### Almacenamiento

<p align="center"><img src="../images/options-amiga-storage-en.png" alt="Amiga configuración de almacenamiento" width="72%"></p>

La página de almacenamiento lista identificadores de dispositivos, tipos, modelos, medios asociados y acciones disponibles. Añadir, configurar o eliminar los dispositivos aquí. Los discos y los CDs pueden ser insertados o reemplazados directamente de una máquina de funcionamiento.

El **identificador de dispositivo ** es cómo el sistema emulado dirige el dispositivo. ** Tipo ** distingue disquete, disco duro, óptico y otros dispositivos compatibles. ** Modelo ** describe el hardware emulado, mientras ** Associated media** identifica la imagen asignada actualmente. Configure el dispositivo antes de asociar valiosos medios de comunicación, y mantenga copias de seguridad de imágenes de disco duro.

### Teclado

<p align="center"><img src="../images/options-amiga-keyboard-en.png" alt="Amiga Ajustes del teclado" width="72%"></p>

Búsqueda Amiga claves y asignaciones de host, asignar nuevas claves, eliminar mapas, restaurar defectos o conflictos claros. La columna de estado informa si cada asignación es válida.

La columna izquierda nombra la emulación Amiga clave; **Association** muestra la combinación de tecla host. Una asignación válida todavía puede ser inconveniente si Windows o la aplicación se reserva el mismo atajo, por lo que prueba combinaciones críticas dentro de la máquina de correr. Evite asignar el atajo de liberación del ratón o pantalla completa a una clave que el software emulado necesita con frecuencia.

### Mouse

<p align="center"><img src="../images/options-amiga-mouse-en.png" alt="Amiga configuración del ratón" width="72%"></p>

Establecer la velocidad del ratón físico, elegir qué palo analógico controla el ratón, ajustar la zona muerta analógica y la velocidad, y configurar mapas de acción del ratón. Restaurar defectos o conflictos de mapeo claros cuando sea necesario.

Aumente la zona muerta si un controlador causa deriva puntero. Ajustar la velocidad izquierda y derecha del palillo independientemente cuando ambas barras están habilitadas. La tabla de mapeo inferior asocia las entradas de host con botones o acciones del ratón; inspeccionar su estado de conflicto después de cambiar las asignaciones del controlador en otros lugares.

### Controladores

<p align="center"><img src="../images/options-amiga-controllers-en.png" alt="Amiga Ajustes del controlador" width="72%"></p>

Detectar controladores conectados, asignar dispositivos y tipos de controladores a Amiga puertos, y configurar mapas de controladores y ajustes de turbo-fire. Las opciones disponibles dependen del hardware detectado y de la máquina seleccionada.

Puerto 1 y Puerto 2 están configurados independientemente. **Automático** El tipo de controlador es un punto de partida sensible, pero el software que espera un joystick particular o el ratón puede requerir un tipo explícito. Ejecute la detección antes de asignar un nuevo controlador conectado. Turbo fire activa repetidamente una entrada mapeada y debe permanecer deshabilitado a menos que el juego o aplicación se beneficie de ella.

## Diagnóstico y mantenimiento de hardware

Estos diálogos se abren desde el **Herramientas ** tab. Cada diálogo preve el generado Greaseweazle Comando. Revise antes de hacer clic ** Ejecutar**.

### Información de control

<p align="center"><img src="../images/tool-controller-information-en.png" alt="Información de control" width="62%"></p>

Muestra información reportada por el controlador seleccionado. Ampliación **Producción bruta** cuando usted necesita la respuesta completa del comando.

Use esto como el primer comando de diagnóstico. Una respuesta exitosa confirma que GW GUI puede iniciar el ejecutable de Host Tools configurado y comunicarse con el dispositivo seleccionado. Grabar el firmware y la información de hardware antes de realizar una actualización.

### USB ancho de banda

<p align="center"><img src="../images/tool-usb-bandwidth-en.png" alt="USB ancho de banda" width="62%"></p>

Medidas disponibles USB ancho de banda de comunicación. Úsalo para diagnosticar transferencias inestables o inadecuadas USB Conexión.

Cerrar otro software usando el controlador antes de probar. Repita la medición después de cambiar la USB puerto, cable o centro. Compare los resultados en condiciones similares en lugar de tratar una sola medición como una garantía absoluta.

### Velocidad de conducción

<p align="center"><img src="../images/tool-drive-speed-en.png" alt="Velocidad de conducción" width="62%"></p>

Mide la velocidad de rotación de la unidad. Aumente el número de mediciones cuando necesite un resultado más representativo.

Una única medición es un cheque rápido; varias mediciones revelan si la velocidad es estable. Deje que la unidad alcance la velocidad normal antes de interpretar el resultado. Un valor inesperado puede indicar una velocidad configurada incorrecta, un problema mecánico o un problema de configuración de medición.

### Busca la cabeza.

<p align="center"><img src="../images/tool-seek-head-en.png" alt="Busca la cabeza." width="62%"></p>

Mueva la cabeza de la unidad a un cilindro seleccionado. **Permitir cilindros extremos ** permisos normalmente restringidos, y ** Mantenga el motor activo** deja el motor funcionando durante la operación. Use posiciones extremas sólo cuando el procedimiento de hardware los requiera explícitamente.

La búsqueda normal es útil para confirmar el movimiento de cabeza o posicionamiento antes de un diagnóstico. Escuche los impactos repetidos anormales y deténgase si el cilindro solicitado es inapropiado para la unidad. Esta herramienta no lee ni valida datos en el cilindro de destino.

### Diagnóstico de alineación

<p align="center"><img src="../images/tool-drive-alignment-en.png" alt="Diagnóstico de alineación" width="62%"></p>

Ejecuta repetidas lecturas para el análisis de la alineación de la unidad. Apoya la selección de pistas, la revolución y los recuentos de lectura, el formato de decodificación, el flujo bruto, el índice, la velocidad, PLL, densidad-pin, sector duro, TG43, y opciones de datos inversos. La labor de alineación requiere medios de referencia adecuados y conocimientos de hardware.

Comience con un disco de referencia conocido y el conjunto más pequeño de overrides. **Carriles alternativos ** define las pistas y las cabezas que se muestren; ** Revoluciones por vía ** controla cada duración de la muestra; ** Número de lecturas** determina la repetición. Permitir una definición de disco personalizada o formato de decodificación sólo cuando coincida con los medios de referencia. Opciones como índice falso, sectores difíciles, PLL anula, pins de densidad, y TG43 son hardware o formato específico y puede invalidar una comparación cuando se utiliza incorrectamente.

### Pernos de hardware

<p align="center"><img src="../images/tool-hardware-pins-en.png" alt="Pernos de hardware" width="62%"></p>

Lee o cambia un pin de controlador compatible. Seleccione el pin, active **Cambio de pin ** sólo cuando escriba un valor, y seleccione ** Alto nivel** cuando sea requerido por la operación de hardware prevista.

Con **Cambio de pin** Desactivado, el comando pide el pin. Este es el defecto más seguro. Cambiar un nivel afecta directamente al controlador I/O y debe hacerse sólo con el correcto Greaseweazle documentación de hardware y cableado adjunto.

### Reiniciar el controlador

<p align="center"><img src="../images/tool-reset-controller-en.png" alt="Reiniciar el controlador" width="62%"></p>

Resetea el Greaseweazle Controlador. Utilice esto cuando se detecta el controlador pero ya no responde normalmente.

Espere a que cualquier operación de disco activo termine antes de reiniciar. Después, escanee el controlador de nuevo si su estado de conexión no se recupera automáticamente. Un reinicio no repara un mal `gw.exe` camino o una desconexión USB Dispositivo.

### Delays

<p align="center"><img src="../images/tool-delays-en.png" alt="Retrasos del controlador" width="62%"></p>

Lee o cambia los valores de tiempo del controlador, incluyendo selección, paso de cabeza, ajuste, motor, deselección automática, tiempo de escritura, y retrasos de máscara de índice. Permite sólo los valores que pretende modificar.

Los campos no comprobados dejan sin cambios el valor correspondiente del controlador. Antes de editar, registre los valores existentes. Los cambios en el tiempo pueden afectar a cada operación física posterior, por lo que prueba con los medios de comunicación fungibles y restaurar los valores conocidos si el comportamiento se vuelve poco confiable.

### Firmware

<p align="center"><img src="../images/tool-firmware-en.png" alt="Actualización de firmware" width="62%"></p>

Actualizaciones del firmware del controlador. **Actualizar descargas de arranque** está explícitamente marcado como arriesgado y debe permanecer incapacitado a menos que el procedimiento oficial de firmware lo requiera. No desconecte el controlador durante una actualización.

Antes de actualizar, confirme el controlador conectado con **Información de control**, utilizar un sistema directo estable USB conexión, y cerrar otro software que podría acceder a él. Después de la terminación, reconecte o vuelva a escanear el controlador y lea su información de nuevo para verificar la versión de firmware reportada.

## Registros e historial de operaciones

Abra el historial de operaciones para inspeccionar registros guardados por operación.

<p align="center"><img src="../images/operation-history-en.png" alt="Historia de la operación" width="68%"></p>

Seleccione un registro en la izquierda para mostrar su contenido. **Exportación** guarda una copia para diagnóstico o soporte. Los caminos y las líneas de comando pueden contener nombres de carpeta personal, así que revise los registros exportados antes de compartirlos.

La consola en vivo en la ventana principal muestra el comando actual y la salida reciente. Su botón de copia copia copia el texto mostrado.

### Leyendo un tronco

Un registro de diagnóstico útil contiene el comando generado, los horarios, la salida del motor y el estado final. Trabajar desde abajo hacia arriba: identificar el error final, luego localizar la primera advertencia o la pista fallida que lo precedió. Un fallo genérico posterior es a menudo sólo la consecuencia de un mensaje anterior, más específico.

Al comparar dos intentos, compruebe que el controlador, unidad, motor, perfil, ruta de origen, formato de salida y argumentos expertos fueron idénticos. De lo contrario, un resultado diferente puede reflejar ajustes cambiados en lugar de inestabilidad de disco.

## Datos de aplicación y uso portátil

GW GUI mantiene los datos de usuario separados de los binarios de aplicaciones. Dependiendo del paquete y modo seleccionados, configuraciones, registros, herramientas descargadas, componentes emuladores, capturas, estados y configuraciones de máquinas se almacenan ya sea en la aplicación `Data` directorio o en las ubicaciones configuradas de datos de usuario.

Antes de reemplazar o mover una instalación portátil, mantén la carpeta completa de la aplicación junto y copia de seguridad `Data` carpeta. No mueva archivos individuales de `lib`, porque la aplicación resuelve sus propias y de terceros bibliotecas de esa estructura.

### Contenido de copia de seguridad sugerido

Retrocede lo siguiente cuando son importantes para su flujo de trabajo:

- configuración y perfiles de aplicación;
- las definiciones del controlador y de la unidad;
- configuraciones de emulación;
- ROM and legally held ROM respaldos;
- imágenes de disco duro y medios extraíbles;
- capturas y estados salvados;
- registros de operaciones utilizados como registros de conservación.

Las imágenes de disco pueden ser mucho más grandes que los ajustes. Almacene maestros de archivo sólo lectura cuando sea posible, y trabaje en copias.

## Corrientes de trabajo recomendadas

### Archivar un disco desconocido

1. Inspeccione y limpie la unidad usando un procedimiento de mantenimiento adecuado.
2. Protege el disco si es posible.
3. Seleccione **Leer más Imagen crudaSCP)**.
4. Use un nombre de archivo descriptivo y lea el rango de pista normal con múltiples revoluciones.
5. Revisa la consola y el registro guardado.
6. Inspeccionar ambos lados **Visualización**.
7. Convertir una copia en formatos sectoriales probables.
8. Prueba las copias convertidas en **Disk Explorer** o software adecuado.
9. Preserve el maestro crudo, registro y notas juntos.

### Recreando un disco de una imagen

1. Inspeccione la imagen y confirme su familia y formato esperados.
2. Insertar un disco fungible o intencionalmente writable del tamaño y densidad correctos.
3. Abierto **Escriba** y seleccionar la imagen.
4. Confirme la unidad configurada y el formato detectado.
5. Escribe el disco.
6. Léalo de nuevo a una imagen de verificación separada.
7. Compare el contenido decodificado y revise las pistas sospechosas visualmente.

### Creación de una emulación Amiga

1. Abierto **Opciones &quot; Emulación Configuraciones** y crear o seleccionar una máquina.
2. In **Amiga , General**, elegir la versión modelo y emulador.
3. Assign a compatible, legalmente obtenido ROM.
4. Mantenga los defectos del modelo para CPU y RAM en la primera bota.
5. Configurar vídeo y audio con configuraciones automáticas conservadoras.
6. Agregue dispositivos de almacenamiento e imágenes de medios copiadas asociadas.
7. Revisar teclado, ratón y tareas de controlador.
8. Guarda la configuración.
9. Volver a **Emulación **, seleccione y haga clic ** Abierto**.
10. Sólo después de una bota de base exitosa, cambiar la aceleración o configuración avanzada uno a la vez.

## Lista de verificación de seguridad

Antes **Leer**:

- el disco fuente está en la unidad correcta;
- la fuente está protegida por escrito cuando sea posible;
- la ruta de salida no sobreescribirá un maestro existente;
- el perfil y el rango de pista coinciden con el disco.

Antes **Escriba ** o ** Borrar**:

- el disco de destino puede ser destruido;
- la imagen y la unidad son correctas;
- el tamaño y la densidad del disco son compatibles;
- ningún maestro de archivo está siendo utilizado como el destino.

Antes de una herramienta de cambio de hardware:

- ninguna otra operación está funcionando;
- se selecciona el controlador correcto;
- se han registrado valores actuales;
- el controlador tiene potencia estable y USB conectividad;
- la acción está respaldada por la documentación de hardware.

## Solución de problemas

### El controlador no está listado

1. Reconecte el controlador directamente al ordenador.
2. Abierto **Opciones &quot; Controladores y unidades**.
3. Haga clic **Scan**.
4. Verifique el estado del controlador y la configuración de la unidad.
5. Corre **Información de control** si la detección tiene éxito pero los comandos fallan.

Si todavía no aparece, prueba otro directo USB puerto y cable, y luego recan. Revise Windows Device Manager para un dispositivo serie recién detectado. Un controlador visible a Windows pero ausente GW GUI usualmente apunta a un puerto ocupado, configuración de establos o problema de Herramientas anfitrionas; un controlador ausente de puntos de Windows a USB, potencia, controlador o hardware.

### `gw.exe` no se puede encontrar

Abierto **Opciones &quot; Controladores y unidades **, entonces uso ** Encontrar gw.exe **, ** Elija **o ** Descargar la última versión**. Confirme que el camino detectado apunta a la Greaseweazle instalación.

Después de seleccionarlo, ejecutar **Información de control**. Si eso falla antes de ponerse en contacto con hardware, inspeccione el registro para una ruta ejecutable inválida, archivos faltantes o una versión que no puede comenzar.

### Una operación utiliza el motor equivocado

Abierto **Opciones &quot; Motores** y comprobar el motor asignado a esa operación exacta. GW GUI no cae silenciosamente al otro motor.

Los ajustes del motor son separados: cambiar el motor de conversión no cambia la lectura, escritura o Disk Explorer. Reabrir la operación de falla después de guardar la opción y confirmar el comando generado en la consola.

### Una imagen no es reconocida

Desactivar la detección automática solo si conoce la máquina y el formato correctos. De lo contrario, prueba el **Visualización** pestaña para inspeccionar la imagen a un nivel inferior.

Compruebe si la fuente es una captura de flujo crudo, una imagen sectorial, un contenedor comprimido, o un archivo no relacionado con una extensión engañosa. Nunca renombre una extensión meramente para la detección de la fuerza; la conversión debe interpretar correctamente la estructura de la fuente.

### La emulación no comienza

Verificar la configuración guardada, versión emulador instalada, seleccionada ROM, vías de almacenamiento y compatibilidad modelo. Revise el registro de la aplicación para los detalles completos del error.

Regreso temporal CPU, RAM, vídeo y almacenamiento a una base simple compatible con modelos. Si la base comienza, restaurar un ajuste personalizado a la vez. Un estado salvado creado con otra versión emulador o definición de máquina también puede fallar incluso cuando una bota limpia funciona.

### Un atajo o entrada no funciona

Revise ambos el mundo **Emulación Atajos** página y la página de teclado, ratón o controlador específico de la máquina. Resolver cualquier asignación marcada como conflictiva.

Si el ratón es capturado, utilice el atajo de liberación mostrado en la barra de herramientas de máquina de correr. Si un controlador fue conectado después de que se abrieron Opciones, vuelva a ejecutar la detección del controlador antes de asignarlo.

### Un comando falla inesperadamente

1. Lea la salida de consola en vivo.
2. Abierto **Historia de la operación** para el registro guardado completo.
3. Confirme el controlador seleccionado, la unidad, el perfil, el motor y las vías de archivo.
4. Exportar el registro correspondiente si debe ser compartido para el diagnóstico.

### Cepillas de audio o pausas

Aumentar latencia de audio de emulación, cerca CPU- aplicaciones intensivas, y devolver el juego de imágenes de vídeo y la aceleración a sus valores anteriores. Verifique que se selecciona el dispositivo de audio de Windows. Cambiar una configuración a la vez para que la corrección efectiva sea identificable.

### La pantalla de emulación está en blanco o lento

Resolución de retorno y modo de línea **Automático**, deshabilitar el corte de marco y la fijación de flicker temporalmente, y probar el renderizador de trabajo anterior. Confirme que el configurado ROM y los medios de arranque insertados son válidos. El FPS indicador ayuda a distinguir un problema de rendimiento de una máquina que simplemente no ha arrancado.

### A read contains unstable tracks

Repetir la lectura a un nuevo nombre de archivo, aumentar las revoluciones cuando sea apropiado, y comparar las pistas afectadas. Limpie los cabezales de la unidad usando un procedimiento correcto e inspeccione el disco para daño físico. No lea repetidamente los medios de comunicación visiblemente rotos o dañados, porque otros pases pueden empeorarlo.

## Glosario

| Mandato | Significado GW GUI |
|---|---|
| Controlador | El Greaseweazle interfaz de hardware conectado USB |
| Drive | La unidad de disquete físico adjunta al controlador |
| Motor | La implementación seleccionada para realizar una operación |
| Flux | Información de tiempo que representa transiciones magnéticas leídas de un disco |
| Imagen cruda | Una captura que conserva información de disco de bajo nivel, como SCP |
| Imagen del sector | Una representación decodificada organizada en sectores lógicos |
| Revolución | Una rotación completa muestra al leer una pista |
| Cilindro | Posición de cabeza radial; un cilindro puede contener una pista en cada lado |
| Head | El lado del disco seleccionado por la unidad física |
| Perfil | Una configuración reutilizable para una operación |
| ROM | Imagen de firmware requerida por una máquina emulada |
| Estado salvado | Una instantánea del estado de la máquina del emulador en funcionamiento |
| Renderer | El backend gráfico utilizado para mostrar la salida de emulación |

## Referencia rápida

| Si quieres... | Ve a... |
|---|---|
| Preserve un disco físico | **Leer** |
| Pon una imagen de nuevo en un disco | **Escriba** |
| Producir otro formato de imagen | **Conversión** |
| Inspeccionar pistas o anomalías de flujo | **Visualización** |
| Buscar archivos dentro de una imagen | **Disk Explorer** |
| Comprobar comunicación del controlador | **Herramientas Información de control** |
| Rotación de la unidad de medición | **Herramientas Velocidad de conducción** |
| Revisar un comando anterior | **Historia de la operación** |
| Configurar hardware | **Opciones &quot; Controladores y unidades** |
| Seleccione las implementaciones | **Opciones &quot; Motores** |
| Crear o editar una máquina emulada | **Opciones &quot; Emulación** |
| Iniciar una máquina guardada | **Emulación** |
