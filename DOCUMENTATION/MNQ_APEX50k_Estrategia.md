
# Estrategia Operativa — MNQ · Cuenta APEX **$50k** (versión 2025-11-05)

**Rol:** Yo analizo; tú ejecutas.  
**Marco:** Intradía en MNQ (Micro E-mini Nasdaq), foco en sesión regular USA (RTH).  
**Objetivo mensual orientativo:** +8R en modo Ligero; disciplina sobre drawdown y cumplimiento APEX.

---

## 1) Límites y reglas de la **cuenta APEX $50k** (resumen operativo)

- **Max loss / Trailing Threshold (evaluación/PA):** **$2,500**; el “safety net” queda en **$52,600** y, en **Rithmic**, el trailing **deja de seguir** cuando el *threshold balance* alcanza el profit target (no el balance), p.ej. 50k→53k (eval). En PA, el trailing se fija al llegar al safety net (+$100).  
  Fuentes: APEX Help — *Evaluation Rules* y *Trailing Drawdown* citeturn0search0turn0search2
- **Contratos máximos del plan 50k:** **10 minis** **o 100 micros**. **Regla de escalado:** hasta alcanzar el safety net, solo se puede usar **la mitad** (5 minis / 50 micros).  
  Fuentes: Sitio oficial de planes y *Contract Scaling Rule* citeturn1search0turn1search2
- **Regla 30% MAE por trade (PA):** el PnL abierto negativo **no puede exceder el 30%** del *profit balance* al inicio del día; si es cuenta nueva/beneficio bajo, se toma el 30% del trailing threshold → **$750** en 50k.  
  Fuente: APEX Help — *30% Negative P&L (MAE)* citeturn2search8
- **Stops obligatorios** y **RR máx. riesgo:recompensa 5:1** (no usar el umbral de la cuenta como stop).  
  Fuente: APEX Help — *PA & Compliance / Prohibited activities* citeturn1search6turn2search4
- **One‑Direction Rule:** no hedging; una sola dirección a la vez en el mismo/correlacionado.  
  Fuente: APEX Help — *One‑Direction Rule* citeturn2search2
- **Payouts (PA):** saldo mínimo para solicitar: **$52,600** en 50k; mínimo **$500** por pago.  
  Fuente: APEX Help — *PA Payout Parameters* citeturn0search7

> **Traducción operativa:** dimensiona el riesgo de forma que (a) nunca superes medio límite de contratos hasta $52,600, (b) cada trade quede muy por debajo de **$750** de MAE, y (c) tu stop siempre esté **predefinido**.

---

## 2) Gestión de riesgo y tamaño de posición

- **R (riesgo fijo por trade):** **$120** (configurable).  
- **Fórmula (MNQ punto = $2):** `contratos = floor( R / (stop_points × 2) )`  
  - *Ejemplo:* stop = 20 pts → riesgo por micro = 20×$2 = **$40** → contratos = floor(120/40) = **3 micros**.
- **Límites de disciplina:**
  - Intentos por día: **1** (segundo intento solo si el 1.º termina **BE** o ≤ **−0.25R**).
  - **Cap diario:** −**1R**. **Hard‑stop mensual:** −**3R**.
  - **No exceder** 50 micros hasta saldo EOD ≥ **$52,600** (scaling). citeturn1search2
  - **MAE APEX**: cada stop/gestión debe garantizar que el PnL abierto negativo **< $750** en cuentas sin profit. citeturn2search8

**Política de RR:** trabajar con **RR esperado ≥ 1.5:1** (objetivo principal **1.8–2.2R**). Cumple sobradamente el máximo APEX (riesgo:recompensa ≤ 5:1). citeturn1search6

---

## 3) Entradas, salidas y validaciones (reglas ejecutables)

### Setups
| Código | Dirección | Disparador (confirmación) | Entrada | Stop (invalidación) | Objetivo |
|---|---|---|---|---|---|
| **A‑L** | Largo | Cierre **>** nivel clave (ONH/PDH/resistencia) **+** volumen | Cierre válido o primer pullback | Bajo nivel/swing | TP1 **+0.5R** → SL a **BE**; objetivo **1.8–2.2R** |
| **B‑L** | Largo | Pullback a **soporte validado** + rechazo + volumen | Rechazo/patrón en lvl | Bajo soporte | Igual que A‑L |
| **A‑S** | Corto | Cierre **<** nivel (ONL/PDL/soporte) **+** volumen | Cierre válido o primer pullback | Sobre nivel/swing | Igual que A‑L |
| **B‑S** | Corto | Pullback a **resistencia validada** + rechazo + volumen | Rechazo/patrón en lvl | Sobre resistencia | Igual que A‑L |
| **B‑M** | Breakout mayor | Ruptura con **cierre + volumen** | Cierre o *retest* | Detrás del extremo roto | **RR ≥ 2.0** (máx 2/semana) |

