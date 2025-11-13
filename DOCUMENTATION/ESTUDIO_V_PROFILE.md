# Add-ons gratuitos de Volume Profile para NinjaTrader 8

**La conclusión crítica**: No existe ningún indicador 100% gratuito que combine VPOC + HVN (≥80 percentil) + LVN (≤20 percentil) con acceso programático desde NinjaScript. Sin embargo, existen múltiples opciones gratuitas parciales y alternativas viables para construir esta funcionalidad.

## Opciones gratuitas con acceso NinjaScript

Solo un indicador gratuito proporciona acceso programático confirmado a valores de volume profile desde estrategias NinjaScript, aunque **sin detección explícita de HVN/LVN**.

### DValueArea - La mejor opción gratuita con API

**Nombre exacto:** DValueArea  
**Fuente:** https://ninjatraderecosystem.com/user-app-share-download/dvaluearea/  
**Estado:** 100% gratuito, sin registro requerido

**Funcionalidades incluidas:**
- ✅ **VPOC (Point of Control)** - Nivel de precio con mayor volumen
- ✅ **RtPOC (Real-time POC)** - POC en tiempo real
- ✅ **Value Area High (VAh)** y **Value Area Low (VAb)** - Límites del área de valor (70% del volumen)
- ✅ **Acceso completo desde NinjaScript** - Compatible con Strategy Builder y Bloodhound
- ✅ Perfiles diarios/sesión
- ✅ Múltiples tipos: VOC, TPO, VWTPO, VTPO
- ❌ **NO incluye HVN/LVN** - Esta es la limitación principal

**Limitaciones vs versiones de pago:**
- No detecta automáticamente High Volume Nodes ni Low Volume Nodes
- Cálculos pueden diferir ligeramente vs otras plataformas (reportado por algunos usuarios)
- Solo perfiles basados en sesión/día visible en pantalla

**Ejemplo de código para acceder desde strategy:**

```csharp
// En State.Configure o State.DataLoaded
DValueArea DValueArea1;

protected override void OnStateChange()
{
    if (State == State.DataLoaded)
    {
        DValueArea1 = DValueArea(Close, 40, 2, 0, 5, false, 
            new TimeSpan(9, 30, 0), 0.68, 2, 2, 
            _dValueEnums.dValueAreaTypes.VWTPO, 0, 100, 1, 6.75, 
            true, 5, true, 0, 60, 300, false, 2, false);
    }
}

protected override void OnBarUpdate()
{
    if (CurrentBar < 20) return;
    
    // Acceder a valores del volume profile
    double poc = DValueArea1.POC[0];           // Point of Control
    double rtpoc = DValueArea1.RtPOC[0];       // Real-time POC
    double valueAreaHigh = DValueArea1.VAt[0]; // Value Area Top
    double valueAreaLow = DValueArea1.VAb[0];  // Value Area Bottom
    
    // Usar en lógica de trading
    if (Close[0] > poc && Close[1] <= poc)
    {
        // Precio cruzó arriba del POC
        Print(String.Format("POC breakout: {0} | VAH: {1} | VAL: {2}", 
            poc, valueAreaHigh, valueAreaLow));
    }
}
```

## Proyectos open-source en GitHub con código modificable

Estas opciones proporcionan código fuente completo que puede modificarse para agregar detección de HVN/LVN y acceso programático.

### trading-code/ninjatrader-freeorderflow - Más activo y popular

**Repositorio:** https://github.com/trading-code/ninjatrader-freeorderflow  
**Licencia:** MIT License (100% libre)  
**Estado:** ⭐ **108 estrellas, 45 forks - Actualizado abril 2025**

**Funcionalidades incluidas:**
- ✅ Fixed range volume profile drawing tool
- ✅ Volume Profile con optimizaciones de rendering
- ✅ Anchored VWAP drawing tool
- ✅ Market Depth visualization
- ✅ Código fuente completo disponible
- ✅ Mantenimiento activo (última release v2, marzo 2025)

**Instalación:**
1. Descargar desde: https://github.com/trading-code/ninjatrader-freeorderflow/releases
2. Importar el archivo .zip en NinjaTrader: Tools → Import → NinjaScript Add-On
3. Habilitar Tick Replay: Tools → Options → Market Data → "Show Tick Replay"

**Limitaciones:**
- Requiere conocimientos técnicos para modificar el código
- No incluye HVN/LVN pre-construidos (hay que agregarlos manualmente)
- Instalación manual más compleja que indicadores pre-compilados

### gbzenobi/CSharp-NT8-OrderFlowKit - El más completo

**Repositorio:** https://github.com/gbzenobi/CSharp-NT8-OrderFlowKit  
**Licencia:** Open-source  
**Desarrollador:** Gabriel Zenobi (para fondos de inversión, bancos y traders)

**Funcionalidades incluidas:**
- ✅ **POC (Point of Control)** - Cluster de máximo volumen
- ✅ **POI (Point of Imbalance)** - Cluster de mínimo volumen (característica única)
- ✅ Full volume profile con distribución horizontal
- ✅ Delta volume, Total Volume
- ✅ Bookmap (order flow visualization)
- ✅ OrderFlow.cs (footprint/cluster chart)
- ✅ VolumeFilter.cs - Detección de áreas importantes de volumen
- ✅ Drag-and-drop para selección de zonas custom

