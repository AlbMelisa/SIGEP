const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5100/api';

export async function apiRequest(endpoint, options = {}) {
  const token = localStorage.getItem('sigep_token') || sessionStorage.getItem('sigep_token');
  const headers = {
    'Content-Type': 'application/json',
    ...(token && token !== 'sigep-local-demo-session' ? { Authorization: `Bearer ${token}` } : {}),
    ...options.headers,
  };

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers,
  });

  const data = await response.json().catch(() => null);

  if (!response.ok) {
    const errorMsg = data?.error || data?.mensaje || `Error en la solicitud: ${response.status} ${response.statusText}`;
    throw new Error(errorMsg);
  }

  return data;
}

export const authApi = {
  login: (username, password) =>
    apiRequest('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ username, password }),
    }),
  getMe: () => apiRequest('/auth/me'),
  logout: () => {
    localStorage.removeItem('sigep_token');
    localStorage.removeItem('sigep_user');
    sessionStorage.removeItem('sigep_token');
    sessionStorage.removeItem('sigep_user');
  },
};

export const personalApi = {
  listar: (soloActivos = false) => apiRequest(`/personal${soloActivos ? '?soloActivos=true' : ''}`),
  obtenerPorId: (id) => apiRequest(`/personal/${id}`),
  crear: (datos) =>
    apiRequest('/personal', {
      method: 'POST',
      body: JSON.stringify(datos),
    }),
  actualizar: (id, datos) =>
    apiRequest(`/personal/${id}`, {
      method: 'PUT',
      body: JSON.stringify(datos),
    }),
  cambiarEstado: (id, nuevoEstado) =>
    apiRequest(`/personal/${id}/estado`, {
      method: 'PATCH',
      body: JSON.stringify({ nuevoEstado }),
    }),
  referencias: () => apiRequest('/personal/referencias'),
};

export const asistenciaApi = {
  obtenerPlanilla: (fecha, turnoId = null) => {
    const query = new URLSearchParams({ fecha });
    if (turnoId) query.append('turnoId', turnoId);
    return apiRequest(`/asistencia/planilla?${query.toString()}`);
  },
  guardarPlanillaLote: (payload) =>
    apiRequest('/asistencia/lote', {
      method: 'POST',
      body: JSON.stringify(payload),
    }),
  consultarHistorial: (filtros = {}) => {
    const params = new URLSearchParams();
    if (filtros.desde) params.append('desde', filtros.desde);
    if (filtros.hasta) params.append('hasta', filtros.hasta);
    if (filtros.empleadoId) params.append('empleadoId', filtros.empleadoId);
    if (filtros.turnoId) params.append('turnoId', filtros.turnoId);
    if (filtros.estado) params.append('estado', filtros.estado);
    if (filtros.tipoJornada) params.append('tipoJornada', filtros.tipoJornada);
    return apiRequest(`/asistencia/historial?${params.toString()}`);
  },
};

export const sabadosApi = {
  generarPropuesta: (fechaSabado, observaciones = null) =>
    apiRequest('/planificacion-sabados/propuesta', {
      method: 'POST',
      body: JSON.stringify({ fechaSabado, observaciones }),
    }),
  obtenerPorFecha: (fecha) => apiRequest(`/planificacion-sabados/por-fecha/${fecha}`),
  obtenerPorId: (id) => apiRequest(`/planificacion-sabados/${id}`),
  obtenerHistorial: (limite = 15) => apiRequest(`/planificacion-sabados/historial?limite=${limite}`),
  modificarTurno: (datos) =>
    apiRequest('/planificacion-sabados/modificar-turno', {
      method: 'PUT',
      body: JSON.stringify(datos),
    }),
  reemplazar: (datos) =>
    apiRequest('/planificacion-sabados/reemplazar', {
      method: 'PUT',
      body: JSON.stringify(datos),
    }),
  confirmar: (id) =>
    apiRequest(`/planificacion-sabados/${id}/confirmar`, {
      method: 'POST',
    }),
};

export const eventosApi = {
  listarActivos: () => apiRequest('/eventos/activos'),
  listarHistorial: (limite = 20) => apiRequest(`/eventos/historial?limite=${limite}`),
  obtenerPorId: (id) => apiRequest(`/eventos/${id}`),
  crear: (datos) =>
    apiRequest('/eventos', {
      method: 'POST',
      body: JSON.stringify(datos),
    }),
  previsualizarScoring: (idTurno) => apiRequest(`/eventos/turnos/${idTurno}/scoring`),
  aplicarScoring: (idTurno) =>
    apiRequest(`/eventos/turnos/${idTurno}/aplicar-scoring`, {
      method: 'POST',
    }),
  ajustar: (datos) =>
    apiRequest('/eventos/turnos/ajustar', {
      method: 'PUT',
      body: JSON.stringify(datos),
    }),
  confirmar: (id) =>
    apiRequest(`/eventos/${id}/confirmar`, {
      method: 'POST',
    }),
};
