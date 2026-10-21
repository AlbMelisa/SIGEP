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
    const errorMsg = data?.error || `Error en la solicitud: ${response.status} ${response.statusText}`;
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

export const usuariosApi = {
  listar: () => apiRequest('/usuarios'),
  crear: (datos) =>
    apiRequest('/usuarios', {
      method: 'POST',
      body: JSON.stringify(datos),
    }),
  cambiarEstado: (id, estado) =>
    apiRequest(`/usuarios/${id}/estado`, {
      method: 'PATCH',
      body: JSON.stringify({ estado }),
    }),
};

