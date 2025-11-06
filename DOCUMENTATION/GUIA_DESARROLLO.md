# Guía de Desarrollo — Agente de Trading MNQ

**Versión:** 1.0.0  
**Fecha:** 2025-11-06  
**Proyecto:** Agente de Trading Autónomo para Futuros MNQ (Apex Trader Funding)

---

## Índice

1. [Introducción](#introducción)
2. [Requisitos del Sistema](#requisitos-del-sistema)
3. [Configuración del Entorno](#configuración-del-entorno)
4. [Estructura del Proyecto](#estructura-del-proyecto)
5. [Estándares de Código](#estándares-de-código)
6. [Workflow de Desarrollo](#workflow-de-desarrollo)
7. [Testing](#testing)
8. [Depuración y Troubleshooting](#depuración-y-troubleshooting)
9. [Recursos](#recursos)

---

## Introducción

Esta guía proporciona instrucciones completas para configurar el entorno de desarrollo, estándares de código y mejores prácticas para trabajar en el Agente de Trading MNQ.

**Objetivo:** Permitir que cualquier desarrollador (o instancia de Claude) pueda:
- Configurar el entorno desde cero
- Entender la estructura del proyecto
- Seguir estándares consistentes
- Contribuir código de calidad

---

## Requisitos del Sistema

### Hardware Mínimo

- **RAM:** 8GB (16GB recomendado)
- **CPU:** 4 cores (8 cores recomendado para backtesting)
- **Disco:** 20GB disponibles
- **Red:** Conexión estable <50ms latencia

### Software Requerido

#### Python
- **Versión:** 3.11 o superior
- **Gestor de paquetes:** pip 23+
- **Entorno virtual:** venv o conda

#### NinjaTrader 8
- **Versión:** 8.0.29.1 o posterior
- **Sistema Operativo:** Windows 10/11 (64-bit)
- **Framework:** .NET Framework 4.8

#### Herramientas de Desarrollo
- **Editor/IDE:** Visual Studio Code (Python) + Visual Studio 2022 (C#)
- **Control de versiones:** Git 2.40+
- **Base de datos:** SQLite 3 (incluido en Python)

#### APIs y Conexiones
- **Rithmic API:** Credenciales de broker compatible
- **Cuenta Apex:** Para testing en simulado o funded account

### Sistema Operativo

**Desarrollo Python:** Windows, Linux, macOS  
**Desarrollo NinjaScript:** Windows únicamente

---

## Configuración del Entorno

### Paso 1: Clonar el Repositorio

```bash
git clone https://github.com/DemFlax/-TRADING_AGENT_IA.git
cd -TRADING_AGENT_IA
```

### Paso 2: Configurar Python

#### Crear Entorno Virtual

```bash
# Windows
python -m venv venv
venv\Scripts\activate

# Linux/macOS
python3 -m venv venv
source venv/bin/activate
```

#### Instalar Dependencias

```bash
pip install --upgrade pip
pip install -r requirements.txt
pip install -r requirements-dev.txt  # Para desarrollo
```

**Archivo `requirements.txt`:**
```
numpy==1.24.3
pandas==2.1.0
async-rithmic==1.2.4
python-dotenv==1.0.0
pyyaml==6.0
loguru==0.7.0
```

**Archivo `requirements-dev.txt`:**
```
pytest==7.4.0
pytest-asyncio==0.21.0
pytest-cov==4.1.0
black==23.0.0
flake8==6.0.0
mypy==1.5.0
```

### Paso 3: Configurar Variables de Entorno

Crear archivo `.env` en la raíz del proyecto:

```bash
# .env (NO commitear a Git)

# Rithmic API
RITHMIC_USER=tu_usuario
RITHMIC_PASSWORD=tu_password
RITHMIC_SYSTEM=Rithmic Test

# Apex Account
APEX_ACCOUNT_ID=Sim101
APEX_ACCOUNT_SIZE=50000

# Trading Parameters
DEFAULT_RISK_PER_TRADE=120
MAX_DAILY_LOSS=120
MAX_MONTHLY_LOSS=360

# NinjaTrader Paths
NT8_INCOMING_PATH=C:\Users\TuUsuario\Documents\NinjaTrader 8\incoming
NT8_OUTGOING_PATH=C:\Users\TuUsuario\Documents\NinjaTrader 8\outgoing

# Telegram (opcional)
TELEGRAM_BOT_TOKEN=
TELEGRAM_CHAT_ID=

# Logging
LOG_LEVEL=INFO
LOG_PATH=logs/
```

**Importante:** Añadir `.env` al `.gitignore` (ya incluido).

### Paso 4: Configurar NinjaTrader 8

#### Instalación

1. Descargar de [ninjatrader.com](https://ninjatrader.com/)
2. Instalar versión 8.0.29.1 o posterior
3. Configurar conexión Rithmic en Tools → Connections

#### Estructura de Directorios NT8

```
C:\Users\[Usuario]\Documents\NinjaTrader 8\
├── bin\
│   └── Custom\
│       ├── AddOns\          # Add-ons compilados (.dll)
│       ├── Strategies\      # Estrategias (.dll)
│       └── Indicators\      # Indicadores (.dll)
├── incoming\                # Comandos ATI entrantes
├── outgoing\                # Respuestas ATI salientes
└── db\                      # Base de datos NT8
```

#### Habilitar ATI (Automated Trading Interface)

1. Tools → Options → Automated Trading Interface
2. Marcar "Enable ATI"
3. Configurar directorio incoming: `Documents\NinjaTrader 8\incoming\`
4. Reiniciar NinjaTrader

### Paso 5: Configurar Visual Studio 2022 (Para NinjaScript)

#### Instalación

1. Descargar [Visual Studio 2022 Community](https://visualstudio.microsoft.com/)
2. Durante instalación, seleccionar:
   - .NET desktop development
   - .NET Framework 4.8
3. Instalar

#### Configurar Proyecto NinjaScript

```bash
# En el proyecto C# de NinjaScript
1. Abrir NinjaTrader 8
2. Tools → Edit NinjaScript → AddOn → New
3. Nombrar: MNQAgentAddon
4. Se abre Visual Studio automáticamente
```

**Referencias requeridas en el proyecto:**
```xml
<Reference Include="NinjaTrader.Core" />
<Reference Include="NinjaTrader.Custom" />
<Reference Include="NinjaTrader.Gui" />
<Reference Include="PresentationCore" />
<Reference Include="PresentationFramework" />
<Reference Include="WindowsBase" />
```

---

## Estructura del Proyecto

```
-TRADING_AGENT_IA/
├── docs/                          # Documentación
│   ├── ARQUITECTURA.md
│   ├── GUIA_DESARROLLO.md        # Este archivo
│   ├── API_REFERENCE.md
│   └── DEPLOYMENT.md
│
├── src/                           # Código fuente Python
│   ├── core/                      # Núcleo del agente
│   │   ├── __init__.py
│   │   ├── trading_agent.py      # Orquestador principal
│   │   ├── state_machine.py      # Máquina de estados
│   │   └── config.py             # Gestión configuración
│   │
│   ├── domain/                    # Lógica de dominio
│   │   ├── entities/
│   │   │   ├── account.py
│   │   │   ├── trade.py
│   │   │   └── signal.py
│   │   ├── value_objects/
│   │   │   ├── market_data.py
│   │   │   ├── order.py
│   │   │   └── level.py
│   │   └── interfaces/
│   │       ├── i_strategy.py
│   │       ├── i_executor.py
│   │       └── i_risk_manager.py
│   │
│   ├── application/               # Casos de uso
│   │   ├── place_trade_usecase.py
│   │   ├── manage_position_usecase.py
│   │   └── generate_report_usecase.py
│   │
│   ├── infrastructure/            # Implementaciones concretas
│   │   ├── rithmic/
│   │   │   ├── data_handler.py   # Stream de datos
│   │   │   └── market_data.py
│   │   ├── ninjatrader/
│   │   │   ├── ati_executor.py   # Ejecución vía ATI
│   │   │   └── ati_parser.py
│   │   ├── database/
│   │   │   └── sqlite_repository.py
│   │   └── notifications/
│   │       └── telegram_notifier.py
│   │
│   ├── strategies/                # Estrategias de trading
│   │   ├── __init__.py
│   │   ├── mnq_levels_strategy.py  # Estrategia actual
│   │   └── strategy_factory.py
│   │
│   └── utils/                     # Utilidades
│       ├── logger.py
│       ├── validators.py
│       └── helpers.py
│
├── ninjatrader/                   # Código C# NinjaScript
│   ├── AddOns/
│   │   └── MNQAgentAddon/
│   │       ├── MNQAgentAddon.cs    # Add-on principal
│   │       ├── AgentPanel.xaml     # UI panel
│   │       ├── AgentPanel.xaml.cs
│   │       └── ATIFileReader.cs    # Lector ATI
│   └── bin/
│       └── Release/
│           └── MNQAgentAddon.dll   # Compilado
│
├── tests/                         # Tests
│   ├── unit/
│   │   ├── test_strategy.py
│   │   ├── test_risk_manager.py
│   │   └── test_order_execution.py
│   ├── integration/
│   │   ├── test_rithmic_connection.py
│   │   └── test_ati_communication.py
│   └── fixtures/
│       └── sample_data.py
│
├── config/                        # Configuración
│   ├── settings.yaml              # Configuración principal
│   └── apex_rules.yaml            # Reglas APEX
│
├── data/                          # Datos (no en Git)
│   ├── journal.db                 # Base de datos SQLite
│   └── historical/                # Datos históricos
│
├── logs/                          # Logs (no en Git)
│   └── agent_2025-11-06.log
│
├── DOCUMENTATION/                 # Documentación operativa
│   ├── MNQ_AgenteIA_Flujo_Diario.md
│   └── MNQ_APEX50k_Estrategia.md
│
├── .env                           # Variables entorno (no en Git)
├── .gitignore
├── requirements.txt
├── requirements-dev.txt
├── README.md
└── LICENSE
```

---

## Estándares de Código

### Python

#### PEP 8 y Formato

- **Seguir [PEP 8](https://peps.python.org/pep-0008/)**
- **Formatter:** Black (line length 88)
- **Linter:** Flake8
- **Type checker:** Mypy

```bash
# Formatear código
black src/ tests/

# Linting
flake8 src/ tests/

# Type checking
mypy src/
```

#### Naming Conventions

```python
# Módulos y paquetes: lowercase_with_underscores
import trading_agent
from strategies import mnq_levels_strategy

# Clases: CapWords (PascalCase)
class TradingAgent:
    pass

class RiskManager:
    pass

# Funciones y variables: lowercase_with_underscores
def calculate_position_size():
    risk_per_trade = 120
    
# Constantes: UPPER_CASE_WITH_UNDERSCORES
MAX_DAILY_LOSS = 120
APEX_ACCOUNT_SIZE = 50000

# Privadas: _leading_underscore
def _internal_helper():
    pass
```

#### Type Hints (Obligatorias)

```python
from typing import List, Dict, Optional, Tuple
from datetime import datetime

def calculate_levels(
    date: datetime,
    ohlc_data: List[Dict[str, float]]
) -> Tuple[float, float]:
    """
    Calcula niveles PDH/PDL para la fecha dada.
    
    Args:
        date: Fecha para cálculo
        ohlc_data: Lista de velas OHLC
        
    Returns:
        Tupla (PDH, PDL)
    """
    pdh: float = max([bar['high'] for bar in ohlc_data])
    pdl: float = min([bar['low'] for bar in ohlc_data])
    return pdh, pdl
```

#### Docstrings (Obligatorias para funciones públicas)

Usar formato **Google Style**:

```python
def place_order(
    account_id: str,
    symbol: str,
    quantity: int,
    side: str,
    order_type: str,
    price: Optional[float] = None
) -> str:
    """
    Coloca una orden en NinjaTrader vía ATI.
    
    Args:
        account_id: ID de la cuenta APEX
        symbol: Símbolo del instrumento (ej: "MNQ 12-24")
        quantity: Número de contratos
        side: "BUY" o "SELL"
        order_type: "LIMIT", "MARKET", "STOP"
        price: Precio límite/stop (None para MARKET)
        
    Returns:
        Order ID asignado por NinjaTrader
        
    Raises:
        ValueError: Si parámetros inválidos
        ConnectionError: Si NinjaTrader no responde
        
    Example:
        >>> order_id = place_order(
        ...     account_id="Sim101",
        ...     symbol="MNQ 12-24",
        ...     quantity=3,
        ...     side="BUY",
        ...     order_type="LIMIT",
        ...     price=18015.0
        ... )
        >>> print(order_id)
        "ORD-123456"
    """
    # Implementación...
```

#### Manejo de Errores

```python
# Específico, nunca bare except
try:
    result = risky_operation()
except ValueError as e:
    logger.error(f"Invalid value: {e}")
    raise
except ConnectionError as e:
    logger.warning(f"Connection failed: {e}")
    # Retry logic
except Exception as e:
    logger.critical(f"Unexpected error: {e}")
    raise

# Context managers para recursos
with open('file.txt', 'r') as f:
    data = f.read()

# Async context managers
async with api_client.session() as session:
    response = await session.get(url)
```

#### Logging

```python
from loguru import logger

# Configuración en main.py
logger.add(
    "logs/agent_{time}.log",
    rotation="1 day",
    retention="90 days",
    level="INFO",
    format="{time:YYYY-MM-DD HH:mm:ss} | {level} | {module}:{function}:{line} | {message}"
)

# Uso
logger.debug("Debugging info")
logger.info("Trade executed successfully")
logger.warning("High volatility detected")
logger.error("Failed to connect to API")
logger.critical("Circuit breaker triggered")
```

### C# (NinjaScript)

#### Naming Conventions

```csharp
// Clases: PascalCase
public class MNQAgentAddon : NinjaTrader.NinjaScript.AddOnBase
{
    // Propiedades públicas: PascalCase
    public string AccountId { get; set; }
    
    // Campos privados: _camelCase con underscore
    private decimal _currentPnL;
    private string _orderStatus;
    
    // Métodos: PascalCase
    public void PlaceOrder()
    {
        // Variables locales: camelCase
        int contractSize = 3;
        double entryPrice = 18015.0;
    }
    
    // Constantes: PascalCase
    private const int MaxContracts = 50;
}
```

#### Documentación XML

```csharp
/// <summary>
/// Coloca una orden bracket (entry + SL + TP) en NinjaTrader
/// </summary>
/// <param name="entry">Precio de entrada</param>
/// <param name="stopLoss">Precio del stop-loss</param>
/// <param name="takeProfit">Precio del take-profit</param>
/// <param name="quantity">Número de contratos</param>
/// <returns>ID de la orden colocada</returns>
public string PlaceBracketOrder(
    double entry,
    double stopLoss,
    double takeProfit,
    int quantity)
{
    // Implementación...
}
```

---

## Workflow de Desarrollo

### Git Branching Strategy

**Modelo:** Git Flow simplificado

```
main (producción)
  ↓
develop (desarrollo)
  ↓
feature/nombre-feature
hotfix/nombre-hotfix
```

#### Comandos Git Frecuentes

```bash
# Crear nueva feature
git checkout develop
git pull origin develop
git checkout -b feature/add-guardian-system

# Desarrollo...
git add .
git commit -m "feat: Add account guardian system"

# Actualizar con develop
git checkout develop
git pull origin develop
git checkout feature/add-guardian-system
git merge develop

# Push y crear PR
git push origin feature/add-guardian-system
# Crear Pull Request en GitHub hacia develop
```

#### Convenciones de Commits

Usar [Conventional Commits](https://www.conventionalcommits.org/):

```bash
# Formato: <tipo>(<scope>): <descripción>

# Tipos válidos:
feat:     # Nueva funcionalidad
fix:      # Corrección de bug
docs:     # Cambios en documentación
style:    # Formato, punto y coma faltantes, etc.
refactor: # Refactorización de código
test:     # Añadir/modificar tests
chore:    # Mantenimiento, dependencias, etc.

# Ejemplos:
git commit -m "feat(strategy): Add B-M breakout setup detection"
git commit -m "fix(risk): Correct position sizing calculation"
git commit -m "docs(readme): Update installation instructions"
git commit -m "test(executor): Add ATI integration tests"
```

### Proceso de Pull Request

1. **Crear feature branch** desde `develop`
2. **Desarrollar y commitear** siguiendo convenciones
3. **Asegurar tests pasan**: `pytest`
4. **Code formatting**: `black src/ tests/`
5. **Type checking**: `mypy src/`
6. **Crear PR** hacia `develop` en GitHub
7. **Descripción completa** en PR:
   - ¿Qué cambia?
   - ¿Por qué?
   - ¿Cómo testear?
8. **Esperar aprobación** (o auto-merge si solo trabajas tú)
9. **Merge y delete branch**

---

## Testing

### Estructura de Tests

```
tests/
├── unit/              # Tests unitarios
├── integration/       # Tests de integración
└── fixtures/          # Datos de prueba
```

### Escribir Tests

```python
# tests/unit/test_risk_manager.py

import pytest
from src.domain.entities.account import Account
from src.core.risk_manager import RiskManager

@pytest.fixture
def mock_account():
    """Fixture de cuenta simulada"""
    return Account(
        account_id="Sim101",
        balance=50000.0,
        daily_pnl=0.0,
        monthly_pnl=0.0
    )

@pytest.fixture
def risk_manager():
    """Fixture de risk manager"""
    return RiskManager(
        max_loss=-2500,
        max_contracts=50,
        mae_limit=750
    )

def test_position_sizing_basic(risk_manager, mock_account):
    """Test cálculo básico de posición"""
    stop_pts = 20
    risk_per_trade = 120
    
    contracts = risk_manager.calculate_position_size(
        account=mock_account,
        stop_pts=stop_pts,
        risk=risk_per_trade
    )
    
    # MNQ: 1 punto = $2
    # Risk por micro = 20 * 2 = $40
    # Contratos = floor(120 / 40) = 3
    assert contracts == 3

def test_position_sizing_respects_scaling(risk_manager, mock_account):
    """Test que respeta límite de scaling"""
    mock_account.balance = 51000  # Bajo threshold $52,600
    
    # Cálculo base daría 10 contratos
    contracts = risk_manager.calculate_position_size(
        account=mock_account,
        stop_pts=6,  # $12 risk per micro
        risk=120
    )
    
    # Debe limitar a 50 (scaling)
    assert contracts == 10  # Base
    
    # Aplicar scaling
    contracts_scaled = risk_manager.apply_scaling(
        contracts=contracts,
        account=mock_account
    )
    
    assert contracts_scaled <= 50

def test_mae_validation(risk_manager):
    """Test validación MAE"""
    # MAE potencial: 50 contratos * 20 pts * $2 = $2000
    # Límite: $750
    is_valid = risk_manager.check_mae_limit(
        contracts=50,
        stop_pts=20
    )
    
    assert is_valid == False  # Excede límite
    
    # MAE válido: 3 contratos * 20 pts * $2 = $120
    is_valid = risk_manager.check_mae_limit(
        contracts=3,
        stop_pts=20
    )
    
    assert is_valid == True

@pytest.mark.asyncio
async def test_rithmic_connection():
    """Test conexión a Rithmic (integración)"""
    # Este test requiere credenciales válidas
    # Marcar como integration test
    pass
```

### Ejecutar Tests

```bash
# Todos los tests
pytest

# Tests específicos
pytest tests/unit/test_risk_manager.py

# Con coverage
pytest --cov=src --cov-report=html

# Solo unitarios (rápidos)
pytest tests/unit/

# Solo integración (lentos, requieren APIs)
pytest tests/integration/

# Verbose
pytest -v

# Stop en primer fallo
pytest -x
```

### Coverage

Objetivo: **>80% coverage** en módulos críticos:
- `risk_manager.py`
- `strategy_engine.py`
- `order_executor.py`

```bash
# Generar reporte HTML
pytest --cov=src --cov-report=html
# Ver en: htmlcov/index.html
```

---

## Depuración y Troubleshooting

### Debugging Python

#### VS Code Launch Configuration

Crear `.vscode/launch.json`:

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "Python: Trading Agent",
            "type": "python",
            "request": "launch",
            "program": "${workspaceFolder}/src/main.py",
            "console": "integratedTerminal",
            "env": {
                "PYTHONPATH": "${workspaceFolder}"
            },
            "args": ["--mode", "SIGNALS"]
        },
        {
            "name": "Python: Tests",
            "type": "python",
            "request": "launch",
            "module": "pytest",
            "args": [
                "tests/",
                "-v"
            ]
        }
    ]
}
```

#### Breakpoints y Logging

```python
# Usar logger en lugar de print
logger.debug(f"Setup detected: {setup}")

# Breakpoint condicional (solo debugger)
if price > 18000:
    breakpoint()  # Python 3.7+
```

### Problemas Comunes

#### Problema: Error al conectar con Rithmic

```
ConnectionError: Failed to connect to Rithmic server
```

**Solución:**
1. Verificar credenciales en `.env`
2. Verificar conexión internet
3. Confirmar que broker soporta Rithmic
4. Revisar firewall/antivirus

```bash
# Test conexión
python -c "from src.infrastructure.rithmic.data_handler import RithmicDataHandler; \
           handler = RithmicDataHandler(); \
           handler.connect()"
```

#### Problema: NinjaTrader no responde a ATI

```
Timeout waiting for NT8 response
```

**Solución:**
1. Verificar ATI habilitado: NT8 → Tools → Options → ATI
2. Verificar path incoming correcto en `.env`
3. Verificar permisos de escritura en directorio
4. Reiniciar NinjaTrader

```bash
# Test escritura ATI
echo "TEST;Sim101;MNQ 12-24;BUY;1;MARKET;0;0" > "%NT8_INCOMING_PATH%\test.txt"
# Verificar en NT8 → Tools → Output Window
```

#### Problema: Add-on NinjaTrader no aparece

**Diagnóstico:**
1. ¿DLL en ubicación correcta?
   - `Documents\NinjaTrader 8\bin\Custom\AddOns\`
2. ¿NT8 reiniciado tras compilar?
3. Revisar NT8 → Tools → Output Window por errores

**Solución:**
```bash
# Copia manual
copy ninjatrader\bin\Release\MNQAgentAddon.dll "%USERPROFILE%\Documents\NinjaTrader 8\bin\Custom\AddOns\"

# Reiniciar NT8
```

---

## Recursos

### Documentación Oficial

- [Python Docs](https://docs.python.org/3/)
- [NinjaTrader 8 Help Guide](https://ninjatrader.com/support/helpguides/nt8/)
- [Rithmic API](https://yyy3.rithmic.com/?page_id=9)
- [Apex Trader Funding Rules](https://apextraderfunding.com/help-center)

### Aprendizaje

- [Real Python](https://realpython.com/)
- [NinjaCoding.net](https://ninjacoding.net/)
- [QuantStart](https://www.quantstart.com/)

### Comunidad

- [Foro Soporte NinjaTrader](https://forum.ninjatrader.com/)
- [r/algotrading](https://reddit.com/r/algotrading)
- Discord del Proyecto (TBD)

---

**Fin de la Guía de Desarrollo**
