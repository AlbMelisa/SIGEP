import { createContext, useContext, useState, useCallback } from 'react';
import { authApi } from '../services/api.js';

const AuthContext = createContext(null);
const DEMO_TOKEN = 'sigep-local-demo-session';

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const saved = localStorage.getItem('sigep_user') || sessionStorage.getItem('sigep_user');
    return saved ? JSON.parse(saved) : null;
  });
  const [token, setToken] = useState(() => localStorage.getItem('sigep_token') || sessionStorage.getItem('sigep_token'));
  const [loading, setLoading] = useState(false);
  const [authError, setAuthError] = useState('');
  const [authTransition, setAuthTransition] = useState(null);

  const login = async (username, password, rememberMe = true) => {
    setLoading(true);
    setAuthError('');
    try {
      const data = await authApi.login(username, password);
      setToken(data.token);
      setUser(data.usuario);
      const storage = rememberMe ? localStorage : sessionStorage;
      const otherStorage = rememberMe ? sessionStorage : localStorage;
      storage.setItem('sigep_token', data.token);
      storage.setItem('sigep_user', JSON.stringify(data.usuario));
      otherStorage.removeItem('sigep_token');
      otherStorage.removeItem('sigep_user');
      setAuthTransition('login');
      return { success: true };
    } catch (err) {
      if (err instanceof TypeError || err.message === 'Failed to fetch') {
        if (username.trim() === 'jefe_admin' && password === 'Password123!') {
          const demoUser = { username: 'jefe_admin', rol: 'Jefe' };
          const storage = rememberMe ? localStorage : sessionStorage;
          const otherStorage = rememberMe ? sessionStorage : localStorage;
          setToken(DEMO_TOKEN);
          setUser(demoUser);
          storage.setItem('sigep_token', DEMO_TOKEN);
          storage.setItem('sigep_user', JSON.stringify(demoUser));
          otherStorage.removeItem('sigep_token');
          otherStorage.removeItem('sigep_user');
          setAuthError('');
          setAuthTransition('login');
          return { success: true, demo: true };
        }
        const message = 'No se pudo conectar con SIGEP. Verificá que la API esté activa en localhost:5100 y PostgreSQL en localhost:5432.';
        setAuthError(message);
        return { success: false, error: message };
      }
      setAuthError(err.message || 'Error al iniciar sesión');
      return { success: false, error: err.message };
    } finally {
      setLoading(false);
    }
  };

  const logout = () => {
    authApi.logout();
    setAuthTransition('logout');
  };

  const completeAuthTransition = useCallback(() => {
    if (authTransition === 'logout') {
      setToken(null);
      setUser(null);
    }
    setAuthTransition(null);
  }, [authTransition]);

  const isJefe = user?.rol === 'Jefe';
  const isEncargado = user?.rol === 'Encargado';
  const isAuthenticated = !!token && !!user;

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        loading,
        authError,
        authTransition,
        setAuthError,
        completeAuthTransition,
        login,
        logout,
        isAuthenticated,
        isJefe,
        isEncargado,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth debe ser utilizado dentro de un AuthProvider');
  }
  return context;
}

