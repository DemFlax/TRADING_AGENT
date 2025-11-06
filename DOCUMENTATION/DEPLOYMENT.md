# Guía de Deployment — Agente de Trading MNQ

**Versión:** 1.0.0  
**Fecha:** 2025-11-06  
**Proyecto:** Agente de Trading Autónomo para Futuros MNQ (Apex Trader Funding)

---

## Índice

1. [Requisitos Previos](#requisitos-previos)
2. [Entorno de Producción](#entorno-de-producción)
3. [Configuración NinjaTrader 8](#configuración-ninjatrader-8)
4. [Configuración Rithmic](#configuración-rithmic)
5. [Configuración Cuenta Apex](#configuración-cuenta-apex)
6. [Instalación del Agente](#instalación-del-agente)
7. [Configuración de Variables](#configuración-de-variables)
8. [Deployment del Add-on NinjaScript](#deployment-del-add-on-ninjaScript)
9. [Testing Post-Deployment](#testing-post-deployment)
10. [Monitoring y Alertas](#monitoring-y-alertas)
11. [Backup y Recuperación](#backup-y-recuperación)
12. [Troubleshooting](#troubleshooting)

---

## Requisitos Previos

### Hardware Recomendado (Producción)

**PC Local:**
- CPU: Intel i7 / AMD Ryzen 7 (8+ cores)
- RAM: 16GB mínimo (32GB recomendado)
- SSD: 256GB mínimo
- GPU: Integrada suficiente
- Red: Ethernet (WiFi no recomendado)

**VPS (Alternativa):**
- Proveedor: AWS, Azure, OVH, Hetzner
- Región: US East (cerca CME Globex)
- Specs: 8vCPU, 16GB RAM, 100GB SSD
- OS: Windows Server 2019/2022
- Network: <20ms latencia a Chicago

### Software Requerido

```
✅ Windows 10/11 Pro (64-bit) o Windows Server 2019+
✅ Python 3.11.x
✅ NinjaTrader 8.0.29.1+
✅ Visual Studio 2022 (para compilar Add-on)
✅ .NET Framework 4.8
✅ Git 2.40+
```

### Cuentas y Credenciales

- ✅ Broker compatible con Rithmic (ej: Dorman Trading, Optimus Futures)
- ✅ Credenciales Rithmic API
- ✅ Cuenta Apex Trader Funding (Evaluation o Funded)
- ✅ Licencia NinjaTrader 8 (Lifetime o Lease)

---

## Entorno de Producción

### Opción 1: PC Local

**Pros:**
- Control total del hardware
- Sin costos mensuales de VPS
- Baja latencia a internet doméstico

**Contras:**
- Depende de estabilidad eléctrica local
- No opera si PC apagado
- Requiere UPS (Sistema de Alimentación Ininterrumpida)

**Configuración recomendada:**
```
1. Instalar UPS de 1500VA mínimo
2. Configurar plan de energía: "Alto rendimiento"
3. Desactivar suspensión/hibernación
4. Desactivar actualizaciones automáticas de Windows
5. Configurar IP estática
6. Abrir puertos firewall para Rithmic
```

### Opción 2: VPS

**Pros:**
- Uptime 99.9%
- Latencia óptima (<10ms a CME)
- No depende de infraestructura local

**Contras:**
- Costo mensual ($50-200)
- Latencia a tu ubicación para monitorear

**Proveedores recomendados:**

| Proveedor | Región | Latencia CME | Precio |
|-----------|--------|--------------|--------|
| AWS EC2 | us-east-1 | ~5ms | $100/mes |
| Azure | East US | ~8ms | $120/mes |
| OVH | Chicago | ~2ms | $80/mes |
| Hetzner | US | ~15ms | $50/mes |

**Setup VPS:**
```powershell
# Conectar vía RDP
mstsc /v:IP_VPS

# Instalar Chrome/Firefox
# Descargar Python 3.11 desde python.org
# Descargar NinjaTrader 8
# Descargar Visual Studio 2022
# Clonar repositorio
```

---

## Configuración NinjaTrader 8

### Instalación

1. **Descargar Instalador**
   - Ir a [ninjatrader.com](https://ninjatrader.com/)
   - Crear cuenta si no tienes
   - Descargar versión 8.0.29.1+

2. **Ejecutar Instalador**
   ```
   NinjaTrader8Setup.exe
   
   Opciones:
   ✅ Full Installation
   ✅ Market Replay (opcional, para testing)
   ✅ Database
   ```

3. **Activar Licencia**
   - Abrir NinjaTrader
   - Help → License Key
   - Ingresar key o usar Trial (14 días)

### Configuración Inicial

#### 1. Workspace

```
Tools → Options → General
├── Time Zone: US/Central (CME)
├── Workspace: Guardar al cerrar
└── Notifications: Habilitar
```

#### 2. Conexiones

```
Tools → Connections
├── Rithmic → Configure (ver sección siguiente)
└── Playback → Configure (para testing)
```

#### 3. Automated Trading Interface (ATI)

```
Tools → Options → Automated Trading Interface
├── ✅ Enable ATI
├── Incoming Path: C:\Users\[User]\Documents\NinjaTrader 8\incoming
├── Outgoing Path: C:\Users\[User]\Documents\NinjaTrader 8\outgoing
├── ✅ Show ATI info in output window
└── Apply
```

**Reiniciar NinjaTrader tras habilitar ATI.**

#### 4. Data Series

```
Tools → Options → Data Series
├── Days to load: 30
├── Bars to load: 2000
└── Tick replay: Habilitado
```

---

## Configuración Rithmic

### Obtener Credenciales

1. **Broker Compatible**
   - Contactar broker que ofrezca Rithmic
   - Ejemplos: Dorman, Optimus, AMP, Edge Clear
   - Solicitar acceso R | API+ (NO solo R | API básico)

2. **Tipo de Conexión**
   - **R | Trader Pro:** $20/mes (básico)
   - **R | API+:** $85/mes (necesario para custom bars, brackets server-side)

3. **Credenciales Recibidas**
   ```
   Username: tu_usuario_rithmic
   Password: tu_password
   System: Rithmic Paper Trading o Rithmic 01 (live)
   Gateway: Especificado por broker
   ```

### Configurar en NinjaTrader

```
1. Tools → Connections
2. Configure → Rithmic
3. Ingresar credenciales:
   ├── User: [tu_usuario]
   ├── Password: [tu_password]
   ├── System: Rithmic Paper Trading (testing) o Rithmic 01 (live)
   └── FCM: [seleccionar tu broker]
4. Test Connection
5. Si OK → Connect
```

### Verificar Conexión

```
Tools → Output Window
Buscar:
✅ "Rithmic: Connected"
✅ "Market data connection established"
✅ "Order routing connection established"

Si errores:
❌ "Invalid credentials" → Verificar user/pass
❌ "Connection timeout" → Verificar firewall/internet
❌ "FCM not found" → Contactar broker
```

### Firewall / Puertos

Rithmic requiere puertos dinámicos. **Permitir programa completo** en firewall:

```powershell
# Windows Defender Firewall
1. Panel de Control → Firewall
2. Configuración avanzada
3. Reglas de entrada → Nueva regla
4. Programa: C:\Program Files\NinjaTrader 8\bin\NinjaTrader.exe
5. Acción: Permitir
6. Perfiles: Todos
7. Nombre: NinjaTrader 8 Rithmic
```

---

## Configuración Cuenta Apex

### Tipo de Cuenta

**Evaluation (Eval):**
- Una vez, $167 (plan $50k)
- Alcanzar $3,000 profit target
- No daily drawdown limit
- Trailing threshold inicia en inicio

**Performance Account (PA - Funded):**
- Tras pasar eval
- Profit split: 100% primeros $25k, luego 90/10
- Payouts bi-semanales

### Vincular con NinjaTrader

1. **Obtener Credenciales Rithmic de Apex**
   ```
   Apex Dashboard → Accounts → Tu cuenta
   ├── Rithmic Username: (ej. Apex123456)
   ├── Rithmic Password: (generado por Apex)
   └── FCM: Dorman Trading
   ```

2. **Agregar Cuenta en NT8**
   ```
   Tools → Account Data → Accounts
   ├── Add → Rithmic
   ├── Username: [Apex Rithmic user]
   ├── Password: [Apex Rithmic pass]
   ├── System: Rithmic 01 (live) o Paper (testing)
   └── Connect
   ```

3. **Verificar Balance**
   ```
   Control Center → Accounts
   Verificar:
   ✅ Balance: $50,000 (o según plan)
   ✅ Status: Connected
   ```

### Reglas Apex a Configurar

Crear archivo `config/apex_rules.yaml`:

```yaml
apex_rules:
  account_size: 50000
  trailing_threshold: 2500
  max_loss: 2500
  scaling:
    threshold: 52600
    max_contracts_before: 50
    max_contracts_after: 100
  mae_percent: 0.30
  mae_base_dollar: 750
  max_rr_ratio: 5.0
  min_rr_ratio: 1.5
  day_trading_only: true
  one_direction: true
```

---

## Instalación del Agente

### Clonar Repositorio

```bash
cd C:\Trading
git clone https://github.com/DemFlax/-TRADING_AGENT_IA.git
cd -TRADING_AGENT_IA
```

### Crear Entorno Virtual

```powershell
# Crear venv
python -m venv venv

# Activar
.\venv\Scripts\Activate.ps1

# Si error de ejecución de scripts:
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Instalar dependencias
pip install --upgrade pip
pip install -r requirements.txt
```

### Verificar Instalación

```python
python -c "import pandas, numpy, asyncio; print('OK')"
# Debe imprimir: OK
```

---

## Configuración de Variables

### Crear Archivo .env

Copiar `.env.example` a `.env`:

```bash
copy .env.example .env
```

### Editar .env

```bash
# Rithmic API
RITHMIC_USER=tu_usuario_rithmic
RITHMIC_PASSWORD=tu_password
RITHMIC_SYSTEM=Rithmic 01

# Apex Account
APEX_ACCOUNT_ID=Apex123456
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

**Importante:** Reemplazar `TuUsuario` con tu username de Windows.

### Verificar Paths

```powershell
# Verificar que directorios existen
Test-Path $env:USERPROFILE\Documents\NinjaTrader 8\incoming
Test-Path $env:USERPROFILE\Documents\NinjaTrader 8\outgoing

# Si FALSE, crearlos:
New-Item -ItemType Directory -Path "$env:USERPROFILE\Documents\NinjaTrader 8\incoming"
New-Item -ItemType Directory -Path "$env:USERPROFILE\Documents\NinjaTrader 8\outgoing"
```

---

## Deployment del Add-on NinjaScript

### Compilar Add-on

1. **Abrir Proyecto en Visual Studio 2022**
   ```
   File → Open → Project/Solution
   Navegar a: ninjatrader\MNQAgentAddon\MNQAgentAddon.csproj
   ```

2. **Configurar Build**
   ```
   Build → Configuration Manager
   ├── Active solution configuration: Release
   └── Platform: x64
   ```

3. **Compilar**
   ```
   Build → Build Solution (Ctrl+Shift+B)
   
   Verificar en Output:
   ✅ "Build succeeded"
   ✅ "0 errors"
   ```

### Instalar Add-on en NinjaTrader

#### Método 1: Copia Manual

```powershell
# Copiar DLL compilado
$source = ".\ninjatrader\bin\Release\MNQAgentAddon.dll"
$dest = "$env:USERPROFILE\Documents\NinjaTrader 8\bin\Custom\AddOns"

Copy-Item $source $dest -Force
```

#### Método 2: Instalador Automático (Recomendado)

```powershell
# Ejecutar script de instalación
.\scripts\install_addon.ps1
```

**Script `install_addon.ps1`:**
```powershell
$dllPath = ".\ninjatrader\bin\Release\MNQAgentAddon.dll"
$ntPath = "$env:USERPROFILE\Documents\NinjaTrader 8\bin\Custom\AddOns"

if (Test-Path $dllPath) {
    Copy-Item $dllPath $ntPath -Force
    Write-Host "Add-on instalado correctamente" -ForegroundColor Green
    Write-Host "Reinicia NinjaTrader 8 para cargar el add-on" -ForegroundColor Yellow
} else {
    Write-Host "Error: DLL no encontrado. Compilar primero." -ForegroundColor Red
}
```

### Verificar Instalación Add-on

1. **Reiniciar NinjaTrader 8** (completamente cerrar y reabrir)

2. **Verificar en Control Center**
   ```
   New → Add On → MNQ Agent
   
   Debe aparecer en la lista. Si no:
   - Tools → Output Window → buscar errores
   - Verificar que DLL está en carpeta correcta
   ```

3. **Abrir Panel**
   ```
   Clic en "MNQ Agent"
   Debe abrir ventana con panel de control
   ```

---

## Testing Post-Deployment

### Test 1: Conexión Python → Rithmic

```bash
# Activar venv
.\venv\Scripts\Activate.ps1

# Test conexión
python -c "
from src.infrastructure.rithmic.data_handler import RithmicDataHandler
handler = RithmicDataHandler()
print('Connecting to Rithmic...')
handler.connect()
print('✅ Connection successful')
handler.disconnect()
"
```

### Test 2: Comunicación ATI

```powershell
# Test escribir comando ATI
$testCommand = "TEST;Apex123456;MNQ 12-24;BUY;1;MARKET;0;0"
$testPath = "$env:USERPROFILE\Documents\NinjaTrader 8\incoming\test_ati.txt"

$testCommand | Out-File -FilePath $testPath -Encoding ASCII

# Verificar en NinjaTrader:
# Tools → Output Window → buscar "TEST command received"
```

### Test 3: Ejecutar Agente en Modo SIGNALS

```bash
# Modo signals (solo notifica, no ejecuta)
python src/main.py --mode SIGNALS --paper

# Debe:
# ✅ Conectar a Rithmic
# ✅ Calcular niveles PDH/PDL
# ✅ Escanear setups (si mercado abierto)
# ✅ Notificar en consola

# Ctrl+C para detener
```

### Test 4: Paper Trading (CRÍTICO)

```bash
# Ejecutar en paper 2-4 SEMANAS antes de live
python src/main.py --mode AUTONOMOUS --paper

# Monitorear:
# - Detección de setups correcta
# - Tamaño de posición según reglas
# - Gestión bracket (SL→BE en +0.5R)
# - Flat EOD a las 22:00
# - Journal actualizado
```

---

## Monitoring y Alertas

### Logs

**Ubicación:** `logs/agent_YYYY-MM-DD.log`

**Niveles:**
```
INFO:  Operaciones normales
WARNING: Situaciones anormales pero controladas
ERROR: Fallos que requieren atención
CRITICAL: Fallos graves (circuit breaker, pérdida conexión)
```

**Monitorear en tiempo real:**
```powershell
# PowerShell
Get-Content .\logs\agent_2025-11-06.log -Wait -Tail 50
```

### Telegram (Opcional)

1. **Crear Bot**
   ```
   - Telegram → buscar @BotFather
   - /newbot
   - Nombrar bot: MNQ_Trading_Agent
   - Copiar token: 123456789:ABCdefGHIjklMNOpqrsTUVwxyz
   ```

2. **Obtener Chat ID**
   ```
   - Enviar mensaje a tu bot
   - Ir a: https://api.telegram.org/bot[TOKEN]/getUpdates
   - Copiar "chat":{"id": 123456789}
   ```

3. **Configurar en .env**
   ```
   TELEGRAM_BOT_TOKEN=123456789:ABCdefGHIjklMNOpqrsTUVwxyz
   TELEGRAM_CHAT_ID=123456789
   ```

### Alertas Configuradas

```python
# src/infrastructure/notifications/telegram_notifier.py

ALERT_TRIGGERS = {
    'trade_executed': True,      # Cada trade
    'sl_to_be': True,            # SL movido a BE
    'target_hit': True,          # TP alcanzado
    'daily_limit_reached': True, # Cap diario
    'connection_lost': True,     # Pérdida conexión
    'circuit_breaker': True      # Circuit breaker activado
}
```

---

## Backup y Recuperación

### Qué Backupear

```
CRÍTICO:
├── data/journal.db           # Journal completo
├── .env                      # Configuración (excluir de Git)
├── config/settings.yaml      # Configuración del agente
└── logs/                     # Logs (últimos 30 días)

OPCIONAL:
├── ninjatrader/bin/Release/  # DLL compilado
└── src/                      # Código fuente (ya en Git)
```

### Script Backup Automático

**`scripts/backup.ps1`:**
```powershell
$backupDir = "C:\TradingBackups\backup_$(Get-Date -Format 'yyyy-MM-dd')"
$sourceDir = "C:\Trading\-TRADING_AGENT_IA"

New-Item -ItemType Directory -Path $backupDir -Force

# Copiar archivos críticos
Copy-Item "$sourceDir\data\journal.db" $backupDir
Copy-Item "$sourceDir\.env" $backupDir
Copy-Item "$sourceDir\config\*" $backupDir -Recurse
Copy-Item "$sourceDir\logs\*" $backupDir -Recurse

Write-Host "Backup completado: $backupDir" -ForegroundColor Green

# Eliminar backups mayores a 30 días
Get-ChildItem "C:\TradingBackups" | 
    Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } |
    Remove-Item -Recurse -Force
```

**Programar en Task Scheduler:**
```
1. Task Scheduler → Create Task
2. Trigger: Daily at 23:00
3. Action: Start Program
   ├── Program: powershell.exe
   └── Arguments: -File "C:\Trading\-TRADING_AGENT_IA\scripts\backup.ps1"
4. OK
```

---

## Troubleshooting

### Problema: Agente no conecta a Rithmic

**Síntomas:**
```
ConnectionError: Failed to connect to Rithmic
```

**Soluciones:**
1. Verificar credenciales en `.env`
2. Verificar que NinjaTrader conectado a Rithmic
3. Verificar firewall no bloquea Python
4. Reiniciar router/modem
5. Contactar broker si persiste

### Problema: ATI no responde

**Síntomas:**
```
Timeout waiting for ATI confirmation
```

**Soluciones:**
1. Verificar ATI habilitado en NT8
2. Verificar paths correctos en `.env`
3. Verificar permisos de escritura en directorios
4. Reiniciar NinjaTrader
5. Verificar que no hay archivos .txt antiguos en incoming/

### Problema: Add-on no aparece

**Diagnóstico:**
```
NT8 → Tools → Output Window
Buscar errores tipo:
"Could not load file MNQAgentAddon.dll"
```

**Soluciones:**
1. Verificar DLL en ubicación correcta
2. Recompilar en Configuration "Release"
3. Verificar .NET Framework 4.8 instalado
4. Eliminar archivos temporales de NT8:
   ```powershell
   Remove-Item "$env:USERPROFILE\Documents\NinjaTrader 8\db\cache\*" -Recurse
   ```

### Problema: Pérdidas Consistentes en Paper

**⚠️ NO PASAR A LIVE HASTA RESOLVER**

**Checklist:**
- [ ] Estrategia backtested correctamente
- [ ] Tamaño de posición respeta reglas
- [ ] RR real ≥1.5 (considerar slippage)
- [ ] Filtros de volumen/OR15' funcionando
- [ ] Flat EOD ejecutándose correctamente
- [ ] Journal registra todos los trades

**Si persiste:** Revisar lógica de estrategia con datos históricos.

---

## Checklist Pre-Live

Antes de operar con cuenta funded:

```
CONFIGURACIÓN:
[ ] NinjaTrader conectado a Rithmic LIVE (no paper)
[ ] Cuenta Apex vinculada correctamente
[ ] Balance correcto en NT8
[ ] ATI habilitado y testeado
[ ] Add-on visible y funcional
[ ] Variables .env configuradas para LIVE

TESTING:
[ ] Paper trading mínimo 2 semanas
[ ] Win rate ≥ 50%
[ ] Expectancy positiva
[ ] Max drawdown < -1.5R
[ ] Cumple reglas APEX (MAE, scaling, etc.)
[ ] Flat EOD funciona 100% días

SEGURIDAD:
[ ] Backup configurado y testeado
[ ] Telegram alertas funcionando
[ ] Circuit breaker testeado
[ ] UPS conectado (si PC local)
[ ] Documentación de emergencia lista

OPERATIVA:
[ ] Horarios claros (15:30-22:00 CET)
[ ] Plan para feriados USA
[ ] Contacto broker disponible
[ ] Cuenta Apex sin problemas
```

**Solo si todos [ ] = [✅], proceder a live.**

---

**Fin de la Guía de Deployment**