**Archivos clave:**
- `VolumeAnalysisProfile.cs` - Indicador principal de volume profile
- `Bookmap.cs` - Visualización de order book
- `OrderFlow.cs` - Gráfico de footprint
- `VolumeFilter.cs` - Filtrado de volumen
- `MarketVolume.cs` - Delta y volumen acumulado

**Instalación:**
1. Clonar repositorio o descargar archivos
2. Copiar archivos de AddOns a: `Documents\NinjaTrader 8\bin\Custom\AddOns`
3. Copiar archivos de indicadores a carpeta de indicadores NT8
4. Activar Tick Replay en opciones

**Ejemplo de código - Propiedades del Volume Profile:**

```csharp
[NinjaScriptProperty]
[Display(Name="Show POC", Order=4, GroupName="Volume Profile Calculations")]
public bool _ShowPOC { get; set; }

[NinjaScriptProperty]
[Display(Name="Show POI", Order=5, GroupName="Volume Profile Calculations")]
public bool _ShowPOI { get; set; }

[NinjaScriptProperty]
[Display(Name="Time", Order=2, GroupName="Volume Profile Calculations")]
public VolumeAnalysis.PeriodMode _PeriodMode { get; set; }
```

**Controles:**
- **Hold CTRL** + seleccionar zona → click izquierdo para mostrar nuevo Volume Profile
- **Hold SHIFT** + seleccionar Volume Profile → click izquierdo para eliminar

### izzylite/volume-profile-indicator - Optimizado y mejorado

**Repositorio:** https://github.com/izzylite/volume-profile-indicator  
**Licencia:** MIT License  
**Estado:** Versión mejorada 2024 con optimizaciones

**Funcionalidades incluidas:**
- ✅ Volume Profile para 4-hour, daily, weekly, monthly
- ✅ Múltiples timeframes
- ✅ Bookmap visualization
- ✅ Volume filtering
- ✅ Basado en CSharp-NT8-OrderFlowKit con bug fixes y mejoras de performance

**Archivos clave:**
- `VolumeProfileLines.cs`
- `VolumeAnalysisProfile.cs`
- `FlexibleVolumeAnalysisProfile.cs`

**Requisitos:**
- Visual Studio 2019 o posterior
- .NET Framework 4.7.2+
- Tick Replay habilitado

**Instalación:**
1. Copiar archivos de carpeta AddOns a directorio AddOns de NT8
2. Copiar DrawingTools a directorio DrawingTools de NT8
3. Copiar indicadores a carpeta Indicators de NT8

### alighten-dev/FootprintOrderFlow - Diseñado para estrategias

**Repositorio:** https://github.com/alighten-dev/FootprintOrderFlow  
**Estado:** Open-source, desarrollado 2024

**Funcionalidades incluidas:**
- ✅ **27 plots para integración con estrategias** - La mayor cantidad de todos
- ✅ **5 plots específicos de POC**
- ✅ POC (Point of Control)
- ✅ Value Area High (VAHigh)
- ✅ Value Area Low (VALow)
- ✅ Distancias desde POC a límites de VA
- ✅ NO requiere Tick Replay (requiere OrderFlow+ subscription)

**Plots disponibles para estrategias:**
- `Values[5]` → pocPrice (Precio del POC)
- `Values[6]` → VAHigh (Value Area High)
- `Values[7]` → VALow (Value Area Low)
- `Values[23]` → pocVA_FromLow (Distancia desde VA low a POC)
- `Values[24]` → pocVA_FromHigh (Distancia desde VA high a POC)
- `Values[25]` → pocBar_FromLow (Distancia desde bar low a POC)
- `Values[26]` → pocBar_FromHigh (Distancia desde bar high a POC)

**Ejemplo de código para strategy:**

```csharp
// En State.Configure
protected override void OnStateChange()
{
    if (State == State.Configure)
    {
        // Configurar datos volumétricos
        AddVolumetric(Instrument.FullName, BarsPeriod.BarsPeriodType, 
            BarsPeriod.Value, VolumetricDeltaType.BidAsk, 1);
    }
    else if (State == State.DataLoaded)
    {
        // Instanciar indicador
        Footprint1 = FootprintOrderFlow(Close, 3, 70, false, ...);
    }
}

protected override void OnBarUpdate()
{
    if (CurrentBar < 1) return;
    
    // Acceder a POC y Value Area
    double pocPrice = Footprint1.Values[5][0];
    double vaHigh = Footprint1.Values[6][0];
    double vaLow = Footprint1.Values[7][0];
    double pocVA_FromLow = Footprint1.Values[23][0];
    double pocVA_FromHigh = Footprint1.Values[24][0];
    
    // Imprimir valores
    Print(String.Format("POC={0:F2} | VAH={1:F2} | VAL={2:F2}", 
        pocPrice, vaHigh, vaLow));
    
    // Lógica de trading basada en posición relativa al POC
    if (Close[0] > pocPrice && Close[1] <= pocPrice)
    {
        // Cruce alcista del POC
        EnterLong();
    }
}
```

**Requisitos especiales:**
- **OrderFlow+ subscription** (requiere Lifetime License o cuenta fondeada)
- Volumetric bars (Time, Range o Tick)
- NO requiere Tick Replay

**Instalación:**
1. Descargar archivo .CS
2. Colocar en: `C:\Users\Documents\NinjaTrader 8\bin\Custom\Indicators`
3. Compilar en NinjaTrader

### michelpmcdonald/Ninjatrader - Implementación limpia

