# Historias de Usuario — Agente de Trading MNQ

**Versión:** 1.0.0  
**Fecha:** 2025-11-06  
**Objetivo del Proyecto:** Sistema productivo para generar ingresos mediante cuentas Apex Trader Funding

---

## Formato de Historias

```
ID: US-XXX
Título: [Nombre descriptivo]
Como: [Rol]
Quiero: [Funcionalidad]
Para: [Beneficio/Valor]

Criterios de Aceptación:
- [ ] Criterio 1
- [ ] Criterio 2

Prioridad: Must/Should/Could/Won't
Estimación: S/M/L/XL
Fase: 1 (MVP) / 2 (Mejoras) / 3 (Expansión)
Dependencias: [IDs de otras US]
```

---

## Épica 1: Ejecución Autónoma (MVP - Pasar Eval)

### US-001: Detección Automática de Setups
**Como** trader, **quiero** que el agente detecte automáticamente setups A-L/B-L/A-S/B-S/B-M en tiempo real, **para** no tener que monitorear gráficos manualmente.

**Criterios de Aceptación:**
- [ ] Detecta correctamente setup A-L cuando precio cierra > PDH/ONH con volumen
- [ ] Detecta correctamente setup B-L en pullback a soporte con rechazo
- [ ] Detecta correctamente setups cortos (A-S, B-S)
- [ ] Detecta correctamente breakout mayor (B-M) con RR ≥2.0
- [ ] Filtra setups según OR15' en rango (25-160 pts)
- [ ] Filtra setups según volumen ≥1.1x mediana
- [ ] Confirmación de setup dentro de ≤2 velas
- [ ] No detecta setups en centro de rango

**Prioridad:** Must  
**Estimación:** L (3-5 días)  
**Fase:** 1  
**Dependencias:** Ninguna

---

### US-002: Cálculo Dinámico de Tamaño de Posición
**Como** trader, **quiero** que el agente calcule automáticamente el tamaño de posición óptimo, **para** arriesgar exactamente $120 por trade respetando reglas APEX.

**Criterios de Aceptación:**
- [ ] Calcula contratos = floor(120 / (stop_pts × $2))
- [ ] Limita a 50 micros si balance < $52,600
- [ ] Limita a 100 micros si balance ≥ $52,600
- [ ] Verifica MAE potencial < $750 antes de ejecutar
- [ ] Rechaza trade si MAE excede límite
- [ ] Ajusta size en tiempo real según balance actualizado

**Prioridad:** Must  
**Estimación:** M (2-3 días)  
**Fase:** 1  
**Dependencias:** Ninguna

---

### US-003: Ejecución de Órdenes Bracket
**Como** trader, **quiero** que el agente coloque automáticamente órdenes bracket (entry + SL + TP), **para** tener protección desde el primer momento sin intervención manual.

**Criterios de Aceptación:**
- [ ] Coloca orden de entrada (LIMIT para pullback, STOP-MARKET para breakout)
- [ ] Coloca SL inmediatamente tras entry
- [ ] Coloca TP inmediatamente tras entry
- [ ] Las 3 órdenes están linkeadas (cancelar una cancela las demás)
- [ ] Confirmación de fill recibida en <5 segundos
- [ ] Timeout y retry si NT8 no responde

**Prioridad:** Must  
**Estimación:** M (2-3 días)  
**Fase:** 1  
**Dependencias:** US-002

---

### US-004: Gestión Dinámica de Stop-Loss
**Como** trader, **quiero** que el agente mueva el SL a breakeven cuando alcance +0.5R, **para** proteger capital y operar risk-free.

**Criterios de Aceptación:**
- [ ] Detecta cuando posición alcanza +0.5R ($60 de ganancia no realizada)
- [ ] Modifica orden SL al precio de entrada automáticamente
- [ ] Confirmación de modificación en <3 segundos
- [ ] Log de evento en journal con timestamp exacto
- [ ] No mueve SL si posición ya cerrada

**Prioridad:** Must  
**Estimación:** S (1 día)  
**Fase:** 1  
**Dependencias:** US-003

