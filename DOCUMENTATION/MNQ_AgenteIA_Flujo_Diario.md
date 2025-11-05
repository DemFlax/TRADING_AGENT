
# Agente IA — Flujo Diario Operativo (MNQ · APEX $50k)
**Versión:** 2025-11-05 · **TZ:** Europe/Madrid · **Principio:** *Yo analizo; tú ejecutas.*

## Parámetros fijos
- **Activo:** MNQ (Micro E-mini Nasdaq). **Sesión:** RTH (15:30–22:00).  
- **R por trade:** **$120**. **RR objetivo:** ≥ **1.5:1** (meta **1.8–2.2R**).  
- **Modo por defecto:** **Ligero** (vol ≥ 1.1×; OR15' 25–160 pts; confirmación ≤ 2 velas).

## Límites APEX aplicados (operativos)
- **Trailing/Máx pérdida de referencia:** $2,500 (control interno).  
- **Scaling:** hasta saldo EOD ≥ **$52,600** ⇒ **≤ 50 micros**; después **≤ 100 micros**.  
- **MAE por trade (control):** **≤ $750**.  
- **Stops obligatorios** (no usar el umbral de la cuenta como stop).  
- **One‑Direction:** sin hedging (una dirección a la vez).

## Setups (disparadores y gestión)
- **A‑L (long agresivo):** cierre > nivel (ONH/PDH/S-R) + volumen; **entry** en cierre validado o 1er pullback; **SL** bajo nivel.  
- **B‑L (long conservador):** pullback a **soporte validado** + rechazo + volumen; **SL** bajo soporte.  
- **A‑S (short agresivo):** cierre < nivel (ONL/PDL/S-R) + volumen; **SL** sobre nivel.  
- **B‑S (short conservador):** pullback a **resistencia validada** + rechazo + volumen; **SL** sobre resistencia.  
- **B‑M (breakout mayor):** ruptura con **cierre + volumen**; **RR ≥ 2.0**; **≤ 2/sem**.  
**Gestión común:** **TP1 +0.5R ⇒ SL→BE**; objetivo **1.8–2.2R**; runner si momentum sin violar MAE.

## Tamaño de posición
- **MNQ:** 1 punto = $2. `contratos = floor( 120 / (stop_pts × 2) )`.  
- Aplicar **scaling/MAE**: `contratos = min(contratos, 50|100)` y verificar que `stop_pts×2×contratos ≤ 750`.

## Flujo PASO A PASO (día de operación)
1. **14:45–15:05 — Arranque**: sincroniza reloj/feed; carga balance/journal.  
   - Si **USA RTH cerrado** (finde/festivo) ⇒ **FIN**.  
2. **15:05–15:14 — Contexto**: calcula **PDH/PDL, ONH/ONL, mid, S/R**; fija modo **Ligero** y **R=$120**.  
3. **15:15 — Tarjeta Previa**: publica **A‑L/B‑L/A‑S/B‑S/B‑M** (Entry/SL/TP/Invalidación, RR, contratos, checklist).  
4. **15:16 — Control**: verifica envío **único y on‑time**; si faltó/duplicado (y USA abierto) ⇒ **penalización +1**.  
5. **15:30–15:45 — OR15' & Volumen**: calcula **OR15'** y **mediana de volumen 1m (20 velas)**.  
6. **15:45–20:30 — Escaneo continuo** (cada nueva vela):  
   - Filtros previos: `attempts<1` (o 2º si 1º=BE/≤−0.25R), `PnL_day>-1R`, `PnL_month>-3R`,  
     `OR15'` en rango, `vol_factor≥1.1`, `RR≥1.5`, `confirmación≤2 velas`, **scaling/MAE OK**.  
   - Detecta setup (A‑L/B‑L/A‑S/B‑S/B‑M).  
7. **Sizing**: calcula `stop_pts`, `contratos` y aplica **scaling/MAE**.  
8. **Orden (bracket)**: coloca **entry/SL/TP** (breakout: stop‑market/limit; pullback: limit).  
9. **Gestión**: en **+0.5R ⇒ SL→BE**; objetivo **1.8–2.2R**; runner opcional con trailing.  
10. **Intentos/Cap**: si **1º = BE o ≤ −0.25R** ⇒ permitir **2º** y nada más; si día **≤ −1R** ⇒ **terminar**.  
11. **22:00 — Cierre**: **Flat EOD**; cancela órdenes pendientes.  
12. **Post‑mercado**: informe breve (vs plan, escenario, resultado TP/SL/BE/no, causas, lecciones, observaciones).  
13. **Journal/KPIs**: registra `fecha, modo, setup, entry, stop, target, contratos, resultado, R, PnL_usd, MFE_pts, MAE_pts, notas`; actualiza **Win%**, **Expectancy (R)**, **Max DD (R)**, **penalizaciones** y **estado de scaling**.

## Pseudocódigo mínimo
```
if not US_RTH_open(today): exit()

state = load_state()             # balance, month_R, day_R, attempts, journal
mode, R = "Ligero", 120
levels  = compute_levels()       # PDH/PDL/ONH/ONL/S-R
or15    = opening_range_15m()
volref  = median_volume_1m(20)

publish_pre_card(15:15)

for bar in stream_bars(15:45, 20:30):
    if day_R <= -1 or month_R <= -3: break
    if attempts >= 1 and not allow_second_attempt(): continue

    setup = detect_setup(bar, levels, volref)
    if not setup: continue
    if projected_RR(setup) < 1.5: continue
    if not confirms_within(setup, 2): continue

    stop_pts = calc_stop_points(setup)
    contracts = floor(R / (stop_pts*2))
    contracts = apply_scaling(contracts, balance, safety_net=52600)
    if stop_pts*2*contracts > 750: continue

    place_bracket(setup.entry, setup.sl, setup.tp, contracts)

    while trade_open():
        if unrealized_R() >= 0.5: move_sl_to_BE()
        trail_runner_if_momentum()
        if time_is_close(): exit_market()

flat_eod(); cancel_pends()
write_postmarket(); log_journal(); update_kpis()
```

## Ejemplos numéricos (rápidos)
- **A‑L:** SL 20 pts → $40/µ → `floor(120/40)=3` micros; **TP1 +0.5R** (10 pts), **TP final ≈ 2.0R** (40 pts).  
- **A‑S:** SL 25 pts → $50/µ → `floor(120/50)=2` micros; gestión igual; **MAE** con 2 µ = $100 ≪ $750.

---

**Fin.**