**Repositorio:** https://github.com/michelpmcdonald/Ninjatrader  
**Estado:** Open-source, 9 forks

**Funcionalidades incluidas:**
- ✅ SessionVolProfile - Volumen de sesión para cada precio
- ✅ Value Area (70% del volumen)
- ✅ POC calculation
- ✅ Value Area High y Low

**Archivo clave:** `SessionVolProfile/SessionVolumeProfile.cs`

**Ejemplo de código - Cálculo de Value Area:**

```csharp
// Calcular Value Area (70% del volumen)
protected void CalcValueArea()
{
    if (totalVolume == 0)
        return;
        
    long expVol = (long)(totalVolume * ValueAreaPct);
    
    // Empezar en POC y expandir hacia afuera
    int highOffset = 1;
    int lowOffset = 1;
    
    while (expVol < (long)(totalVolume * ValueAreaPct))
    {
        long lv = 0;
        // Calcular volumen en dos niveles de precio inferiores
        if ((pocIndex - lowOffset) >= 0)
        {
            lv = VolPrices.Values[pocIndex - lowOffset].BidVol;
            lv += VolPrices.Values[pocIndex - lowOffset].AskVol;
        }
        
        long hv = 0;
        // Calcular volumen en dos niveles de precio superiores
        if ((pocIndex + highOffset) < VolPrices.Count)
        {
            hv = VolPrices.Values[pocIndex + highOffset].BidVol;
            hv += VolPrices.Values[pocIndex + highOffset].AskVol;
        }
        
        // Tomar nivel de precio con más volumen
        if (hv >= lv)
        {
            expVol += hv;
            ValueAreaHigh = VolPrices.Keys[pocIndex + highOffset];
            highOffset += 2;
        }
        else
        {
            expVol += lv;
            ValueAreaLow = VolPrices.Keys[pocIndex - lowOffset];
            lowOffset += 2;
        }
    }
}
```

## Opciones gratuitas solo para visualización (sin acceso NinjaScript)

### automated-trading.ch Volume Profile Indicator - El más pulido visualmente

**Nombre exacto:** automated-trading.ch Volume Profile Indicator  
**Fuente:** https://automated-trading.ch/NT8/indicators/volume-profile-indicator  
**Estado:** GRATUITO (requiere cuenta gratuita para license key)

**Funcionalidades incluidas:**
- ✅ VPOC (Volume Point of Control) visual
- ✅ Value Area High y Low (configurable, default 68%)
- ✅ Perfiles diarios y compuestos (multi-día)
- ✅ Auto-composición de volume profiles basada en value area overlapping
- ✅ Funciona en barras Renko y todos los timeframes
- ✅ Funciona SIN OrderFlow data (usa Tick Replay)
- ✅ Merge/split interactivo de perfiles
- ✅ Actualizado activamente (última versión marzo 2025 v1.7.0.3)

**Limitaciones críticas:**
- ❌ **NO proporciona acceso desde NinjaScript** - Declaración explícita del desarrollador: "this indicator only do rendering and doesn't provide data that can be used from within a strategy"
- ❌ No incluye HVN/LVN explícitos

**Requisitos:**
- Cuenta gratuita en automated-trading.ch para obtener license key
- Tick Replay habilitado (si no tienes OrderFlow data)
- NinjaTrader 8

**Ventajas:**
- Muy pulido visualmente
- Soporte activo del desarrollador vía Discord
- Actualizaciones regulares con optimizaciones de performance
- Funciona sin Lifetime License

**Acceso:**
1. Crear cuenta gratuita en automated-trading.ch
2. Descargar indicador desde página de producto
3. Obtener license key gratuito desde billing page
4. Importar .zip en NinjaTrader

## Capacidades nativas de NinjaTrader 8

### Order Flow+ Volume Profile - Requiere subscripción de pago

**Incluido en:** NinjaTrader 8 con Order Flow+ (requiere Lifetime License ~$1,499-$1,799 o cuenta fondeada)  
**Documentación:** https://ninjatrader.com/support/helpguides/nt8/order_flow_volume_profile.htm

**Funcionalidades:**
- ✅ VPOC - Point of Control en tiempo real
- ✅ Value Area High (VAH) y Value Area Low (VAL)
- ✅ 3 modos de perfil: Sessions, Bars, Weeks, Months, Composite
- ✅ 6 modos de visualización (Buy/Sell, heatmap, etc.)
- ✅ Disponible como indicador y herramienta de dibujo

**Limitaciones críticas:**
- ❌ **NO incluye HVN/LVN** - Feature request SFT-3297 pendiente desde 2018
- ❌ **NO tiene acceso desde NinjaScript** - Feature request SFT-3402 pendiente
- ❌ **NO es gratuito** - Requiere Lifetime License o cuenta fondeada
- Confirmado por soporte de NinjaTrader: "The Order Flow Volume Profile indicator is not available for access through NinjaScript"

**Alternativa disponible:** Order Flow Volumetric Bars SÍ tienen API de NinjaScript

### Volumetric Bars API - La alternativa oficial con acceso NinjaScript

**Documentación:** https://ninjatrader.com/support/helpguides/nt8/order_flow_volumetric_bars2.htm

**Métodos disponibles para construir volume profile manualmente:**

