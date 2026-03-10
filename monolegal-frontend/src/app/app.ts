import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FacturaService, Factura, ResultadoProceso } from './factura.service';

/**
 * Componente raíz de la aplicación.
 *
 * En Angular, un componente = lógica (TypeScript) + plantilla (HTML) + estilos (CSS).
 *
 * @Component define los metadatos:
 *   - selector:    la etiqueta HTML que representa este componente (<app-root>)
 *   - imports:     módulos que necesita la plantilla (CommonModule para *ngIf, *ngFor, etc.)
 *   - templateUrl: archivo HTML de la vista
 *   - styleUrl:    archivo CSS de estilos
 */
@Component({
  selector: 'app-root',
  imports: [CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {

  // Lista de facturas cargadas desde la API
  facturas: Factura[] = [];

  // Resultado del último proceso de recordatorios
  resultado: ResultadoProceso | null = null;

  // Controla si se está cargando datos (para mostrar mensaje de espera)
  cargando = false;

  // Controla si se está procesando (para deshabilitar el botón)
  procesando = false;

  // Mensaje de error si algo falla
  error: string | null = null;

  /**
   * FacturaService se inyecta automáticamente por Angular.
   * Solo declaramos el tipo — Angular se encarga de crear la instancia.
   */
  constructor(private facturaService: FacturaService) {}

  /**
   * ngOnInit es el "gancho de ciclo de vida" que Angular llama
   * automáticamente cuando el componente está listo en pantalla.
   * Es el lugar correcto para cargar datos iniciales.
   */
  ngOnInit(): void {
    this.cargarFacturas();
  }

  /**
   * Llama a la API para obtener todas las facturas y las guarda en this.facturas.
   * El componente reacciona al Observable mediante subscribe().
   */
  cargarFacturas(): void {
    this.cargando = true;
    this.error = null;

    this.facturaService.getFacturas().subscribe({
      next: (datos) => {
        this.facturas = datos;
        this.cargando = false;
      },
      error: (err) => {
        this.error = 'Error al conectar con la API. ¿Está el backend en marcha?';
        this.cargando = false;
        console.error(err);
      }
    });
  }

  /**
   * Llama al endpoint /procesar del backend.
   * Una vez terminado, recarga las facturas para mostrar los estados actualizados.
   */
  procesarRecordatorios(): void {
    this.procesando = true;
    this.resultado = null;
    this.error = null;

    this.facturaService.procesarFacturas().subscribe({
      next: (res) => {
        this.resultado = res;
        this.procesando = false;
        // Recargamos la tabla para ver los nuevos estados actualizados
        this.cargarFacturas();
      },
      error: (err) => {
        this.error = 'Error al ejecutar el proceso de recordatorios.';
        this.procesando = false;
        console.error(err);
      }
    });
  }

  /**
   * Devuelve una clase CSS según el estado de la factura.
   * Usado en la plantilla para colorear los badges de estado.
   */
  getClaseEstado(estado: string): string {
    switch (estado) {
      case 'primerrecordatorio':  return 'badge-primero';
      case 'segundorecordatorio': return 'badge-segundo';
      case 'desactivado':         return 'badge-desactivado';
      case 'pagado':              return 'badge-pagado';
      default:                    return 'badge-default';
    }
  }

  /**
   * Convierte el código de estado a texto legible para mostrar en pantalla.
   */
  getTextoEstado(estado: string): string {
    switch (estado) {
      case 'primerrecordatorio':  return '1er Recordatorio';
      case 'segundorecordatorio': return '2do Recordatorio';
      case 'desactivado':         return 'Desactivado';
      case 'pagado':              return 'Pagado';
      default:                    return estado;
    }
  }

  /**
   * Cuenta las facturas por estado — usado para las tarjetas del resumen superior.
   */
  contarPorEstado(estado: string): number {
    return this.facturas.filter(f => f.estado === estado).length;
  }
}