---

### US-005: Flat End-of-Day Automático
**Como** trader, **quiero** que el agente cierre todas las posiciones a las 22:00 CET, **para** cumplir regla APEX de day-trading only sin riesgo de overnight.

**Criterios de Aceptación:**
- [ ] A las 22:00 CET, cancela todas las órdenes pendientes
- [ ] Cierra cualquier posición abierta a mercado
- [ ] Confirmación de flat en <10 segundos
- [ ] Registra en journal como "CLOSED_MANUAL" si cierre forzado
- [ ] Funciona 100% de días sin fallo

**Prioridad:** Must (Crítico APEX)  
**Estimación:** S (1 día)  
**Fase:** 1  
**Dependencias:** US-003

---

### US-006: Circuit Breaker Diario
**Como** trader, **quiero** que el agente deje de operar si alcanza -1R de pérdida en el día, **para** proteger capital y evitar revenge trading.

**Criterios de Aceptación:**
- [ ] Monitorea PnL diario en tiempo real
- [ ] Bloquea nuevos trades si PnL ≤ -$120
- [ ] Permite segundo intento solo si primero fue BE o ≤-0.25R
- [ ] Notifica vía Telegram cuando se activa
- [ ] Resetea contador al inicio del día siguiente

**Prioridad:** Must  
**Estimación:** S (1 día)  
**Fase:** 1  
**Dependencias:** US-007

---

### US-007: Journal Automático en SQLite
**Como** trader, **quiero** que todos los trades se registren automáticamente en una base de datos, **para** tener trazabilidad completa y calcular KPIs.

**Criterios de Aceptación:**
- [ ] Registra cada trade con: fecha, setup, entry, SL, TP, contratos, resultado
- [ ] Actualiza MFE/MAE en tiempo real durante posición abierta
- [ ] Calcula resultado en R y USD al cerrar
- [ ] Persiste en SQLite (`data/journal.db`)
- [ ] No pierde datos si agente se reinicia
- [ ] Backup automático diario

**Prioridad:** Must  
**Estimación:** M (2 días)  
**Fase:** 1  
**Dependencias:** Ninguna

---

## Épica 2: Monitoreo y Control (Post-MVP)

### US-008: Panel Visual en NinjaTrader
**Como** trader, **quiero** ver el estado del agente en un panel dentro de NinjaTrader, **para** monitorear sin cambiar de aplicación.

**Criterios de Aceptación:**
- [ ] Panel muestra: modo actual, balance, PnL día, próximo setup
- [ ] Botones: MANUAL, AUTONOMOUS, SIGNALS
- [ ] Indicadores visuales: verde (OK), amarillo (warning), rojo (error)
- [ ] Actualización en tiempo real (<1 segundo)
- [ ] Dibuja niveles PDH/PDL/ONH/ONL en chart
- [ ] Dibuja flechas en entry points

**Prioridad:** Should  
**Estimación:** L (4-5 días)  
**Fase:** 2  
**Dependencias:** US-001, US-003

---

### US-009: Notificaciones Telegram
**Como** trader, **quiero** recibir notificaciones en Telegram de eventos críticos, **para** estar informado sin revisar logs constantemente.

**Criterios de Aceptación:**
- [ ] Notifica: trade ejecutado (entry, SL, TP)
- [ ] Notifica: SL movido a BE
- [ ] Notifica: target alcanzado (resultado final)
- [ ] Notifica: circuit breaker activado
- [ ] Notifica: pérdida de conexión Rithmic/NT8
- [ ] Formato conciso y legible en móvil

**Prioridad:** Should  
**Estimación:** S (1 día)  
**Fase:** 2  
**Dependencias:** US-007

---

### US-010: Post-Market Report Automático
**Como** trader, **quiero** recibir un reporte diario automático al cierre, **para** revisar performance sin consultar journal manualmente.

