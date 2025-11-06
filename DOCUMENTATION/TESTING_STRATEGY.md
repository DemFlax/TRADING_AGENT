# Estrategia de Testing — Agente de Trading MNQ

**Versión:** 1.0.0  
**Fecha:** 2025-11-06  
**Proyecto:** Agente de Trading Autónomo para Futuros MNQ (Apex Trader Funding)

---

## Índice

1. [Filosofía de Testing](#filosofía-de-testing)
2. [Niveles de Testing](#niveles-de-testing)
3. [Testing Unitario](#testing-unitario)
4. [Testing de Integración](#testing-de-integración)
5. [Backtesting](#backtesting)
6. [Paper Trading](#paper-trading)
7. [Criterios para Live Trading](#criterios-para-live-trading)
8. [Testing Continuo](#testing-continuo)

---

## Filosofía de Testing

### Principios Fundamentales

**1. Testing NO es opcional**
- El trading automatizado sin testing exhaustivo = pérdida garantizada
- Cada línea de código crítica debe estar testeada
- Target: >80% coverage en módulos de riesgo, estrategia y ejecución

**2. Progresión Obligatoria**
```
Unit Tests → Integration Tests → Backtesting → Paper Trading → Live
```
**NO saltar etapas.** Cada nivel valida aspectos diferentes.

**3. Fallar Rápido, Fallar Barato**
- Detectar bugs en tests unitarios (gratis)
- NO detectar bugs en live trading (muy caro)

**4. Testing de Reglas APEX**
- Validar cumplimiento de reglas APEX en CADA nivel
- Un incumplimiento = cuenta cerrada permanentemente

---

## Niveles de Testing

### Pirámide de Testing

```
               🔺 Manual Testing
              /  \  (Mínimo, solo exploración)
             /____\
            /      \
           / E2E    \  End-to-End
          /  Tests   \ (Pocos, críticos)
         /____________\
        /              \
       / Integration    \ (Moderados, APIs/DB)
      /     Tests        \
     /____________________\
    /                      \
   /    Unit Tests          \ (Muchos, rápidos)
  /__________________________\
```

### Tiempo Esperado por Nivel

| Nivel | Duración | Frecuencia |
|-------|----------|------------|
| Unit Tests | <1 min | Cada commit |
| Integration Tests | 2-5 min | Cada PR |
| Backtesting | 10-60 min | Semanal |
| Paper Trading | 2-4 semanas | Pre-live |
| Live Trading | Continuo | Post-validación |

---

## Testing Unitario

### Qué Testear

**Módulos críticos (coverage >90%):**
- `risk_manager.py`
- `strategy_engine.py`
- `order_executor.py`
- `apex_rules_validator.py`

**Módulos importantes (coverage >80%):**
- `level_calculator.py`
- `position_tracker.py`
- `journal_manager.py`

### Estructura de Tests

```python
# tests/unit/test_risk_manager.py

import pytest
from src.core.risk_manager import RiskManager
from src.domain.entities.account import Account

@pytest.fixture
def risk_manager():
    """Fixture con reglas APEX"""
    return RiskManager(
        max_loss=-2500,
        scaling_threshold=52600,
        max_contracts_before_scaling=50,
        max_contracts_after_scaling=100,
        mae_limit=750
    )

@pytest.fixture
def account_below_threshold():
    """Cuenta bajo threshold de scaling"""
    return Account(
        account_id="Sim101",
        balance=51000,
        daily_pnl=0,
        monthly_pnl=0
    )

@pytest.fixture
def account_above_threshold():
    """Cuenta sobre threshold de scaling"""
    return Account(
        account_id="Sim101",
        balance=53000,
        daily_pnl=0,
        monthly_pnl=0
    )

class TestPositionSizing:
    """Tests de cálculo de tamaño de posición"""
    
    def test_basic_calculation(self, risk_manager):
        """Test cálculo básico sin límites"""
        # R = $120, stop = 20 pts (20 * $2 = $40/micro)
        # Expected: floor(120 / 40) = 3 micros
        contracts = risk_manager.calculate_position_size(
            stop_pts=20,
            R_risk=120
        )
        assert contracts == 3
    
    def test_respects_scaling_below_threshold(
        self, 
        risk_manager, 
        account_below_threshold
    ):
        """Test que respeta scaling bajo threshold"""
        # Cálculo daría 60 micros, pero debe limitar a 50
        contracts = risk_manager.calculate_position_size(
            stop_pts=2,  # $4 risk per micro
            R_risk=120   # 120/4 = 30, pero caps a 50
        )
        
        contracts_scaled = risk_manager.apply_scaling(
            contracts=contracts,
            account=account_below_threshold
        )
        
        assert contracts_scaled <= 50
    
    def test_allows_full_size_above_threshold(
        self,
        risk_manager,
        account_above_threshold
    ):
        """Test que permite full size sobre threshold"""
        contracts = risk_manager.calculate_position_size(
            stop_pts=2,
            R_risk=120
        )
        
        contracts_scaled = risk_manager.apply_scaling(
            contracts=contracts,
            account=account_above_threshold
        )
        
        assert contracts_scaled <= 100

class TestMAEValidation:
    """Tests de validación MAE"""
    
    def test_rejects_excessive_mae(self, risk_manager):
        """Test rechaza MAE excesivo"""
        # 50 micros * 20 pts * $2 = $2000 MAE
        # Límite: $750
        is_valid = risk_manager.check_mae_limit(
            contracts=50,
            stop_pts=20
        )
        assert is_valid == False
    
    def test_accepts_valid_mae(self, risk_manager):
        """Test acepta MAE válido"""
        # 3 micros * 20 pts * $2 = $120 MAE
        is_valid = risk_manager.check_mae_limit(
            contracts=3,
            stop_pts=20
        )
        assert is_valid == True
    
    def test_boundary_condition(self, risk_manager):
        """Test condición límite exacta"""
        # 18 micros * 20 pts * $2 = $720 MAE (bajo límite)
        is_valid = risk_manager.check_mae_limit(
            contracts=18,
            stop_pts=20
        )
        assert is_valid == True
        
        # 19 micros * 20 pts * $2 = $760 MAE (sobre límite)
        is_valid = risk_manager.check_mae_limit(
            contracts=19,
            stop_pts=20
        )
        assert is_valid == False

class TestDailyLimits:
    """Tests de límites diarios"""
    
    def test_stops_trading_at_daily_limit(self, risk_manager):
        """Test detiene trading al alcanzar límite diario"""
        account = Account(
            account_id="Sim101",
            balance=50000,
            daily_pnl=-120,  # -1R
            monthly_pnl=-120
        )
        
        assert account.can_trade(daily_limit_R=-1.0) == False
    
    def test_allows_trading_within_limit(self, risk_manager):
        """Test permite trading dentro de límite"""
        account = Account(
            account_id="Sim101",
            balance=50000,
            daily_pnl=-60,  # -0.5R
            monthly_pnl=-60
        )
        
        assert account.can_trade(daily_limit_R=-1.0) == True
```

### Ejecutar Tests Unitarios

```bash
# Todos los tests unitarios
pytest tests/unit/ -v

# Con coverage
pytest tests/unit/ --cov=src --cov-report=html

# Tests específicos
pytest tests/unit/test_risk_manager.py::TestPositionSizing

# Stop en primer fallo
pytest tests/unit/ -x
```

---

## Testing de Integración

### Qué Testear

- Comunicación Python ↔ Rithmic API
- Comunicación Python ↔ NinjaTrader ATI
- Persistencia en SQLite
- Flujo completo: detección → validación → ejecución

### Tests de Integración Rithmic

```python
# tests/integration/test_rithmic_connection.py

import pytest
import asyncio
from src.infrastructure.rithmic.data_handler import RithmicDataHandler

@pytest.mark.integration
@pytest.mark.asyncio
async def test_rithmic_connection():
    """Test conexión real a Rithmic (requiere credenciales)"""
    handler = RithmicDataHandler()
    
    try:
        await handler.connect()
        assert handler.is_connected() == True
    finally:
        await handler.disconnect()

@pytest.mark.integration
@pytest.mark.asyncio
async def test_market_data_stream():
    """Test streaming de datos"""
    handler = RithmicDataHandler()
    
    received_ticks = []
    
    def on_tick(data):
        received_ticks.append(data)
    
    handler.on_tick(on_tick)
    
    await handler.connect()
    await handler.subscribe_instrument("MNQ")
    
    # Esperar 5 segundos de datos
    await asyncio.sleep(5)
    
    await handler.disconnect()
    
    # Verificar que recibió ticks
    assert len(received_ticks) > 0
    
    # Verificar estructura de datos
    tick = received_ticks[0]
    assert 'price' in tick
    assert 'volume' in tick
    assert 'timestamp' in tick
```

### Tests de Integración ATI

```python
# tests/integration/test_ati_communication.py

import pytest
import os
import time
from src.infrastructure.ninjatrader.ati_executor import ATIExecutor
from src.domain.value_objects.order import Order, OrderType, OrderSide

@pytest.mark.integration
def test_ati_file_write():
    """Test escritura de archivo ATI"""
    ati_path = os.path.join(
        os.environ['USERPROFILE'],
        'Documents', 'NinjaTrader 8', 'incoming'
    )
    
    executor = ATIExecutor(ati_path=ati_path)
    
    order = Order(
        order_id=uuid4(),
        account_id="Sim101",
        symbol="MNQ 12-24",
        side=OrderSide.BUY,
        order_type=OrderType.LIMIT,
        quantity=1,
        limit_price=18000.0
    )
    
    # Escribir orden
    order_id = executor.place_order(order)
    
    # Verificar que se escribió archivo
    expected_file = os.path.join(ati_path, f"order_{order.order_id}.txt")
    assert os.path.exists(expected_file)
    
    # Limpiar
    os.remove(expected_file)

@pytest.mark.integration
def test_ati_end_to_end():
    """
    Test completo ATI (requiere NinjaTrader ejecutándose)
    """
    # Este test solo pasa si NT8 está abierto y conectado
    pytest.skip("Requiere NT8 manual - ejecutar solo en pre-deployment")
```

---

## Backtesting

### Metodología

**1. Walk-Forward Analysis**
```
Datos históricos divididos en:
- Training: 70% (optimizar parámetros)
- Validation: 15% (validar out-of-sample)
- Testing: 15% (resultados finales)
```

**2. Métricas a Evaluar**

| Métrica | Target Mínimo |
|---------|---------------|
| Win Rate | ≥50% |
| Expectancy | ≥0.5R |
| Max Drawdown | ≤-3R |
| Profit Factor | ≥1.5 |
| Sharpe Ratio | ≥1.0 |

**3. Validación de Reglas APEX**
- Cada trade simulado debe cumplir MAE <$750
- Scaling aplicado correctamente según balance
- RR 1.5:1 - 5:1 en todos los trades

### Script de Backtesting

```python
# scripts/backtest.py

import pandas as pd
from datetime import datetime, timedelta
from src.strategies.mnq_levels_strategy import MNQLevelsStrategy
from src.core.risk_manager import RiskManager

def backtest(start_date: str, end_date: str):
    """
    Ejecuta backtest de estrategia MNQ.
    
    Args:
        start_date: Fecha inicio (YYYY-MM-DD)
        end_date: Fecha fin (YYYY-MM-DD)
    """
    # Cargar datos históricos
    data = load_historical_data("MNQ", start_date, end_date)
    
    # Inicializar estrategia
    strategy = MNQLevelsStrategy()
    risk_mgr = RiskManager()
    
    # Estado inicial
    balance = 50000
    trades = []
    
    # Iterar días
    for date in pd.date_range(start_date, end_date):
        day_data = data[data['date'] == date]
        
        if len(day_data) == 0:
            continue
        
        # Calcular niveles
        levels = strategy.calculate_levels(date, day_data)
        
        # Escanear barras
        for bar in day_data.itertuples():
            signal = strategy.detect_setup(bar, levels)
            
            if signal and signal.is_valid():
                # Calcular tamaño
                contracts = risk_mgr.calculate_position_size(
                    stop_pts=signal.stop_pts,
                    R_risk=120
                )
                
                # Simular ejecución
                trade = simulate_trade(signal, contracts, day_data)
                trades.append(trade)
                
                balance += trade['pnl_usd']
    
    # Calcular métricas
    results = calculate_metrics(trades, balance)
    
    return results

def simulate_trade(signal, contracts, data):
    """Simula ejecución de un trade"""
    # Implementación simplificada
    # En realidad, recorrer barras siguientes
    # hasta hit SL o TP
    pass

def calculate_metrics(trades, final_balance):
    """Calcula métricas de performance"""
    df = pd.DataFrame(trades)
    
    wins = len(df[df['result_R'] > 0])
    losses = len(df[df['result_R'] < 0])
    
    metrics = {
        'total_trades': len(df),
        'win_rate': wins / len(df) if len(df) > 0 else 0,
        'avg_win': df[df['result_R'] > 0]['result_R'].mean(),
        'avg_loss': df[df['result_R'] < 0]['result_R'].mean(),
        'expectancy': df['result_R'].mean(),
        'max_dd_R': df['result_R'].cumsum().min(),
        'final_balance': final_balance
    }
    
    return metrics
```

### Ejecutar Backtesting

```bash
# Backtest año completo
python scripts/backtest.py --start 2024-01-01 --end 2024-12-31

# Con reporte detallado
python scripts/backtest.py --start 2024-01-01 --end 2024-12-31 --verbose

# Guardar resultados
python scripts/backtest.py --start 2024-01-01 --end 2024-12-31 --output results.csv
```

---

## Paper Trading

### Objetivo

Validar el sistema completo en condiciones de mercado real SIN arriesgar capital.

### Duración Mínima

**2-4 semanas** de paper trading continuo antes de live.

### Qué Validar

```
✅ Detección de setups en tiempo real
✅ Cálculo correcto de tamaño de posición
✅ Ejecución de órdenes vía ATI
✅ Gestión bracket (SL→BE, trailing)
✅ Flat EOD automático
✅ Journal actualizado correctamente
✅ Cumplimiento reglas APEX
✅ No hay bugs/crashes
✅ Latencia aceptable (<200ms)
```

### Ejecutar Paper Trading

```bash
# Activar paper trading mode
python src/main.py --mode AUTONOMOUS --paper

# Verificar en logs que está en paper:
# "Paper Trading Mode: ACTIVE"
```

### Checklist Diario

```markdown
## Paper Trading Day [N] - [YYYY-MM-DD]

### Pre-Market (14:45)
- [ ] Agente arrancado correctamente
- [ ] Conexión Rithmic: OK
- [ ] Balance paper: $50,000
- [ ] Niveles calculados: PDH/PDL/ONH/ONL

### During Market (15:30-22:00)
- [ ] Detección de setups: [N] detecciones
- [ ] Trades ejecutados: [N]
- [ ] Errores: [Ninguno / Detallar]

### Post-Market (22:00+)
- [ ] Flat EOD ejecutado: Sí/No
- [ ] Journal actualizado: Sí/No
- [ ] PnL del día: +X.XR ($XXX)
- [ ] Cumplimiento APEX: ✅ / ❌

### Observaciones
[Notas sobre comportamiento, bugs, mejoras]
```

---

## Criterios para Live Trading

### Métricas Mínimas (Paper Trading)

```
✅ Win Rate ≥ 50%
✅ Expectancy ≥ +0.5R
✅ Max Drawdown ≤ -1.5R
✅ Trades ejecutados: ≥20
✅ Días operados: ≥15
✅ Uptime: 100% (sin crashes)
✅ Flat EOD: 100% de días
✅ Cumplimiento APEX: 100%
```

### Checklist Pre-Live

```
TESTING:
[ ] Tests unitarios: 100% passing
[ ] Tests integración: 100% passing
[ ] Backtesting: Métricas OK
[ ] Paper trading: ≥2 semanas
[ ] Todas las métricas mínimas alcanzadas

TÉCNICO:
[ ] Zero bugs conocidos
[ ] Latencia <200ms consistente
[ ] Conexiones estables
[ ] Add-on NT8 funcional
[ ] Backup configurado

OPERATIVO:
[ ] Reglas APEX memorizadas
[ ] Plan de contingencia documentado
[ ] Alertas Telegram funcionando
[ ] Capital disponible
[ ] Estado mental: Confiado, no ansioso
```

**Solo si TODOS = [✅], proceder a live.**

---

## Testing Continuo

### Durante Live Trading

**Monitoring Diario:**
- Revisar logs cada noche
- Verificar journal completo
- Comparar PnL esperado vs real
- Detectar drift en métricas

**Testing Semanal:**
```bash
# Re-ejecutar tests unitarios
pytest tests/unit/

# Verificar no hay regresiones
```

**Testing Mensual:**
- Backtest estrategia con datos del mes
- Comparar backtest vs live results
- Si divergencia >20% → investigar

**Actualización de Tests:**
- Añadir test por cada bug encontrado
- Mantener coverage >80%
- Refactorizar tests obsoletos

---

**Fin de la Estrategia de Testing**
