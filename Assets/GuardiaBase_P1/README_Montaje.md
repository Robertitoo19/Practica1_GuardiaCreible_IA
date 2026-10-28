# GuardiaBase_P1 — Montaje del proyecto

**Práctica 1 · «Guardia creíble»** — Inteligencia Artificial (IA3)
Grado en Diseño y Desarrollo de Videojuegos · UDIT · Curso 2026/2027
Dr. Luis Peña Sánchez

---

## 0. Qué es esto y qué no

Esto es **el andamio**, no la práctica. Contiene:

- La **arquitectura de capas** ya montada y funcionando (el ciclo Percibir → Pensar → Actuar).
- El ***runtime* del árbol de comportamiento**, terminado. **No lo modifiquéis.**
  Lo explicamos línea a línea el 28 de octubre, en la sesión 9.
- La **capa de movimiento** terminada.
- Un **estado de ejemplo** (`EstadoPatrullar`) y un **nodo hoja de ejemplo** (`Patrullar`),
  para que veáis la forma.
- Un sensor de visión que **está mal a propósito** (ver §5).

No contiene escena. **La escena la montáis vosotros**, y estas son las instrucciones.
Es media hora, y es la media hora en la que se entiende por qué el `NavMesh` se hornea y qué
es una capa de oclusión.

---

## 1. Requisitos

| | |
|---|---|
| **Unity** | **6000.1 o superior** (el proyecto está probado en `6000.3.20f1`) |
| **Paquete** | **AI Navigation** (`com.unity.ai.navigation`) — *Window > Package Manager > Unity Registry* |
| **Render pipeline** | Indiferente. El proyecto no depende de ninguno |

> En Unity 6 el `NavMeshSurface` **ya no viene de serie**: vive en el paquete *AI Navigation*.
> Si no lo instaláis, no encontraréis el componente y perderéis media tarde buscándolo.

---

## 2. Estructura de los scripts

```
Assets/GuardiaBase/Scripts/
│
├── Nucleo/
│   ├── ClavesBB.cs         ← el contrato del blackboard (las 7 claves)
│   ├── Contratos.cs        ← ISensor e IDecisor. Aquí vive la intercambiabilidad
│   └── GuardiaAgente.cs    ← el ciclo Percibir → Pensar → Actuar
│
├── Percepcion/
│   ├── NivelSospecha.cs    ← tranquilo / sospecha / alerta
│   └── SensorVision.cs     ⚠️ VUESTRO TRABAJO PRINCIPAL (está mal a propósito)
│
├── Movimiento/
│   ├── LocomocionNavMesh.cs ✅ terminado — NO lo toquéis
│   └── RutaPatrulla.cs      ✅ terminado
│
├── FSM/
│   ├── StateMachine.cs      ✅ el patrón Estado de la sesión 7
│   ├── DecisorFSM.cs        ← monta la máquina
│   └── Estados/
│       └── EstadoPatrullar.cs  ← EJEMPLO COMPLETO. Los otros 3 son vuestros
│
├── BT/
│   ├── BehaviorTree.cs      ✅ el runtime. NO lo toquéis
│   └── DecisorBT.cs         ← VUESTRO ÁRBOL
│
└── Depuracion/
    └── GuardiaGizmos.cs     ✅ cono, barra de sospecha y última posición
```

### La regla que lo sostiene todo

```
PERCEPCIÓN  ──escribe──►  BLACKBOARD  ──lee──►  DECISIÓN
                               ▲                    │
                               └───escribe destino──┘
                                        │
                                        ▼
                                   MOVIMIENTO
```

- Un **sensor** mira el mundo y escribe. No decide.
- Un **decisor** (FSM o árbol) lee y escribe intenciones. **No mira el mundo y no mueve.**
- La **locomoción** lee el destino y mueve. Es la única que toca el `NavMeshAgent`.

Si respetáis esto, el apartado D del enunciado —cambiar de FSM a árbol— es un desplegable
del inspector. Si no, es una tarde.

---

## 3. Montar la escena, paso a paso

### 3.1 El suelo

1. *GameObject > 3D Object > Plane*. Nombradlo `Suelo`.
2. Escala **(4, 1, 4)** → 40 × 40 metros. Suficiente para que patrullar tenga sentido.
3. Marcadlo como **Static** (arriba a la derecha del inspector).

### 3.2 Los muros — *la parte que importa*

*GameObject > 3D Object > Cube*, escalados a modo de tabiques (por ejemplo `(1, 3, 14)`),
y **marcados como Static**.

