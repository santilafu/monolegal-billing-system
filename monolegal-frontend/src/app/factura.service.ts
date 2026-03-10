import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * Interfaz TypeScript que refleja el modelo Factura del backend.
 * TypeScript usará esta interfaz para dar autocompletado y detectar errores.
 */
export interface Factura {
  id: string;
  clienteId: string;
  nombreCliente: string;
  emailCliente: string;
  monto: number;
  estado: string;
  fechaFactura: string;
}

/**
 * Interfaz para la respuesta del endpoint /procesar
 */
export interface ResultadoProceso {
  mensaje: string;
  totalPendientes: number;
  procesadas: number;
  fallidas: number;
  errores: string[];
}

/**
 * Servicio Angular que encapsula todas las llamadas HTTP a la API.
 *
 * PRINCIPIO DE RESPONSABILIDAD ÚNICA: Este servicio solo se ocupa
 * de la comunicación con el backend. Los componentes no hacen llamadas
 * HTTP directamente — siempre a través de este servicio.
 *
 * @Injectable({ providedIn: 'root' }) → registra el servicio como Singleton
 * global disponible en toda la aplicación sin tener que declararlo en módulos.
 */
@Injectable({
  providedIn: 'root'
})
export class FacturaService {

  // URL base de la API. En producción se cambiaría a la URL del servidor real.
  private readonly apiUrl = 'http://localhost:5218/api/facturas';

  /**
   * HttpClient es inyectado automáticamente por Angular.
   * Para que funcione, debe registrarse provideHttpClient() en app.config.ts.
   */
  constructor(private http: HttpClient) {}

  /**
   * Llama al endpoint GET /api/facturas.
   * Devuelve un Observable — Angular trabaja con Observables para operaciones asíncronas.
   * El componente se "suscribe" al Observable para recibir los datos cuando lleguen.
   */
  getFacturas(): Observable<Factura[]> {
    return this.http.get<Factura[]>(this.apiUrl);
  }

  /**
   * Llama al endpoint POST /api/facturas/procesar.
   * Ejecuta el proceso de recordatorios en el backend.
   * No envía cuerpo en la petición (segundo parámetro = {}).
   */
  procesarFacturas(): Observable<ResultadoProceso> {
    return this.http.post<ResultadoProceso>(`${this.apiUrl}/procesar`, {});
  }
}