```csharp
protected override void OnBarUpdate()
{
    // Obtener acceso a volumetric bars
    NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType barsType = 
        Bars.BarsSeries.BarsType as NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType;
    
    if (barsType == null) return;
    
    try
    {
        // Obtener VPOC (precio con máximo volumen)
        double pocPrice;
        double pocVolume = barsType.Volumes[CurrentBar].GetMaximumVolume(null, out pocPrice);
        
        // Obtener volumen en precio específico
        double volumeAtPrice = barsType.Volumes[CurrentBar].GetTotalVolumeForPrice(Close[0]);
        
        // Obtener buy/sell volume
        double buyVolume = barsType.Volumes[CurrentBar].GetBuyingVolumeForPrice(Close[0]);
        double sellVolume = barsType.Volumes[CurrentBar].GetSellingVolumeForPrice(Close[0]);
        
        // Bar delta
        double barDelta = barsType.Volumes[CurrentBar].BarDelta;
        
        Print(String.Format("VPOC: {0:F2} @ {1:F2} | Vol at Close: {2}", 
            pocVolume, pocPrice, volumeAtPrice));
    }
    catch (Exception ex)
    {
        Print("Error: " + ex.Message);
    }
}
```

**Loop por todos los precios en una barra:**

```csharp
protected override void OnBarUpdate()
{
    NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType barsType = 
        Bars.BarsSeries.BarsType as NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType;
    
    if (barsType == null) return;
    
    double tickSize = Instrument.MasterInstrument.TickSize;
    double barLow = Low[0];
    double barHigh = High[0];
    
    // Construir profile manualmente
    Dictionary<double, double> volumeProfile = new Dictionary<double, double>();
    
    for (double price = barHigh; price >= barLow; price -= tickSize)
    {
        double volumeAtPrice = barsType.Volumes[CurrentBar].GetTotalVolumeForPrice(price);
        if (volumeAtPrice > 0)
        {
            volumeProfile[price] = volumeAtPrice;
        }
    }
    
    // Identificar VPOC manualmente
    var pocEntry = volumeProfile.OrderByDescending(x => x.Value).FirstOrDefault();
    Print(String.Format("Manual VPOC: {0:F2} with volume {1}", pocEntry.Key, pocEntry.Value));
}
```

## Workarounds para calcular HVN/LVN manualmente

Dado que ninguna opción gratuita incluye detección automática de HVN/LVN con acceso NinjaScript, aquí están los algoritmos para implementarlo:

### Algoritmo para detectar HVN y LVN usando percentiles

```csharp
protected Dictionary<double, double> BuildVolumeProfile()
{
    NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType barsType = 
        Bars.BarsSeries.BarsType as NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType;
    
    if (barsType == null) return null;
    
    Dictionary<double, double> volumeProfile = new Dictionary<double, double>();
    double tickSize = Instrument.MasterInstrument.TickSize;
    
    for (double price = High[0]; price >= Low[0]; price -= tickSize)
    {
        double vol = barsType.Volumes[CurrentBar].GetTotalVolumeForPrice(price);
        if (vol > 0)
            volumeProfile[price] = vol;
    }
    
    return volumeProfile;
}

protected void DetectHVNandLVN()
{
    Dictionary<double, double> profile = BuildVolumeProfile();
    if (profile == null || profile.Count == 0) return;
    
    // Ordenar volúmenes para calcular percentiles
    List<double> sortedVolumes = profile.Values.OrderBy(v => v).ToList();
    int count = sortedVolumes.Count;
    
    // Calcular percentil 80 (HVN threshold)
    int hvnIndex = (int)Math.Ceiling(count * 0.80) - 1;
    double hvnThreshold = sortedVolumes[hvnIndex];
    
    // Calcular percentil 20 (LVN threshold)
    int lvnIndex = (int)Math.Ceiling(count * 0.20) - 1;
    double lvnThreshold = sortedVolumes[lvnIndex];
    
    // Identificar HVN y LVN
    List<double> hvnLevels = new List<double>();
    List<double> lvnLevels = new List<double>();
    
    foreach (var priceLevel in profile)
    {
        if (priceLevel.Value >= hvnThreshold)
        {
            hvnLevels.Add(priceLevel.Key);  // High Volume Node
        }
        else if (priceLevel.Value <= lvnThreshold)
        {
            lvnLevels.Add(priceLevel.Key);  // Low Volume Node
        }
    }
    
    // Dibujar HVN y LVN en el chart
    foreach (double hvn in hvnLevels)
    {
        Draw.Ray(this, "HVN_" + hvn.ToString(), false, 
            0, hvn, 1, hvn, Brushes.Green, DashStyleHelper.Solid, 2);
    }
    
    foreach (double lvn in lvnLevels)
    {
        Draw.Ray(this, "LVN_" + lvn.ToString(), false, 
            0, lvn, 1, lvn, Brushes.Red, DashStyleHelper.Dash, 1);
    }
    
    Print(String.Format("HVN count: {0} | LVN count: {1}", hvnLevels.Count, lvnLevels.Count));
}
```

### Algoritmo alternativo usando desviación estándar