**Criterios de Aceptación:**
- [ ] Generado automáticamente a las 22:05 CET
- [ ] Incluye: plan vs realidad, resultado, MFE/MAE, observaciones
- [ ] Formato markdown legible
- [ ] Enviado vía Telegram
- [ ] También guardado en `reports/daily_YYYY-MM-DD.md`

**Prioridad:** Should  
**Estimación:** M (2 días)  
**Fase:** 2  
**Dependencias:** US-007, US-009

---

### US-011: Dashboard KPIs
**Como** trader, **quiero** visualizar KPIs clave (Win%, Expectancy, Drawdown) en un dashboard, **para** evaluar performance rápidamente.

**Criterios de Aceptación:**
- [ ] Métricas: Win Rate, Expectancy (R), Max DD (R), Profit Factor, Sharpe
- [ ] Filtros: Día, Semana, Mes, Todo
- [ ] Gráfico equity curve
- [ ] Gráfico distribución de R por trade
- [ ] Accesible vía web browser local

**Prioridad:** Could  
**Estimación:** XL (1 semana)  
**Fase:** 2  
**Dependencias:** US-007

---

## Épica 3: Sistema Guardian (Protección Psicológica)

### US-012: Detector de Patterns Emocionales
**Como** trader, **quiero** que el agente detecte si estoy cayendo en revenge trading, **para** evitar decisiones irracionales.

**Criterios de Aceptación:**
- [ ] Detecta: ≥2 intentos en <30 min tras pérdida
- [ ] Detecta: override manual inmediatamente tras SL
- [ ] Activa cooldown de 30 min
- [ ] Notifica: "Guardian: Patrón emocional detectado, cooldown activo"
- [ ] Bloquea modo MANUAL durante cooldown

**Prioridad:** Should  
**Estimación:** M (2-3 días)  
**Fase:** 2  
**Dependencias:** US-006

---

### US-013: Bloqueo Manual en Tilt
**Como** trader, **quiero** que el agente me bloquee el trading manual si pierdo >0.5R en corto tiempo, **para** protegerme de mí mismo.

**Criterios de Aceptación:**
- [ ] Si PnL cae -$60 en <1 hora → bloquea modo MANUAL
- [ ] Solo permite modo AUTONOMOUS (agente toma control)
- [ ] Notifica: "Guardian: Control manual deshabilitado por protección"
- [ ] Reset automático inicio día siguiente
- [ ] Override solo con confirmación en 2 pasos

**Prioridad:** Could  
**Estimación:** M (2 días)  
**Fase:** 2  
**Dependencias:** US-012

---

## Épica 4: Escalabilidad Multi-Cuenta

### US-014: Gestión de Múltiples Cuentas
**Como** trader, **quiero** operar hasta 5 cuentas Apex simultáneamente, **para** escalar ingresos tras pasar evaluaciones.

**Criterios de Aceptación:**
- [ ] Soporta configuración de N cuentas en `config/accounts.yaml`
- [ ] Cada cuenta: ID, balance, estrategia, modo
- [ ] Journal separado por cuenta
- [ ] Dashboard multi-cuenta con resumen agregado
- [ ] Gestión de riesgo independiente por cuenta
- [ ] No hay cross-contamination entre cuentas

**Prioridad:** Won't (Fase 3)  
**Estimación:** XL (1-2 semanas)  
**Fase:** 3  
**Dependencias:** US-001 a US-007

---

### US-015: Orquestador Multi-Instancia
**Como** trader, **quiero** que un orquestador central coordine múltiples instancias del agente, **para** distribuir carga y evitar conflictos.

**Criterios de Aceptación:**
- [ ] API Gateway (FastAPI) centralizado
- [ ] Load balancer entre instancias
- [ ] Base de datos compartida (PostgreSQL)
- [ ] Sincronización de estado en tiempo real
- [ ] Failover automático si instancia falla

**Prioridad:** Won't (Fase 3+)  
**Estimación:** XL (2-3 semanas)  
**Fase:** 3  
**Dependencias:** US-014

---

## Épica 5: Optimización y Machine Learning

