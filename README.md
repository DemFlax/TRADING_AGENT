# Agente de Trading Autónomo MNQ

**Sistema de trading algorítmico para futuros MNQ con cuentas Apex Trader Funding**

[![Python](https://img.shields.io/badge/Python-3.11+-blue.svg)](https://www.python.org/downloads/)
[![NinjaTrader](https://img.shields.io/badge/NinjaTrader-8.0.29+-green.svg)](https://ninjatrader.com/)
[![License](https://img.shields.io/badge/License-Privado-red.svg)](LICENSE)

---

## Descripción

Agente de trading de inteligencia artificial diseñado para operar futuros MNQ (Micro E-mini Nasdaq) en cuentas de Apex Trader Funding. El sistema combina análisis técnico basado en niveles clave (PDH/PDL, ONH/ONL) con ejecución automatizada de ultra-baja latencia mediante NinjaTrader 8 y Rithmic API.

### Características Principales

- ✅ **Ejecución 100% Autónoma** con override manual
- ✅ **Cumplimiento APEX integrado** (scaling, MAE 30%, trailing threshold)
- ✅ **Latencia <100ms** (ATI) con path a <20ms (CrossTrade)
- ✅ **Gestión de Riesgo** avanzada (R fijo, position sizing dinámico, circuit breakers)
- ✅ **Journal Completo** con tracking KPIs en tiempo real
- ✅ **Multi-Modo:** Autónomo, Señales-Only, Monitor

### Estrategia de Trading

**Instrumento:** MNQ (Micro E-mini Nasdaq)  
**Sesión:** RTH (15:30-22:00 CET)  
**Riesgo/Trade:** $120 (configurable)  
**RR Objetivo:** ≥1.5:1 (típico 1.8-2.2R)  
**Setups:** A-L, B-L, A-S, B-S, B-M

**Reglas APEX $50k:**
- Scaling: 50 micros hasta $52,600 EOD → 100 micros
- MAE máximo: $750 por trade
- Trailing threshold: $2,500
- Day trading only (no overnight)

---

## Tech Stack

### Backend (Python 3.11+)
- **Rithmic API** para market data de baja latencia
- **pandas/numpy** para análisis técnico
- **asyncio** para procesamiento asíncrono
- **SQLite** para journal y persistencia

### Frontend Ejecución (C# .NET 4.8)
- **NinjaTrader 8** como plataforma de ejecución
- **NinjaScript** para add-on visual
- **ATI (Automated Trading Interface)** para comunicación Python ↔ NT8

### Infraestructura
- **Windows 10/11** (requerido por NinjaTrader)
- **Git** para control de versiones
- **VPS opcional** para 24/7 uptime

---

## Instalación Rápida

### Requisitos Previos

```bash
# Verificar versiones
python --version    # 3.11+
git --version       # 2.40+
```

**Software requerido:**
- NinjaTrader 8.0.29.1+
- Cuenta Rithmic (a través de broker compatible)
- Cuenta Apex Trader Funding (sim o funded)

### Paso 1: Clonar Repositorio

```bash
git clone https://github.com/DemFlax/-TRADING_AGENT_IA.git
cd -TRADING_AGENT_IA
```

### Paso 2: Configurar Entorno Python

```bash
# Crear y activar entorno virtual
python -m venv venv
venv\Scripts\activate  # Windows
source venv/bin/activate  # Linux/macOS

# Instalar dependencias
pip install -r requirements.txt
```

### Paso 3: Configurar Variables de Entorno

Copiar `.env.example` a `.env` y completar:

```bash
RITHMIC_USER=tu_usuario
RITHMIC_PASSWORD=tu_password
APEX_ACCOUNT_ID=Sim101
NT8_INCOMING_PATH=C:\Users\TuUsuario\Documents\NinjaTrader 8\incoming
```

### Paso 4: Configurar NinjaTrader

1. Instalar [NinjaTrader 8](https://ninjatrader.com/)
2. Habilitar ATI: Tools → Options → Automated Trading Interface
3. Configurar conexión Rithmic

### Paso 5: Ejecutar

```bash
# Modo SIGNALS (solo análisis, no ejecuta)
python src/main.py --mode SIGNALS

# Modo AUTÓNOMO (ejecuta trades automáticamente)
python src/main.py --mode AUTONOMOUS

# Con configuración personalizada
python src/main.py --config config/custom_settings.yaml
```

---

## Documentación

- 📘 [**ARQUITECTURA.md**](ARQUITECTURA.md) - Diseño completo del sistema
- 📗 [**GUIA_DESARROLLO.md**](GUIA_DESARROLLO.md) - Setup, estándares, workflow
- 📙 [**API_REFERENCE.md**](API_REFERENCE.md) - Referencia de clases/métodos
- 📕 [**DEPLOYMENT.md**](DEPLOYMENT.md) - Instalación en producción

**Documentación Operativa:**
- [Flujo Diario Operativo](DOCUMENTATION/MNQ_AgenteIA_Flujo_Diario.md)
- [Estrategia APEX $50k](DOCUMENTATION/MNQ_APEX50k_Estrategia.md)

---

## Uso

### Modo Autónomo

```python
from src.core.trading_agent import TradingAgent
from src.strategies.mnq_levels_strategy import MNQLevelsStrategy

# Inicializar agente
agent = TradingAgent(
    account_id="Sim101",
    strategy=MNQLevelsStrategy(),
    mode="AUTONOMOUS"
)

# Ejecutar
agent.run()
```

El agente ejecutará automáticamente:
1. **14:45** - Boot y carga de estado
2. **15:05** - Cálculo de niveles (PDH/PDL/ONH/ONL)
3. **15:30** - Cálculo OR15' y escaneo continuo
4. **15:45-20:30** - Detección y ejecución de setups
5. **22:00** - Flat EOD y post-market report

### Modo Señales

```python
agent = TradingAgent(
    account_id="Sim101",
    strategy=MNQLevelsStrategy(),
    mode="SIGNALS"
)

# Solo notifica, no ejecuta
agent.run()
```

### Override Manual

Desde el add-on de NinjaTrader 8:
- Clic en botón "MANUAL" → Agente pausa escaneo
- Ejecutas trade manualmente
- Agente entra en modo MONITOR
- Al cerrar posición → Agente reanuda AUTÓNOMO

---

## Testing

```bash
# Tests unitarios
pytest tests/unit/

# Tests de integración (requiere APIs)
pytest tests/integration/

# Con coverage
pytest --cov=src --cov-report=html
```

**Backtesting:**
```bash
python scripts/backtest.py --start 2024-01-01 --end 2024-12-31
```

**Paper Trading:**
```bash
# Siempre paper trading primero (2-4 semanas)
python src/main.py --mode AUTONOMOUS --paper
```

---

## Estructura del Proyecto

```
├── src/                    # Código fuente Python
│   ├── core/              # Núcleo del agente
│   ├── domain/            # Lógica de dominio
│   ├── application/       # Casos de uso
│   ├── infrastructure/    # Implementaciones (Rithmic, NT8)
│   └── strategies/        # Estrategias de trading
├── ninjatrader/           # Add-on NinjaScript (C#)
├── tests/                 # Tests unitarios e integración
├── config/                # Configuración YAML
├── data/                  # Journal SQLite
├── logs/                  # Logs del agente
└── DOCUMENTATION/         # Docs operativas
```

---

## Roadmap

### Fase 1: MVP (Actual)
- [x] Estrategia MNQ basada en niveles
- [x] Ejecución vía ATI
- [x] Cumplimiento APEX
- [x] Journal y KPIs
- [ ] Testing extensivo (en progreso)

### Fase 2: Mejoras (Q1 2026)
- [ ] Dashboard web para métricas
- [ ] Multi-cuenta (hasta 5 cuentas simultáneas)
- [ ] Sistema Guardian anti-tilt
- [ ] Integración CrossTrade API (<20ms latency)

### Fase 3: Expansión (Q2-Q3 2026)
- [ ] Estrategias adicionales (VWAP, ICT concepts)
- [ ] Backtesting comprehensivo
- [ ] Machine Learning para optimización
- [ ] Soporte multi-activo (ES, NQ, YM)

---

## Seguridad

⚠️ **IMPORTANTE:**
- NO commitear `.env` (ya en `.gitignore`)
- Usar API keys read-only cuando sea posible
- Activar 2FA en Rithmic/Apex
- Revisar permisos API (deshabilitar withdrawal)
- Limitar IP whitelisting

**Gestión de Secretos:**
```bash
# Desarrollo
.env (local, gitignored)

# Producción
Variables de entorno del sistema o gestor de secretos
```

---

## Performance

### Benchmarks Actuales

| Métrica | Objetivo | Medido |
|---------|----------|---------|
| Latencia tick processing | <1ms | 0.3ms |
| Detección setup | <10ms | 5ms |
| Ejecución end-to-end | <200ms | 120ms |
| Uptime (horas mercado) | 99.9% | TBD |

### Uso de Recursos

- **RAM:** 150-200MB
- **CPU:** 5-15% (picos 20%)
- **Disco:** ~1MB/mes (journal)
- **Red:** ~50KB/s (streaming Rithmic)

---

## Contribuir

Este es un proyecto privado de aprendizaje. No se aceptan contribuciones externas en este momento.

Para desarrolladores del proyecto:
1. Crear feature branch desde `develop`
2. Seguir [Guía de Desarrollo](GUIA_DESARROLLO.md)
3. Escribir tests
4. Crear PR hacia `develop`

---

## Licencia

**Privado - Todos los derechos reservados**

Este código es privado y no puede ser usado, copiado, modificado o distribuido sin permiso explícito del autor.

---

## Contacto

**Desarrollador:** Dan  
**Proyecto:** CFGS DAM (Desarrollo de Aplicaciones Multiplataforma)  
**Repositorio:** https://github.com/DemFlax/-TRADING_AGENT_IA

---

## Disclaimer

**⚠️ AVISO IMPORTANTE:**

Este software se proporciona con fines educativos y de investigación. El trading de futuros implica un riesgo significativo de pérdida de capital. El autor no se responsabiliza por pérdidas financieras derivadas del uso de este sistema.

**Antes de operar con dinero real:**
1. Probar exhaustivamente en simulado (mínimo 2-4 semanas)
2. Entender completamente las reglas de Apex Trader Funding
3. Comenzar con capital que puedas permitirte perder
4. Monitorear el sistema continuamente
5. Tener plan de contingencia para fallos técnicos

El rendimiento pasado no garantiza resultados futuros.

---

**Última actualización:** 2025-11-06