```csharp
protected void DetectHVNandLVN_StdDev()
{
    Dictionary<double, double> profile = BuildVolumeProfile();
    if (profile == null || profile.Count == 0) return;
    
    // Calcular media y desviación estándar
    double averageVolume = profile.Values.Average();
    double sumOfSquares = profile.Values.Sum(v => Math.Pow(v - averageVolume, 2));
    double stdDeviation = Math.Sqrt(sumOfSquares / profile.Count);
    
    // HVN: volumen > promedio + 1 desviación estándar
    double hvnThreshold = averageVolume + stdDeviation;
    
    // LVN: volumen < promedio - 1 desviación estándar
    double lvnThreshold = averageVolume - stdDeviation;
    
    List<double> hvnLevels = new List<double>();
    List<double> lvnLevels = new List<double>();
    
    foreach (var priceLevel in profile)
    {
        if (priceLevel.Value > hvnThreshold)
        {
            hvnLevels.Add(priceLevel.Key);
            Print(String.Format("HVN detected at {0:F2} with volume {1} (threshold: {2:F0})", 
                priceLevel.Key, priceLevel.Value, hvnThreshold));
        }
        else if (priceLevel.Value < lvnThreshold)
        {
            lvnLevels.Add(priceLevel.Key);
            Print(String.Format("LVN detected at {0:F2} with volume {1} (threshold: {2:F0})", 
                priceLevel.Key, priceLevel.Value, lvnThreshold));
        }
    }
}
```

### Algoritmo para Value Area (para usar con DValueArea o implementación custom)

```csharp
protected void CalculateValueArea(Dictionary<double, double> volumeProfile, out double VAH, out double VAL, out double POC)
{
    if (volumeProfile == null || volumeProfile.Count == 0)
    {
        VAH = VAL = POC = 0;
        return;
    }
    
    // Paso 1: Calcular volumen total y 70% threshold
    double totalVolume = volumeProfile.Values.Sum();
    double targetVolume = totalVolume * 0.70;
    
    // Paso 2: Encontrar POC (precio con mayor volumen)
    var pocEntry = volumeProfile.OrderByDescending(x => x.Value).First();
    POC = pocEntry.Key;
    
    // Paso 3: Expandir desde POC hasta acumular 70% del volumen
    List<double> valueAreaPrices = new List<double> { POC };
    double accumulatedVolume = pocEntry.Value;
    
    List<double> sortedPrices = volumeProfile.Keys.OrderBy(p => p).ToList();
    int pocIndex = sortedPrices.IndexOf(POC);
    
    int upperIndex = pocIndex + 1;
    int lowerIndex = pocIndex - 1;
    
    while (accumulatedVolume < targetVolume)
    {
        double upperVolume = 0;
        double lowerVolume = 0;
        
        // Calcular volumen 2 niveles arriba
        if (upperIndex < sortedPrices.Count)
        {
            upperVolume = volumeProfile[sortedPrices[upperIndex]];
            if (upperIndex + 1 < sortedPrices.Count)
                upperVolume += volumeProfile[sortedPrices[upperIndex + 1]];
        }
        
        // Calcular volumen 2 niveles abajo
        if (lowerIndex >= 0)
        {
            lowerVolume = volumeProfile[sortedPrices[lowerIndex]];
            if (lowerIndex - 1 >= 0)
                lowerVolume += volumeProfile[sortedPrices[lowerIndex - 1]];
        }
        
        // Agregar lado con mayor volumen
        if (upperVolume >= lowerVolume && upperIndex < sortedPrices.Count)
        {
            valueAreaPrices.Add(sortedPrices[upperIndex]);
            accumulatedVolume += volumeProfile[sortedPrices[upperIndex]];
            upperIndex++;
        }
        else if (lowerIndex >= 0)
        {
            valueAreaPrices.Add(sortedPrices[lowerIndex]);
            accumulatedVolume += volumeProfile[sortedPrices[lowerIndex]];
            lowerIndex--;
        }
        else
        {
            break; // No hay más niveles para expandir
        }
    }
    
    // Paso 4: Determinar VAH y VAL
    VAH = valueAreaPrices.Max();
    VAL = valueAreaPrices.Min();
    
    Print(String.Format("Value Area: VAH={0:F2} | POC={1:F2} | VAL={2:F2} | Coverage={3:P1}", 
        VAH, POC, VAL, accumulatedVolume / totalVolume));
}
```

## Tabla comparativa de opciones gratuitas

| Indicador | VPOC | HVN/LVN | NinjaScript API | Perfiles compuestos | Código abierto | Mantenimiento |
|-----------|------|---------|-----------------|---------------------|----------------|---------------|
| **DValueArea** | ✅ | ❌ | ✅ | ✅ | ⚠️ Parcial | ⚠️ Moderado |
| **automated-trading.ch** | ✅ | ❌ | ❌ | ✅ | ❌ | ✅ Activo 2025 |
| **trading-code/ninjatrader-freeorderflow** | ✅ | ⚠️ Modificable | ✅ | ✅ | ✅ MIT | ✅ Activo 2025 |
| **gbzenobi/CSharp-NT8-OrderFlowKit** | ✅ | ⚠️ POC/POI | ✅ | ✅ | ✅ | ✅ Activo 2024 |
| **izzylite/volume-profile-indicator** | ✅ | ⚠️ Modificable | ✅ | ✅ | ✅ MIT | ✅ Activo 2024 |
| **alighten-dev/FootprintOrderFlow** | ✅ | ⚠️ Modificable | ✅ | ⚠️ | ✅ | ⚠️ Moderado |
| **michelpmcdonald/Ninjatrader** | ✅ | ❌ | ✅ | ⚠️ | ✅ | ⚠️ Moderado |
| **NT8 Order Flow+ Volume Profile** | ✅ | ❌ | ❌ | ✅ | ❌ | ✅ Oficial |
| **NT8 Volumetric Bars API** | ✅ | ⚠️ Manual | ✅ | ✅ | ⚠️ Docs | ✅ Oficial |