### US-016: Backtesting Automático
**Como** trader, **quiero** ejecutar backtests con datos históricos automáticamente, **para** validar cambios en estrategia antes de live.

**Criterios de Aceptación:**
- [ ] Script `backtest.py --start YYYY-MM-DD --end YYYY-MM-DD`
- [ ] Carga datos históricos de MNQ (1min bars)
- [ ] Simula detección y ejecución de trades
- [ ] Genera reporte con métricas clave
- [ ] Guarda resultados en `backtest_results/`
- [ ] Walk-forward analysis automático

**Prioridad:** Should  
**Estimación:** L (4-5 días)  
**Fase:** 2  
**Dependencias:** US-001, US-002

---

### US-017: Optimización de Parámetros
**Como** trader, **quiero** encontrar los parámetros óptimos de estrategia automáticamente, **para** maximizar expectancy sin overfitting.

**Criterios de Aceptación:**
- [ ] Grid search sobre parámetros: R_risk, min_RR, vol_factor, OR15_range
- [ ] Usa walk-forward para validar out-of-sample
- [ ] Penaliza overfitting (max trades, max params)
- [ ] Genera reporte con superficie de parámetros
- [ ] Sugiere configuración óptima

**Prioridad:** Could  
**Estimación:** XL (1 semana)  
**Fase:** 3  
**Dependencias:** US-016

---

### US-018: Machine Learning para Timing
**Como** trader, **quiero** usar ML para mejorar timing de entrada, **para** incrementar expectancy manteniendo framework de reglas.

**Criterios de Aceptación:**
- [ ] Feature engineering sobre datos históricos
- [ ] Modelo RL (Reinforcement Learning) para optimizar entry timing
- [ ] Backtesting con modelo vs sin modelo
- [ ] Mejora mínima de +0.2R en expectancy
- [ ] Modelo reentrenable con datos recientes

**Prioridad:** Won't (Fase 3+)  
**Estimación:** XL (2-3 semanas)  
**Fase:** 3  
**Dependencias:** US-016, US-017

---

## Priorización Global (Roadmap)

### Fase 1: MVP - Pasar Evaluación (8-12 semanas)
```
Must Have:
✅ US-001: Detección Automática Setups
✅ US-002: Cálculo Tamaño Posición
✅ US-003: Ejecución Órdenes Bracket
✅ US-004: Gestión SL Dinámico
✅ US-005: Flat EOD
✅ US-006: Circuit Breaker
✅ US-007: Journal Automático

Objetivo: Sistema funcional para pasar eval Apex en 2-4 semanas
```

### Fase 2: Mejoras y Control (4-8 semanas)
```
Should Have:
→ US-008: Panel Visual NT8
→ US-009: Notificaciones Telegram
→ US-010: Post-Market Report
→ US-012: Detector Patterns Emocionales
→ US-016: Backtesting Automático

Could Have:
→ US-011: Dashboard KPIs
→ US-013: Bloqueo Manual Tilt

Objetivo: Mejorar UX y monitoreo para operación sostenida
```

### Fase 3: Escalabilidad (3-6 meses)
```
Won't Have (ahora):
→ US-014: Multi-Cuenta
→ US-015: Orquestador
→ US-017: Optimización Parámetros
→ US-018: Machine Learning

Objetivo: Escalar a 5-20 cuentas con optimización continua
```

---

## Criterios de Éxito del Proyecto

### Fase 1 Exitosa Si:
- [ ] Agente pasa evaluación Apex ($3k profit) en ≤6 semanas
- [ ] Zero violaciones de reglas APEX
- [ ] Win Rate ≥50%, Expectancy ≥+0.5R
- [ ] Uptime 100% durante horas mercado
- [ ] Max Drawdown ≤-1.5R

### Proyecto Exitoso Si:
- [ ] Genera ≥$2,000/mes netos consistentes (3+ meses)
- [ ] Opera ≥3 cuentas funded simultáneamente
- [ ] Requiere <1 hora/día de supervisión
- [ ] Sistema estable, bugs mínimos

---

**Última actualización:** 2025-11-06
