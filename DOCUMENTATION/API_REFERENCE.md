# Referencia API — Agente de Trading MNQ

**Versión:** 1.0.0  
**Fecha:** 2025-11-06  
**Lenguaje:** Python 3.11+ / C# .NET 4.8

---

## Índice

1. [Capa de Dominio](#capa-de-dominio)
2. [Capa de Aplicación](#capa-de-aplicación)
3. [Capa de Infraestructura](#capa-de-infraestructura)
4. [Núcleo del Agente](#núcleo-del-agente)
5. [NinjaScript (C#)](#ninjatrader-ninjaScript-c)

---

## Capa de Dominio

### Entidades

#### `Account`

```python
from dataclasses import dataclass
from typing import Optional
from datetime import datetime

@dataclass
class Account:
    """
    Entidad que representa una cuenta de trading.
    
    Attributes:
        account_id: ID de la cuenta (ej: "Sim101")
        balance: Balance actual en USD
        daily_pnl: PnL del día actual en USD
        monthly_pnl: PnL del mes actual en USD
        trailing_threshold: Umbral de drawdown dinámico
        max_contracts: Máximo de contratos permitidos (scaling)
        updated_at: Timestamp de última actualización
    """
    account_id: str
    balance: float
    daily_pnl: float = 0.0
    monthly_pnl: float = 0.0
    trailing_threshold: float = 2500.0
    max_contracts: int = 50
    updated_at: Optional[datetime] = None
    
    @property
    def daily_pnl_R(self) -> float:
        """PnL diario en múltiplos de R (R=120)"""
        return self.daily_pnl / 120.0
    
    @property
    def monthly_pnl_R(self) -> float:
        """PnL mensual en múltiplos de R"""
        return self.monthly_pnl / 120.0
    
    def can_trade(self, daily_limit_R: float = -1.0) -> bool:
        """
        Verifica si la cuenta puede seguir operando.
        
        Args:
            daily_limit_R: Límite diario en R (default -1.0R)
            
        Returns:
            True si puede operar, False si alcanzó límite
        """
        return self.daily_pnl_R > daily_limit_R
```

#### `Trade`

```python
from dataclasses import dataclass
from enum import Enum
from uuid import UUID, uuid4
from datetime import datetime
from typing import Optional

class TradeStatus(Enum):
    """Estados posibles de un trade"""
    PENDING = "pendiente"
    OPEN = "abierto"
    BREAKEVEN = "breakeven"
    CLOSED_TP = "cerrado_tp"
    CLOSED_SL = "cerrado_sl"
    CLOSED_BE = "cerrado_be"
    CLOSED_MANUAL = "cerrado_manual"
    CANCELLED = "cancelado"

@dataclass
class Trade:
    """
    Entidad que representa un trade completo.
    
    Attributes:
        id: UUID único del trade
        timestamp: Momento de creación
        account_id: Cuenta en la que se ejecuta
        strategy_name: Estrategia que generó la señal
        setup_type: Tipo de setup (A-L, B-L, etc.)
        entry_price: Precio de entrada
        stop_loss: Precio de stop-loss
        take_profit: Precio de take-profit
        contracts: Número de contratos
        status: Estado actual del trade
        result_R: Resultado en múltiplos de R
        result_usd: Resultado en USD
        mfe_pts: Maximum Favorable Excursion en puntos
        mae_pts: Maximum Adverse Excursion en puntos
        notes: Notas adicionales
    """
    id: UUID
    timestamp: datetime
    account_id: str
    strategy_name: str
    setup_type: str
    entry_price: float
    stop_loss: float
    take_profit: float
    contracts: int
    status: TradeStatus = TradeStatus.PENDING
    result_R: float = 0.0
    result_usd: float = 0.0
    mfe_pts: float = 0.0
    mae_pts: float = 0.0
    notes: str = ""
    
    @staticmethod
    def create(
        account_id: str,
        strategy_name: str,
        setup_type: str,
        entry_price: float,
        stop_loss: float,
        take_profit: float,
        contracts: int
    ) -> 'Trade':
        """
        Factory method para crear un nuevo trade.
        
        Returns:
            Nueva instancia de Trade
        """
        return Trade(
            id=uuid4(),
            timestamp=datetime.now(),
            account_id=account_id,
            strategy_name=strategy_name,
            setup_type=setup_type,
            entry_price=entry_price,
            stop_loss=stop_loss,
            take_profit=take_profit,
            contracts=contracts
        )
    
    def calculate_result(self, exit_price: float) -> None:
        """
        Calcula el resultado del trade al cerrar.
        
        Args:
            exit_price: Precio de salida
        """
        # MNQ: 1 punto = $2
        pts_gained = exit_price - self.entry_price
        self.result_usd = pts_gained * self.contracts * 2
        
        # Calcular en R
        stop_pts = abs(self.entry_price - self.stop_loss)
        risk_usd = stop_pts * self.contracts * 2
        if risk_usd > 0:
            self.result_R = self.result_usd / risk_usd
```

#### `Signal`

```python
from dataclasses import dataclass
from datetime import datetime
from typing import Optional

@dataclass
class Signal:
    """
    Value object que representa una señal de trading.
    
    Attributes:
        setup_type: Tipo de setup (A-L, B-L, A-S, B-S, B-M)
        direction: "LONG" o "SHORT"
        entry_price: Precio de entrada sugerido
        stop_loss: Precio de stop-loss
        take_profit: Precio de take-profit
        confidence: Nivel de confianza [0.0, 1.0]
        timestamp: Momento de generación
        invalidation: Condición que invalida la señal
        notes: Notas adicionales
    """
    setup_type: str
    direction: str
    entry_price: float
    stop_loss: float
    take_profit: float
    confidence: float = 1.0
    timestamp: datetime = datetime.now()
    invalidation: Optional[str] = None
    notes: str = ""
    
    @property
    def stop_pts(self) -> float:
        """Distancia del stop en puntos"""
        return abs(self.entry_price - self.stop_loss)
    
    @property
    def target_pts(self) -> float:
        """Distancia del target en puntos"""
        return abs(self.take_profit - self.entry_price)
    
    @property
    def rr_ratio(self) -> float:
        """Ratio Risk:Reward"""
        if self.stop_pts == 0:
            return 0.0
        return self.target_pts / self.stop_pts
    
    def is_valid(self, min_rr: float = 1.5) -> bool:
        """
        Valida si la señal cumple requisitos mínimos.
        
        Args:
            min_rr: RR mínimo requerido
            
        Returns:
            True si válida
        """
        return self.rr_ratio >= min_rr and self.confidence > 0.5
```

### Value Objects

#### `MarketData`

```python
from dataclasses import dataclass
from datetime import datetime
from typing import List

@dataclass
class MarketData:
    """
    Snapshot inmutable de datos de mercado.
    
    Attributes:
        timestamp: Momento del snapshot
        symbol: Símbolo del instrumento
        price: Precio actual
        bid: Mejor bid
        ask: Mejor ask
        volume: Volumen de la vela/tick
        bid_size: Tamaño del bid
        ask_size: Tamaño del ask
        pdh: Previous Day High
        pdl: Previous Day Low
        onh: Opening New High
        onl: Opening New Low
        or15_high: Opening Range 15min High
        or15_low: Opening Range 15min Low
        or15_range: Rango del OR15'
        volume_factor: Factor de volumen vs mediana
        support_levels: Niveles de soporte identificados
        resistance_levels: Niveles de resistencia identificados
    """
    timestamp: datetime
    symbol: str
    price: float
    bid: float
    ask: float
    volume: int
    bid_size: int
    ask_size: int
    pdh: float
    pdl: float
    onh: float
    onl: float
    or15_high: float
    or15_low: float
    or15_range: float
    volume_factor: float
    support_levels: List[float]
    resistance_levels: List[float]
    
    @property
    def spread(self) -> float:
        """Spread bid-ask"""
        return self.ask - self.bid
    
    @property
    def mid_price(self) -> float:
        """Precio medio"""
        return (self.bid + self.ask) / 2
```

#### `Order`

```python
from dataclasses import dataclass
from enum import Enum
from uuid import UUID
from typing import Optional

class OrderType(Enum):
    """Tipos de orden"""
    MARKET = "MARKET"
    LIMIT = "LIMIT"
    STOP = "STOP"
    STOP_LIMIT = "STOP_LIMIT"

class OrderSide(Enum):
    """Lado de la orden"""
    BUY = "BUY"
    SELL = "SELL"

@dataclass
class Order:
    """
    Value object que representa una orden.
    
    Attributes:
        order_id: ID único de la orden
        account_id: Cuenta en la que se ejecuta
        symbol: Instrumento
        side: BUY o SELL
        order_type: Tipo de orden
        quantity: Cantidad de contratos
        limit_price: Precio límite (si aplica)
        stop_price: Precio stop (si aplica)
        strategy_id: ID de estrategia (para bracket orders)
    """
    order_id: UUID
    account_id: str
    symbol: str
    side: OrderSide
    order_type: OrderType
    quantity: int
    limit_price: Optional[float] = None
    stop_price: Optional[float] = None
    strategy_id: Optional[str] = None
    
    def to_ati_command(self) -> str:
        """
        Convierte la orden a formato ATI para NinjaTrader.
        
        Returns:
            Comando ATI como string
            
        Example:
            >>> order.to_ati_command()
            'PLACE;Sim101;MNQ 12-24;BUY;3;LIMIT;18015.0;0'
        """
        cmd = f"PLACE;{self.account_id};{self.symbol};{self.side.value};{self.quantity}"
        cmd += f";{self.order_type.value}"
        
        if self.order_type == OrderType.LIMIT:
            cmd += f";{self.limit_price};0"
        elif self.order_type == OrderType.STOP:
            cmd += f";0;{self.stop_price}"
        elif self.order_type == OrderType.STOP_LIMIT:
            cmd += f";{self.limit_price};{self.stop_price}"
        else:  # MARKET
            cmd += ";0;0"
        
        return cmd
```

### Interfaces

#### `IStrategy`

```python
from abc import ABC, abstractmethod
from typing import Optional, Tuple

class IStrategy(ABC):
    """
    Interfaz para estrategias de trading.
    """
    
    @abstractmethod
    def detect_setup(self, data: MarketData) -> Optional[Signal]:
        """
        Detecta si existe un setup válido en los datos actuales.
        
        Args:
            data: Datos de mercado actuales
            
        Returns:
            Signal si detecta setup, None si no
        """
        pass
    
    @abstractmethod
    def calculate_stops(self, signal: Signal) -> Tuple[float, float]:
        """
        Calcula los niveles de SL y TP para una señal.
        
        Args:
            signal: Señal detectada
            
        Returns:
            Tupla (stop_loss, take_profit)
        """
        pass
    
    @abstractmethod
    def validate_conditions(self, signal: Signal, data: MarketData) -> bool:
        """
        Valida condiciones adicionales antes de ejecutar.
        
        Args:
            signal: Señal a validar
            data: Datos actuales del mercado
            
        Returns:
            True si pasa validación
        """
        pass
```

#### `IExecutor`

```python
from abc import ABC, abstractmethod
from typing import Optional

class IExecutor(ABC):
    """
    Interfaz para ejecutores de órdenes.
    """
    
    @abstractmethod
    def place_order(self, order: Order) -> str:
        """
        Coloca una orden en el mercado.
        
        Args:
            order: Orden a colocar
            
        Returns:
            Order ID asignado
            
        Raises:
            ConnectionError: Si no puede conectar con broker
            ValueError: Si parámetros de orden inválidos
        """
        pass
    
    @abstractmethod
    def modify_order(self, order_id: str, new_sl: float) -> None:
        """
        Modifica el stop-loss de una orden existente.
        
        Args:
            order_id: ID de la orden
            new_sl: Nuevo precio de stop-loss
        """
        pass
    
    @abstractmethod
    def cancel_order(self, order_id: str) -> None:
        """
        Cancela una orden pendiente.
        
        Args:
            order_id: ID de la orden a cancelar
        """
        pass
    
    @abstractmethod
    def cancel_all(self, account_id: str) -> None:
        """
        Cancela todas las órdenes de una cuenta.
        
        Args:
            account_id: ID de la cuenta
        """
        pass
```

#### `IRiskManager`

```python
from abc import ABC, abstractmethod
from typing import Tuple

class IRiskManager(ABC):
    """
    Interfaz para gestión de riesgo.
    """
    
    @abstractmethod
    def calculate_position_size(
        self,
        account: Account,
        stop_pts: float,
        R_risk: float = 120.0
    ) -> int:
        """
        Calcula el tamaño de posición óptimo.
        
        Args:
            account: Cuenta en la que se opera
            stop_pts: Distancia del stop en puntos
            R_risk: Riesgo por trade en USD
            
        Returns:
            Número de contratos
        """
        pass
    
    @abstractmethod
    def validate_trade(
        self,
        account: Account,
        signal: Signal
    ) -> Tuple[bool, str]:
        """
        Valida si un trade cumple reglas de riesgo.
        
        Args:
            account: Cuenta
            signal: Señal a validar
            
        Returns:
            (es_válido, razón)
        """
        pass
    
    @abstractmethod
    def check_mae_limit(
        self,
        contracts: int,
        stop_pts: float,
        limit: float = 750.0
    ) -> bool:
        """
        Verifica que MAE potencial no exceda límite.
        
        Args:
            contracts: Número de contratos
            stop_pts: Distancia del stop
            limit: Límite MAE en USD
            
        Returns:
            True si dentro del límite
        """
        pass
```

---

## Capa de Aplicación

### `TradingAgent`

```python
from enum import Enum
from typing import Optional

class AgentMode(Enum):
    """Modos de operación del agente"""
    AUTONOMOUS = "autonomo"
    SIGNALS = "senales"
    MONITOR = "monitor"
    MANUAL = "manual"

class AgentState(Enum):
    """Estados del ciclo de vida"""
    BOOT = "boot"
    PRE_MARKET = "pre_mercado"
    TRADING = "trading"
    CLOSE = "cierre"
    POST_MARKET = "post_mercado"
    ERROR = "error"
    SHUTDOWN = "apagado"

class TradingAgent:
    """
    Orquestador principal del agente de trading.
    
    Attributes:
        account: Cuenta en la que opera
        strategy: Estrategia de trading
        risk_manager: Gestor de riesgo
        executor: Ejecutor de órdenes
        journal: Gestor de journal
        guardian: Guardian de cuenta
        mode: Modo de operación actual
        state: Estado del ciclo de vida
    """
    
    def __init__(
        self,
        account: Account,
        strategy: IStrategy,
        risk_mgr: IRiskManager,
        executor: IExecutor,
        journal: 'JournalManager',
        guardian: 'AccountGuardian',
        mode: AgentMode = AgentMode.AUTONOMOUS
    ):
        """
        Inicializa el agente de trading.
        
        Args:
            account: Cuenta a operar
            strategy: Implementación de estrategia
            risk_mgr: Gestor de riesgo
            executor: Ejecutor de órdenes
            journal: Gestor de journal
            guardian: Guardian de cuenta
            mode: Modo inicial
        """
        self.account = account
        self.strategy = strategy
        self.risk_manager = risk_mgr
        self.executor = executor
        self.journal = journal
        self.guardian = guardian
        self.mode = mode
        self.state = AgentState.BOOT
    
    async def run(self) -> None:
        """
        Ejecuta el ciclo principal del agente.
        
        Secuencia de estados:
        BOOT → PRE_MARKET → TRADING → CLOSE → POST_MARKET → SHUTDOWN
        """
        try:
            await self._boot_sequence()
            await self._pre_market_analysis()
            await self._trading_session()
            await self._end_of_day_close()
            await self._post_market_report()
        except Exception as e:
            self.state = AgentState.ERROR
            logger.critical(f"Agent error: {e}")
            raise
        finally:
            await self.shutdown()
    
    async def _boot_sequence(self) -> None:
        """Secuencia de arranque (14:45-15:05)"""
        pass
    
    async def _pre_market_analysis(self) -> None:
        """Análisis pre-mercado (15:05-15:30)"""
        pass
    
    async def _trading_session(self) -> None:
        """Sesión de trading (15:30-22:00)"""
        pass
    
    async def _end_of_day_close(self) -> None:
        """Cierre de día (22:00)"""
        pass
    
    async def _post_market_report(self) -> None:
        """Reporte post-mercado (22:00+)"""
        pass
    
    def switch_mode(self, new_mode: AgentMode) -> None:
        """
        Cambia el modo de operación del agente.
        
        Args:
            new_mode: Nuevo modo
        """
        logger.info(f"Switching mode: {self.mode} → {new_mode}")
        self.mode = new_mode
    
    async def shutdown(self) -> None:
        """Apaga el agente limpiamente"""
        self.state = AgentState.SHUTDOWN
        logger.info("Agent shutdown complete")
```

---

## Capa de Infraestructura

### Rithmic

#### `RithmicDataHandler`

```python
class RithmicDataHandler:
    """
    Manejador de datos de mercado desde Rithmic API.
    
    Attributes:
        connection: Conexión activa a Rithmic
        subscribers: Callbacks suscritos a datos
    """
    
    def __init__(self, credentials: Dict[str, str]):
        """
        Args:
            credentials: Diccionario con user, password, system
        """
        self.credentials = credentials
        self.connection = None
        self.subscribers = []
    
    async def connect(self) -> None:
        """
        Establece conexión WebSocket con Rithmic.
        
        Raises:
            ConnectionError: Si no puede conectar
        """
        pass
    
    async def subscribe_instrument(self, symbol: str) -> None:
        """
        Suscribe a datos de un instrumento.
        
        Args:
            symbol: Símbolo (ej: "MNQ")
        """
        pass
    
    def on_tick(self, callback: Callable[[MarketData], None]) -> None:
        """
        Registra un callback para cada tick.
        
        Args:
            callback: Función a llamar con cada tick
        """
        self.subscribers.append(callback)
    
    async def get_current_price(self) -> float:
        """
        Obtiene el precio actual.
        
        Returns:
            Precio actual del instrumento
        """
        pass
    
    async def disconnect(self) -> None:
        """Cierra conexión limpiamente"""
        pass
```

### NinjaTrader

#### `ATIExecutor`

```python
class ATIExecutor(IExecutor):
    """
    Ejecutor de órdenes vía ATI (Automated Trading Interface).
    
    Attributes:
        ati_path: Path al directorio incoming de NT8
        timeout: Timeout para esperar confirmación (segundos)
    """
    
    def __init__(self, ati_path: str, timeout: int = 10):
        """
        Args:
            ati_path: Path completo a incoming directory
            timeout: Timeout en segundos
        """
        self.ati_path = ati_path
        self.timeout = timeout
    
    def place_order(self, order: Order) -> str:
        """
        Coloca orden escribiendo archivo ATI.
        
        Implementation:
        1. Convierte Order a comando ATI
        2. Escribe archivo .txt en incoming/
        3. Espera confirmación en outgoing/
        4. Parsea order ID
        
        Args:
            order: Orden a colocar
            
        Returns:
            Order ID asignado por NT8
        """
        # Generar comando ATI
        command = order.to_ati_command()
        
        # Escribir archivo
        filename = f"order_{order.order_id}.txt"
        filepath = os.path.join(self.ati_path, filename)
        
        with open(filepath, 'w') as f:
            f.write(command)
        
        # Esperar confirmación
        order_id = self._wait_for_confirmation(order.order_id)
        return order_id
    
    def _wait_for_confirmation(self, order_uuid: UUID) -> str:
        """Espera confirmación de NT8"""
        pass
    
    def modify_order(self, order_id: str, new_sl: float) -> None:
        """Modifica SL de orden existente"""
        pass
    
    def cancel_order(self, order_id: str) -> None:
        """Cancela orden"""
        pass
    
    def cancel_all(self, account_id: str) -> None:
        """Cancela todas las órdenes"""
        command = f"CANCELALL;{account_id}"
        self._write_ati_command(command)
```

### Base de Datos

#### `JournalManager`

```python
import sqlite3
from typing import List, Dict
from datetime import datetime, date

class JournalManager:
    """
    Gestor de journal y persistencia en SQLite.
    
    Attributes:
        db_path: Path a base de datos SQLite
        connection: Conexión activa
    """
    
    def __init__(self, db_path: str):
        """
        Args:
            db_path: Path al archivo .db
        """
        self.db_path = db_path
        self.connection = sqlite3.connect(db_path)
        self._init_tables()
    
    def _init_tables(self) -> None:
        """Crea tablas si no existen"""
        with self.connection:
            self.connection.execute("""
                CREATE TABLE IF NOT EXISTS trades (
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
                )
            """)
            
            self.connection.execute("""
                CREATE TABLE IF NOT EXISTS daily_summary (
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
                )
            """)
    
    def log_trade(self, trade: Trade) -> None:
        """
        Registra un trade en el journal.
        
        Args:
            trade: Trade a registrar
        """
        with self.connection:
            self.connection.execute("""
                INSERT INTO trades VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            """, (
                str(trade.id),
                trade.timestamp,
                trade.account_id,
                trade.strategy_name,
                trade.setup_type,
                trade.entry_price,
                trade.stop_loss,
                trade.take_profit,
                trade.contracts,
                trade.status.value,
                trade.result_R,
                trade.result_usd,
                trade.mfe_pts,
                trade.mae_pts,
                trade.notes
            ))
    
    def update_trade_result(
        self,
        trade_id: UUID,
        status: TradeStatus,
        result_R: float,
        result_usd: float,
        mfe_pts: float,
        mae_pts: float
    ) -> None:
        """
        Actualiza resultado de un trade.
        
        Args:
            trade_id: ID del trade
            status: Estado final
            result_R: Resultado en R
            result_usd: Resultado en USD
            mfe_pts: MFE en puntos
            mae_pts: MAE en puntos
        """
        with self.connection:
            self.connection.execute("""
                UPDATE trades
                SET status=?, result_R=?, result_usd=?, mfe_pts=?, mae_pts=?
                WHERE id=?
            """, (status.value, result_R, result_usd, mfe_pts, mae_pts, str(trade_id)))
    
    def get_daily_summary(self, date: date) -> Dict:
        """
        Obtiene resumen del día.
        
        Args:
            date: Fecha a consultar
            
        Returns:
            Diccionario con métricas del día
        """
        cursor = self.connection.execute("""
            SELECT * FROM daily_summary WHERE date=?
        """, (date,))
        
        row = cursor.fetchone()
        if not row:
            return {}
        
        return {
            'date': row[0],
            'attempts': row[1],
            'trades': row[2],
            'wins': row[3],
            'losses': row[4],
            'breakevens': row[5],
            'total_R': row[6],
            'total_usd': row[7],
            'max_dd_R': row[8],
            'penalties': row[9]
        }
    
    def get_monthly_kpis(self) -> Dict:
        """
        Calcula KPIs del mes actual.
        
        Returns:
            Diccionario con KPIs mensuales
        """
        # Implementación...
        pass
```

---

## NinjaTrader (NinjaScript C#)

### `MNQAgentAddon`

```csharp
using NinjaTrader.NinjaScript;
using NinjaTrader.Gui;
using System.Windows.Controls;

namespace NinjaTrader.NinjaScript.AddOns
{
    /// <summary>
    /// Add-on principal para visualización y control del agente MNQ.
    /// </summary>
    public class MNQAgentAddon : AddOnBase
    {
        private AgentPanel panel;
        private ATIFileReader atiReader;
        
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Panel de control Agente MNQ";
                Name = "MNQ Agent";
            }
            else if (State == State.Terminated)
            {
                if (panel != null)
                {
                    panel.Close();
                }
            }
        }
        
        /// <summary>
        /// Inicializa el panel visual del agente.
        /// </summary>
        protected override void OnWindowCreated(Window window)
        {
            // Crear panel WPF
            panel = new AgentPanel();
            
            // Inicializar lector ATI
            atiReader = new ATIFileReader();
            atiReader.OnCommandReceived += HandleATICommand;
            
            // Añadir al window de NT8
            var tabItem = new TabItem
            {
                Header = "MNQ Agent",
                Content = panel
            };
            
            window.MainTabControl.Items.Add(tabItem);
        }
        
        /// <summary>
        /// Maneja comandos ATI entrantes desde Python.
        /// </summary>
        private void HandleATICommand(string command)
        {
            // Parse y ejecuta comando
            var parts = command.Split(';');
            var action = parts[0];
            
            switch (action)
            {
                case "PLACE":
                    PlaceOrder(parts);
                    break;
                case "CANCEL":
                    CancelOrder(parts[1]);
                    break;
                case "CANCELALL":
                    CancelAllOrders(parts[1]);
                    break;
            }
        }
        
        /// <summary>
        /// Coloca una orden en NinjaTrader.
        /// </summary>
        private void PlaceOrder(string[] parts)
        {
            string accountId = parts[1];
            string instrument = parts[2];
            string side = parts[3];
            int quantity = int.Parse(parts[4]);
            string orderType = parts[5];
            double limitPrice = double.Parse(parts[6]);
            double stopPrice = double.Parse(parts[7]);
            
            // Ejecutar orden vía NT8 API
            // ...
        }
    }
}
```

---

**Fin de la Referencia API**