## Recomendaciones finales por caso de uso

### Para trading automatizado con acceso NinjaScript inmediato

**Opción recomendada:** DValueArea + implementación manual de HVN/LVN

1. Usar **DValueArea** para obtener POC, VAh, VAb desde strategies
2. Implementar detección de HVN/LVN usando uno de los algoritmos proporcionados arriba
3. Combinar ambos en una strategy personalizada

**Ventajas:**
- Funcionamiento inmediato sin programación compleja
- Acceso probado desde Strategy Builder
- Comunidad activa para soporte

**Desventajas:**
- Requiere implementar HVN/LVN manualmente
- Cálculos pueden diferir ligeramente de otras plataformas

### Para máxima flexibilidad y personalización

**Opción recomendada:** Fork de trading-code/ninjatrader-freeorderflow o gbzenobi/CSharp-NT8-OrderFlowKit

1. Clonar repositorio GitHub (preferir trading-code por mantenimiento activo 2025)
2. Modificar código fuente para agregar detección de HVN/LVN percentil-based
3. Exponer valores como plots accesibles desde strategies
4. Compilar y usar en trading automatizado

**Ventajas:**
- Control total sobre algoritmos
- Código base profesional y optimizado
- Licencia MIT permite modificaciones
- Comunidad activa (108 estrellas en trading-code)

**Desventajas:**
- Requiere conocimientos de C# y Visual Studio
- Tiempo de desarrollo: 1-2 semanas
- Mantenimiento propio del código

### Para construcción desde cero con API oficial

**Opción recomendada:** Usar Volumetric Bars API de NinjaTrader

1. Usar métodos nativos: `GetMaximumVolume()`, `GetTotalVolumeForPrice()`
2. Implementar algoritmos completos de VPOC, Value Area, HVN, LVN
3. Crear indicador custom que exponga valores como plots

**Ventajas:**
- API oficial y estable de NinjaTrader
- Documentación completa
- No depende de indicadores de terceros

**Desventajas:**
- Complejidad alta: requiere implementar todo desde cero
- Tiempo de desarrollo: 2-4 semanas
- Requiere conocimientos avanzados de C# y algoritmos

### Para visualización sin trading automatizado

**Opción recomendada:** automated-trading.ch Volume Profile Indicator

1. Crear cuenta gratuita en automated-trading.ch
2. Descargar y obtener license key gratuito
3. Usar para análisis visual y toma de decisiones manual

**Ventajas:**
- Más pulido visualmente de todas las opciones gratuitas
- Soporte activo del desarrollador
- Actualizaciones regulares (última: marzo 2025)
- Funciona sin Lifetime License

**Desventajas:**
- No accesible desde strategies
- Solo para uso visual

## Código open-source completo para calcular VPOC/HVN/LVN desde cero

Para aquellos que quieren implementar una solución completamente custom, aquí está un esqueleto completo:

```csharp
#region Using declarations
using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class CustomVolumeProfile : Indicator
    {
        private Dictionary<double, double> currentProfile;
        
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Custom Volume Profile with VPOC, HVN, LVN";
                Name = "CustomVolumeProfile";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                
                // Parámetros
                HVNPercentile = 80;
                LVNPercentile = 20;
                ValueAreaPercent = 70;
                
                // Plots para acceso desde strategies
                AddPlot(Brushes.Magenta, "VPOC");
                AddPlot(Brushes.Green, "VAH");
                AddPlot(Brushes.Red, "VAL");
                AddPlot(Brushes.Orange, "HVN");
                AddPlot(Brushes.Blue, "LVN");
            }
            else if (State == State.DataLoaded)
            {
                currentProfile = new Dictionary<double, double>();
            }
        }
        
        protected override void OnBarUpdate()
        {
            if (CurrentBar < 1) return;
            
            // Obtener volumetric bars
            var barsType = Bars.BarsSeries.BarsType as 
                NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType;
            
            if (barsType == null)
            {
                Print("Error: This indicator requires Volumetric bars");
                return;
            }
            
            try
            {
                // Construir volume profile
                BuildVolumeProfile(barsType);
                
                // Calcular VPOC
                double vpoc = CalculateVPOC();
                Values[0][0] = vpoc; // Plot accesible como VPOC[0]
                
                // Calcular Value Area
                double vah, val;
                CalculateValueArea(out vah, out val);
                Values[1][0] = vah; // Plot accesible como VAH[0]
                Values[2][0] = val; // Plot accesible como VAL[0]
                
                // Calcular HVN y LVN
                List<double> hvnList, lvnList;
                DetectHVNandLVN(out hvnList, out lvnList);
                
                // Almacenar primer HVN y LVN para plots
                // (en implementación real, usar series separadas para múltiples valores)
                Values[3][0] = hvnList.Count > 0 ? hvnList[0] : 0;
                Values[4][0] = lvnList.Count > 0 ? lvnList[0] : 0;
                
                // Dibujar líneas en chart
                DrawVolumeProfileLines(vpoc, vah, val, hvnList, lvnList);
            }
            catch (Exception ex)
            {
                Print("Error in CustomVolumeProfile: " + ex.Message);
            }
        }
        
        private void BuildVolumeProfile(NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType barsType)
        {
            currentProfile.Clear();
            double tickSize = Instrument.MasterInstrument.TickSize;
            
            for (double price = High[0]; price >= Low[0]; price -= tickSize)
            {
                double vol = barsType.Volumes[CurrentBar].GetTotalVolumeForPrice(price);
                if (vol > 0)
                    currentProfile[price] = vol;
            }
        }
        
        private double CalculateVPOC()
        {
            if (currentProfile.Count == 0) return 0;
            return currentProfile.OrderByDescending(x => x.Value).First().Key;
        }
        
        private void CalculateValueArea(out double VAH, out double VAL)
        {
            VAH = VAL = 0;
            if (currentProfile.Count == 0) return;
            
            double totalVolume = currentProfile.Values.Sum();
            double targetVolume = totalVolume * (ValueAreaPercent / 100.0);
            double vpoc = CalculateVPOC();
            
            List<double> valueAreaPrices = new List<double> { vpoc };
            double accumulatedVolume = currentProfile[vpoc];
            
            List<double> sortedPrices = currentProfile.Keys.OrderBy(p => p).ToList();
            int pocIndex = sortedPrices.IndexOf(vpoc);
            int upperIndex = pocIndex + 1;
            int lowerIndex = pocIndex - 1;
            
            while (accumulatedVolume < targetVolume)
            {
                double upperVolume = 0;
                double lowerVolume = 0;
                
                if (upperIndex < sortedPrices.Count)
                    upperVolume = currentProfile[sortedPrices[upperIndex]];
                    
                if (lowerIndex >= 0)
                    lowerVolume = currentProfile[sortedPrices[lowerIndex]];
                
                if (upperVolume >= lowerVolume && upperIndex < sortedPrices.Count)
                {
                    valueAreaPrices.Add(sortedPrices[upperIndex]);
                    accumulatedVolume += upperVolume;
                    upperIndex++;
                }
                else if (lowerIndex >= 0)
                {
                    valueAreaPrices.Add(sortedPrices[lowerIndex]);
                    accumulatedVolume += lowerVolume;
                    lowerIndex--;
                }
                else
                {
                    break;
                }
            }
            
            VAH = valueAreaPrices.Max();
            VAL = valueAreaPrices.Min();
        }
        
        private void DetectHVNandLVN(out List<double> hvnList, out List<double> lvnList)
        {
            hvnList = new List<double>();
            lvnList = new List<double>();
            
            if (currentProfile.Count == 0) return;
            
            List<double> sortedVolumes = currentProfile.Values.OrderBy(v => v).ToList();
            int count = sortedVolumes.Count;
            
            // Calcular thresholds basados en percentiles
            int hvnIndex = (int)Math.Ceiling(count * (HVNPercentile / 100.0)) - 1;
            double hvnThreshold = sortedVolumes[Math.Max(0, Math.Min(hvnIndex, count - 1))];
            
            int lvnIndex = (int)Math.Ceiling(count * (LVNPercentile / 100.0)) - 1;
            double lvnThreshold = sortedVolumes[Math.Max(0, Math.Min(lvnIndex, count - 1))];
            
            foreach (var priceLevel in currentProfile)
            {
                if (priceLevel.Value >= hvnThreshold)
                    hvnList.Add(priceLevel.Key);
                else if (priceLevel.Value <= lvnThreshold)
                    lvnList.Add(priceLevel.Key);
            }
        }
        
        private void DrawVolumeProfileLines(double vpoc, double vah, double val, 
            List<double> hvnList, List<double> lvnList)
        {
            // Dibujar VPOC
            Draw.HorizontalLine(this, "VPOC_" + CurrentBar, vpoc, 
                Brushes.Magenta, DashStyleHelper.Solid, 2);
            
            // Dibujar Value Area
            Draw.HorizontalLine(this, "VAH_" + CurrentBar, vah, 
                Brushes.Green, DashStyleHelper.Dash, 1);
            Draw.HorizontalLine(this, "VAL_" + CurrentBar, val, 
                Brushes.Red, DashStyleHelper.Dash, 1);
            
            // Dibujar HVN
            for (int i = 0; i < hvnList.Count; i++)
            {
                Draw.Ray(this, "HVN_" + CurrentBar + "_" + i, false,
                    0, hvnList[i], 1, hvnList[i], 
                    Brushes.Orange, DashStyleHelper.Dot, 1);
            }
            
            // Dibujar LVN
            for (int i = 0; i < lvnList.Count; i++)
            {
                Draw.Ray(this, "LVN_" + CurrentBar + "_" + i, false,
                    0, lvnList[i], 1, lvnList[i], 
                    Brushes.Blue, DashStyleHelper.Dot, 1);
            }
        }
        
        #region Properties
        [NinjaScriptProperty]
        [Range(50, 100)]
        [Display(Name="HVN Percentile", Order=1, GroupName="Parameters")]
        public int HVNPercentile { get; set; }
        
        [NinjaScriptProperty]
        [Range(0, 50)]
        [Display(Name="LVN Percentile", Order=2, GroupName="Parameters")]
        public int LVNPercentile { get; set; }
        
        [NinjaScriptProperty]
        [Range(50, 100)]
        [Display(Name="Value Area Percent", Order=3, GroupName="Parameters")]
        public int ValueAreaPercent { get; set; }
        #endregion
    }
}
```

## Recursos educativos y documentación

### Cursos de pago recomendados