**Filtros previos (checklist mínimo):**  
1) RR proyectado ≥ **1.5**. 2) Volumen ≥ **1.1×** de la mediana (1m, 20 velas). 3) Evitar centro de rango; operar a favor de estructura del día. 4) Confirmación ≤ **2 velas** tras señal.

**Gestión estándar:**  
- **Colocación bracket** (entry/SL/TP). Al alcanzar **+0.5R**, mover SL a **BE**.  
- Si momentum claro, permitir *runner* hasta 2.3–2.5R con trailing técnico **sin** elevar MAE más allá de $750. citeturn2search8

---

## 4) Ejemplos numéricos (MNQ)

- **Ejemplo 1 — A‑L (pullback):**  
  Nivel PDH 18,000. Entrada 18,015; **SL 17,995** (20 pts = $40/µ). **R=$120** → **3 micros**.  
  TP1 a **+0.5R**: 10 pts; mover SL a BE. Objetivo **2.0R**: +40 pts (18,055). PnL esperado ≈ **$240**.

- **Ejemplo 2 — A‑S (fallo de soporte):**  
  Entrada 17,980; SL 18,000 (20 pts); 3 micros. BE si +10 pts; salida parcial en **+1.8R** (36 pts).

> Ambos casos respetan: contratos ≤50 (scaling), MAE ≪ $750, y RR ≥ 1.5:1. citeturn1search2turn2search8

---

## 5) Reglas de ejecución diaria (resumen)

1. **Pre‑open:** niveles PDH/PDL, ONH/ONL, S/R; calcula tamaño con `R=$120`.  
2. **Apertura:** mide OR15' y volumen base; no entrar si RR < 1.5.  
3. **Durante:** esperar **confirmación** ≤2 velas; colocar bracket completo; **sin hedging**; 1 intento/día (2º solo BE/≤−0.25R). citeturn2search2  
4. **Cierre:** *Flat EOD* por control de riesgo; actualiza **journal** y KPIs.  
5. **Cumplimiento APEX:** respeta **50 micros** hasta $52,600 y **MAE 30%** (o $750). citeturn1search2turn2search8

---

## 6) **Trading journal** (campos y normas)

- **Campos (CSV):** `fecha, modo, setup, entry, stop, target, contratos, resultado(TP/SL/BE/no), R, PnL_usd, MFE_pts, MAE_pts, notas`  
- **Reglas:** registrar **cada** trade el mismo día; adjuntar *screenshot* del momento de la entrada y de la salida; marcar si hubo penalización o cambio de plan.  
- **KPIs mínimos:** Win%, Expectancy (R), Max DD (R), Nº días operados, Penalizaciones.

---

## 7) Parametrización rápida

- **Riesgo por trade (R):** default **$120** (ajustable $60–$150 según volatilidad).  
- **Max micros por trade (mientras no haya safety net):** `min(50, tamaño_por_stop)`; tras $52,600 → `min(100, tamaño_por_stop)`. citeturn1search2
- **Stop típico:** 15–30 pts (según estructura). **Nunca** fijar un stop que pueda llevar el PnL abierto más allá de **$750** con tu tamaño actual. citeturn2search8

---

## 8) Avisos de cumplimiento APEX (para no fallar)

- **No** exceder contratos máximos ni “sumar” exposición con micros para saltarte el límite. citeturn1search6  
- **Siempre** con stop definido; **prohibido** usar el *trailing* de la cuenta como stop. citeturn2search4  
- **Una dirección a la vez** (sin hedging). citeturn2search2  
- **Payout**: no pedir retiros por debajo de **$52,600** en 50k; mínimo **$500**. citeturn0search7

---

## 9) Plantilla “Plan de trade” (copiar/pegar)

```
Fecha/TZ: {YYYY-MM-DD} Europe/Madrid
Instrumento: MNQ    Modo: Ligero/Conservador
Setup: A-L / B-L / A-S / B-S / B-M

Nivel clave: {PDH/PDL/ONH/ONL/S-R}
Entrada: {precio}  Stop: {precio}  Target: {precio}  RR: {x.x}
R=$120  Stop_pts={N}  Riesgo/µ=${N*2}  Contratos={floor(120/(N*2))}

Checklist: RR≥1.5 | Vol≥1.1x | Confirm≤2 velas | No centro de rango
Gestión: TP1 +0.5R → SL=BE | Objetivo 1.8–2.2R | Runner si momentum

Cumplimiento APEX: contratos≤50 hasta $52,600 | MAE<$750 | One‑Direction
Resultado: TP / SL / BE / NoTrade   PnL={±$}   R={±x.x}   Notas={...}
```

---

**Fin del documento.**
