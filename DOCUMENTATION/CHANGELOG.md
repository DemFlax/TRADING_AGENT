# Changelog — Agente de Trading MNQ

Todos los cambios notables al proyecto serán documentados en este archivo.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.0.0/),
y este proyecto adhiere a [Versionado Semántico](https://semver.org/lang/es/).

---

## [Unreleased]

### Próximas Features
- Dashboard web para visualización de métricas
- Sistema Guardian anti-tilt
- Multi-cuenta (hasta 5 cuentas simultáneas)
- Integración CrossTrade API (<20ms latency)

---

## [1.0.0] - 2025-11-06

### Agregado
- Arquitectura completa del sistema (ARQUITECTURA.md)
- Guía de desarrollo (GUIA_DESARROLLO.md)
- Referencia API (API_REFERENCE.md)
- Guía de deployment (DEPLOYMENT.md)
- Estrategia de testing (TESTING_STRATEGY.md)
- Documentación operativa (Flujo Diario, Estrategia APEX)
- README.md con información del proyecto
- Estructura base del proyecto en Python
- Diseño de dominio con entidades, value objects e interfaces
- Arquitectura en capas (Dominio, Aplicación, Infraestructura)
- Integración con Rithmic API (diseño)
- Integración con NinjaTrader 8 vía ATI (diseño)
- Add-on NinjaScript para NT8 (diseño)
- Sistema de journal con SQLite
- Gestión de riesgo según reglas APEX
- Estrategia MNQ basada en niveles (PDH/PDL/ONH/ONL)
- Setups: A-L, B-L, A-S, B-S, B-M
- Modo dual: Autónomo y Señales-Only
- Circuit breakers y límites de riesgo
- Flujo diario completo (Boot → Pre-Market → Trading → Close → Post-Market)
- Configuración `.env` para variables de entorno
- `.gitignore` para archivos sensibles

### Documentación
- Documentación técnica completa
- Estándares de código Python y C#
- Workflow Git y convenciones de commits
- Guía de testing unitario, integración y backtesting
- Proceso de deployment paso a paso
- Troubleshooting común

---

## [0.1.0] - 2025-11-05

### Agregado
- Inicio del proyecto
- Conexión al repositorio GitHub
- Documentación operativa inicial (MNQ_AgenteIA_Flujo_Diario.md, MNQ_APEX50k_Estrategia.md)
- Definición de requisitos mediante entrevista estructurada
- Aprobación de arquitectura híbrida Python-C#

---

## Tipos de Cambios

- **Agregado** para nuevas funcionalidades
- **Cambiado** para cambios en funcionalidad existente
- **Obsoleto** para funcionalidades que serán removidas
- **Removido** para funcionalidades eliminadas
- **Corregido** para corrección de bugs
- **Seguridad** para vulnerabilidades

---

## Formato de Versiones

`MAJOR.MINOR.PATCH`

- **MAJOR:** Cambios incompatibles de API
- **MINOR:** Nueva funcionalidad compatible con versiones anteriores
- **PATCH:** Corrección de bugs compatible con versiones anteriores

---

## Links de Comparación

[Unreleased]: https://github.com/DemFlax/-TRADING_AGENT_IA/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/DemFlax/-TRADING_AGENT_IA/compare/v0.1.0...v1.0.0
[0.1.0]: https://github.com/DemFlax/-TRADING_AGENT_IA/releases/tag/v0.1.0