**NinjaCoding.net Volume Profile Course**
- URL: https://ninjacoding.net/ninjatrader/courses/volumeprofilecourse
- Contenido: Construcción completa de volume profile desde cero
- Temas: POC calculation, Value Area, TPO Market Profile, Custom Render
- Valoración: Excelente según estudiantes ("freaking awesome")
- Complejidad: Media a Avanzada

### Documentación oficial NinjaTrader

1. **Order Flow Volumetric Bars (NinjaScript API)**
   - https://ninjatrader.com/support/helpguides/nt8/order_flow_volumetric_bars2.htm
   - Métodos: GetMaximumVolume(), GetTotalVolumeForPrice(), BarDelta

2. **AddVolumetric() Method**
   - https://ninjatrader.com/support/helpguides/nt8/addvolumetric.htm
   - Para agregar series volumétricas secundarias

3. **Order Flow+ General**
   - https://ninjatrader.com/support/helpguides/nt8/order_flow_plus.htm

### Hilos de foros con código útil

1. **"Calculate Value Area of volume at price"** (NinjaTrader Forum)
   - Contiene código histórico de dValueArea
   - Discusiones sobre VOC, TPO, VWTPO

2. **"Candle VPOC"** - Extracción de VPOC con ejemplos de código

3. **"Looping through all prices in a bar"** - Construcción de profiles custom

### Repositorios GitHub para estudiar

1. **trading-code/ninjatrader-freeorderflow** (108 ⭐, activo 2025)
2. **gbzenobi/CSharp-NT8-OrderFlowKit** (base de múltiples proyectos)
3. **izzylite/volume-profile-indicator** (optimizado, MIT license)

## Requisitos técnicos generales

### Software necesario

- **NinjaTrader 8** (versión 8.0.15.1 o superior recomendada)
- **.NET Framework** 4.7.2+ (incluido con NT8)
- **Visual Studio 2019 o posterior** (solo para modificar código open-source)
- **Tick Replay habilitado** (para datos históricos sin OrderFlow+)

### Activar Tick Replay

```
1. Tools → Options → Market Data
2. Marcar "Show Tick Replay"
3. Al crear nuevo chart: Properties → Enable "Tick Replay"
```

### Data feed requirements

- Datos tick-by-tick para perfiles precisos
- Kinetick, CQG/Continuum, u otros feeds compatibles
- OrderFlow+ subscription (opcional pero recomendado para profesionales)

### Conocimientos de programación

**Mínimo:**
- C# básico
- Sintaxis NinjaScript
- Estructura de indicadores

**Recomendado:**
- Estructuras de datos (Dictionary, List, LINQ)
- Loops y condicionales
- Manejo de excepciones

**Avanzado (para rendering custom):**
- SharpDX
- Método OnRender()
- WPF rendering
- Optimización de performance

## Comparación con indicadores de pago

Para contexto, los indicadores comerciales con HVN/LVN + NinjaScript access cuestan:

- **TradeDevils Volume Profile**: ~$200-300 (trial 7 días)
  - Incluye HVN/LVN con región size options
  - Full NinjaScript API para strategies/Bloodhound
  - Delta + heatmap modes

- **MZpack Volume Profile**: €149-€369
  - Suite completa de order flow tools
  - Profesional-grade features

- **ninZa.co**: $286-$386
  - Indicadores profesionales
  - Soporte dedicado

**Conclusión sobre paid vs free:** Los indicadores de pago ofrecen HVN/LVN + API integrados, pero las opciones gratuitas pueden lograr la misma funcionalidad con trabajo de desarrollo adicional (1-3 semanas dependiendo de experiencia).

## Estado del ecosistema NinjaTrader (2024-2025)

### Feature requests pendientes de NinjaTrader

- **SFT-3297**: Agregar HVN y LVN al Order Flow Volume Profile (pendiente desde 2018)
- **SFT-3402**: Acceso NinjaScript al Order Flow Volume Profile (pendiente)

Estas features NO están implementadas en NT8 nativo a noviembre 2025.

### Comunidad activa

- **NinjaTrader Ecosystem**: 100+ indicadores gratuitos user-contributed
- **GitHub**: Múltiples proyectos open-source activos en 2024-2025
- **Automated-trading.ch**: Actualizaciones mensuales, Discord activo
- **Forums**: Support forum muy activo con code snippets

## Conclusión y path recomendado

Para cumplir con **TODOS** los requisitos (VPOC + HVN≥80 + LVN≤20 + acceso NinjaScript + gratuito):

**Solución híbrida recomendada:**

1. **Usar DValueArea** para VPOC y Value Area con acceso NinjaScript inmediato
2. **Implementar detección de HVN/LVN** usando el algoritmo de percentiles proporcionado
3. **Combinar en strategy personalizada** que acceda a DValueArea plots y calcule HVN/LVN

**Alternativa para desarrolladores:**

1. **Fork de trading-code/ninjatrader-freeorderflow** (MIT license, activo 2025)
2. **Modificar para agregar** detección de HVN/LVN percentil-based
3. **Exponer como plots** accesibles desde strategies

**Tiempo estimado de implementación:**
- Solución híbrida: 2-4 horas (usando código provisto)
- Fork y modificación: 1-2 semanas
- Construcción completa desde cero: 2-4 semanas

**Ninguna opción 100% gratuita "plug-and-play" existe actualmente** que cumpla todos los requisitos sin trabajo de programación adicional. Las opciones comerciales como TradeDevils (~$200-300) son la única alternativa si se requiere funcionalidad completa sin desarrollo custom.