Ponedlos todos en una **capa propia, `Muros`**: *Layer > Add Layer…*, escribid `Muros` en un
hueco libre y asignádsela a cada muro. Es la capa de oclusión, la que luego marcaréis en el
sensor (3.6).

Poned **al menos cuatro**, formando esquinas y un par de pasillos con recodos.

> ⚠️ **Esto no es decoración.** El escenario no se puntúa por bonito: se puntúa por que
> **tenga oclusiones que hagan interesante la percepción**. En un descampado, un sensor malo
> y uno bueno se comportan igual, y entonces vuestro trabajo no se ve. Aseguraos de que hay
> sitios desde donde el guardia **no** puede veros aunque estéis cerca.

### 3.3 El jugador

1. *GameObject > 3D Object > Capsule*, nombrada `Jugador`.
2. **Tag `Player`** (el sensor lo busca por ese tag si no le asignáis el objetivo a mano).
3. Ponedle un control de movimiento cualquiera. Vale un script de diez líneas con
   `Input.GetAxis`: **el jugador no se evalúa**.
4. Dejadlo en la capa **`Default`**, nunca en `Muros`: si el jugador estuviera en una capa
   que ocluye, los *raycasts* chocarían con él y el guardia no lo vería nunca.

> ⚠️ La cápsula de Unity mide 2 m y tiene el **pivote en el centro**, no en los pies. Para
> los *raycasts* a cabeza, torso y pies (apartado A), contad desde ahí: **+0,8**, **0** y
> **−0,9** en Y. Las cifras +1,6 / +0,9 / +0,1 del comentario de `SensorVision` son para un
> personaje con el pivote en los pies.

### 3.4 Los puntos de patrulla

1. Un objeto vacío `Waypoints` como padre.
2. Dentro, cuatro o cinco vacíos `WP_0`, `WP_1`… repartidos por el mapa.
3. Colocadlos **al otro lado de los muros** entre sí, para que las rutas sean largas y el
   guardia se cruce con las oclusiones.

### 3.5 El guardia

1. *GameObject > 3D Object > Capsule*, nombrada `Guardia`.
2. **Quitadle el `Capsule Collider`**: el `NavMeshAgent` ya gestiona su cuerpo, y dejarlo
   provoca empujones raros entre agentes.
3. Añadid, en este orden:

| Componente | Ajustes recomendados |
|---|---|
| `NavMeshAgent` | Speed **3.5** · Angular Speed **240** · Stopping Distance **0.5** |
| `RutaPatrulla` | Arrastrad los `WP_*` al array **Puntos** |
| `SensorVision` | Ver 3.6 |
| `LocomocionNavMesh` | Nada que tocar |
| `GuardiaAgente` | **Cerebro** = `MaquinaDeEstados` · Frecuencia **10** Hz |
| `GuardiaGizmos` | Nada que tocar |

4. Como hijo del guardia, un vacío **`Ojos`** en `(0, 0.7, 0)`. Arrastradlo al campo `Ojos`
   del sensor.

> Mirar desde el *transform* del guardia significa mirar **desde la cintura**, porque el pivote
> de la cápsula está en el centro. Los *raycasts* chocan contra cualquier obstáculo bajo y el
> guardia se queda ciego sin motivo aparente.

### 3.6 Ajustes del sensor

| Campo | Valor de partida | Comentario |
|---|---|---|
| `Objetivo` | el `Jugador` | O dejadlo vacío y que lo busque por tag |
| `Ojos` | el hijo `Ojos` | |
| `Alcance` | 15 | |
| `AnguloVision` | 70 | Es el cono **completo**, no el semiángulo |
| `CapasQueOcluyen` | **solo `Muros`** | ⚠️ Si lo dejáis en `Nothing`, **no habrá oclusión**; si marcáis `Default`, el jugador se tapa a sí mismo |

Los cinco campos del medidor (`VelocidadSubida`, `VelocidadBajada`, los dos umbrales y la
`Histeresis`) son **vuestros**: ajustarlos y justificar por qué es el apartado G de la nota.

### 3.7 Hornear el NavMesh

1. En el `Suelo`, *Add Component >* **`NavMeshSurface`**.
2. `Collect Objects` = **All**.
3. Botón **Bake**.
4. Comprobad que la malla azul **rodea los muros** en lugar de atravesarlos. Si los atraviesa,
   es que los muros no están marcados como *Static* o no tienen `Collider`.

