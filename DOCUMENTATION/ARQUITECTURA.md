# Agente de Trading MNQ - Arquitectura del Sistema

**Versión:** 1.0.0  
**Fecha:** 2025-11-06  
**Proyecto:** Agente de Trading Autónomo para Futuros MNQ (Apex Trader Funding)

---

## Índice

1. [Resumen Ejecutivo](#resumen-ejecutivo)
2. [Visión General del Sistema](#visión-general-del-sistema)
3. [Arquitectura Core](#arquitectura-core)
4. [Detalles de Componentes](#detalles-de-componentes)
5. [Flujos de Comunicación](#flujos-de-comunicación)
6. [Arquitectura de Datos](#arquitectura-de-datos)
7. [Stack Tecnológico](#stack-tecnológico)
8. [Patrones de Diseño](#patrones-de-diseño)
9. [Escalabilidad y Expansión Futura](#escalabilidad-y-expansión-futura)
10. [Consideraciones de Seguridad](#consideraciones-de-seguridad)

---

## Resumen Ejecutivo

El Agente de Trading MNQ es un sistema basado en Python de modo dual (autónomo/señales) diseñado para operar intradía futuros MNQ (Micro E-mini Nasdaq) en cuentas de Apex Trader Funding. El sistema opera con NinjaTrader 8 como plataforma de ejecución e integra la API de Rithmic para datos de mercado de ultra-baja latencia.

**Características Clave:**
- Ejecución 100% autónoma con override manual
- Estrategia basada en reglas (niveles PDH/PDL/ONH/ONL)
- Cumplimiento APEX integrado (scaling, MAE, trailing threshold)
- Gestión de riesgo en tiempo real
- Soporte multi-cuenta (futuro)
- Sistema Guardian para prevenir trading irracional
- Journal completo y seguimiento de KPIs

**Objetivos de Performance:**
- Latencia: <100ms (ATI), mejorable a <20ms (CrossTrade)
- Throughput: 1 trade/día típico, máx 2 intentos
- Confiabilidad: 99.9% uptime durante horas de mercado (15:30-22:00 CET)

---

## Visión General del Sistema

### Arquitectura de Alto Nivel

```
┌─────────────────────────────────────────────────────────────────────┐
│                       SISTEMAS EXTERNOS                              │
├─────────────────────────────────────────────────────────────────────┤
│  Rithmic Market Data ──→ WebSocket Stream (datos tick)              │
│  NinjaTrader 8 ────────→ Interfaz ATI (ejecución órdenes)           │
│  Telegram/Discord ─────→ Notificaciones (opcional)                  │
└─────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────┐
│                    MOTOR CORE PYTHON                                 │
├─────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────────┐   │
│  │ Manejador Datos │  │ Motor Estrategia│  │ Gestor Riesgo    │   │
│  │ - Conexión      │  │ - Detección     │  │ - Tamaño         │   │
│  │   Rithmic       │  │   setups        │  │   posición       │   │
│  │ - Cálculo       │  │ - Validación    │  │ - Reglas APEX    │   │
│  │   niveles       │  │ - Generación    │  │ - Chequeo MAE    │   │
│  │ - OR15'/Volumen │  │   señales       │  │                  │   │
│  └─────────────────┘  └─────────────────┘  └──────────────────┘   │
│                                                                      │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────────┐   │
│  │ Controlador     │  │ Gestor Journal  │  │ Guardián Cuenta  │   │
│  │ Ejecución       │  │ - Base SQLite   │  │ - Prevención     │   │
│  │ - Cambio modo   │  │ - Tracking KPIs │  │   tilt           │   │
│  │ - Comandos ATI  │  │ - Card diaria   │  │ - Bloqueo manual │   │
│  │ - Orden bracket │  │                 │  │ - Umbral pérdida │   │
│  └─────────────────┘  └─────────────────┘  └──────────────────┘   │
│                                                                      │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │         Orquestador Agente de Trading                       │   │
│  │  - Loop de eventos principal                                │   │
│  │  - Máquina de estados (BOOT → PRE_MARKET → TRADING → CLOSE)│   │
│  │  - Gestión de modos (AUTO/MANUAL/MONITOR)                   │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────┐
│                 NINJATRADER 8 + ADD-ON (C#)                         │
├─────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────────┐   │
│  │  Panel Visual   │  │ Marcadores Chart│  │ Ejecutor Órdenes │   │
│  │  - Estado agente│  │ - Dibujo niveles│  │ - Conexión       │   │
│  │  - Display PnL  │  │ - Flechas entry │  │   Rithmic        │   │
│  │  - Próximo setup│  │ - Líneas SL/TP  │  │ - Lógica bracket │   │
│  └─────────────────┘  └─────────────────┘  └──────────────────┘   │
│                                                                      │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │      Lector Archivos ATI (directorio incoming)              │   │
│  │  Monitorea: C:\Users\...\NinjaTrader 8\incoming\*.txt       │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────┐
│                    CAPA DE PERSISTENCIA                              │
├─────────────────────────────────────────────────────────────────────┤
│  Base de Datos SQLite: journal.db                                   │
│  - trades (tick-by-tick)                                            │
│  - daily_summary                                                    │
│  - account_state                                                    │
│  - strategy_params                                                  │
│                                                                      │
│  Configuración: config/settings.yaml                                │
│  Logs: logs/agent_{fecha}.log                                       │
└─────────────────────────────────────────────────────────────────────┘
```

### Modos de Operación

**1. Modo AUTÓNOMO (Por Defecto)**
- Agente detecta setups automáticamente
- Ejecuta trades vía ATI sin intervención humana
- Monitorea posición y aplica gestión bracket
- Flat EOD automático

**2. Modo SEÑALES**
- Agente detecta y valida setups
- Envía notificaciones (panel visual + Telegram opcional)
- Usuario ejecuta manualmente en NinjaTrader
- Sin ejecución automática

**3. Modo MONITOR (Activación Automática)**
- Se activa cuando usuario toma control manual durante AUTÓNOMO
- Agente deja de escanear nuevos setups
- Monitorea posición activa del usuario
- Reanuda AUTÓNOMO cuando posición cerrada

---

## Arquitectura Core

### Estructura de Capas

```
┌──────────────────────────────────────────────────────────────┐
│                   CAPA DE PRESENTACIÓN                       │
│  - Add-on NinjaTrader 8 (C# + XAML)                         │
│  - Bot Telegram/Discord (Python)                            │
└──────────────────────────────────────────────────────────────┘
                           ↓
┌──────────────────────────────────────────────────────────────┐
│                   CAPA DE APLICACIÓN                         │
│  - TradingAgent (orquestador)                               │
│  - MultiAccountManager (futuro)                             │
│  - Casos de Uso (PlaceTradeUseCase, etc.)                  │
└──────────────────────────────────────────────────────────────┘
                           ↓
┌──────────────────────────────────────────────────────────────┐
│                      CAPA DE DOMINIO                         │
│  - Entidades: Account, Trade, Signal                        │
│  - Value Objects: MarketData, Order, Level                  │
│  - Interfaces: IStrategy, IExecutor, IRiskManager           │
│  - Servicios Dominio: LevelCalculator, SetupDetector        │
└──────────────────────────────────────────────────────────────┘
                           ↓
┌──────────────────────────────────────────────────────────────┐
│                 CAPA DE INFRAESTRUCTURA                      │
│  - Rithmic: MarketDataStream, HistoricalDataFetcher         │
│  - NinjaTrader: ATIExecutor, CrossTradeExecutor (futuro)    │
│  - Base de Datos: SQLiteRepository                          │
│  - Externos: TelegramNotifier                               │
└──────────────────────────────────────────────────────────────┘
```

### Principios de Diseño Aplicados

**Principios SOLID:**
- **S**ingle Responsibility: Cada clase tiene una sola razón para cambiar
- **O**pen/Closed: Extensible vía interfaces (IStrategy, IExecutor)
- **L**iskov Substitution: Ejecutores intercambiables (ATI ↔ CrossTrade)
- **I**nterface Segregation: Interfaces pequeñas y enfocadas
- **D**ependency Inversion: Dependencia de abstracciones, no concreciones

**Patrones Adicionales:**
- Dependency Injection para testabilidad
- Repository Pattern para persistencia de datos
- Observer Pattern para eventos de market data
- State Machine para ciclo de vida del agente
- Factory Pattern para instanciación de estrategias

---

## Detalles de Componentes

### 1. Manejador de Datos (Data Handler)

**Propósito:** Ingesta y procesa datos de mercado desde Rithmic

**Responsabilidades:**
- Establecer conexión WebSocket a Rithmic
- Stream de datos tick en tiempo real para MNQ
- Calcular métricas derivadas (OR15', mediana volumen, niveles)
- Mantener buffer histórico para indicadores

**Clases Clave:**
```python
class RithmicDataHandler:
    - connect() -> None
    - subscribe_instrument(symbol: str) -> None
    - on_tick(callback: Callable) -> None
    - get_current_price() -> float
    - disconnect() -> None

class LevelCalculator:
    - calculate_pdh_pdl(date: datetime) -> Tuple[float, float]
    - calculate_onh_onl() -> Tuple[float, float]
    - find_support_resistance() -> List[Level]

class VolumeAnalyzer:
    - median_volume_1m(bars: int = 20) -> float
    - current_volume_factor() -> float
```

**Flujo de Datos:**
```
Rithmic WebSocket → on_tick() → MarketData VO → Event Bus → Suscriptores
```

**Performance:**
- Procesamiento tick: <1ms
- Cálculo niveles: <10ms (una vez por sesión)
- Análisis volumen: <5ms (cada minuto)

---

### 2. Motor de Estrategia (Strategy Engine)

**Propósito:** Detecta setups de trading basados en price action y niveles

**Responsabilidades:**
- Implementar lógica de setups (A-L, B-L, A-S, B-S, B-M)
- Validar condiciones (volumen, RR, confirmación)
- Generar value objects Signal
- Soportar múltiples estrategias vía Factory

**Clases Clave:**
```python
class IStrategy(ABC):
    @abstractmethod
    def detect_setup(data: MarketData) -> Optional[Signal]
    @abstractmethod
    def calculate_stops(signal: Signal) -> Tuple[float, float]
    @abstractmethod
    def validate_conditions(signal: Signal) -> bool

class MNQLevelsStrategy(IStrategy):
    # Implementación actual - PDH/PDL/ONH/ONL
    def detect_setup(...) -> Optional[Signal]
    def _detect_AL_setup(...) -> Optional[Signal]
    def _detect_BL_setup(...) -> Optional[Signal]
    def _detect_AS_setup(...) -> Optional[Signal]
    def _detect_BS_setup(...) -> Optional[Signal]
    def _detect_BM_setup(...) -> Optional[Signal]

class StrategyFactory:
    @staticmethod
    def create(name: str, config: Dict) -> IStrategy
```

**Algoritmo de Detección de Setup:**
```
1. Verificar pre-filtros:
   - Horas de mercado (15:45-20:30)
   - OR15' en rango (25-160 pts)
   - Factor volumen ≥1.1x
   - No en centro de rango
   - Límite intentos OK

2. Escanear patrones:
   - A-L: Cierre > nivel + volumen
   - B-L: Pullback a soporte + rechazo
   - A-S: Cierre < nivel + volumen  
   - B-S: Pullback a resistencia + rechazo
   - B-M: Breakout con cierre + volumen

3. Calcular stops:
   - Entry, SL, TP según tipo de setup
   - Validar RR ≥1.5

4. Chequeo confirmación:
   - Debe confirmar dentro de 2 velas
   - Volumen sostenido

5. Generar Signal si todo pasa
```

---

### 3. Gestor de Riesgo (Risk Manager)

**Propósito:** Hacer cumplir reglas APEX y lógica de position sizing

**Responsabilidades:**
- Calcular tamaño de posición (floor(R / (stop_pts × $2)))
- Aplicar scaling (50 micros hasta $52,600, luego 100)
- Validar MAE <$750 por trade
- Verificar caps diarios/mensuales (-1R/-3R)
- Hacer cumplir límite de intentos (1-2/día)

**Clases Clave:**
```python
class RiskManager:
    def __init__(apex_rules: ApexRules)
    def calculate_position_size(
        account: Account,
        stop_pts: float,
        R_risk: float = 120
    ) -> int
    def validate_trade(
        account: Account, 
        signal: Signal
    ) -> Tuple[bool, str]
    def check_mae_limit(
        contracts: int, 
        stop_pts: float
    ) -> bool

class ApexRules:
    MAX_LOSS: float = -2500
    TRAILING_THRESHOLD: float = 2500
    SCALING_THRESHOLD: float = 52600
    MAE_PERCENT: float = 0.30
    MAE_BASE_DOLLAR: float = 750
    MAX_CONTRACTS_BEFORE_SCALING: int = 50
    MAX_CONTRACTS_AFTER_SCALING: int = 100
    MAX_RR_RATIO: float = 5.0
    MIN_RR_RATIO: float = 1.5
```

**Ejemplo de Position Sizing:**
```python
# Ejemplo: Setup A-L con stop de 20 puntos
stop_pts = 20
R_risk = 120

# Cálculo base
risk_per_micro = stop_pts * 2  # 20 × $2 = $40
base_contracts = floor(120 / 40)  # = 3 micros

# Aplicar scaling
if account.balance < 52600:
    max_allowed = 50
else:
    max_allowed = 100
contracts = min(base_contracts, max_allowed)  # = 3

# Chequeo MAE
mae_potential = contracts * stop_pts * 2  # 3 × 20 × 2 = $120
if mae_potential > 750:
    reject_trade()

# Resultado: 3 micros, riesgo $120, MAE $120 << $750 ✓
```

---

### 4. Controlador de Ejecución (Execution Controller)

**Propósito:** Gestionar ejecución de trades y ciclo de vida de órdenes

**Responsabilidades:**
- Cambiar entre modos AUTO/MANUAL/MONITOR
- Enviar comandos ATI a NinjaTrader
- Colocar órdenes bracket (entry + SL + TP)
- Modificar órdenes (SL→BE en +0.5R)
- Cancelar todas en EOD

**Clases Clave:**
```python
class IExecutor(ABC):
    @abstractmethod
    def place_order(order: Order) -> str  # Retorna order_id
    @abstractmethod
    def modify_order(order_id: str, new_sl: float) -> None
    @abstractmethod
    def cancel_order(order_id: str) -> None
    @abstractmethod
    def cancel_all(account_id: str) -> None

class ATIExecutor(IExecutor):
    def __init__(ati_path: str)
    def place_order(order: Order) -> str
    def _write_ati_file(command: str) -> None
    def _read_confirmation() -> str

class ExecutionController:
    def __init__(executor: IExecutor, mode: AgentMode)
    def execute_signal(signal: Signal, contracts: int) -> str
    def build_bracket_order(signal: Signal, contracts: int) -> Order
    def move_sl_to_breakeven(order_id: str) -> None
    def flatten_position(account_id: str) -> None
```

**Formato Comando ATI:**
```
PLACE;<CUENTA>;<INSTRUMENTO>;<ACCIÓN>;<QTY>;<TIPO>;<LIMIT>;<STOP>

Ejemplo Entry:
PLACE;Sim101;MNQ 12-24;BUY;3;LIMIT;18015;0

Ejemplo Stop-Loss:
PLACE;Sim101;MNQ 12-24;SELL;3;STOP;17995;0

Ejemplo Take-Profit:
PLACE;Sim101;MNQ 12-24;SELL;3;LIMIT;18055;0
```

**Lógica Orden Bracket:**
1. Colocar entry (LIMIT o STOP-MARKET)
2. Inmediatamente colocar SL (orden STOP)
3. Inmediatamente colocar TP (orden LIMIT)
4. Linkear órdenes vía strategy_id
5. Monitorear fills
6. En +0.5R → modificar SL a precio de entrada (BE)

---

### 5. Gestor de Journal (Journal Manager)

**Propósito:** Persistir toda actividad de trading y calcular KPIs

**Responsabilidades:**
- Registrar cada trade en SQLite
- Trackear MFE/MAE en tiempo real
- Calcular métricas diarias/mensuales
- Generar card post-mercado
- Mantener audit trail

**Clases Clave:**
```python
class JournalManager:
    def __init__(db_path: str)
    def log_trade(trade: Trade) -> None
    def update_trade_result(trade_id: UUID, result: TradeResult) -> None
    def get_daily_summary(date: datetime) -> DailySummary
    def get_monthly_kpis() -> MonthlyKPIs
    def generate_post_market_card() -> str

class Trade(Entity):
    id: UUID
    timestamp: datetime
    account_id: str
    strategy_name: str
    setup_type: str  # A-L, B-L, etc.
    entry_price: float
    stop_loss: float
    take_profit: float
    contracts: int
    status: TradeStatus  # OPEN, CLOSED, BE, CANCELLED
    result_R: float
    result_usd: float
    mfe_pts: float
    mae_pts: float
    notes: str
```

**Esquema Base de Datos:**
```sql
-- Tabla trades
CREATE TABLE trades (
    id TEXT PRIMARY KEY,
    timestamp DATETIME,
    account_id TEXT,
    strategy_name TEXT,
    setup_type TEXT,
    entry_price REAL,
    stop_loss REAL,
    take_profit REAL,
    contracts INTEGER,
    status TEXT,
    result_R REAL,
    result_usd REAL,
    mfe_pts REAL,
    mae_pts REAL,
    notes TEXT
);

-- Tabla daily_summary
CREATE TABLE daily_summary (
    date DATE PRIMARY KEY,
    attempts INTEGER,
    trades INTEGER,
    wins INTEGER,
    losses INTEGER,
    breakevens INTEGER,
    total_R REAL,
    total_usd REAL,
    max_dd_R REAL,
    penalties INTEGER
);

-- Tabla account_state
CREATE TABLE account_state (
    account_id TEXT PRIMARY KEY,
    balance REAL,
    daily_pnl REAL,
    monthly_pnl REAL,
    trailing_threshold REAL,
    max_contracts INTEGER,
    updated_at DATETIME
);
```

**Formato Card Post-Mercado:**
```markdown
📊 Reporte Trading MNQ | 2025-11-06
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📍 PLAN
Setup: A-L @ 18,015 | RR: 1.8 | Vol: 1.3x
SL: 17,995 (20pts) | TP: 18,055 (40pts)
Contratos: 3 micros | Riesgo: $120

⚡ EJECUCIÓN  
Entry: 15:47:32 | Fill: 18,015.00
Gestión: SL→BE @ +0.5R (15:51:18) ✓

✅ RESULTADO
TP Alcanzado: 18,055.00 @ 16:04:51
Duración: 17m 19s
Resultado: +2.1R ($252.00)
MFE: +48pts | MAE: -8pts

📈 ESTADÍSTICAS DIARIAS
Intentos: 1/2 | Win Rate: 100%
PnL Día: +2.1R ($252) | Mes: +7.8R ($936)
Balance: $50,252 → Scaling: 50 micros

💡 OBSERVACIONES
- Momentum fuerte post-OR breakout
- Confirmación en 1 vela (rápido)
- Spike volumen sostenido durante movimiento

🔧 MEJORAS
- Considerar trailing runner en momentum similar
- Entry podría afinarse 2-3pts más cerca del nivel
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

---

### 6. Guardián de Cuenta (Account Guardian)

**Propósito:** Prevenir trading irracional y hacer cumplir disciplina

**Responsabilidades:**
- Monitorear PnL diario en tiempo real
- Bloquear modo manual si umbral de pérdida alcanzado
- Prevenir patrones de "revenge trading"
- Registrar todas las intervenciones
- Reset diario al cierre de mercado

**Clases Clave:**
```python
class AccountGuardian:
    def __init__(
        loss_threshold_R: float = -0.5,
        max_attempts: int = 2,
        cooldown_minutes: int = 30
    )
    def evaluate_state(account: Account) -> GuardianDecision
    def block_manual_mode() -> None
    def unblock_manual_mode() -> None
    def log_intervention(reason: str) -> None

class GuardianDecision(Enum):
    ALLOW = "permitir"
    BLOCK_MANUAL = "bloquear_manual"
    BLOCK_ALL = "bloquear_todo"
    FORCE_FLAT = "forzar_flat"
```

**Reglas del Guardian:**
```python
# Regla 1: Umbral de pérdida
if account.daily_pnl_R < -0.5:
    block_manual_mode()
    send_notification("Guardian: Modo manual bloqueado (pérdida -0.5R)")

# Regla 2: Intentos rápidos
if attempts_in_last_30min >= 2:
    cooldown(30_minutes)
    
# Regla 3: Patrones emocionales
if loss_then_immediate_retry():  # <5min entre trades
    block_all_for(1_hour)
    
# Regla 4: Protección fin de día
if time >= 21:30 and daily_pnl_R < 0:
    force_flatten()
```

---

### 7. Agente de Trading (Orquestador)

**Propósito:** Loop de control principal y máquina de estados

**Responsabilidades:**
- Coordinar todos los componentes
- Gestionar ciclo de vida del agente
- Manejar cambios de modo
- Ejecutar workflow diario
- Recuperación de errores

**Clases Clave:**
```python
class TradingAgent:
    def __init__(
        account: Account,
        strategy: IStrategy,
        risk_mgr: RiskManager,
        executor: IExecutor,
        journal: JournalManager,
        guardian: AccountGuardian,
        mode: AgentMode = AgentMode.AUTONOMOUS
    )
    
    def run() -> None
    def shutdown() -> None
    
    # Métodos máquina de estados
    def _boot_sequence() -> None
    def _pre_market_analysis() -> None
    def _trading_session() -> None
    def _end_of_day_close() -> None
    def _post_market_report() -> None
    
    # Flujo trading
    def _autonomous_flow(data: MarketData) -> None
    def _signals_flow(data: MarketData) -> None
    def _monitor_flow(data: MarketData) -> None
    
    def switch_mode(new_mode: AgentMode) -> None
```

**Máquina de Estados:**
```
BOOT (14:45-15:05)
  ├─> Cargar estado cuenta
  ├─> Conectar Rithmic
  ├─> Inicializar componentes
  └─> Transición a PRE_MARKET

PRE_MARKET (15:05-15:30)
  ├─> Calcular niveles (PDH/PDL/ONH/ONL)
  ├─> Generar card pre-mercado (interno)
  ├─> Esperar apertura mercado
  └─> Transición a TRADING

TRADING (15:30-22:00)
  ├─> Calcular OR15' (15:30-15:45)
  ├─> Iniciar escaneo (15:45+)
  ├─> Ejecutar trades (si condiciones cumplen)
  ├─> Monitorear posiciones
  ├─> Aplicar gestión bracket
  └─> Transición a CLOSE

CLOSE (22:00)
  ├─> Flatear posiciones abiertas
  ├─> Cancelar órdenes pendientes
  ├─> Transición a POST_MARKET

POST_MARKET (22:00+)
  ├─> Actualizar journal
  ├─> Calcular KPIs
  ├─> Generar card diaria
  ├─> Enviar notificaciones
  └─> SHUTDOWN

ERROR_STATE (cualquier momento)
  ├─> Registrar detalles error
  ├─> Intentar recuperación
  ├─> Notificar usuario
  └─> Si crítico: EMERGENCY_SHUTDOWN
```

**Loop Principal (estado TRADING):**
```python
async def _trading_session(self):
    """Loop de trading principal"""
    
    while self.state == AgentState.TRADING:
        # Obtener últimos datos mercado
        data = await self.data_handler.get_next_tick()
        
        # Flujo específico por modo
        if self.mode == AgentMode.AUTONOMOUS:
            await self._autonomous_flow(data)
            
        elif self.mode == AgentMode.SIGNALS:
            await self._signals_flow(data)
            
        elif self.mode == AgentMode.MONITOR:
            await self._monitor_flow(data)
        
        # Verificar EOD
        if self._is_close_time():
            self.state = AgentState.CLOSE
            break
        
        # Throttle loop
        await asyncio.sleep(0.01)  # Sleep 10ms
```

---

## Flujos de Comunicación

### 1. Flujo Ejecución Trade (Modo Autónomo)

```
┌─────────────┐
│ Market Data │
│   (Tick)    │
└──────┬──────┘
       │
       v
┌──────────────────┐
│ Motor Estrategia │ ──> Detecta setup A-L @ 18,015
│                  │     SL: 17,995 | TP: 18,055
└──────┬───────────┘
       │
       v
┌──────────────────┐
│  Gestor Riesgo   │ ──> Calcula: 3 micros
│                  │     Valida: MAE OK, RR OK
└──────┬───────────┘
       │
       v
┌──────────────────┐
│ Guardián Cuenta  │ ──> Chequea: Intentos OK
│                  │     PnL Diario: -0.2R → PERMITIR
└──────┬───────────┘
       │
       v
┌──────────────────┐
│ Controlador Exec │ ──> Construye orden bracket
│                  │     Escribe archivo ATI
└──────┬───────────┘
       │
       v
┌──────────────────┐
│ NinjaTrader 8    │ ──> Lee comando ATI
│                  │     Ejecuta vía Rithmic
│                  │     Envía fills de vuelta
└──────┬───────────┘
       │
       v
┌──────────────────┐
│ Position Tracker │ ──> Monitorea fill
│                  │     Trackea MFE/MAE
│                  │     En +0.5R: SL→BE
└──────┬───────────┘
       │
       v
┌──────────────────┐
│ Gestor Journal   │ ──> Registra trade completo
│                  │     Actualiza KPIs
└──────────────────┘
```

### 2. Flujo Override Manual

```
Usuario hace clic en "MANUAL" en Add-on NT8
       │
       v
┌──────────────────┐
│  Add-on (C#)     │ ──> Envía comando cambio modo
└──────┬───────────┘
       │
       v
┌──────────────────┐
│ Trading Agent    │ ──> Cambia a modo MANUAL
│                  │     Detiene escaneo setups
└──────┬───────────┘
       │
       v
┌──────────────────┐
│ Motor Estrategia │ ──> Si se detecta señal:
│                  │       - Envía notificación
│                  │       - Dibuja en chart
│                  │       - Espera acción usuario
└──────────────────┘

Usuario coloca trade manualmente en NT8
       │
       v
┌──────────────────┐
│ Trading Agent    │ ──> Detecta posición manual
│                  │     Cambia a modo MONITOR
└──────┬───────────┘
       │
       v
┌──────────────────┐
│ Position Tracker │ ──> Monitorea trade del usuario
│                  │     Trackea PnL
│                  │     Alerta si cerca límites riesgo
└──────┬───────────┘
       │
       v
Usuario cierra posición
       │
       v
┌──────────────────┐
│ Trading Agent    │ ──> Detecta flat
│                  │     Vuelve a modo AUTÓNOMO
└──────────────────┘
```

---

## Arquitectura de Datos

### Estructura Market Data

```python
@dataclass
class MarketData:
    """Snapshot inmutable de datos de mercado"""
    timestamp: datetime
    symbol: str
    price: float
    bid: float
    ask: float
    volume: int
    bid_size: int
    ask_size: int
    
    # Campos derivados
    pdh: float
    pdl: float
    onh: float
    onl: float
    or15_high: float
    or15_low: float
    or15_range: float
    volume_factor: float
    
    # Técnicos
    support_levels: List[float]
    resistance_levels: List[float]
```

### Estados Ciclo de Vida Trade

```python
class TradeStatus(Enum):
    PENDING = "pendiente"         # Orden colocada, no llena
    OPEN = "abierto"              # Posición activa
    BREAKEVEN = "breakeven"       # SL movido a entrada
    CLOSED_TP = "cerrado_tp"      # Hit take-profit
    CLOSED_SL = "cerrado_sl"      # Hit stop-loss
    CLOSED_BE = "cerrado_be"      # Hit breakeven
    CLOSED_MANUAL = "cerrado_manual"  # Usuario cerró
    CANCELLED = "cancelado"       # Orden cancelada
```

---

## Stack Tecnológico

### Python Core (3.11+)

**Librerías Requeridas:**
```
# Core
python = "^3.11"
asyncio = "*"  # Built-in

# Procesamiento Datos
numpy = "^1.24.3"
pandas = "^2.1.0"

# API Rithmic
async-rithmic = "^1.2.4"

# Base de Datos
sqlite3 = "*"  # Built-in

# Utilidades
python-dotenv = "^1.0.0"
pyyaml = "^6.0"
loguru = "^0.7.0"

# Opcional (notificaciones)
python-telegram-bot = "^20.0"

# Desarrollo
pytest = "^7.4.0"
pytest-asyncio = "^0.21.0"
black = "^23.0.0"
mypy = "^1.5.0"
```

**Comando Instalación:**
```bash
pip install numpy==1.24.3 pandas==2.1.0 async-rithmic==1.2.4 \
    python-dotenv==1.0.0 pyyaml==6.0 loguru==0.7.0
```

### Add-on NinjaTrader 8 (C#)

**Requisitos:**
- .NET Framework 4.8
- NinjaTrader 8.0.29.1 o posterior
- Visual Studio 2022 Community (para desarrollo)

**Tecnologías Clave:**
- C# 8.0
- WPF (XAML) para UI
- System.IO para operaciones archivos ATI
- Namespace NinjaTrader.NinjaScript

### Base de Datos

**SQLite 3:**
- Basada en archivo: `data/journal.db`
- No requiere servidor
- ACID compliant
- Path migración futuro a PostgreSQL para multi-instancia

### Protocolos de Comunicación

**Rithmic → Python:**
- Protocolo: WebSocket (Protocol Buffers)
- Puerto: Dinámico (asignado por Rithmic)
- Latencia: <5ms típico

**Python → NinjaTrader:**
- Protocolo: Basado en archivos (ATI)
- Path: `C:\Users\<user>\Documents\NinjaTrader 8\incoming\`
- Latencia: ~50-100ms
- Futuro: CrossTrade REST API (~20ms)

**NinjaTrader → Rithmic:**
- Integración nativa
- Latencia: <5ms

---

## Patrones de Diseño

### 1. Strategy Pattern

**Propósito:** Permitir múltiples estrategias de trading intercambiables en runtime

```python
# Interface
class IStrategy(ABC):
    @abstractmethod
    def detect_setup(self, data: MarketData) -> Optional[Signal]:
        pass

# Implementaciones
class MNQLevelsStrategy(IStrategy):
    def detect_setup(self, data: MarketData) -> Optional[Signal]:
        # Lógica PDH/PDL/ONH/ONL
        pass

class VWAPStrategy(IStrategy):  # Futuro
    def detect_setup(self, data: MarketData) -> Optional[Signal]:
        # Lógica mean reversion VWAP
        pass

# Uso
strategy = StrategyFactory.create("mnq_levels", config)
agent = TradingAgent(strategy=strategy, ...)
```

### 2. Observer Pattern

**Propósito:** Eventos de market data notifican a múltiples suscriptores

```python
class MarketDataStream:
    def __init__(self):
        self._subscribers: List[Callable] = []
    
    def subscribe(self, callback: Callable) -> None:
        self._subscribers.append(callback)
    
    def _notify(self, data: MarketData) -> None:
        for callback in self._subscribers:
            callback(data)

# Uso
stream = MarketDataStream()
stream.subscribe(strategy_engine.on_data)
stream.subscribe(chart_display.update)
stream.subscribe(position_tracker.update_mfe_mae)
```

---

## Escalabilidad y Expansión Futura

### Fase 1 (Actual) - Una Cuenta, Basado en Reglas

**Características:**
- Solo MNQ
- 1 estrategia (niveles)
- 1 cuenta
- Ejecutor ATI
- Base de datos SQLite

**Restricciones:**
- Máx 2 trades/día
- 50-100 micros según balance
- Ejecución single-threaded

---

### Fase 2 (Meses 6-12) - Multi-Estrategia, Recolección Datos

**Nuevas Características:**
- Añadir 2-3 estrategias adicionales
- Lógica rotación/selección estrategia
- Logging comprehensivo de datos para ML
- Dashboard comparación performance

**Cambios Técnicos:**
- Patrón registry estrategias
- Framework A/B testing
- Métricas mejoradas (Sharpe, Sortino, Calmar)

---

### Fase 3 (Año 2) - Integración Machine Learning

**Nuevas Características:**
- Modelo RL para optimización timing entrada
- Position sizing basado en ML
- Integración análisis sentimiento
- Lógica stop-loss adaptativa

**Arquitectura Adicional:**
```
┌─────────────────────────────────────┐
│     Capa ML/RL (Nueva)              │
├─────────────────────────────────────┤
│  - Servidor Modelo (TF Serving)     │
│  - Pipeline Feature Engineering     │
│  - Módulo Online Learning           │
│  - Registry Modelos                 │
└─────────────────────────────────────┘
         ↓
[Motor Estrategia Existente]
```

---

### Fase 4 (Año 3+) - Multi-Cuenta, Multi-Activo

**Nuevas Características:**
- Gestión simultánea multi-cuenta (hasta 20 cuentas Apex)
- Soporte multi-activo (ES, NQ, YM, RTY)
- Análisis correlación cross-instrumento
- Gestión riesgo nivel portfolio

**Arquitectura Evolutiva:**
```
┌───────────────────────────────────────────────────────────┐
│              API Gateway (FastAPI)                        │
└───────────┬───────────────────────────────────────────────┘
            │
    ┌───────┴────────┐
    │  Load Balancer │
    └───────┬────────┘
            │
    ┌───────┴────────────────────────────┐
    │                                     │
    v                                     v
┌─────────────────┐            ┌─────────────────┐
│ Instancia Agent│            │ Instancia Agent│
│ - Cuentas 1-5  │            │ - Cuentas 16-20│
│ - MNQ/ES       │            │ - MNQ/YM       │
└─────────────────┘            └─────────────────┘
         │                              │
         └──────────────┬───────────────┘
                        │
                        v
               ┌─────────────────┐
               │  PostgreSQL DB  │
               │ (Master/Slave)  │
               └─────────────────┘
```

---

## Consideraciones de Seguridad

### 1. Gestión de Credenciales

**Actual (Desarrollo):**
```
Archivo .env (NO commiteado a git)
├─ RITHMIC_USER=xxx
├─ RITHMIC_PASSWORD=xxx
├─ RITHMIC_SYSTEM=Rithmic Test
├─ TELEGRAM_BOT_TOKEN=xxx
└─ TELEGRAM_CHAT_ID=xxx
```

**Producción:**
- Usar variables de entorno
- Considerar: AWS Secrets Manager, Azure Key Vault
- Rotar credenciales trimestralmente

### 2. Seguridad API Key

**Reglas:**
- Nunca hardcodear API keys
- Nunca commitear a control de versiones
- Usar keys read-only cuando sea posible
- Implementar IP whitelisting (Rithmic)

### 3. Salvaguardas de Riesgo

**Circuit Breakers:**
```python
class CircuitBreaker:
    def __init__(
        self,
        max_daily_loss: float = -120,  # -1R
        max_monthly_loss: float = -360,  # -3R
        max_mae_single_trade: float = 750
    ):
        self.max_daily_loss = max_daily_loss
        self.max_monthly_loss = max_monthly_loss
        self.max_mae = max_mae_single_trade
    
    def should_halt_trading(self, account: Account) -> bool:
        if account.daily_pnl <= self.max_daily_loss:
            return True
        if account.monthly_pnl <= self.max_monthly_loss:
            return True
        return False
```

**Emergency Stop:**
- Hotkey en Add-on NT8: Ctrl+Shift+E
- Cancela todas órdenes + flatea posiciones inmediatamente
- Deshabilita agente hasta reset manual

---

## Benchmarks de Performance

### Objetivos de Latencia

| Componente | Objetivo | Medido |
|-----------|----------|---------|
| Procesamiento tick Rithmic | <1ms | 0.3ms |
| Detección setup estrategia | <10ms | 5ms |
| Validación riesgo | <5ms | 2ms |
| Escritura archivo ATI | <50ms | 30ms |
| Ejecución orden NT8 | <100ms | 80ms |
| **End-to-end (señal→orden)** | **<200ms** | **120ms** |

### Uso de Recursos

**RAM:**
- Baseline: 150MB
- Pico (posición activa): 200MB

**CPU:**
- Idle: <1%
- Escaneo activo: 5-10%
- Pico (ejecución trade): 15%

**Disco:**
- Crecimiento BD: ~1MB/mes
- Logs: ~100MB/mes

**Red:**
- Rithmic: ~50KB/s sostenido
- Picos: 500KB/s durante alta volatilidad

---

## Monitoreo y Observabilidad

### Estrategia de Logging

**Niveles:**
```python
# Configuración loguru
logger.add(
    "logs/agent_{time}.log",
    rotation="1 day",
    retention="90 days",
    level="INFO",
    format="{time:YYYY-MM-DD HH:mm:ss} | {level} | {module}:{function}:{line} | {message}"
)
```

**Qué Loggear:**
- INFO: Transiciones estado, ejecuciones trade, resúmenes diarios
- DEBUG: Detecciones setup (aunque se filtren), cálculos riesgo
- WARNING: Casi-violaciones riesgo, alta latencia
- ERROR: Ejecuciones fallidas, desconexiones API
- CRITICAL: Triggers circuit breaker, emergency stops

---

## Conclusión

Esta arquitectura proporciona una base sólida para un sistema de trading automatizado de nivel profesional. Fortalezas clave:

1. **Modularidad:** Fácil añadir estrategias, ejecutores, cuentas
2. **Testabilidad:** Dependency injection, componentes mockeados
3. **Escalabilidad:** Path claro de 1 cuenta a 20+
4. **Seguridad:** Guardian, circuit breakers, cumplimiento APEX
5. **Observabilidad:** Logging comprehensivo y métricas

El sistema está diseñado para evolucionar desde basado-en-reglas (Fase 1) a mejorado-con-ML (Fase 3) sin refactoring mayor. Todas las abstracciones core (IStrategy, IExecutor, IRiskManager) soportan esta progresión.

---

**Próximos Pasos:**
1. Revisar este documento de arquitectura
2. Proceder a GUIA_DESARROLLO.md para detalles implementación
3. Referenciar API_REFERENCE.md para especificaciones de clases
4. Seguir DEPLOYMENT.md para instrucciones setup

---

*Fin del Documento de Arquitectura*