### 3.8 Segundo guardia

El enunciado pide **al menos dos**. Duplicad el guardia y **dadle otra ruta**: dos guardias
haciendo exactamente lo mismo no demuestran nada, y con rutas distintas empezaréis a ver
solos por qué apetece un *blackboard* compartido (apartado opcional).

---

## 4. Comprobar que funciona

Dadle a **Play**. Deberíais ver:

- El guardia recorriendo los waypoints.
- El **cono de visión** dibujado (activad *Gizmos* en la vista de escena).
- La **barra de sospecha** sobre su cabeza.
- Un **cubo naranja** en la última posición conocida cuando os pierda de vista.

Y en el inspector, en pleno *Play*, cambiad **Cerebro** de `MaquinaDeEstados` a
`ArbolDeComportamiento`. **El guardia debe seguir comportándose igual.** Ese cambio en
caliente es exactamente lo que os voy a pedir que enseñéis en el vídeo de la entrega.

Activad también **Mostrar Traza** en el `GuardiaAgente`: el árbol os dirá por qué rama ha
bajado en cada *tick*. Un árbol sin traza es imposible de depurar.

---

## 5. ⚠️ Lo que está mal a propósito

**`SensorVision` detecta por distancia y nada más.** Ahora mismo el guardia os ve a través
de las paredes y de espaldas, y pasa de 0 a alerta máxima en un *frame*.

Es el primero de los errores típicos de la sesión 4, y está puesto ahí para que lo veáis con
vuestros propios ojos antes de arreglarlo. Vuestro trabajo:

| Etapa | Estado | Dónde |
|---|---|---|
| 1 · Alcance | ✅ hecho | `ComprobarVision()` |
| 2 · Ángulo | ❌ **TODO** | `ComprobarVision()` |
| 3 · Oclusión con varios puntos | ❌ **TODO** | `ComprobarVision()` |
| Medidor de sospecha (3 factores, decaimiento) | ❌ **TODO** | `ActualizarSospecha()` |
| Umbrales con histéresis | ✅ hecho, os lo doy | `ActualizarNivel()` |

Y en la decisión:

| | Estado |
|---|---|
| `EstadoPatrullar` | ✅ ejemplo completo |
| `EstadoInvestigar`, `EstadoPerseguir`, `EstadoBuscar` | ❌ **vuestros** |
| Rama de patrulla del árbol | ✅ ejemplo completo |
| Ramas de alerta, sospecha y búsqueda | ❌ **vuestras** |

---

## 6. Fallos frecuentes de montaje

| Síntoma | Causa casi segura |
|---|---|
| El guardia no se mueve y no hay errores | No habéis horneado el `NavMeshSurface`, o el agente está fuera de la malla |
| `"SetDestination" can only be called on an active agent` | El agente no está sobre el NavMesh. Bajadlo al suelo y re-hornead |
| El guardia ve a través de las paredes | `CapasQueOcluyen` está en `Nothing`, los muros no están en la capa `Muros` o no tienen `Collider` |
| El guardia no ve nunca | `CapasQueOcluyen` incluye la capa **del propio jugador**: el *raycast* choca con él y lo interpretáis como oclusión |
| Warning «no tiene RutaPatrulla con puntos» | El array `Puntos` está vacío |
| Cambia de estado sesenta veces por segundo | No estáis usando la histéresis, o el medidor sigue siendo el interruptor de partida |
| El guardia persigue eternamente a alguien a quien no ve | Habéis usado `Sequence` donde hacía falta `Monitor`. Es **el** error del árbol, y lo vemos en la sesión 9 |

---

## 7. Antes de entregar

- [ ] Compila sin errores **ni warnings** al abrir el proyecto en limpio
- [ ] Las tres etapas de visión implementadas, con *raycast* a **varios puntos**
- [ ] Medidor con **≥3 factores**, decaimiento asimétrico y umbrales con histéresis
- [ ] Los **cuatro estados** de la FSM y las **cuatro ramas** del árbol
- [ ] Cambiar `Cerebro` en el inspector **no** exige tocar percepción ni locomoción
- [ ] Ningún `Raycast`, `OverlapSphere` ni `SetDestination` fuera de su capa
- [ ] **Dos guardias** con rutas distintas
- [ ] Memoria, `.unitypackage` (o repo sin `Library/`) y vídeo de 2-4 min, en **un ZIP**

Entrega: **viernes 20 de noviembre de 2026, 23:59**, en el Campus Virtual.
